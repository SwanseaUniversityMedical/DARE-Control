using Serilog;
using System.Reflection;
using System.Text.RegularExpressions;


namespace Tre_Camunda.Services
{

    public interface IBpmnService
    {
        Task DeployProcessDefinition();
    }
    public class BpmnService : IBpmnService
    {
        private IServicedZeebeClient _camunda;

        public BpmnService(IServicedZeebeClient IServicedZeebeClient)
        {

            _camunda = IServicedZeebeClient;
        }

        public async Task DeployProcessDefinition()
        {
            var bpmnModels = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(x => x.ToLower().Contains(".bpmnmodels."));

            foreach (var model in bpmnModels)
            {
                var name = Path.GetFileNameWithoutExtension(model.Split('.').Last());
                Log.Information("Deploying process definition with name: " + name);

                using var bpmnResourceStream = GetType().Assembly.GetManifestResourceStream(model);

                if (bpmnResourceStream == null)
                {
                    Log.Warning("Could not find resource stream for: " + model);
                    continue;
                }

                try
                {
                    await _camunda.DeployModel(bpmnResourceStream, name + ".bpmn");
                }
                catch (Exception e)
                {
                    Log.Error("Failed to deploy process definition with name: " + name + ", error: " + e);                   
                    throw new ApplicationException("Failed to deploy process definition", e);
                }
            }
        }
    }
}
