using DARE_Control.Models.Services;
using DARE_Control.Models.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Project_Admin.Repositories.DbContexts;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")
));
ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

string AppName = typeof(Program).Module.Name.Replace(".dll", "");


// -- authentication here
var keyCloakSettings = new KeyCloakSettings();
configuration.Bind(nameof(keyCloakSettings), keyCloakSettings);
builder.Services.AddSingleton(keyCloakSettings);


builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();


//add services here
builder.Services.AddScoped<CustomCookieEvent>();



builder.Services.AddMvc().AddViewComponentsAsServices();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    options.OnAppendCookie = cookieContext =>
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    options.OnDeleteCookie = cookieContext =>
        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
});

builder.Services.AddAuthorization(options =>
{
    //options.AddPolicy("admin", policy =>
    //    policy.RequireClaim("groups", "dare-control-admin")); //MIGHT NEED TO CHANGE LATER

    //probably not needed
    options.AddPolicy(
            "admin",
            policyBuilder => policyBuilder.RequireAssertion(
                context => context.User.HasClaim(claim =>
                    claim.Type == "groups"
                    && claim.Value.Contains("dare-control-admin"))));
    options.AddPolicy(
                "company",
                policyBuilder => policyBuilder.RequireAssertion(
                    context => context.User.HasClaim(claim =>
                        claim.Type == "groups"
                        && claim.Value.Contains("dare-control-company"))));
    options.AddPolicy(
            "user",
            policyBuilder => policyBuilder.RequireAssertion(
                context => context.User.HasClaim(claim =>
                    claim.Type == "groups"
                    && claim.Value.Contains("dare-control-user"))));

});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
            .AddCookie(options => options.EventsType = typeof(CustomCookieEvent))
            .AddOpenIdConnect(options =>
            {
                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    UseProxy = true,
                    UseDefaultCredentials = true,
                    Proxy = new WebProxy()
                    {
                        Address = new Uri("https://proxy:8080"),
                        BypassList = new[] { "keycloak-dev" }
                    }
                };

                // URL of the Keycloak server
                options.Authority = keyCloakSettings.Authority;
                // Client configured in the Keycloak
                options.ClientId = keyCloakSettings.ClientId;
                // Client secret shared with Keycloak
                options.ClientSecret = keyCloakSettings.ClientSecret;
                options.SaveTokens = true;

                options.ResponseType = OpenIdConnectResponseType.Code; //Configuration["Oidc:ResponseType"];
                                                                       // For testing we disable https (should be true for production)
                options.RemoteSignOutPath = keyCloakSettings.RemoteSignOutPath;
                options.SignedOutRedirectUri = keyCloakSettings.SignedOutRedirectUri;
                options.RequireHttpsMetadata = false;
                options.GetClaimsFromUserInfoEndpoint = true;
                //options.Scope.Add("openid");
                //options.Scope.Add("profile");
                //options.Scope.Add("email");
                //options.Scope.Add("claims");
                //options.SaveTokens = true;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.Events = new OpenIdConnectEvents
                {
                    OnRemoteFailure = context =>
                    {
                        //Log.Error("OnRemoteFailure: {ex}", context.Failure);
                        if (context.Failure.Message.Contains("Correlation failed"))
                        {
                            //Log.Warning("call TokenExpiredAddress {TokenExpiredAddress}", keyCloakSettings.TokenExpiredAddress);
                            context.Response.Redirect(keyCloakSettings.TokenExpiredAddress);
                        }
                        else
                        {
                            //Log.Warning("call /Error/500");
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
                        // Log.Information("HttpContext.Connection.RemoteIpAddress : {RemoteIpAddress}", context.HttpContext.Connection.RemoteIpAddress);
                        //Log.Information("HttpContext.Connection.RemotePort : {RemotePort}", context.HttpContext.Connection.RemotePort);
                        // Log.Information("HttpContext.Request.Scheme : {Scheme}", context.HttpContext.Request.Scheme);
                        // Log.Information("HttpContext.Request.Host : {Host}", context.HttpContext.Request.Host);

                        foreach (var header in context.HttpContext.Request.Headers)
                        {
                            // Log.Information("Request Header {key} - {value}", header.Key, header.Value);
                        }

                        foreach (var header in context.HttpContext.Response.Headers)
                        {
                            // Log.Information("Response Header {key} - {value}", header.Key, header.Value);
                        }


                        //context.ProtocolMessage.RedirectUri = Configuration["Oidc:RedirectUri"];
                        //Log.Information( context.ProtocolMessage.RedirectUri);
                        //Log.Information( context.ProtocolMessage.RedirectUri);
                        await Task.FromResult(0);
                    }
                };

                options.NonceCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SameSite = SameSiteMode.None;
            });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    DataInitaliser.SeedData(db).Wait();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting(); 

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

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
            options.SameSite = SameSiteMode.Unspecified;
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