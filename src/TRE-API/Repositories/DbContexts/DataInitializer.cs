﻿


using BL.Models;
using BL.Models.ViewModels;
using BL.Services;
using Serilog;

namespace TRE_API.Repositories.DbContexts
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

        public void SeedData()
        {

            try
            {
                if (!_dbContext.KeycloakCredentials.Any(x => x.CredentialType == CredentialType.Submission))
                {


                    _dbContext.KeycloakCredentials.Add(new KeycloakCredentials()
                    {
                        UserName = "sailtreapi",
                        CredentialType = CredentialType.Submission,
                        PasswordEnc = _encDecHelper.Encrypt("password123")
                    });
                    _dbContext.SaveChanges();
                }

                if (!_dbContext.KeycloakCredentials.Any(x => x.CredentialType == CredentialType.Egress))
                {


                    _dbContext.KeycloakCredentials.Add(new KeycloakCredentials()
                    {
                        UserName = "sailegressapi",
                        CredentialType = CredentialType.Egress,
                        PasswordEnc = _encDecHelper.Encrypt("password123")
                    });
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "{Function} Error seeding data", "SeedData");
                throw;
            }




        }
    }
}
