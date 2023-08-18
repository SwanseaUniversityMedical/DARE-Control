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
        private string baseURL = "";
        const string in_projectForm="/formio/project.json";
        const string in_userForm="/formio/user.json";
        //const string in_endpointForm="/formio/endpoint.json";
        const string in_endpointForm = "https://rikojtsvfwnqslz.form.io/darecontrolendpoint";




        public bool UseInternal { get => useInternal; set => useInternal = value; }
        public string EndpointForm
        {
            get => useInternal==true ? in_endpointForm :  endpointForm;
            set => endpointForm = value;
        }

        
        public string UserForm
        {
            get => useInternal==true ?  in_userForm : userForm;
            set => userForm = value;
        }
        public string ProjectForm
        {
            get => useInternal ?  in_projectForm : projectForm;
            set => projectForm = value;
        }
    }


}
