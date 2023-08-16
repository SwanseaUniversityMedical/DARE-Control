using BL.Models;
using BL.Models.Tes;
using EasyNetQ.Management.Client.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading;
using BL.Models.Enums;
using DARE_API.Services;
using Serilog;
using Endpoint = BL.Models.Endpoint;
using User = BL.Models.User;

namespace DARE_API.Repositories.DbContexts
{
    public class DataInitaliser
    {

        public static void SeedData(ApplicationDbContext context)
        {
            try
            {

            
            var head = CreateProject("Head", context);
            var shoulders = CreateProject("Shoulders", context);
            var knees = CreateProject("Knees", context);
            var toes = CreateProject("Toes", context);

            var jaybee = CreateUser("jaybee", "justin@chi.swan.ac.uk", context);
            var simon = CreateUser("simon", "simon@chi.swan.ac.uk", context);
            var luke = CreateUser("luke.young", "luke.young@chi.swan.ac.uk", context);
            var mikeb = CreateUser("michael", "michael@chi.swan.ac.uk", context);
            var mikew = CreateUser("mikew", "mikew@chi.swan.ac.uk", context);
            var gayathri = CreateUser("gayathri.menon", "gayathri.menon@chi.swan.ac.uk", context);
            var patricia = CreateUser("Patricia", "Patricia@chi.swan.ac.uk", context);
            var mahadi = CreateUser("mahadi", "mahadi@chi.swan.ac.uk", context);
            var hazel = CreateUser("hazel", "hazel@chi.swan.ac.uk", context);

            var sail = CreateEndpoint("SAIL", "sailtreapi", context);
            var dpuk = CreateEndpoint("DPUK", "dpuktreapi", context);
            var alspac = CreateEndpoint("ALSPAC", "alspactreapi", context);
            var msregister = CreateEndpoint("MSRegister", "msregistertreapi", context);

            AddMissingEndpoint(head, sail);
            AddMissingEndpoint(head, dpuk);
            AddMissingEndpoint(head, alspac);
            AddMissingUser(head, jaybee);
                AddMissingUser(head, mikeb);
                AddMissingUser(head, mikew);
                AddMissingUser(head, simon);
                AddMissingUser(head, luke);
                AddMissingUser(head, gayathri);
                AddMissingUser(head, patricia);
                AddMissingUser(head, mahadi);
                AddMissingUser(head, hazel);

                AddMissingEndpoint(shoulders, sail);
                AddMissingEndpoint(shoulders, msregister);
                AddMissingUser(shoulders, jaybee);
                AddMissingUser(shoulders, simon);
                AddMissingUser(shoulders, luke);

                AddMissingEndpoint(knees, dpuk);
                AddMissingUser(knees, jaybee);
                AddMissingUser(knees, simon);
                AddMissingUser(knees, luke);
                context.SaveChanges();
            AddSubmission("Sub1", "Head", "jaybee", "", context);
                AddSubmission("Sub2", "Head", "simon", "SAIL|DPUK", context);
                AddSubmission("Sub3", "Shoulders", "luke.young", "MSRegister", context);
                AddSubmission("Sub4", "Knees", "jaybee", "", context);

            }
            catch (Exception e)
            {
                Log.Error(e, "{Function} Error seeding data", "SeedData");
                throw;
            }




        }

        private static Project CreateProject(string name, ApplicationDbContext context)
        {
            var proj = context.Projects.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            if (proj == null)
            {
                proj = new Project()
                {
                    Name = name,
                    Display = name,
                    EndDate = DateTime.Now.ToUniversalTime(),
                    StartDate = DateTime.Now.ToUniversalTime(),
                    Endpoints = new List<Endpoint>(),
                    Users = new List<BL.Models.User>(),
                    Submissions = new List<Submission>()
                };
                proj.FormData = JsonConvert.SerializeObject(proj);
                context.Projects.Add(proj);
                
            }
            return proj;

        }

        private static User CreateUser(string name, string email, ApplicationDbContext context)
        {
            var user = context.Users.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            if (user == null)
            {
                user = new User()
                {
                    Name = name,
                    Email = email
                };
                user.FormData = JsonConvert.SerializeObject(user);
                context.Users.Add(user);
            }
            return user;

        }

