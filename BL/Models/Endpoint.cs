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

        public List<Projects> Projects { get; set; }

        public string Name { get; set; }
    }
}
