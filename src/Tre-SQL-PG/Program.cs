using System;
using System.Data;
using System.IO;
using Npgsql;

Console.WriteLine("TREFX SQL Runner Module : Postgress");

// Tre-SQL-PG.exe "--Query=select * from \"profileForm\"" --Output=data.csv

var Token = "";
var OutputFilename = "data.csv";
var Query = "SELECT * FROM \"profileForm\"";
string connectionString = "Host=localhost:5432;Username=admin;Password=admin;Database=assets3";

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
