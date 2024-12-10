


using BL.Models;
using BL.Models.Enums;
using BL.Services;
using Serilog;

namespace Data_Egress_API.Repositories.DbContexts
{
    public class DataInitaliser
    {
        private readonly ApplicationDbContext _dbContext;
        public IEncDecHelper _encDecHelper { get; set; }

        public DataInitaliser(ApplicationDbContext dbContext, IEncDecHelper encDec)
        {

            _dbContext = dbContext;
            _encDecHelper = encDec;


        }

        public void SeedAllInOneData(string password)
        {
            
            try
            {
                if (!_dbContext.KeycloakCredentials.Any(x => x.CredentialType == CredentialType.Tre))
                {


                    _dbContext.KeycloakCredentials.Add(new KeycloakCredentials()
                    {
                        UserName = "accessfromegresstotre",
                        CredentialType = CredentialType.Tre,
                        PasswordEnc = _encDecHelper.Encrypt(password)
                    });
                    _dbContext.SaveChanges();
                }

               


            }
            catch (Exception e)
            {
                Log.Error(e, "{Function} Error seeding data", "SeedAllInOneData");
                throw;
            }




        }

        public void SeedData()
        {
            return;
            try
            {
                if (!_dbContext.KeycloakCredentials.Any(x => x.CredentialType == CredentialType.Tre))
                {


                    _dbContext.KeycloakCredentials.Add(new KeycloakCredentials()
                    {
                        UserName = "sailegressapi",
                        CredentialType = CredentialType.Tre,
                        PasswordEnc = _encDecHelper.Encrypt("password123")
                    });
                    _dbContext.SaveChanges();
                }

                //if (!_dbContext.EgressSubmissions.Any())
                //{
                //    var egress = new EgressSubmission()
                //    {
                //        SubmissionId = 1.ToString(),
                //        OutputBucket = "asdasdadasdasdas",
                //        Status = EgressStatus.NotCompleted,
                //        Files = new List<EgressFile>()
                //        {
                //            new EgressFile()
                //            {
                //                Name = "file1.txt",
                //                Status = FileStatus.Undecided
                //            },
                //            new EgressFile()
                //            {
                //                Name = "file2.txt",
                //                Status = FileStatus.Undecided
                //            },
                //            new EgressFile()
                //            {
                //                Name = "file3.txt",
                //                Status = FileStatus.Undecided
                //            },

                //        }
                //    };
                //    _dbContext.EgressSubmissions.Add(egress);
                //    egress = new EgressSubmission()
                //    {
                //        SubmissionId = 2.ToString(),
                //        OutputBucket = "fgfasdasdadasdasdas",
                //        Status = EgressStatus.NotCompleted,
                //        Files = new List<EgressFile>()
                //        {
                //            new EgressFile()
                //            {
                //                Name = "file11.txt",
                //                Status = FileStatus.Undecided
                //            },
                //            new EgressFile()
                //            {
                //                Name = "file12.txt",
                //                Status = FileStatus.Undecided
                //            },
                //            new EgressFile()
                //            {
                //                Name = "file13.txt",
                //                Status = FileStatus.Undecided
                //            },

                //        }
                //    };
                //    _dbContext.EgressSubmissions.Add(egress);
                //    egress = new EgressSubmission()
                //    {
                //        SubmissionId = 3.ToString(),
                //        OutputBucket = "hhasdasdadasdasfgfdas",
                //        Status = EgressStatus.NotCompleted,
                //        Files = new List<EgressFile>()
                //        {
                //            new EgressFile()
                //            {
                //                Name = "file21.txt",
                //                Status = FileStatus.Undecided
                //            },
                //            new EgressFile()
                //            {
                //                Name = "file22.txt",
                //                Status = FileStatus.Undecided
                //            },
                //            new EgressFile()
                //            {
                //                Name = "file23.txt",
                //                Status = FileStatus.Undecided
                //            },

                //        }
                //    };
                //    _dbContext.EgressSubmissions.Add(egress);
                //    _dbContext.SaveChanges();
                //}

                
            }
            catch (Exception e)
            {
                Log.Error(e, "{Function} Error seeding data", "SeedData");
                throw;
            }




        }
    }
}
