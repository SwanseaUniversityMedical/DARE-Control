using BL.Models.Settings;
using Data_Egress_API.Repositories.DbContexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net;
using Newtonsoft.Json;
using BL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using BL.Models.ViewModels;

using Microsoft.AspNetCore.Http.Connections;
using Data_Egress_API.Services;
using DARE_Egress.Services;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using BL.Models;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

Log.Logger = CreateSerilogLogger(configuration, environment);
Log.Information("Data_Egress API logging LastStatusUpdate.");
if (configuration["SuppressAntiforgery"] != null && configuration["SuppressAntiforgery"].ToLower() == "true")
{
    Log.Warning("{Function} Disabling Anti Forgery token. Only do if testing", "Main");
    builder.Services.AddAntiforgery(options => options.SuppressXFrameOptionsHeader = true);
}
// Add services to the container.

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

var dataEgressKeyCloakSettings = new DataEgressKeyCloakSettings();
configuration.Bind(nameof(dataEgressKeyCloakSettings), dataEgressKeyCloakSettings);
builder.Services.AddSingleton(dataEgressKeyCloakSettings);

var minioSettings = new MinioSettings();
configuration.Bind(nameof(MinioSettings), minioSettings);
builder.Services.AddSingleton(minioSettings);

var emailSettings = new EmailSettings();
configuration.Bind(nameof(emailSettings), emailSettings);
builder.Services.AddSingleton(emailSettings);

var treKeyCloakSettings = new TreKeyCloakSettings();
configuration.Bind(nameof(treKeyCloakSettings), treKeyCloakSettings);
builder.Services.AddSingleton(treKeyCloakSettings);
builder.Services.AddScoped<ITreClientWithoutTokenHelper, TreClientWithoutTokenHelper>();
builder.Services.AddScoped<IMinioHelper, MinioHelper>();

builder.Services.AddScoped<IDareEmailService, DareEmailService>();

builder.Services.AddScoped<IKeyCloakService, KeyCloakService>();


var encryptionSettings = new EncryptionSettings();
configuration.Bind(nameof(encryptionSettings), encryptionSettings);
builder.Services.AddSingleton(encryptionSettings);

builder.Services.AddScoped<IEncDecHelper, EncDecHelper>();

var TVP = new TokenValidationParameters
{
    ValidateAudience = true,
    ValidAudiences = dataEgressKeyCloakSettings.ValidAudiences.Trim().Split(',').ToList(),
    ValidIssuer = dataEgressKeyCloakSettings.Authority,
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

builder.Services.AddMailKit(optionBuilder =>
{
    optionBuilder.UseMailKit(new MailKitOptions
    {
        Server = emailSettings.Host,
        Port = emailSettings.Port,
        SenderName = emailSettings.FromDisplayName,
        SenderEmail = emailSettings.FromAddress,

        // can be optional with no authentication 
        //Account = Configuration["Account"],
        //Password = Configuration["Password"],
        // enable ssl or tls
        Security = emailSettings.EnableSSL
    });
});

builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformerBL>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        if (dataEgressKeyCloakSettings.Proxy)
        {
            options.BackchannelHttpHandler = new HttpClientHandler
            {
                UseProxy = true,
                UseDefaultCredentials = true,
                Proxy = new WebProxy()
                {
                    Address = new Uri(dataEgressKeyCloakSettings.ProxyAddresURL),
                    BypassList = new[] { dataEgressKeyCloakSettings.BypassProxy }
                }
            };
        }
        options.Authority = dataEgressKeyCloakSettings.Authority;
        options.Audience = dataEgressKeyCloakSettings.ClientId;



        options.MetadataAddress = dataEgressKeyCloakSettings.MetadataAddress;

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
                          policy.WithOrigins(configuration["DataEgressAPISettings:Address"])
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                      });

});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var encDec = scope.ServiceProvider.GetRequiredService<IEncDecHelper>();
    db.Database.Migrate();
    var initialiser = new DataInitaliser(db, encDec);
    initialiser.SeedData();
}

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
    c.OAuthClientId(dataEgressKeyCloakSettings.ClientId);
    c.OAuthClientSecret(dataEgressKeyCloakSettings.ClientSecret);
    c.OAuthAppName(dataEgressKeyCloakSettings.ClientId);
});
if (app.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();
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

app.Run();



