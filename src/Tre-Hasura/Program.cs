// See https://aka.ms/new-console-template for more information
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tre_Hasura;
using Microsoft.AspNetCore.Hosting;
using static System.Formats.Asn1.AsnWriter;
using BL.Services;

Console.WriteLine("Hello, World!");

using IHost host = CreateHostBuilder(args).Build();
using var scope = host.Services.CreateScope();

var services = scope.ServiceProvider;

try
{
    services.GetRequiredService<IHasuraQuery>().Run(args);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

IHostBuilder CreateHostBuilder(string[] strings)
{
    return Host.CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton<ITREClientHelper, TREClientHelper>();
            services.AddSingleton<IHasuraQuery, HasuraQuery>();        
         
            services.AddHttpClient();
            services.AddHttpContextAccessor();
        });
}