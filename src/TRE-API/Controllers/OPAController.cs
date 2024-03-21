using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BL.Models;
using BL.Models.APISimpleTypeReturns;
using BL.Models.Enums;
using BL.Models.ViewModels;
using Serilog;
using Microsoft.Extensions.Options;
using BL.Services;
using TRE_API.Repositories.DbContexts;
using TRE_API.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using BL.Models.Tes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO.Compression;
using System.Text;
using CommunityToolkit.HighPerformance;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Text.RegularExpressions;
using System.Reflection;

namespace OPA.Controllers
{

    [Route("api/[controller]")]

    public class OPAController : Controller
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly IDareClientWithoutTokenHelper _dareHelper;
        private readonly OpaService _opaService;
        private readonly OPASettings _opaSettings;
        public OPAController(IDareClientWithoutTokenHelper helper, ApplicationDbContext applicationDbContext, OpaService opaservice, OPASettings opasettings)
        {
            _dareHelper = helper;
            _DbContext = applicationDbContext;
            _opaService = opaservice;
            _opaSettings = opasettings;
        }




        public void CreateBundle(string dataJsonContent, string rego, string outputDirectory)
        {
            // Create a _ folder and add the _data.json file with the serialized data
            string dataJsonPath = Path.Combine(outputDirectory, "data.json");
            Directory.CreateDirectory(Path.Combine(outputDirectory));

            using (FileStream dataJsonStream = new FileStream(dataJsonPath, FileMode.Create))
            {
                using (StreamWriter jsonWriter = new StreamWriter(dataJsonStream))
                {
                    jsonWriter.Write(dataJsonContent);
                }
            }

            Directory.CreateDirectory(Path.Combine(outputDirectory, "stuff"));



            // Create a discovery.rego file in the _ folder
            string discoveryRegoPath = Path.Combine(outputDirectory, "stuff", "discovery.rego");
            using (FileStream regoStream = new FileStream(discoveryRegoPath, FileMode.Create))
            {
                using (StreamWriter regoWriter = new StreamWriter(regoStream))
                {
                    regoWriter.Write(rego);
                }
            }

            string bundleTarPath = Path.Combine(outputDirectory, "..", "tarme", "bundle.tar");
           // ZipFile.CreateFromDirectory(outputDirectory, bundleTarPath);

            string bundleTarPathgz = Path.Combine(outputDirectory, "..", "tarme", "bundle.tar.gz");
            //ZipFile.CreateFromDirectory(Path.Combine(outputDirectory, "..", "tarme"), bundleTarPathgz);

            


            Stream outStream = System.IO.File.Create(bundleTarPathgz);
            Stream gzoStream = new GZipOutputStream(outStream);
            TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

            // Note that the RootPath is currently case sensitive and must be forward slashes e.g. "c:/temp"
            // and must not end with a slash, otherwise cuts off first char of filename
            // This is scheduled for fix in next release
            tarArchive.RootPath = outputDirectory.Replace('\\', '/');
            if (tarArchive.RootPath.EndsWith("/"))
                tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

            AddDirectoryFilesToTar(tarArchive, outputDirectory, true);

            tarArchive.Close();
        }

        private void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
            // Optionally, write an entry for the directory itself.
            // Specify false for recursion here if we will add the directory's files individually.
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);

            // Write each file to the tar.
            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames)
            {
                tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {
                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                    AddDirectoryFilesToTar(tarArchive, directory, recurse);
            }
        }


        [HttpGet("BundleGet/{ServiceName}/{Resource}")]
        public async Task<IActionResult> BundleGet(string ServiceName, string Resource)
        {


            var  data = @"
package play
import rego.v1
import input
default allow = false

allow if {
   print(""input"", input)
   input.identity.user == input.resource.table.schemaName
}
";
           // CreateBundle(@"{""cool"" : 2}", data, @"C:/Users/john.vaughan/Desktop/stuff/llm");

          //  var value = System.IO.File.ReadAllBytes("C:/Users/john.vaughan/Desktop/bundle.tar.gz");


          //  var bytedata = CompressString(data);

           // var putput = DecompressToString(bytedata);

            var tar = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(x => x.ToLower().Contains($".tar.gz".ToLower()));

            var tarStream = GetType()
                     .Assembly
                     .GetManifestResourceStream(tar.First());


            return File(tarStream, "application/gzip");

        }


        private static byte[] CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();

            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            return compressedData;
        }

        private static string DecompressToString(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var decompressedStream = new MemoryStream())
            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                gzipStream.CopyTo(decompressedStream);

                return Encoding.UTF8.GetString(decompressedStream.ToArray());
            }
        }


        [HttpGet("CheckUserAccess")]
        public async Task<bool> CheckUserAccess()
        {
            try
            {
                //var userName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
                var userName = "test";
                var treData = _DbContext.Projects.Where(x => x.Decision == Decision.Undecided).ToList();
              
                //update ProjectExpiryDate if greater than today

                DateTime today = DateTime.Today;
                var resultList = new List<TreProject>();
                foreach (var project in treData)
                {
                    var dbproject = _DbContext.Projects.FirstOrDefault(x => x.Id == project.Id);

                    if (project.ProjectExpiryDate > today)
                    {
                        dbproject.ProjectExpiryDate = DateTime.Now.AddMinutes(_opaSettings.ExpiryDelayMinutes);
                        resultList.Add(dbproject);
                    }
                   
                    await _DbContext.SaveChangesAsync();
                }                          
                bool hasAccess = await _opaService.CheckAccess(userName, today, treData);
                if (hasAccess)
                {
                    Log.Information("{Function} User Access Allowed", "CheckUserAccess");
                    return true;
                }
                else
                {
                    Log.Information("{Function} User Access Denied", "CheckUserAccess");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "CheckUserAccess");
                return false;
                throw;
            }

        }


    }

}