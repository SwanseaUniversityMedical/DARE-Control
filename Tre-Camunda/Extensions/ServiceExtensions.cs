using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tre_Camunda.Services;
using Tre_Camunda.Settings;



namespace Tre_Camunda.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddBusinessServices(this IServiceCollection services, IConfiguration configuration) 
        {


            var apiSettings = new ApiSettings();
            configuration.Bind(nameof(apiSettings), apiSettings);
            services.AddSingleton(apiSettings);

            services.AddHttpClient();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();            
        }
        public static void AddAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            var keyCloakSettings = new KeyCloakSettings();
            configuration.Bind(nameof(keyCloakSettings), keyCloakSettings);
            services.AddSingleton(keyCloakSettings);
        }

        public static void ConfigureCamunda(this IServiceCollection services, IConfiguration configuration)
        {
            var camundaSettings = new CamundaSettings();
            configuration.Bind(nameof(camundaSettings), camundaSettings);
            services.AddSingleton(camundaSettings);

            services.AddHostedService<ProcessDeployService>();
            services.AddScoped<IProcessModelService, ProcessModelService>();

            services.AddScoped<IServicedZeebeClient, ServicedZeebeClient>();
          
        }

    }
}
