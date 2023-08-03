﻿using System;
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
        
        const string in_projectForm="/formio/project.json";
        const string in_userForm="/formio/user.json";
        const string in_endpointForm="/form/endpoint.json";

        public bool UseInternal { get; set; } = false;
        public string EndpointForm
        {
            get => UseInternal ? in_endpointForm :  endpointForm;
            set => endpointForm = value;
        }
        public string UserForm
        {
            get => UseInternal ? in_userForm : userForm;
            set => userForm = value;
        }
        public string ProjectForm
        {
            get => UseInternal ? in_projectForm : projectForm;
            set => projectForm = value;
        }
    }


}
