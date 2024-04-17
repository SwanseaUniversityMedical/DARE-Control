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
using System.Security.Policy;
using Build.Security.AspNetCore.Middleware.Dto;
using BL.Models.Settings;

namespace OPA.Controllers
{

    [Route("api/[controller]")]

    public class OPAController : Controller
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly IDareClientWithoutTokenHelper _dareHelper;
        private readonly OpaService _opaService;
        private readonly OPASettings _opaSettings;
        private readonly IKeyCloakService _IKeyCloakService;
        private readonly TreKeyCloakSettings _TreKeyCloakSettings;

        public OPAController(IDareClientWithoutTokenHelper helper, ApplicationDbContext applicationDbContext, OpaService opaservice, OPASettings opasettings, IKeyCloakService iKeyCloakService, TreKeyCloakSettings TreKeyCloakSettings)
        {
            _dareHelper = helper;
            _DbContext = applicationDbContext;
            _opaService = opaservice;
            _opaSettings = opasettings;
            _IKeyCloakService = iKeyCloakService;
            _TreKeyCloakSettings = TreKeyCloakSettings;
        }




        private void CreateBundle(string dataJsonContent, string rego, string outputDirectory, string Name)
        {
            if (Directory.Exists(Path.Combine(outputDirectory)))
            {
                Directory.Delete(Path.Combine(outputDirectory), true);
            }
            
            Directory.CreateDirectory(Path.Combine(outputDirectory));
            //2021-09-30 14:50:23
            var manifest = @"{
  ""revision"": """ +  string.Format("{0:yyyy-MM-dd HH:mm:ss}" , DateTime.UtcNow)  + @""",
  ""roots"": [""play"", ""data/play""]
}";

            var manifestPath  = Path.Combine(outputDirectory, ".manifest");
            System.IO.File.Delete(manifestPath);

            using (FileStream dataJsonStream = new FileStream(manifestPath, FileMode.Create))
            {
                using (StreamWriter jsonWriter = new StreamWriter(dataJsonStream))
                {
                    jsonWriter.Write(manifest);
                }
            }



            Directory.CreateDirectory(Path.Combine(outputDirectory,"data"));
            Directory.CreateDirectory(Path.Combine(outputDirectory,"data", Name));


            // Create a _ folder and add the _data.json file with the serialized data
            string dataJsonPath = Path.Combine(outputDirectory, "data", Name, "data.json");
            System.IO.File.Delete(dataJsonPath);
            using (FileStream dataJsonStream = new FileStream(dataJsonPath, FileMode.Create))
            {
                using (StreamWriter jsonWriter = new StreamWriter(dataJsonStream))
                {
                    jsonWriter.Write(dataJsonContent);
                }
            }

            Directory.CreateDirectory(Path.Combine(outputDirectory, "policies"));
            Directory.CreateDirectory(Path.Combine(outputDirectory, "policies", Name));

            string discoveryRegoPath = Path.Combine(outputDirectory, "policies", Name, $"allow.rego");
            System.IO.File.Delete(discoveryRegoPath);
            using (FileStream regoStream = new FileStream(discoveryRegoPath, FileMode.Create))
            {
                using (StreamWriter regoWriter = new StreamWriter(regoStream))
                {
                    regoWriter.Write(rego);
                }
            }

            string bundleTarPath = Path.Combine(outputDirectory, "..", "output", "bundle.tar");
            
            string bundleTarPathgz = Path.Combine(outputDirectory, "..", "output", "bundle.tar.gz");
            Directory.CreateDirectory(Path.Combine(outputDirectory, "..", "output"));
            if (System.IO.File.Exists(bundleTarPathgz))
            {
                System.IO.File.Delete(bundleTarPathgz);
            }
            
            Stream outStream = System.IO.File.Create(bundleTarPathgz);
            Stream gzoStream = new GZipOutputStream(outStream);
            TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);


            tarArchive.RootPath = outputDirectory.Replace('\\', '/');
            if (tarArchive.RootPath.EndsWith("/"))
                tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

            AddDirectoryFilesToTar(tarArchive, outputDirectory, true);

            tarArchive.Close();
        }


        private void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);

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


        [HttpPost("TokenPlease/{pass}/{name}")]

        public async Task<IActionResult> TokenPlease(string pass, string name)
        {
            return Ok(await _IKeyCloakService.GenAccessTokenSimple(name, pass, _TreKeyCloakSettings.TokenRefreshSeconds));

        }

        [HttpPost("UpdateOPA")]

        public async Task<IActionResult> UpdateOPA()
        {
            var httpClient = new HttpClient();

            var url = _opaSettings.OPAPolicyUploadURL;

            var apiClient = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, url);
            var requestContent = new StringContent(
                @"package play
import rego.v1
import input

default allow := false

allow if {
    input.action.operation in [""ExecuteQuery"",""AccessCatalog""]
}

allow if {
    input.action.operation in [""ExecuteQuery"",""AccessCatalog"", ""SelectFromColumns""]
    input.context.identity.user == input.action.resource.table.schemaName
}



", Encoding.UTF8, "application/json");

            var response = await apiClient.PutAsync(url, requestContent);

            if (response.IsSuccessStatusCode)
            {
                // Get the response content as a string
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response{ responseContent}");
            }
            else
            {

                Console.WriteLine($"Error { (int)response.StatusCode}- { response.ReasonPhrase}");
            }
            return Ok();
        }


        [HttpGet("BundleGet/{ServiceName}/{Resource}")]
        public async Task<IActionResult> BundleGet(string ServiceName, string Resource)
        {
            string path = Path.Combine( Assembly.GetEntryAssembly()?.Location.Replace("TRE-API.dll",""), "..", "gen" ).Replace('\\', '/');
            try
            {

                var data = @"package play
import rego.v1
import input

default allow := false

allow if {
    input.action.operation in [""ExecuteQuery"",""AccessCatalog""]
}

allow if {
    input.action.operation in [""ExecuteQuery"",""AccessCatalog"", ""SelectFromColumns""]
    input.context.identity.user == input.action.resource.table.schemaName
}



";
              
                CreateBundle(@"{""play"" : 2}", data, path, "play");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            var Outpath = Path.Combine(path.Replace("gen", "output"), "bundle.tar.gz").Replace('\\', '/');

            var value = System.IO.File.ReadAllBytes(Outpath);


            return File(value, "application/gzip");

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