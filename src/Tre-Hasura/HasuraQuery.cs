using BL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BL.Models;
using BL.Models.ViewModels;
using Microsoft.Extensions.Hosting;

namespace Tre_Hasura
{
    public interface IHasuraQuery
    {
        void RunQuery(string token,  string Query);
    }
    public class HasuraQuery : IHasuraQuery
    {
        private readonly ITREClientHelper _treclientHelper;
        public HasuraQuery(ITREClientHelper treClient)
        {

            _treclientHelper = treClient;
        }

        public class Response
        {
            public string result { get; set; }
           
        }

        public void RunQuery(string token, string Query)
        {
            //dont need to check token as Hasura does this 
            //make query

            var Tokenparamlist = new Dictionary<string, string>
            {
               { "role", "select"}
            };

            var t = _treclientHelper.CallAPIWithoutModel<Response>("/api/HasuraAuthentication/GetNewToken/", Tokenparamlist).Result;
            token = t.result;


            var paramlist = new Dictionary<string, string>
            {
               { "token", token},
               { "Query", Query}
            };

            var result = _treclientHelper.CallAPIWithoutModel<Response>("/api/Hasura/RunQuery/", paramlist).Result;

            

        }
    }
}
