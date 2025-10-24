using Serilog;
using Tre_Camunda.Extensions;
using Tre_Camunda.Settings;
using Zeebe.Client;
using System.Reflection;
using Zeebe.Client.Accelerator.Extensions;
using Tre_Camunda.Services;

var builder = WebApplication.CreateBuilder(args);

string appName = typeof(Program).Module.Name.Replace(".dll", "");
var configurations = builder.Configuration;

Log.Logger = CreateSerilogLogger(configurations);
Log.Information("Camunda logging Start.");

Serilog.ILogger CreateSerilogLogger(IConfiguration cfg)
{
    var seqServerUrl = cfg["Serilog:SeqServerUrl"];
    var seqApiKey = cfg["Serilog:SeqApiKey"];

    return new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.WithProperty("ApplicationContext", appName)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(seqServerUrl ?? "seqServerUrl == null", apiKey: seqApiKey)
        .ReadFrom.Configuration(cfg)
        .CreateLogger();
}

// register services on the same builder
builder.Services.BootstrapZeebe(
    configurations.GetSection("ZeebeBootstrap"),
    Assembly.GetExecutingAssembly()
);
builder.Services.AddZeebeBuilders();
builder.Services.BootstrapZeebe(configurations.GetSection("ZeebeConfiguration"), typeof(Program).Assembly);
builder.Services.Configure<LdapSettings>(configurations.GetSection("LdapSettings"));
builder.Services.Configure<VaultSettings>(configurations.GetSection("VaultSettings"));
builder.Services.AddHttpClient();
builder.Services.AddBusinessServices(configurations);
builder.Services.ConfigureCamunda(configurations);

var app = builder.Build();
await app.RunAsync();