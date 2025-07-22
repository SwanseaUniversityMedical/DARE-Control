
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeRP_Forms_camunda.Settings;
using Tre_Camunda.Services;
using Tre_Camunda.Settings;


namespace Tre_Camunda.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddBusinessServices(this IServiceCollection services, IConfiguration configuration) // add services here
        {

            //var apiSettings = new ApiSettings();
            //configuration.Bind(nameof(apiSettings), apiSettings);
            //services.AddSingleton(apiSettings);

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

            services.AddHostedService<BpmnProcessDeployService>();
            services.AddScoped<IProcessModelService, ProcessModelService>();

            services.AddScoped<IServicedZeebeClient, ServicedZeebeClient>();

            /*/
            services.AddCamundaWorker("Worker")
                .AddHandler<StartTaskForUsers>()
                .AddHandler<BlankTask>()
                .AddHandler<RegisterUserTask>()
                .AddHandler<CompleteUserTask>()
                .AddHandler<RegisterNewTracking>()
                .AddHandler<RegisterFinishTracking>()
                .AddHandler<GetViewForFlow>()
                .AddHandler<SaveFormData>()
                .AddHandler<PullOutUserVariable>()
                .AddHandler<EnterCode>()
                .AddHandler<BlockRandomisation>()
                .AddHandler<SendAbuseEmail>()
                .AddHandler<DeleteAllData>()
                .AddHandler<RenameTracking>()
                .AddHandler<SignalRHandler>()
                .AddHandler<SetUserVariable>()
                .AddHandler<CalculatePregWeeks>()
                .AddHandler<GenericEmail>()
                .AddHandler<GenerateAnonymousID>()
                .AddHandler<TestRegistration>()
                .ConfigurePipeline(pipeline =>
              {
                  pipeline.Use(next => async context =>
                  {
                      Log.Information("Started processing of task {Id}", context.Task.Id);
                      await next(context);
                      Log.Information("Finished processing of task {Id}", context.Task.Id);
                  });
              });
            /*/
        }

    }
}
