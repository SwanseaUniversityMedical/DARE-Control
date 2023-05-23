using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
public class ProjectMembership
    {
        public int Id { get; set; }
        public virtual Projects Projects { get; set; }
        public virtual User Users { get; set; }


    }
}
