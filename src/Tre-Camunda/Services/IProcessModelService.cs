using Serilog;
using System.Reflection;
using System.Text.RegularExpressions;
using Zeebe.Client;

namespace Tre_Camunda.Services
{
    public interface IProcessModelService
    {
        Task DeployProcessDefinitionAndDecisionModels();
    }
   
}





