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
        private string projectForm;
        private string userForm;
        private string endpointForm;
        private bool useInternal = false;

        public bool UseInternal { get => useInternal; set => useInternal = value; }
        public string EndpointForm
        {
            get => endpointForm;
            set => endpointForm = value;
        }
        public string UserForm
        {
            get => userForm;
            set => userForm = value;
        }
        public string ProjectForm
        {
            get => projectForm;
            set => projectForm = value;
        }
    }


}
