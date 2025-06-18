
namespace TRE_API.Models
{
    public class DmnRequest
    {

        public string DecisionId {  get; set; }

        public Dictionary<string, object> Variables { get; set; } = new();
    }
}
