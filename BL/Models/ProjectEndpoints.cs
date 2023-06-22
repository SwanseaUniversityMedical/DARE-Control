using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
    public class ProjectEndpoints
    {
        public int Id { get; set; }
        public virtual Projects Projects { get; set; }
        public virtual Endpoints Endpoints { get; set; }


    }
}
