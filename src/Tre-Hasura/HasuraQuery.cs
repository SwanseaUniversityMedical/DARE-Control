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
using System.Reflection;
using Amazon.Runtime.Internal.Util;
using System.Text.RegularExpressions;

namespace Tre_Hasura
{
    public interface IHasuraQuery
    {
        Task<string> RunQuery(string token,  string Query, string URL, string Proxy);
        Task Run(string[] args);


    }
    public class HasuraQuery : IHasuraQuery
    {
  

        public async Task Run(string[] args)
        {
            var Proxy = "";

            var Token = "";

            var URL = "http://localhost:8080";

            var Query = @"query MyQuery {
              Anewschema_two(order_by: {AAAAA: asc}) {
                id
                AAAAA
              }
            }
            ";

            foreach (var arg in args)
            {
                if (arg.StartsWith("--URL_"))
                {
                    URL = arg.Replace("--URL_", "");
                }

                if (arg.StartsWith("--Token_"))
                {
                    Token = arg.Replace("--Token_", "");
                }

                if (arg.StartsWith("--Query_"))
                {
                    Query = arg.Replace("--Query_", "");
                }

                if (arg.StartsWith("--Proxy_"))
                {
                    Proxy = arg.Replace("--Proxy_", "");
                }

            }

            Query = Regex.Replace(Query, @"\r\n?|\n", " "); //no new lines in json

            var Payload = new Payload()
            {
                query = Query
            };

            Query = JsonConvert.SerializeObject(Payload);
            Console.WriteLine("Query > " + Query);
            Console.WriteLine("Token > " + Token);
            Console.WriteLine("URL > " + URL);
            Console.WriteLine("Proxy > " + Proxy);
            
            var data = await RunQuery(Token, Query, URL, Proxy);

            DirectoryInfo directory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory()));
            var SubDirectory = directory.CreateSubdirectory("data");
            File.WriteAllText(Path.Combine(SubDirectory.ToString(), $"data_{DateTime.UtcNow.Ticks}.json"), data);
        
        
        }


        public class Payload
        {
            public string query { get; set; }

        }

        public class ReturnData
        {
            public string result_type { get; set; }
            public List<List<string>> result { get; set; }
        }

        public async Task<string> RunQuery(string token, string Query, string URL, string Proxy)
        {
            
            // Set the endpoint URL
            //need to get the headers to send from the token userid

            string endpointUrl = URL + "/v1/graphql";
            Console.WriteLine(endpointUrl);

            try
            {
                var Result = await HttpClient(endpointUrl, Query, token, Proxy);

                Console.WriteLine(Result.StatusCode);

                var Content = await Result.Content.ReadAsStringAsync();
                Console.WriteLine(Content);
                var data = Content;

                return data;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> HttpClient(string endpointUrl, string payload, string token, string proxy)
        {
            HttpClientHandler handler = new HttpClientHandler();

            if (string.IsNullOrEmpty(proxy) == false)
            {
                handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(proxy),
                    UseProxy = true,
                };
            }

            HttpResponseMessage response = null;
            // Create the HttpClient
            using (HttpClient client = new HttpClient(handler))
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

