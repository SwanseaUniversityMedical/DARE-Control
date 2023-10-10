


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
                if (!_dbContext.SubmissionCredentials.Any())
                {


                    _dbContext.SubmissionCredentials.Add(new KeycloakCredentials()
                    {
                        UserName = "sailtreapi",
                        PasswordEnc = _encDecHelper.Encrypt("password123")
                    });
                    _dbContext.SaveChanges();
                }

                if (!_dbContext.DataEgressCredentials.Any())
                {


                    _dbContext.DataEgressCredentials.Add(new KeycloakCredentials()
                    {
                        UserName = "sailegressapi",
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
