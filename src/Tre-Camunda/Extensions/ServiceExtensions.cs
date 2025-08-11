
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tre_Camunda.ProcessHandlers;
using Tre_Camunda.Services;
using Tre_Camunda.Settings;
using Zeebe.Client.Accelerator.Extensions;
using Zeebe.Client.Accelerator.Abstractions;
using BL.Services.Contract;
using BL.Services;


namespace Tre_Camunda.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddBusinessServices(this IServiceCollection services, IConfiguration configuration) // add services here
        {


            services.AddHttpClient();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        }

        public static void ConfigureCamunda(this IServiceCollection services, IConfiguration configuration)
        {
            var camundaSettings = new CamundaSettings();
            configuration.Bind(nameof(camundaSettings), camundaSettings);
            services.AddSingleton(camundaSettings);           

            services.AddHostedService<BpmnProcessDeployService>();
            services.AddScoped<IProcessModelService, ProcessModelService>();

            services.AddScoped<IServicedZeebeClient, ServicedZeebeClient>();

            services.AddScoped<IPostgreSQLUserManagementService, PostgreSQLUserManagementService>();

            services.AddScoped<ILdapUserManagementService, LdapUserManagementService>();


        }
    }
}