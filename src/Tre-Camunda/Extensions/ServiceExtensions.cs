using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using Tre_Camunda.ProcessHandlers;
using Tre_Camunda.Services;
using Tre_Camunda.Settings;
using Zeebe.Client.Accelerator.Abstractions;
using IVaultCredentialsService = Tre_Camunda.Services.IVaultCredentialsService;
using VaultCredentialsService = Tre_Camunda.Services.VaultCredentialsService;
using IPostgreSQLUserManagementService = Tre_Camunda.Services.IPostgreSQLUserManagementService;
using PostgreSQLUserManagementService = Tre_Camunda.Services.PostgreSQLUserManagementService;
using Tre_Credentials.DbContexts;
using Tre_Credentials.Services;
using Microsoft.EntityFrameworkCore;
using Zeebe.Client.Accelerator.Extensions;



namespace Tre_Camunda.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddBusinessServices(this IServiceCollection services, IConfiguration configuration) // add services here
        {


            services.AddHttpClient();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.Configure<VaultSettings>(configuration.GetSection("VaultSettings"));
            services.Configure<MinioSettings>(configuration.GetSection("MinioSettings"));
            services.Configure<LdapSettings>(configuration.GetSection("LdapSettings"));

            services.AddHttpClient<IVaultCredentialsService, VaultCredentialsService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<VaultSettings>>().Value;

                client.BaseAddress = new Uri(settings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("X-Vault-Token", settings.Token);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });

            services.AddSingleton(sp =>
            {
                var minioSettings = sp.GetRequiredService<IOptions<MinioSettings>>().Value;
                return minioSettings;
            });

            services.AddDbContext<CredentialsDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("CredentialsConnection")));

        }

        public static void ConfigureCamunda(this IServiceCollection services, IConfiguration configuration)
        {
            var camundaSettings = new CamundaSettings();
            configuration.Bind(nameof(camundaSettings), camundaSettings);
            services.AddSingleton(camundaSettings);

            services.AddHostedService<BpmnProcessDeployService>();
            services.AddScoped<IProcessModelService, ProcessModelService>();

            services.AddScoped<Tre_Credentials.Services.IServicedZeebeClient, Tre_Credentials.Services.ServicedZeebeClient>();

            services.AddScoped<IPostgreSQLUserManagementService, PostgreSQLUserManagementService>();
            services.AddScoped<CreatePostgresUserHandler>();

            services.AddScoped<ILdapUserManagementService, LdapUserManagementService>();
            services.AddScoped<CreateTrinoUserHandler>();

            services.AddScoped<IMinioManagementService, MinioManagementService>();
            services.AddScoped<CreateMinioSecretHandler>();
            services.AddScoped<DeleteMinioSecretHandler>();

            services.AddScoped<IEphemeralCredentialsService, EphemeralCredentialsService>();

            services.AddScoped<CreateTreCredentialsHandler>();
            services.AddScoped<DeleteTreCredentialsHandler>();
        }
    }
}