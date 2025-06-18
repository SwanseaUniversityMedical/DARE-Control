namespace TRE_API.Models
{
    public class DmnResponse
    {

        public string DecisionId { get; set; }

        public Dictionary<string, object> Result { get; set; } = new();
    }
}
