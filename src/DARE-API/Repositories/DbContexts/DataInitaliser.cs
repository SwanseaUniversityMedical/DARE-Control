using BL.Models;
using BL.Models.Tes;
using Newtonsoft.Json;
using BL.Models.Enums;
using DARE_API.Services;
using Serilog;
using User = BL.Models.User;
using DARE_API.Services.Contract;
using BL.Models.ViewModels;
using BL.Services;
using EasyNetQ.Management.Client.Model;
using Microsoft.EntityFrameworkCore;
using System;

namespace DARE_API.Repositories.DbContexts
{
    public class DataInitaliser
    {
        private readonly MinioSettings _minioSettings;
        private readonly IMinioHelper _minioHelper;
        private readonly ApplicationDbContext _dbContext;
        private readonly IKeyclockTokenAPIHelper _keyclockTokenAPIHelper;
        private readonly IKeycloakMinioUserService _userService;
        public DataInitaliser(MinioSettings minioSettings, IMinioHelper minioHelper, ApplicationDbContext dbContext, IKeyclockTokenAPIHelper keyclockTokenAPIHelper, IKeycloakMinioUserService userService)
        {
            _minioSettings = minioSettings;
            _minioHelper = minioHelper;
            _dbContext = dbContext;
            _keyclockTokenAPIHelper = keyclockTokenAPIHelper;
            _userService = userService;

        }
        public void SeedData()
        {
            var token = _keyclockTokenAPIHelper.GetTokenForUser("minioadmin", "password123", "").Result;
            try
            {


                var head = CreateProject("Head");
                var shoulders = CreateProject("Shoulders");
                var knees = CreateProject("Knees");
                var toes = CreateProject("Toes");

                //var jaybee = CreateUser("jaybee", "justin@chi.swan.ac.uk");
                //var simon = CreateUser("simon", "simon@chi.swan.ac.uk");
                //var luke = CreateUser("luke.young", "luke.young@chi.swan.ac.uk");
                //var mikeb = CreateUser("michael", "michael@chi.swan.ac.uk");
                //var mikew = CreateUser("mikew", "mikew@chi.swan.ac.uk");
                //var gayathri = CreateUser("gayathri.menon", "gayathri.menon@chi.swan.ac.uk");
                //var patricia = CreateUser("Patricia", "Patricia@chi.swan.ac.uk");
                //var mahadi = CreateUser("mahadi", "mahadi@chi.swan.ac.uk");
                //var hazel = CreateUser("hazel", "hazel@chi.swan.ac.uk");
                var testing = CreateUser("testing", "testing@chi.swan.ac.uk");
               

                var sail = CreateTre("SAIL", "sailtreapi");
                var dpuk = CreateTre("DPUK", "dpuktreapi");
                var alspac = CreateTre("ALSPAC", "alspactreapi");
                var msregister = CreateTre("MSRegister", "msregistertreapi");
                //noadtestinguser
                AddMissingTre(head, sail);
                AddMissingTre(head, dpuk);
                AddMissingTre(head, alspac);
                //AddMissingUser(head, mahadi);
                //AddMissingUser(head, jaybee);
                //AddMissingUser(head, mikeb);
                //AddMissingUser(head, mikew);
                //AddMissingUser(head, simon);
                //AddMissingUser(head, luke);
                //AddMissingUser(head, gayathri);
                //AddMissingUser(head, patricia);
                //AddMissingUser(head, hazel);

                
                AddMissingUser(head, testing);
                AddMissingUser(shoulders, testing);
                AddMissingUser(knees, testing);
                AddMissingUser(toes, testing);


                AddMissingTre(shoulders, sail);
                AddMissingTre(shoulders, msregister);
                //AddMissingUser(shoulders, jaybee);
                //AddMissingUser(shoulders, simon);
                //AddMissingUser(shoulders, luke);

                AddMissingTre(knees, dpuk);
                //AddMissingUser(knees, jaybee);
                //AddMissingUser(knees, simon);
                //AddMissingUser(knees, luke);
                _dbContext.SaveChanges();
                //AddSubmission("Sub1", "Head", "jaybee", "");
                //AddSubmission("Sub2", "Head", "simon", "SAIL|DPUK");
                //AddSubmission("Sub3", "Shoulders", "luke.young", "MSRegister");
                //AddSubmission("Sub4", "Knees", "jaybee", "");

                //CreateHistoricStatus1(2);
                //CreateHistoricStatus2(2);
                //CreateHistoricStatus3(2);
                //CreateHistoricStatus4(2);
                //CreateHistoricStatus5(2);
                //CreateHistoricStatus6(2);
                //CreateHistoricStatus7(2);
                //CreateHistoricStatus8(2);

                //CreateHistoricStatus5(3);
                //CreateHistoricStatus6(3);
                //CreateHistoricStatus7(3);
                //CreateHistoricStatus8(3);

                //CreateHistoricStatus1(4);
                //CreateHistoricStatus2(4);
                //CreateHistoricStatus4(4);
                //CreateHistoricStatus8(4);
                //CreateHistoricStatus7(4);
            }
            catch (Exception e)
            {
                Log.Error(e, "{Function} Error seeding data", "SeedData");
                throw;
            }




        }

