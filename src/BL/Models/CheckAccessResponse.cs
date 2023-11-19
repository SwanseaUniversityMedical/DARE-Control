using BL.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
    public class CheckAccessResponse
    {
        public CheckAccessResult? Result { get; set; }
    }
    public class CheckAccessResult {

        public bool Allow { get; set; }
    }

}
