using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRE_API.Settings
{
    public class VaultSettings
    {
        public string BaseUrl { get; set; } 
        public string Token { get; set; } 
        public int TimeoutSeconds { get; set; } 
        public string SecretEngine { get; set; } 
        public bool EnableRetry { get; set; } 
        public int MaxRetryAttempts { get; set; } 
    }
}
