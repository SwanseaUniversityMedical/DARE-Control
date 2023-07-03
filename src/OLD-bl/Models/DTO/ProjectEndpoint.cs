﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BL.Models.DTO
{
    public class ProjectEndpoint
    {
        public int ProjectId { get; set; }
        public int EndpointId { get; set; }

        [JsonIgnore]
        public IEnumerable<SelectListItem>? ProjectItemList { get; set; }

        [JsonIgnore]
        public IEnumerable<SelectListItem>? EndpointItemList { get; set; }
    }
}
