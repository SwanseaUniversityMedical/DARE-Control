using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tre_Hasura
{
    public interface IHasuraQuery
    {
        void RunQuery(string token, string role, string Query);
    }
    public class HasuraQuery : IHasuraQuery    
    {
        public void RunQuery(string token, string role, string Query)
        {

        }
    }
}
