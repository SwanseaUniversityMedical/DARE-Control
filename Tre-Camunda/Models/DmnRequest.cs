namespace Tre_Camunda.Models
{
    public class DmnRequest
    {
        public string DecisionId { get; set; }

        public Dictionary<string, object> Variables { get; set; } = new();
    }
}
