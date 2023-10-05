// See https://aka.ms/new-console-template for more information

using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");
var configuration = GetConfiguration();
string AppName = typeof(Program).Module.Name.Replace(".dll", "");

Log.Logger = CreateSerilogLogger(configuration);
Log.Information("Camunda logging Start.");

/// <summary>
/// CreateSerilogLogger
/// </summary>
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

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {

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

