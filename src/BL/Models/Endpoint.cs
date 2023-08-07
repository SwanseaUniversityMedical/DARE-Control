﻿using BL.Models.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
    public class Endpoint
    {
        public int Id { get; set; }        
        public virtual List<Project> Projects { get; set; }
        public string Name { get; set; }
        public string FormData { get; set; }
        public virtual List<Submission> Submissions { get; set; }

    }
}