        private Project CreateProject(string name)
        {
            var proj = _dbContext.Projects.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());

            if (proj == null)
            {
                var submission = GenerateRandomName(name.ToLower()) + "submission";
                var output = GenerateRandomName(name.ToLower()) + "output";
                var submissionBucket = _minioHelper.CreateBucket(submission.ToLower()).Result;
                var submistionBucketPolicy = _minioHelper.CreateBucketPolicy(submission.ToLower()).Result;
                var outputBucket = _minioHelper.CreateBucket(output.ToLower()).Result;
                var outputBucketPolicy = _minioHelper.CreateBucketPolicy(output.ToLower()).Result;

                proj = new Project()
                {
                    Name = name,
                    Display = name,
                    EndDate = DateTime.Now.ToUniversalTime(),
                    StartDate = DateTime.Now.ToUniversalTime(),
                    SubmissionBucket = submission,
                    OutputBucket = output,
                    Tres = new List<Tre>(),
                    Users = new List<BL.Models.User>(),
                    Submissions = new List<Submission>(),
                    ProjectDescription = ""
                };
                proj.FormData = JsonConvert.SerializeObject(proj);
                _dbContext.Projects.Add(proj);

            }
            return proj;

        }

        private User CreateUser(string name, string email)
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            if (user == null)
            {
                user = new User()
                {
                    Name = name,
                    Email = email
                };
                user.FormData = JsonConvert.SerializeObject(user);
                _dbContext.Users.Add(user);
            }
            return user;

        }

        private Tre CreateTre(string name, string adminUser)
        {
            var tre = _dbContext.Tres.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            if (tre == null)
            {
                tre = new Tre()
                {
                    Name = name,
                    AdminUsername = adminUser,
                    About = ""
                };
                tre.FormData = JsonConvert.SerializeObject(tre);
                _dbContext.Tres.Add(tre);
            }
            return tre;

        }

        private void AddMissingTre(Project project, Tre tre)
        {
            if (!project.Tres.Contains(tre))
            {
                project.Tres.Add(tre);
            }
        }

        private void AddMissingUser(Project project, User user)
        {
            if (!project.Users.Contains(user))
            {
                //Test User for Testing
                var accessToken = _keyclockTokenAPIHelper.GetTokenForUser("minioadmin", "password123", "").Result;
                var attributeName = _minioSettings.AttributeName;

                var submissionUserAttribute = _userService.SetMinioUserAttribute(accessToken, user.Name.ToString(), attributeName, project.SubmissionBucket.ToLower() + "_policy").Result;
                var outputUserAttribute = _userService.SetMinioUserAttribute(accessToken, user.Name.ToString(), attributeName, project.OutputBucket.ToLower() + "_policy").Result;
                project.Users.Add(user);
            }
        }

        private void AddSubmission(string name, string project, string username, string treStr)
        {
            try
            {


                if (_dbContext.Submissions.Any(x => x.TesName.ToLower() == name.ToLower()))
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
                              "\"tags\":{\"project\":\"{project}\",\"tres\":\"{tres}\"}," +
                              "\"logs\":null," +
                              "\"creation_time\":null" +
                              "}";

                var tesString = template.Replace("{name}", name).Replace("{project}", project)
                    .Replace("{tres}", treStr);
                var tesTask = JsonConvert.DeserializeObject<TesTask>(tesString);
                var dbProject = _dbContext.Projects.First(x => x.Name.ToLower() == project.ToLower());
                var user = _dbContext.Users.First(x => x.Name.ToLower() == username.ToLower());
                var sub = new Submission()
                {
                    DockerInputLocation = tesTask.Executors.First().Image,
                    Project = dbProject,
                    Status = StatusType.WaitingForChildSubsToComplete,
                    StartTime = DateTime.Now.ToUniversalTime(),
                    LastStatusUpdate = DateTime.Now.ToUniversalTime(),
                    SubmittedBy = user,
                    TesName = tesTask.Name,
                    HistoricStatuses = new List<HistoricStatus>(),
                    SourceCrate = tesTask.Executors.First().Image,
                };



                _dbContext.Submissions.Add(sub);
                _dbContext.SaveChanges();
                tesTask.Id = sub.Id.ToString();
                sub.TesId = tesTask.Id;
                var newTesString = JsonConvert.SerializeObject(tesTask);
                sub.TesJson = newTesString;
                _dbContext.SaveChanges();


                List<string> tres = new List<string>();
                if (!string.IsNullOrWhiteSpace(treStr))
                {
                    tres = treStr.Split('|').Select(x => x.ToLower()).ToList();
                }



                var dbTres = new List<BL.Models.Tre>();

                if (tres.Count == 0)
                {
                    dbTres = dbProject.Tres;
                }
                else
                {
                    foreach (var tre in tres)
                    {
                        dbTres.Add(dbProject.Tres.First(x => x.Name.ToLower() == tre.ToLower()));
                    }
                }
               // UpdateSubmissionStatus.UpdateStatus(sub, StatusType.WaitingForChildSubsToComplete, "");

                foreach (var tre in dbTres)
                {
                    _dbContext.Add(new Submission()
                    {

                        DockerInputLocation = tesTask.Executors.First().Image,
                        Project = dbProject,
                        Status = StatusType.WaitingForAgentToTransfer,
                        StartTime = DateTime.Now.ToUniversalTime(),
                        LastStatusUpdate = DateTime.Now.ToUniversalTime(),
                        SubmittedBy = sub.SubmittedBy,
                        Parent = sub,
                        TesId = tesTask.Id,
                        TesJson = sub.TesJson,
                        HistoricStatuses = new List<HistoricStatus>(),
                        Tre = tre,
                        TesName = tesTask.Name,
                        SourceCrate = tesTask.Executors.First().Image,

                    });
                }

                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        private bool CreateHistoricStatus1(int subID)
        {
            var subcheck = _dbContext.HistoricStatuses.Count();
            if (subcheck > 16)
            {
                return false;
            }
            else
            {
                var sub = _dbContext.Submissions.FirstOrDefault(x => x.Id == subID);
                var status = new HistoricStatus()
                {
                    Start = DateTime.Now.ToUniversalTime(),
                    End = DateTime.Now.ToUniversalTime(),
                    Status = StatusType.InvalidUser,
                    Submission = sub,
                    StatusDescription = ""
                };

                if (sub != null)
                {
                    _dbContext.HistoricStatuses.Add(status);
                    _dbContext.SaveChanges();
                }

                return true;
            }


        }
        private bool CreateHistoricStatus2(int subID)
        {
            var subcheck = _dbContext.HistoricStatuses.Count();
            if (subcheck > 16)
            {
                return false;
            }
            else
            {
                var sub = _dbContext.Submissions.FirstOrDefault(x => x.Id == subID);
                var status = new HistoricStatus()
                {
                    Start = DateTime.Now.ToUniversalTime(),
                    End = DateTime.Now.ToUniversalTime(),
                    Status = StatusType.UserNotOnProject,
                    Submission = sub,
                    StatusDescription = ""
                };

                if (sub != null)
                {
                    _dbContext.HistoricStatuses.Add(status);
                    _dbContext.SaveChanges();
                }

                return true;
            }
            

        }
        private bool CreateHistoricStatus3(int subID)
        {
            var subcheck = _dbContext.HistoricStatuses.Count();
            if (subcheck > 16)
            {
                return false;
            }
            else
            {
                var sub = _dbContext.Submissions.FirstOrDefault(x => x.Id == subID);
                var status = new HistoricStatus()
                {
                    Start = DateTime.Now.ToUniversalTime(),
                    End = DateTime.Now.ToUniversalTime(),
                    Status = StatusType.Completed,
                    Submission = sub,
                    StatusDescription = ""
                };

                if (sub != null)
                {
                    _dbContext.HistoricStatuses.Add(status);
                    _dbContext.SaveChanges();
                }

                return true;
            }


        }
        private bool CreateHistoricStatus4(int subID)
        {
            var subcheck = _dbContext.HistoricStatuses.Count();
            if (subcheck > 16)
            {
                return false;
            }
            else
            {
                var sub = _dbContext.Submissions.FirstOrDefault(x => x.Id == subID);
                var status = new HistoricStatus()
                {
                    Start = DateTime.Now.ToUniversalTime(),
                    End = DateTime.Now.ToUniversalTime(),
                    Status = StatusType.DataOutApproved,
                    Submission = sub,
                    StatusDescription = ""
                };

                if (sub != null)
                {
                    _dbContext.HistoricStatuses.Add(status);
                    _dbContext.SaveChanges();
                }

                return true;
            }


        }
        private bool CreateHistoricStatus5(int subID)
        {
            var subcheck = _dbContext.HistoricStatuses.Count();
            if (subcheck > 16)
            {
                return false;
            }
            else
            {
                var sub = _dbContext.Submissions.FirstOrDefault(x => x.Id == subID);
                var status = new HistoricStatus()
                {
                    Start = DateTime.Now.ToUniversalTime(),
                    End = DateTime.Now.ToUniversalTime(),
                    Status = StatusType.PodProcessingComplete,
                    Submission = sub,
                    StatusDescription = ""
                };

                if (sub != null)
                {
                    _dbContext.HistoricStatuses.Add(status);
                    _dbContext.SaveChanges();
                }

                return true;
            }


        }

        private bool CreateHistoricStatus6(int subID)
        {
            var subcheck = _dbContext.HistoricStatuses.Count();
            if (subcheck > 16)
            {
                return false;
            }
            else
            {
                var sub = _dbContext.Submissions.FirstOrDefault(x => x.Id == subID);
                var status = new HistoricStatus()
                {
                    Start = DateTime.Now.ToUniversalTime(),
                    End = DateTime.Now.ToUniversalTime(),
                    Status = StatusType.TransferredToPod,
                    Submission = sub,
                    StatusDescription = ""
                };

                if (sub != null)
                {
                    _dbContext.HistoricStatuses.Add(status);
                    _dbContext.SaveChanges();
                }

                return true;
            }


        }

        private bool CreateHistoricStatus7(int subID)
        {
            var subcheck = _dbContext.HistoricStatuses.Count();
            if (subcheck > 16)
            {
                return false;
            }
            else
            {
                var sub = _dbContext.Submissions.FirstOrDefault(x => x.Id == subID);
                var status = new HistoricStatus()
                {
                    Start = DateTime.Now.ToUniversalTime(),
                    End = DateTime.Now.ToUniversalTime(),
                    Status = StatusType.PodProcessing,
                    Submission = sub,
                    StatusDescription = ""
                };

                if (sub != null)
                {
                    _dbContext.HistoricStatuses.Add(status);
                    _dbContext.SaveChanges();
                }

                return true;
            }


        }

        private bool CreateHistoricStatus8(int subID)
        {
            var subcheck = _dbContext.HistoricStatuses.Count();
            if (subcheck > 16)
            {
                return false;
            }
            else
            {
                var sub = _dbContext.Submissions.FirstOrDefault(x => x.Id == subID);
                var status = new HistoricStatus()
                {
                    Start = DateTime.Now.ToUniversalTime(),
                    End = DateTime.Now.ToUniversalTime(),
                    Status = StatusType.CancellingChildren,
                    Submission = sub,
                    StatusDescription = ""
                };

                if (sub != null)
                {
                    _dbContext.HistoricStatuses.Add(status);
                    _dbContext.SaveChanges();
                }

                return true;
            }


        }
        private string GenerateRandomName(string prefix)
        {
            Random random = new Random();
            string randomName = prefix + random.Next(1000, 9999);
            return randomName;
        }


    }
}
