// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tre_Hasura;
using Microsoft.AspNetCore.Hosting;
using static System.Formats.Asn1.AsnWriter;
using Tre_Hasura.Models;


Console.WriteLine("TREFX Hashura Runner");
var configuration = GetConfiguration();

using IHost host = Host.CreateDefaultBuilder(args).ConfigureServices
  (services =>
  {
      services.AddSingleton<IHasuraQuery, HasuraQuery>();
      var HasuraSettings = new HasuraSettings();
      configuration.Bind(nameof(HasuraSettings), HasuraSettings);
      services.AddSingleton(HasuraSettings);
      services.AddHttpClient();
  }).Build();

using var scope = host.Services.CreateScope();

var services = scope.ServiceProvider;


try
{
    await services.GetRequiredService<IHasuraQuery>().Run(args);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

IConfiguration GetConfiguration()
{

    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.development.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    return builder.Build();
}