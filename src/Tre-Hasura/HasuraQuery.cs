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
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Reflection.PortableExecutable;

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
            Console.WriteLine("AAAAA");

            var Token = "";
            string Query = "query {   testHasura_testing {     id     name   } }";

            if (args is null)
                foreach (var arg in args)
                {
                    if (arg.StartsWith("--")) {
                        Token = arg.Replace("--", "");
                    }

                    if (arg.StartsWith("@"))
                    {
                        Query = arg.Replace("@", "");
                    }

                }
        

           Token = "G1CrUPkW1Rz28vQdJeuIjS4Ypm5x0bIx2KxJzw4W37xwAIpRTMdZwjb9PNubTPaUOGrDVOqsUwgh4jiqg5svUesQgqnT5rEwANMRswla8U59n9R1Jh9SfBJe0BJbKFK8";

            Console.WriteLine("Query > " + Query);
            Console.WriteLine("Token > " + Token);
            RunQuery(Token, Query);
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

            if (result != null)
            {
                var jsonResult = JsonSerializer.Serialize(result.ToString());
                System.IO.File.WriteAllText(@"~/output.json" + DateTime.Now, jsonResult);
                //need to move to bucket
            }

        }
    }
}
