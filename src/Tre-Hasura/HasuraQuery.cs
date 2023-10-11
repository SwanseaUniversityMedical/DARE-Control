using BL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BL.Models;
using BL.Models.ViewModels;

namespace Tre_Hasura
{
    public interface IHasuraQuery
    {
        void RunQuery(string token, string role, string Query);
    }
    public class HasuraQuery : IHasuraQuery    
    {
        private readonly ITREClientHelper _treclientHelper;
        public HasuraQuery(ITREClientHelper treClient)
        {

            _treclientHelper = treClient;
        }

        public void RunQuery(string token, string role, string Query)
        {
            //dont need to check token as Hasura does this 
            //make query
            var paramlist = new Dictionary<string, string>
            {
               { "token", token}
            };

            string result = _treclientHelper.CallAPIWithoutModel<string>("/api/Hasura/RunQuery/", paramlist).Result.ToString();

         

        }
    }
}
