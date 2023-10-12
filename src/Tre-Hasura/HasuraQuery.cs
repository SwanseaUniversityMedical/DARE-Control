using BL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BL.Models;
using BL.Models.ViewModels;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Tre_Hasura
{
    public interface IHasuraQuery
    {
        void RunQuery(string token,  string Query);
        void Run(string[] args);


    }
    public class HasuraQuery : IHasuraQuery
    {
        private readonly ITREClientHelper _treclientHelper;
        public HasuraQuery(ITREClientHelper treClient)
        {

            _treclientHelper = treClient;
        }
        public void Run(string[] args)
        {

            var Tokenparamlist = new Dictionary<string, string>
            {
               { "role", "select"}
            };

            var t = _treclientHelper.CallAPIWithoutModel<Response>("/api/HasuraAuthentication/GetNewToken/", Tokenparamlist).Result;          

            RunQuery(t.result, "");
        }

        public class Response
        {
            public string result { get; set; }

           
        }

        public void RunQuery(string token, string Query)
        {
            //dont need to check token as Hasura does this 
            //make query



            var paramlist = new Dictionary<string, string>
            {
               { "token", token},
               { "Query", Query}
            };

            var result = _treclientHelper.CallAPIWithoutModel<Response>("/api/Hasura/RunQuery/", paramlist).Result;

            

        }
    }
}
