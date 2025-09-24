using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tre_Credentials.Models
{
    [Table("EphemeralCredentials")]
    public class EphemeralCredential
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; } //Get from submission
        public long ProcessInstanceKey { get; set; } //Get from camunda flow

        public string VaultPath { get; set; } = string.Empty;

        public string CredentialType { get; set; } //Need to set it to identify which type : postgres/trino and minio
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsProcessed { get; set; } = false; //a flag to know if the creds are process in TRE-API or not
        public string? ErrorMessage { get; set; }

        public DateTime? ExpiredAt { get; set; }
    }
}
