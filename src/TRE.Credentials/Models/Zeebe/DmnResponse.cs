using System.Collections.Generic;

namespace Tre_Credentials.Models.Zeebe
{
    public class DmnResponse
    {
        public string DecisionId { get; set; }

        public Dictionary<string, object> Result { get; set; } = new();
    }
}