        private static Endpoint CreateEndpoint(string name, string adminUser, ApplicationDbContext context)
        {
            var endpoint = context.Endpoints.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            if (endpoint == null)
            {
                endpoint = new Endpoint()
                {
                    Name = name,
                    AdminUsername = adminUser
                };
                endpoint.FormData = JsonConvert.SerializeObject(endpoint);
                context.Endpoints.Add(endpoint);
            }
            return endpoint;

        }

        private static void AddMissingEndpoint(Project project, Endpoint endpoint)
        {
            if (!project.Endpoints.Contains(endpoint))
            {
                project.Endpoints.Add(endpoint);
            }
        }

        private static void AddMissingUser(Project project, User user)
        {
            if (!project.Users.Contains(user))
            {
                project.Users.Add(user);
            }
        }

        private static void AddSubmission(string name, string project,string username, string endpointStr, ApplicationDbContext dbContext)
        {
            try
            {

           
            if (dbContext.Submissions.Any(x => x.TesName.ToLower() == name.ToLower()))
            {
                return;
            }
            string template = "{" +
                          "\"id\":null," +
                          "\"state\":0," +
                          "\"name\":\"{name}\"," +
                          "\"description\":null," +
                          "\"inputs\":null," +
                          "\"outputs\":null," +
                          "\"resources\":null," +
                          "\"executors\":[{" +
                          "\"image\":\"\\\\\\\\minio\\\\justin1.crate\"," +
                          "\"command\":null," +
                          "\"workdir\":null," +
                          "\"stdin\":null," +
                          "\"stdout\":null," +
                          "\"stderr\":null," +
                          "\"env\":null" +
                          "}]," +
                          "\"volumes\":null," +
                          "\"tags\":{\"project\":\"{project}\",\"endpoints\":\"{endpoints}\"}," +
                          "\"logs\":null," +
                          "\"creation_time\":null" +
                          "}";
           
            var tesString = template.Replace("{name}", name).Replace("{project}", project)
                .Replace("{endpoints}", endpointStr);
            var tesTask = JsonConvert.DeserializeObject<TesTask>(tesString);
            var dbProject = dbContext.Projects.First(x => x.Name.ToLower() == project.ToLower());
            var user = dbContext.Users.First(x => x.Name.ToLower() == username.ToLower());
            var sub = new Submission()
            {
                DockerInputLocation = tesTask.Executors.First().Image,
                Project = dbProject,
                Status = StatusType.WaitingForChildSubsToComplete,
                LastStatusUpdate = DateTime.Now.ToUniversalTime(),
                SubmittedBy = user,
                TesName = tesTask.Name,
                HistoricStatuses = new List<HistoricStatus>(),
                SourceCrate = tesTask.Executors.First().Image,
            };



            dbContext.Submissions.Add(sub);
            dbContext.SaveChanges();
            tesTask.Id = sub.Id.ToString();
            sub.TesId = tesTask.Id;
            var newTesString = JsonConvert.SerializeObject(tesTask);
            sub.TesJson = newTesString;
            dbContext.SaveChanges();

            
            List<string> endpoints = new List<string>();
            if (!string.IsNullOrWhiteSpace(endpointStr))
            {
                endpoints = endpointStr.Split('|').Select(x => x.ToLower()).ToList();
            }



            var dbEndpoints = new List<BL.Models.Endpoint>();

            if (endpoints.Count == 0)
            {
                dbEndpoints = dbProject.Endpoints;
            }
            else
            {
                foreach (var endpoint in endpoints)
                {
                    dbEndpoints.Add(dbProject.Endpoints.First(x => x.Name.ToLower() == endpoint.ToLower()));
                }
            }
            UpdateSubmissionStatus.UpdateStatus(sub, StatusType.WaitingForChildSubsToComplete, "");
            
            foreach (var endpoint in dbEndpoints)
            {
                dbContext.Add(new Submission()
                {
                    DockerInputLocation = tesTask.Executors.First().Image,
                    Project = dbProject,
                    Status = StatusType.WaitingForAgentToTransfer,
                    LastStatusUpdate = DateTime.Now.ToUniversalTime(),
                    SubmittedBy = sub.SubmittedBy,
                    Parent = sub,
                    TesId = tesTask.Id,
                    TesJson = sub.TesJson,
                    HistoricStatuses = new List<HistoricStatus>(),
                    EndPoint = endpoint,
                    TesName = tesTask.Name,
                    SourceCrate = tesTask.Executors.First().Image,
                });

            }

            dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        
    }
}
