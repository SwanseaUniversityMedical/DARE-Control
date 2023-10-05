using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TREAgent.Repositories
{
    public class TokenToExpire
    {
        public int Id { get; set; }

        public string Token { get; set; }

        public string TesId { get; set; }
    }
}
