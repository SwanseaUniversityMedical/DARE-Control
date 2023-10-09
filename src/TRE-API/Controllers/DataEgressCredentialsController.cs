
using BL.Models;
using BL.Models.APISimpleTypeReturns;
using BL.Models.Settings;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TRE_API.Repositories.DbContexts;


namespace TRE_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "dare-tre-admin")]
    [ApiController]
    public class DataEgressCredentialsController : Controller
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly IEncDecHelper _encDecHelper;
        public KeycloakTokenHelper _keycloakTokenHelper { get; set; }
        

        public DataEgressCredentialsController(ApplicationDbContext applicationDbContext, IEncDecHelper encDec, DataEgressKeyCloakSettings keycloakSettings)
        {
            _encDecHelper = encDec;
            _DbContext = applicationDbContext;
            _keycloakTokenHelper = new KeycloakTokenHelper(keycloakSettings.BaseUrl, keycloakSettings.ClientId,
                keycloakSettings.ClientSecret);
        }

        
        [HttpGet("CheckCredentialsAreValid")]
        public async Task<BoolReturn> CheckCredentialsAreValidAsync()
        {
            var result = new BoolReturn(){Result = false};
            var creds = _DbContext.DataEgressCredentials.FirstOrDefault();
            if (creds != null)
            {
                var token = await _keycloakTokenHelper.GetTokenForUser(creds.UserName,
                    _encDecHelper.Decrypt(creds.PasswordEnc), "dare-tre-admin");
                result.Result = !string.IsNullOrWhiteSpace(token);
                    
            }

            return result;
        }

        [HttpPost("UpdateCredentials")]
        public async Task<KeycloakCredentials> UpdateCredentials(KeycloakCredentials creds)
        {
            try
            {
                creds.Valid = true;
                var token = await _keycloakTokenHelper.GetTokenForUser(creds.UserName,
                    creds.PasswordEnc, "dare-tre-admin");
                if (string.IsNullOrWhiteSpace(token))
                {
                    creds.Valid = false;
                    return creds;
                }
                
                var add = true;
                var dbcred = _DbContext.DataEgressCredentials.FirstOrDefault();
                if (dbcred != null)
                {
                    creds.Id = dbcred.Id;
                    add = false;
                }

                creds.PasswordEnc = _encDecHelper.Encrypt(creds.PasswordEnc);
                if (add)
                {
                    _DbContext.DataEgressCredentials.Add(creds);
                    
                }
                else
                {
                    _DbContext.DataEgressCredentials.Update(creds);
                }
                
                await _DbContext.SaveChangesAsync();

                Log.Information("{Function} Credentials Successfully update", "UpdateCredentials");
                return creds;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "UpdateCredentials");
                throw;
            }
        }
    }
}
