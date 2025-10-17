using System;
using System.Data;
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

    Console.WriteLine(" Done! All file paths logged to /app/data/file_log.txt");
}
catch (Exception ex)
{
    Console.WriteLine($" Error: {ex.Message}");
}