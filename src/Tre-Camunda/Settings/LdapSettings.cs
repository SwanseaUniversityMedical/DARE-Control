using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tre_Camunda.Services
{
    public class LdapSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public string AdminDn { get; set; }

        public string AdminPassword { get; set; }

        public string BaseDn { get; set; }

        public string UserOu {  get; set; }

        public bool UseSSL { get; set; }
    
    }
}
