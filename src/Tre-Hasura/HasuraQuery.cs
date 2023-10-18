using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json;
using Tre_Hasura.Models;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Mvc;
using GraphQL;
using GraphQL.Types;
using GraphQL.NewtonsoftJson;
using System.Reflection;
using Amazon.Runtime.Internal.Util;
using System.Text.RegularExpressions;

namespace Tre_Hasura
{
    public interface IHasuraQuery
    {
        Task<string> RunQuery(string token,  string Query);
        Task Run(string[] args);


    }
    public class HasuraQuery : IHasuraQuery
    {
        private readonly HasuraSettings _hasuraSettings;
        public HasuraQuery(HasuraSettings HasuraSettings)
        {
            _hasuraSettings = HasuraSettings;

        }

        public async Task Run(string[] args)
        {
            var Token = "";



            var Query = @"query MyQuery {
  Anewschema_two(order_by: {AAAAA: asc}) {
    id
    AAAAA
  }
}
";

//            var Query = @"{
//  ""query"" : ""query MyQuery { Anewschema_two { AAAAA id }}""
//}";

//  { "query": "Query MyQuery { Anewschema_two { AAAAA id}}" }

            foreach (var arg in args)
            {
                if (arg.StartsWith("--"))
                {
                    Token = arg.Replace("--", "");
                }

                if (arg.StartsWith("@"))
                {
                    Query = arg.Replace("@", "");
                }

            }

            Query = Regex.Replace(Query, @"\r\n?|\n", " ");

            Query = @"{ ""query"": """ + Query + @""" }";
            Console.WriteLine("Query > " + Query);
            Console.WriteLine("Token > " + Token);
            var data = await RunQuery(Token, Query);
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), $"data_{DateTime.UtcNow.Ticks}.json"), data);
        }


        public class ReturnData
        {
            public string result_type { get; set; }
            public List<List<string>> result { get; set; }
        }

        public async Task<string> RunQuery(string token, string Query)
        {
            //dont need to check token as Hasura does this 

            var paramlist = new Dictionary<string, string>
            {
               { "token", token},
               { "Query", Query}
            };

            // Set the endpoint URL

            //need to get the headers to send from the token userid
            string endpointUrl = _hasuraSettings.HasuraURL + "/v1/graphql";
            Console.WriteLine(endpointUrl);

            try
            {
                var Result = await HttpClient(endpointUrl, Query, false, token);

                Console.WriteLine(Result.StatusCode);

                var Content = await Result.Content.ReadAsStringAsync();
                Console.WriteLine(Content);
                var data = Content;

                return data;

            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message);
            }

      
			return null;

        }

        public async Task<HttpResponseMessage> HttpClient(string endpointUrl, string payload, bool doto = false, string token = "")
        {
            HttpResponseMessage response = null;
            // Create the HttpClient
            using (HttpClient client = new HttpClient())
            {
                // Set the request headers
                HttpRequestMessage re = new HttpRequestMessage(HttpMethod.Post, endpointUrl);


                re.Headers.Add("x-hasura-admin-secret", "ohCOOl");
                re.Headers.Add("token", token);
            
           
                //need to deseralise token not in db
                re.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                re.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.SendAsync(re);
            }
            return response;
        }

    }
}
