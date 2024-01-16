using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models
{
  
    public partial class UserExpiryInfo 
    {
        public string? name { get; set; }
        public DateTime? expiry { get; set; }


    }
}
