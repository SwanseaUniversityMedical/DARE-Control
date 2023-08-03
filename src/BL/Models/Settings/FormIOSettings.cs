using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models.Settings
{
    public interface IFormIOSettings
    {
        string EndpointForm { get; set; }
        string ProjectForm { get; set; }
        bool UseInternal { get; set; }
        string UserForm { get; set; }
    }

    public class FormIOSettings : IFormIOSettings
    {
        public bool UseInternal { get; set; } = false;
        public string EndpointForm { get; set; }
        public string UserForm { get; set; }
        public string ProjectForm { get; set; }
    }


}
