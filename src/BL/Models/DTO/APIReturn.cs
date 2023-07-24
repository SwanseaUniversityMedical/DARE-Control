using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models.DTO
{
    public class APIReturn
    {
        public bool BoolReturn { get; set; }
        public ReturnType ReturnType { get; set; }

    }


    public enum ReturnType
    {
        voidReturn,
        boolReturn
    }
}
