using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tre_Credentials.Models
{
    public class EphemeralCredsReadyMessage
    {
        public int Id { get; set; }
        public Guid SubmissionId { get; set; } //Get from submission
        public long ProcessInstanceKey { get; set; } //Get from camunda flow

        public string VaultPath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsProcessed { get; set; } = false; //a flag to know if the creds are process in TRE-API or not
        public string? ErrorMessage { get; set; }
    }
}
