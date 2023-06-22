using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
    public class Endpoints
    {
        public int Id { get; set; }

        [JsonIgnore]
        public List<Projects> Projects { get; set; }
        public virtual List<ProjectEndpoints> ProjectEndpoints { get; set; }

        public string Name { get; set; }


    }
}
