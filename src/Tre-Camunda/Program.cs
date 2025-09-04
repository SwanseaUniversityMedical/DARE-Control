using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Tre_Camunda.Extensions;
using Tre_Camunda.Settings;
using Zeebe.Client;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Dashboard;
using Hangfire.Dashboard.BasicAuthorization;
using System.Reflection;
using Zeebe.Client.Accelerator.Extensions;
using Tre_Camunda.Services;
using Google.Protobuf.WellKnownTypes;

var builder = WebApplication.CreateBuilder(args);
var configuration = GetConfiguration();
string AppName = typeof(Program).Module.Name.Replace(".dll", "");

ConfigurationManager configurations = builder.Configuration;

Log.Logger = CreateSerilogLogger(configuration);
Log.Information("Camunda logging Start.");

Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var seqApiKey = configuration["Serilog:SeqApiKey"];

    if (seqServerUrl == null)
    {
        Log.Error("seqServerUrl is null");
        seqServerUrl = "seqServerUrl == null";
    }

    return new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .Enrich.WithProperty("ApplicationContext", AppName)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(seqServerUrl, apiKey: seqApiKey)
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

}

string hangfireConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHangfire(config => { config.UsePostgreSqlStorage(hangfireConnectionString); });

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 0; //Since no work is being done in this project, only storage
});

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.BootstrapZeebe(
          configuration.GetSection("ZeebeBootstrap"),
          Assembly.GetExecutingAssembly()
      );

        services.AddZeebeBuilders();
        services.BootstrapZeebe(configuration.GetSection("ZeebeConfiguration"), typeof(Program).Assembly);

        services.Configure<LdapSettings>(configuration.GetSection("LdapSettings"));
        services.Configure<VaultSettings>(configuration.GetSection("VaultSettings"));
        services.AddHttpClient();
        services.AddBusinessServices(configuration);
        services.ConfigureCamunda(configuration);

    })
    .Build()
    .RunAsync();

/// <summary>
/// GetConfiguration
/// </summary>
IConfiguration GetConfiguration()
{
    var a = Directory.GetCurrentDirectory();
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.development.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    return builder.Build();
}
