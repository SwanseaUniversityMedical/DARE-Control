namespace Tre_Camunda.Models
{
    public class DmnResponse
    {
        public string DecisionId { get; set; }

        public Dictionary<string, object> Result { get; set; } = new();
    }
}
