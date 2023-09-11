// See https://aka.ms/new-console-template for more information

using BL.Models.Settings;
using BL.Rabbit;
using BL.Services;
using EasyNetQ;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using TREAgent;
using TREAgent.Repositories.DbContexts;
using TREAgent.Services;

Console.WriteLine("Hello, World!");


var hostBuilder = new HostBuilder()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        var treKeyCloakSettings = new BaseKeyCloakSettings();
        hostContext.Configuration.Bind(nameof(treKeyCloakSettings), treKeyCloakSettings);
        services.AddSingleton(treKeyCloakSettings);


        var encryptionSettings = new EncryptionSettings();
        hostContext.Configuration.Bind(nameof(encryptionSettings), encryptionSettings);
        services.AddSingleton(encryptionSettings);

        var storedKeycloakLogin = new StoredKeycloakLogin();
        hostContext.Configuration.Bind(nameof(storedKeycloakLogin), storedKeycloakLogin);
        services.AddSingleton(storedKeycloakLogin);
        services.AddScoped<IEncDecHelper, EncDecHelper>();
        services.AddScoped<IKeycloakTokenHelper, KeycloakTokenHelper>();
        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddDbContext<ApplicationDbContext>(options => options
            .UseLazyLoadingProxies(true)
            .UseNpgsql(
                hostContext.Configuration.GetConnectionString("DefaultConnection")
            ));

        services.AddScoped<IDoWork, DoWork>();

        services.AddScoped<ITreClientWithoutTokenHelper, TreClientWithoutTokenHelper>();


    }).ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
        webBuilder.UseUrls("http://localhost:5000"); // Specify the desired port here
    });

    

// Build and run the host
await hostBuilder.RunConsoleAsync();



public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Configure Hangfire
        string hangfireConnectionString = Configuration.GetConnectionString("DefaultConnection");
        services.AddHangfire(config => { config.UsePostgreSqlStorage(hangfireConnectionString); });
        services.AddHangfireServer();

 

        services.Configure<RabbitMQSetting>(Configuration.GetSection("RabbitMQ"));
        services.AddTransient(cfg => cfg.GetService<IOptions<RabbitMQSetting>>().Value);
        var bus =
            services.AddSingleton(RabbitHutch.CreateBus(
                $"host={Configuration["RabbitMQ:HostAddress"]}:{int.Parse(Configuration["RabbitMQ:PortNumber"])};" +
                $"virtualHost={Configuration["RabbitMQ:VirtualHost"]};" +
                $"username={Configuration["RabbitMQ:Username"]};" +
                $"password={Configuration["RabbitMQ:Password"]}"));

        Task task = SetUpRabbitMQ.DoItAsync(Configuration["RabbitMQ:HostAddress"], 
            Configuration["RabbitMQ:PortNumber"],
            Configuration["RabbitMQ:VirtualHost"], 
            Configuration["RabbitMQ:Username"],
            Configuration["RabbitMQ:Password"]);

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

        app.UseHangfireDashboard();
        RecurringJob.RemoveIfExists("Scan Submissions");

        //RecurringJob.AddOrUpdate<IDoWork>("Scan Submissions",a => a.Execute(), Cron.MinuteInterval(10));
        //RecurringJob.AddOrUpdate<IDoWork>("task-999", a => a.CheckTESK("simon"), Cron.MinuteInterval(1));
        RecurringJob.AddOrUpdate<IDoWork>("testing", a => a.testing(), Cron.MinuteInterval(1));


        var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
        var port = serverAddressesFeature?.Addresses.FirstOrDefault()?.Split(':').Last();

        // Print the port number
        Console.WriteLine("*** TRE AGENT ***");
        Console.WriteLine("Application is running on port: " + port);

    }




}