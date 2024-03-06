using System;
using System.Data;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;


Console.WriteLine("TREFX SQL Runner Module : Trino");

// Tre-SQL-Trino.exe "--Query=select * from \"profileForm\"" --Output=data.csv

string trinoUrl = "http://trino-server:8080/v1/statement";

var Token = "";

var OutputFilename = "data.csv";

var Query = "SELECT * FROM \"profileForm\"";

foreach (var arg in args)
{
    Console.WriteLine("Arg = " + arg);

    if (arg.StartsWith("--Output"))
    {
        OutputFilename = arg.Replace("--Output=", "");
    }

    if (arg.StartsWith("--Token_"))
    {
        Token = arg.Replace("--Token_", "");
    }

    if (arg.StartsWith("--Query"))
    {
        Query = arg.Replace("--Query=", "");
    }

}

Console.WriteLine("Query > " + Query);
Console.WriteLine("Token > " + Token);
Console.WriteLine("Output > " + OutputFilename);

await ExportToCsv(trinoUrl, Query, OutputFilename);

Console.WriteLine("Completed");


static async Task ExportToCsv(string trinoUrl, string sqlQuery, string csvFilePath)
{
    using (var httpClient = new HttpClient())
    {
        var requestBody = new
        {
            query = sqlQuery
        };

        var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody));
        requestContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        using (var response = await httpClient.PostAsync(trinoUrl, requestContent))
        {
            response.EnsureSuccessStatusCode();

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            using (var streamReader = new StreamReader(responseStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var jsonSerializer = new JsonSerializer();

                using (var streamWriter = new StreamWriter(csvFilePath))
                using (var csvWriter = new CsvHelper.CsvWriter(streamWriter, System.Globalization.CultureInfo.InvariantCulture))
                {
                    while (jsonReader.Read())
                    {
                        if (jsonReader.TokenType == JsonToken.StartObject)
                        {
                            // Deserialize JSON response into a dynamic object
                            var jsonObject = jsonSerializer.Deserialize<JObject>(jsonReader);

                            // Write column headers
                            var columns = jsonObject["columns"];
                            foreach (var column in columns)
                            {
                                csvWriter.WriteField(column["name"].ToString());
                            }
                            csvWriter.NextRecord();

                            // Write data rows
                            var data = jsonObject["data"];
                            foreach (var rowData in data)
                            {
                                foreach (var value in rowData)
                                {
                                    csvWriter.WriteField(value.ToString());
                                }
                                csvWriter.NextRecord();
                            }
                        }
                    }
                }
            }
        }
    }
}

