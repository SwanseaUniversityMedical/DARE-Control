using BL.Models.Settings;
using DARE_API.Repositories.DbContexts;
using DARE_API.Services.Contract;
using DARE_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net;

using Newtonsoft.Json;
using BL.Rabbit;
using EasyNetQ;
using BL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using BL.Models.ViewModels;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

Log.Logger = CreateSerilogLogger(configuration, environment);
Log.Information("API logging LastStatusUpdate.");

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

//Add Services
AddServices(builder);

//Add Dependancies
AddDependencies(builder, configuration);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});



builder.Services.Configure<RabbitMQSetting>(configuration.GetSection("RabbitMQ"));
builder.Services.AddTransient(cfg => cfg.GetService<IOptions<RabbitMQSetting>>().Value);
var bus =
builder.Services.AddSingleton(RabbitHutch.CreateBus($"host={configuration["RabbitMQ:HostAddress"]}:{int.Parse(configuration["RabbitMQ:PortNumber"])};virtualHost={configuration["RabbitMQ:VirtualHost"]};username={configuration["RabbitMQ:Username"]};password={configuration["RabbitMQ:Password"]}"));
Task task = SetUpRabbitMQ.DoItAsync(configuration["RabbitMQ:HostAddress"], configuration["RabbitMQ:PortNumber"], configuration["RabbitMQ:VirtualHost"], configuration["RabbitMQ:Username"], configuration["RabbitMQ:Password"]);

var controlKeyCloakSettings = new ControlKeyCloakSettings();
configuration.Bind(nameof(controlKeyCloakSettings), controlKeyCloakSettings);
builder.Services.AddSingleton(controlKeyCloakSettings);


var minioSettings = new MinioSettings();
configuration.Bind(nameof(MinioSettings), minioSettings);
builder.Services.AddSingleton(minioSettings);

builder.Services.AddHostedService<ConsumeInternalMessageService>();
var TVP = new TokenValidationParameters
{
    ValidateAudience = true,
    ValidAudiences = controlKeyCloakSettings.ValidAudiences.Trim().Split(',').ToList(),
    ValidIssuer = controlKeyCloakSettings.Authority,
    ValidateIssuerSigningKey = true,
    ValidateIssuer = true,
    ValidateLifetime = true
};

builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformerBL>();



builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        if (controlKeyCloakSettings.Proxy)
        {
            options.BackchannelHttpHandler = new HttpClientHandler
            {
                UseProxy = true,
                UseDefaultCredentials = true,
                Proxy = new WebProxy()
                {
                    Address = new Uri(controlKeyCloakSettings.ProxyAddresURL),
                    BypassList = new[] { controlKeyCloakSettings.BypassProxy }
                }
            };
        }
        options.Authority = controlKeyCloakSettings.Authority;
        options.Audience = controlKeyCloakSettings.ClientId;
        options.MetadataAddress = controlKeyCloakSettings.MetadataAddress;

        options.RequireHttpsMetadata = false; // dev only
        options.IncludeErrorDetails = true;

        options.TokenValidationParameters = TVP;


    });

// - authorize here
builder.Services.AddAuthorization(options =>
{

});

var app = builder.Build();

var serviceScopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});
// --- Session Token

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.EnableValidator(null);
        c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{environment.ApplicationName} v1");
        c.OAuthClientId(controlKeyCloakSettings.ClientId);
        c.OAuthClientSecret(controlKeyCloakSettings.ClientSecret);
        c.OAuthAppName(controlKeyCloakSettings.ClientId);
    });
    app.UseDeveloperExceptionPage();
}

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
    var keytoken = scope.ServiceProvider.GetRequiredService<IKeyclockTokenAPIHelper>();
    var miniosettings = scope.ServiceProvider.GetRequiredService<MinioSettings>();
    var miniohelper = scope.ServiceProvider.GetRequiredService<IMinioHelper>();
    var userService = scope.ServiceProvider.GetRequiredService<IKeycloakMinioUserService>();

    db.Database.Migrate();
    var initialiser = new DataInitaliser(miniosettings, miniohelper, db, keytoken, userService);
    initialiser.SeedData();
}


//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
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

void AddDependencies(WebApplicationBuilder builder, ConfigurationManager configuration)
{

    builder.Services.AddHttpContextAccessor();


    builder.Services.AddScoped<IMinioService, MinioService>();
    builder.Services.AddScoped<IMinioHelper, MinioHelper>();
    builder.Services.AddScoped<IKeycloakMinioUserService, KeycloakMinioUserService>();
    builder.Services.AddScoped<IKeyclockTokenAPIHelper, KeyclockTokenAPIHelper>();



}


/// <summary>
/// Add Services
/// </summary>
async void AddServices(WebApplicationBuilder builder)
{
    builder.Services.AddHttpClient();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSignalR();
    builder.Services.Configure<TREAPISettings>(configuration.GetSection("TREAPI"));
    builder.Services.AddHostedService<DAREBackgroundService>();

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

app.Run();

