using Microsoft.IdentityModel.Logging;
using BL.Services;
using BL.Models.Services;
using BL.Models.Settings;
using Serilog;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

IdentityModelEventSource.ShowPII = true;

builder.Services.AddControllersWithViews().AddNewtonsoftJson(options => {
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
}).AddRazorRuntimeCompilation();

ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

Log.Logger = CreateSerilogLogger(configuration, environment);
Log.Information("TRE-UI logging LastStatusUpdate.");



// -- authentication here
var treKeyCloakSettings = new TreKeyCloakSettings();
configuration.Bind(nameof(treKeyCloakSettings), treKeyCloakSettings);
builder.Services.AddSingleton(treKeyCloakSettings);


builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();


//add services here
builder.Services.AddScoped<CustomCookieEvent>();

builder.Services.AddScoped<ITREClientHelper, TREClientHelper>();


builder.Services.AddMvc().AddViewComponentsAsServices();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.OnAppendCookie = cookieContext =>
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    options.OnDeleteCookie = cookieContext =>
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
});


JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(configuration["TreAPISettings:Address"])
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});


builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformerBL>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
             
             .AddCookie(o =>
             {
                 o.SessionStore = new MemoryCacheTicketStore();
                 o.EventsType = typeof(CustomCookieEvent);
             })
            .AddOpenIdConnect(options =>
            {
                if (treKeyCloakSettings.Proxy)
                {
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        UseProxy = true,
                        UseDefaultCredentials = true,
                        Proxy = new WebProxy()
                        {
                            Address = new Uri(treKeyCloakSettings.ProxyAddresURL),
                            BypassList = new[] { treKeyCloakSettings.BypassProxy }
                        }
                    };
                }

                
                // URL of the Keycloak server
                options.Authority = treKeyCloakSettings.Authority;
                //// Client configured in the Keycloak
                options.ClientId = treKeyCloakSettings.ClientId;
                //// Client secret shared with Keycloak
                options.ClientSecret = treKeyCloakSettings.ClientSecret;
                options.MetadataAddress = treKeyCloakSettings.MetadataAddress;

                options.SaveTokens = true;

                options.ResponseType = OpenIdConnectResponseType.Code; //Configuration["Oidc:ResponseType"];
                                                                       // For testing we disable https (should be true for production)
                options.RemoteSignOutPath = treKeyCloakSettings.RemoteSignOutPath;
                options.SignedOutRedirectUri = treKeyCloakSettings.SignedOutRedirectUri;
                options.RequireHttpsMetadata = false;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                
                options.SaveTokens = true;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.Events = new OpenIdConnectEvents
                {
                    OnAccessDenied = context =>
                    {
                        Log.Error("{Function}: {ex}", "OnAccessDenied", context.AccessDeniedPath);
                        context.HandleResponse();
                        return context.Response.CompleteAsync();

                    },
                    OnAuthenticationFailed = context =>
                    {
                        Log.Error("{Function}: {ex}", "OnAuthFailed", context.Exception.Message);
                        context.HandleResponse();
                        return context.Response.CompleteAsync();
                    },
                    OnRemoteFailure = context =>
                    {
                        Log.Error("OnRemoteFailure: {ex}", context.Failure);
                        if (context.Failure.Message.Contains("Correlation failed"))
                        {
                            Log.Warning("call TokenExpiredAddress {TokenExpiredAddress}", treKeyCloakSettings.TokenExpiredAddress);
                            context.Response.Redirect(treKeyCloakSettings.TokenExpiredAddress);
                        }
                        else
                        {
                            Log.Warning("call /Error/500");
                            context.Response.Redirect("/Error/500");
                        }

                        context.HandleResponse();

                        return context.Response.CompleteAsync();
                    },
                    OnMessageReceived = context =>
                    {
                        string accessToken = context.Request.Query["access_token"];
                        PathString path = context.HttpContext.Request.Path;

                        if (
                            !string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/api/SignalRHub")
                        )
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                    OnRedirectToIdentityProvider = async context =>
                    {
                        Log.Information("HttpContext.Connection.RemoteIpAddress : {RemoteIpAddress}", context.HttpContext.Connection.RemoteIpAddress);
                        Log.Information("HttpContext.Connection.RemotePort : {RemotePort}", context.HttpContext.Connection.RemotePort);
                        Log.Information("HttpContext.Request.Scheme : {Scheme}", context.HttpContext.Request.Scheme);
                        Log.Information("HttpContext.Request.Host : {Host}", context.HttpContext.Request.Host);

                        foreach (var header in context.HttpContext.Request.Headers)
                        {
                             Log.Information("Request Header {key} - {value}", header.Key, header.Value);
                        }

                        foreach (var header in context.HttpContext.Response.Headers)
                        {
                             Log.Information("Response Header {key} - {value}", header.Key, header.Value);
                        }

                        if (treKeyCloakSettings.UseRedirectURL)
                        {
                            context.ProtocolMessage.RedirectUri = treKeyCloakSettings.RedirectURL;
                        }
                        Log.Information(context.ProtocolMessage.RedirectUri);
                        
                        await Task.FromResult(0);
                    }
                };

                options.NonceCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = ClaimTypes.Role,
                    ValidateIssuer = true
                };
            });


var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

Serilog.ILogger CreateSerilogLogger(ConfigurationManager configuration, IWebHostEnvironment environment)
{
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var seqApiKey = configuration["Serilog:SeqApiKey"];



    return new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .Enrich.WithProperty("ApplicationContext", environment.ApplicationName)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(seqServerUrl, apiKey: seqApiKey)
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

}

//removed to stop redirection
//app.UseHttpsRedirection();

app.UseStaticFiles();

// ST: try removing to stop https redirect
//app.UseCookiePolicy(new CookiePolicyOptions
//{
//    Secure = CookieSecurePolicy.Always
//});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseCors();

app.Run();

#region SameSite Cookie Issue - https://community.auth0.com/t/correlation-failed-unknown-location-error-on-chrome-but-not-in-safari/40013/7

void CheckSameSite(HttpContext httpContext, CookieOptions options)
{
    if (options.SameSite == SameSiteMode.None)
    {
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        //configure cookie policy to omit samesite=none when request is not https
        if (!httpContext.Request.IsHttps || DisallowsSameSiteNone(userAgent))
        {
            options.SameSite = SameSiteMode.None;
        }
    }
}


//  Read comments in https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
bool DisallowsSameSiteNone(string userAgent)
{
    // Check if a null or empty string has been passed in, since this
    // will cause further interrogation of the useragent to fail.
    if (String.IsNullOrWhiteSpace(userAgent))
        return false;

    // Cover all iOS based browsers here. This includes:
    // - Safari on iOS 12 for iPhone, iPod Touch, iPad
    // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
    // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
    // All of which are broken by SameSite=None, because they use the iOS networking
    // stack.
    if (userAgent.Contains("CPU iPhone OS 12") ||
        userAgent.Contains("iPad; CPU OS 12"))
    {
        return true;
    }

    // Cover Mac OS X based browsers that use the Mac OS networking stack. 
    // This includes:
    // - Safari on Mac OS X.
    // This does not include:
    // - Chrome on Mac OS X
    // Because they do not use the Mac OS networking stack.
    if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
        userAgent.Contains("Version/") && userAgent.Contains("Safari"))
    {
        return true;
    }

    // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
    // and none in this range require it.
    // Note: this covers some pre-Chromium Edge versions, 
    // but pre-Chromium Edge does not require SameSite=None.
    if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
    {
        return true;
    }

    return false;
}


#endregion
