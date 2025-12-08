using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Data;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;

Console.WriteLine("TRE-ListFiles");

try
{
    string logDir = "/app/data";
    string logFile = Path.Combine(logDir, "file_log.txt");
    string startDirectory = Directory.GetCurrentDirectory();

    // Ensure the directory exists
    Directory.CreateDirectory(logDir);

    using (StreamWriter writer = new StreamWriter(logFile, false))
    {
        Console.WriteLine($"Scanning: {startDirectory}");
        Console.WriteLine($"Logging to: {logFile}");

        foreach (string file in Directory.EnumerateFiles(startDirectory, "*", SearchOption.AllDirectories))
        {
            writer.WriteLine(file);
            Console.WriteLine(file);
        }
    }

    logFile = Path.Combine(logDir, "file_logData.txt");
    startDirectory = "/data";
    using (StreamWriter writer = new StreamWriter(logFile, false))
    {
        Console.WriteLine($"Scanning: {startDirectory}");
        Console.WriteLine($"Logging to: {logFile}");

        foreach (string file in Directory.EnumerateFiles(startDirectory, "*", SearchOption.AllDirectories))
        {
            writer.WriteLine(file);
            Console.WriteLine(file);
        }
    }
    logFile = Path.Combine(logDir, "file_logdataset.txt");
    startDirectory = "/dataset";
    using (StreamWriter writer = new StreamWriter(logFile, false))
    {
        Console.WriteLine($"Scanning: {startDirectory}");
        Console.WriteLine($"Logging to: {logFile}");

        foreach (string file in Directory.EnumerateFiles(startDirectory, "*", SearchOption.AllDirectories))
        {
            if (file.EndsWith(".csv"))
            {
                var stey = string.Join(",", GetCsvHeaders(file));
                writer.WriteLine(stey);
                Console.WriteLine(stey);
            }

            writer.WriteLine(file);
            Console.WriteLine(file);
        }
    }

  
    Console.WriteLine(" Done! All file paths logged to /app/data/file_log.txt");
}
catch (Exception ex)
{
    Console.WriteLine($" Error: {ex.Message}");
}

 List<string> GetCsvHeaders(string filePath)
{
    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true, 
    };

    using (var reader = new StreamReader(filePath))
    using (var csv = new CsvReader(reader, config))
    {
        csv.Read();
        csv.ReadHeader();
        List<string> headers = csv.HeaderRecord?.ToList();

        return headers;
    }
}