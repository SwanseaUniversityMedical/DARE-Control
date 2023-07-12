using BL.Models.Settings;
using BL.Repositories.DbContexts;
using BL.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System;


var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

Log.Logger = CreateSerilogLogger(configuration, environment);
Log.Information("API logging Start.");

// Add services to the container.
builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
); ;
builder.Services.AddDbContext<ApplicationDbContext>(options => options
    .UseLazyLoadingProxies(true)
    .UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")
));
AddServices(builder);

//Add Dependancies
AddDependencies(builder, configuration);

var keyCloakSettings = new KeyCloakSettings();
configuration.Bind(nameof(keyCloakSettings), keyCloakSettings);
builder.Services.AddSingleton(keyCloakSettings);



var TVP = new TokenValidationParameters
{
    ValidateAudience = true,
    ValidAudiences = keyCloakSettings.ValidAudiences.Trim().Split(',').ToList(),
    ValidIssuer = keyCloakSettings.Authority,
    ValidateIssuerSigningKey = true,
    ValidateIssuer = true,
    ValidateLifetime = true
};

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.Authority = keyCloakSettings.Authority;
        options.Audience = keyCloakSettings.ClientId;

        // URL of the Keycloak server
        options.Authority = keyCloakSettings.Authority;
        //// Client configured in the Keycloak

        //// Client secret shared with Keycloak

        options.MetadataAddress = keyCloakSettings.MetadataAddress;

        options.RequireHttpsMetadata = false; // dev only
        options.IncludeErrorDetails = true;

        options.TokenValidationParameters = TVP;

        //var proxy = new WebProxy { Address = new Uri("http://192.168.10.15:8080") };

        //HttpClient.DefaultProxy = proxy;

        //options.BackchannelHttpHandler = new HttpClientHandler
        //{
        //    UseProxy = true,
        //    UseDefaultCredentials = true,
        //    Proxy = proxy
        //};

        //options.Events = new JwtBearerEvents
        //{

        //    OnAuthenticationFailed = f =>
        //    {
        //        f.NoResult();
        //        f.Response.StatusCode = 401;
        //        f.Response.ContentType = "text/plain";


        //        f.Response.Redirect($"https://localhost:5001/Account/LoginAfterTokenExpired", true);

        //        //return f.Response.WriteAsync(f.Exception.ToString());
        //        return f.Response.CompleteAsync();
        //    },
        //    OnChallenge = f => Task.CompletedTask
        //};
    });

// - authorize here
builder.Services.AddAuthorization(options =>
{

});

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins("https://localhost:7290")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();
// --- Session Token

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.EnableValidator(null);
        c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{environment.ApplicationName} v1");
        c.OAuthClientId(keyCloakSettings.ClientId);
        c.OAuthClientSecret(keyCloakSettings.ClientSecret);
        c.OAuthAppName(keyCloakSettings.ClientId);
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
    db.Database.Migrate();
   
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
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


    //builder.Services.AddScoped<IMinioService, MinioService>();
    builder.Services.AddMvc().AddControllersAsServices()
    //    .AddNewtonsoftJson(options =>
    //    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
    //); 
    ;

    //builder.Services.AddScoped<ICodeService, CodeService>();

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


