using BL.Models.Settings;

using BL.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net;
using Microsoft.AspNetCore.Http.Connections;
using TRE_API.Services.SignalR;
using BL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using TRE_API.Repositories.DbContexts;
using TRE_API.Services;
using Newtonsoft.Json;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Hosting.Server.Features;
using TRE_API;
using BL.Models.ViewModels;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

Log.Logger = CreateSerilogLogger(configuration, environment);
Log.Information("TRE API logging LastStatusUpdate.");


// Add services to the container.
builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
}
); ;
builder.Services.AddDbContext<ApplicationDbContext>(options => options
    .UseLazyLoadingProxies(true)
    .UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")
));
AddServices(builder);

//Add Dependancies
AddDependencies(builder, configuration);

var treKeyCloakSettings = new TreKeyCloakSettings();
configuration.Bind(nameof(treKeyCloakSettings), treKeyCloakSettings);
builder.Services.AddSingleton(treKeyCloakSettings);

var minioSettings = new MinioSettings();
configuration.Bind(nameof(MinioSettings), minioSettings);
builder.Services.AddSingleton(minioSettings);

var submissionKeyCloakSettings = new BaseKeyCloakSettings();
configuration.Bind(nameof(submissionKeyCloakSettings), submissionKeyCloakSettings);
builder.Services.AddSingleton(submissionKeyCloakSettings);

builder.Services.AddScoped<IDareClientWithoutTokenHelper, DareClientWithoutTokenHelper>();
builder.Services.AddScoped<IDataEgressClientHelper, DataEgressClientHelper>();

string hangfireConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHangfire(config => { config.UsePostgreSqlStorage(hangfireConnectionString); });

builder.Services.AddHangfireServer();
var encryptionSettings = new EncryptionSettings();
configuration.Bind(nameof(encryptionSettings), encryptionSettings);
builder.Services.AddSingleton(encryptionSettings);
builder.Services.AddScoped<IKeycloakTokenHelper, KeycloakTokenHelper>();
builder.Services.AddScoped<IEncDecHelper, EncDecHelper>();
builder.Services.AddScoped<IDareSyncHelper, DareSyncHelper>();
builder.Services.AddScoped<IDoWork, DoWork>();


var TVP = new TokenValidationParameters
{
    ValidateAudience = true,
    ValidAudiences = treKeyCloakSettings.ValidAudiences.Trim().Split(',').ToList(),
    ValidIssuer = treKeyCloakSettings.Authority,
    ValidateIssuerSigningKey = true,
    ValidateIssuer = true,
    ValidateLifetime = true
};
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformerBL>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
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
        options.Authority = treKeyCloakSettings.Authority;
        options.Audience = treKeyCloakSettings.ClientId;
        
        
        
        options.MetadataAddress = treKeyCloakSettings.MetadataAddress;

        options.RequireHttpsMetadata = false; // dev only
        options.IncludeErrorDetails = true;

        options.TokenValidationParameters = TVP;

    });

// - authorize here
builder.Services.AddAuthorization(options =>
{

});

// Enable CORS
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

var app = builder.Build();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});
// --- Session Token

// Configure the HTTP request pipeline.

    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.EnableValidator(null);
        c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{environment.ApplicationName} v1");
        c.OAuthClientId(treKeyCloakSettings.ClientId);
        c.OAuthClientSecret(treKeyCloakSettings.ClientSecret);
        c.OAuthAppName(treKeyCloakSettings.ClientId);
    });
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}




using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

}


//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
//app.UseCookiePolicy(new CookiePolicyOptions
//{
//    Secure = CookieSecurePolicy.Always
//});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


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
void AddDependencies(WebApplicationBuilder builder, ConfigurationManager configuration)
{

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddScoped<IMinioHelper, MinioHelper>();
    builder.Services.AddScoped<ISignalRService, SignalRService>();
    builder.Services.AddMvc().AddControllersAsServices();

}



/// <summary>
/// Add Services
/// </summary>
void AddServices(WebApplicationBuilder builder)
{
    builder.Services.AddHttpClient();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSignalR();

    //TODO
    builder.Services.AddSwaggerGen(c =>
    {

        c.SwaggerDoc("v1", new OpenApiInfo { Title = environment.ApplicationName, Version = "v1" });
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "JWT Authentication",
            Description = "Enter JWT token.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Id = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme
            }
        };

        c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { securityScheme, new string[] { } }
        });
    }
    );

    if (!string.IsNullOrEmpty(configuration.GetConnectionString("DefaultConnection")))
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(
          builder.Configuration.GetConnectionString("DefaultConnection")
      ));
    }
}

//for SignalR
app.UseCors();
app.MapHub<SignalRService>("/signalRHub", options =>
{
    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
}).RequireCors(MyAllowSpecificOrigins);
app.UseHangfireDashboard();
RecurringJob.AddOrUpdate<IDoWork>(a => a.Execute(), Cron.MinuteInterval(10));

var port = app.Environment.WebRootPath;

// Print the port number
Console.WriteLine("Application is running on port: " + port);
app.Run();


