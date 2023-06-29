// See https://aka.ms/new-console-template for more information

using BL.Models.DTO;
using BL.Models.Services;
using BL.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TREAgent;

Console.WriteLine("Hello, World!");


var hostBuilder = new HostBuilder()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {




        var dareAPISettings = new DareAPISettings();
        hostContext.Configuration.Bind(nameof(dareAPISettings), dareAPISettings);
        services.AddSingleton(dareAPISettings);
        var treAPISettings = new TREAPISettings();
        hostContext.Configuration.Bind(nameof(treAPISettings), treAPISettings);
        services.AddSingleton(treAPISettings);
        services.AddHttpContextAccessor();
        services.AddHttpClient();


        services.AddScoped<IDoWork, DoWork>();
        services.AddScoped<IDareClientHelper, DareClientHelper>();
        services.AddScoped<ITREClientHelper, TREClientHelper>();

        // Configure Hangfire
        //services.AddHangfire(configuration => configuration
        //    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        //    .UseSimpleAssemblyNameTypeSerializer()
        //    .UseRecommendedSerializerSettings()
        //    .UsePostgreSqlStorage(hostContext.Configuration.GetConnectionString("DefaultConnection")));
        
        // Register Hangfire's JobActivator
        //services.AddSingleton<JobActivator>(new HangfireJobActivator(services.BuildServiceProvider()));
    }).ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
        webBuilder.UseUrls("http://localhost:5000"); // Specify the desired port here
    });
;
    

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
        services.AddHangfire(config =>
        {
            config.UsePostgreSqlStorage(hangfireConnectionString);
        });

        services.AddHangfireServer();

        // Register the Hangfire job
        //services.AddTransient<IHangfireJob, HangfireJob>();

        // Add other services or dependencies as needed
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Other middleware configurations
        
        // Enable Hangfire Dashboard

        app.UseHangfireDashboard();
        RecurringJob.AddOrUpdate<IDoWork>(a => a.Execute(), Cron.MinuteInterval(10));
        var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
        var port = serverAddressesFeature?.Addresses.FirstOrDefault()?.Split(':').Last();

        // Print the port number
        Console.WriteLine("Application is running on port: " + port);
        // Other app configurations
    }

    

    
}