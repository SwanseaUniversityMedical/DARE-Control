using System;
using System.Data;
using System.IO;
using Npgsql;

Console.WriteLine("TREFX SQL Runner Module : Postgress");

// Tre-SQL-PG.exe "--Query=select * from \"profileForm\"" --Output=data.csv

var Token = "";
var OutputFilename = "data.csv";
var Query = "SELECT * FROM \"profileForm\"";

var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "32769";
var postgresUsername = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "postgres";
var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "password123";
var postgresDatabase = Environment.GetEnvironmentVariable("POSTGRES_DATABASE") ?? "postgres";

string connectionString = $"Host={postgresHost}:{postgresPort};Username={postgresUsername};Password={postgresPassword};Database={postgresDatabase}";

Console.WriteLine("=== Environment Variables ===");
Console.WriteLine($"POSTGRES_HOST: {postgresHost}");
Console.WriteLine($"POSTGRES_PORT: {postgresPort}");
Console.WriteLine($"POSTGRES_USERNAME: {postgresUsername}");
Console.WriteLine($"POSTGRES_PASSWORD: {(string.IsNullOrEmpty(postgresPassword) ? "NOT SET" : "***SET***")}");
Console.WriteLine($"POSTGRES_DATABASE: {postgresDatabase}");
Console.WriteLine($"Connection String: Host={postgresHost}:{postgresPort};Database={postgresDatabase}");
Console.WriteLine("============================\n");

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

    if (arg.StartsWith("--Connection"))
    {
        connectionString = arg.Replace("--Connection=", "");
    }

    if (arg.StartsWith("--Query"))
    {
        Query = arg.Replace("--Query=", "");
    }

}

Console.WriteLine("Query > " + Query);
Console.WriteLine("Token > " + Token);
Console.WriteLine("Output > " + OutputFilename);

ExportToCsv(connectionString, Query, OutputFilename);

Console.WriteLine("Completed");



static void ExportToCsv(string connectionString, string sqlQuery, string csvFilePath)
{
    using (var connection = new NpgsqlConnection(connectionString))
    {
        connection.Open();
        using (var command = new NpgsqlCommand(sqlQuery, connection))
        {
            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    using (var streamWriter = new StreamWriter(csvFilePath))
                    {
                        // Write column headers
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (i > 0)
                                streamWriter.Write(",");
                            streamWriter.Write(reader.GetName(i));
                        }
                        streamWriter.WriteLine();

                        // Write data rows
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                if (i > 0)
                                    streamWriter.Write(",");
                                streamWriter.Write(reader[i].ToString());
                            }
                            streamWriter.WriteLine();
                        }
                    }
                }
            }
        }
    }
}
