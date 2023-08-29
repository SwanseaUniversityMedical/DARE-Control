using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
    public class BaseModel
    {
        [NotMapped]
        public bool Error { get; set; }
        [NotMapped]

        public string? ErrorMessage { get; set; }
    }

    
}
