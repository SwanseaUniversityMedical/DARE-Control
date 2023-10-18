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
using Newtonsoft.Json;
using Tre_Hasura.Models;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Mvc;


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
        

            Console.WriteLine("Query > " + Query);
            Console.WriteLine("Token > " + Token);
            var data = await RunQuery(Token, Query);
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), $"data_{DateTime.Now}.json"), data);
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
            string endpointUrl = _hasuraSettings.HasuraURL + "/v1/query";
            try
            {
                Query = System.Text.Json.JsonSerializer.Serialize(Query);
                var Result = await HttpClient(endpointUrl, Query, false, token);

                var Content = await Result.Content.ReadAsStringAsync();

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
