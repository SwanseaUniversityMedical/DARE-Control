using BL.Models.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
    public class Files
    {
        public int? Id { get; set; }

        public byte[]? File { get; set; } 

    }
}
