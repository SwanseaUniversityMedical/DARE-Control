using Serilog;
using System.Reflection;
using System.Text.RegularExpressions;


namespace Tre_Camunda.Services
{

    public interface IProcessModelService
    {
        Task DeployProcessDefinitionAndDecisionModels();
    }   
}
