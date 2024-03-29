﻿// See https://aka.ms/new-console-template for more information

using BL.Models.Settings;
using BL.Rabbit;
using BL.Services;
using Castle.Core.Configuration;
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
using Minio;
using System;
using TREAgent;
using TREAgent.Repositories.DbContexts;
using TREAgent.Services;
using TREAgent.Models;


Console.WriteLine("loading ..");
Console.WriteLine("");


;


var hostBuilder = new HostBuilder()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        var HasuraSettings = new HasuraSettings();
        hostContext.Configuration.Bind(nameof(HasuraSettings), HasuraSettings);
        services.AddSingleton(HasuraSettings);

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
        services.AddScoped<IHasuraService, HasuraService>();
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


var host = hostBuilder.Build();

// Do database migration
var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await db.Database.MigrateAsync();


await host.RunAsync();

// Build and run the host
//await hostBuilder.RunConsoleAsync();



public class Startup
{
    public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; }

    public Startup(Microsoft.Extensions.Configuration.IConfiguration configuration)
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
        RecurringJob.RemoveIfExists("testings2");

        //RecurringJob.AddOrUpdate<IDoWork>("Scan Submissions",a => a.Execute(), Cron.MinuteInterval(10));
        //RecurringJob.AddOrUpdate<IDoWork>("task-999", a => a.CheckTESK("simon"), Cron.MinuteInterval(1));
        RecurringJob.AddOrUpdate<IDoWork>("testing2", a => a.testing(), Cron.MinuteInterval(1));

        var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
        var port = serverAddressesFeature?.Addresses.FirstOrDefault()?.Split(':').Last();

        // Print the port number
        Console.WriteLine("  _______ _____  ______                            _   ");
        Console.WriteLine(" |__   __|  __ \\|  ____|     /\\                   | |  ");
        Console.WriteLine("    | |  | |__) | |__       /  \\   __ _  ___ _ __ | |_ ");
        Console.WriteLine("    | |  |  _  /|  __|     / /\\ \\ / _` |/ _ \\ '_ \\| __|");
        Console.WriteLine("    | |  | | \\ \\| |____   / ____ \\ (_| |  __/ | | | |_ ");
        Console.WriteLine("    |_|  |_|  \\_\\______| /_/    \\_\\__, |\\___|_| |_|\\__|");
        Console.WriteLine("                                   __/ |            ");
        Console.WriteLine("                                  |___/            ");
        Console.WriteLine("");
        Console.WriteLine("Application is running on port: " + port);

    }




}