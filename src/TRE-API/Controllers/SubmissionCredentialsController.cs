
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
    
    [ApiController]
    public class SubmissionCredentialsController : Controller
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly IEncDecHelper _encDecHelper;
        private readonly KeycloakTokenHelper _keycloakTokenHelper;

        public SubmissionCredentialsController(ApplicationDbContext applicationDbContext, IEncDecHelper encDec, SubmissionKeyCloakSettings keycloakSettings)
        {
            _encDecHelper = encDec;
            _DbContext = applicationDbContext;
            _keycloakTokenHelper = new KeycloakTokenHelper(keycloakSettings.BaseUrl, keycloakSettings.ClientId,
                keycloakSettings.ClientSecret, keycloakSettings.Proxy, keycloakSettings.ProxyAddresURL);
            
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet("CheckCredentialsAreValid")]
        public async Task<BoolReturn> CheckCredentialsAreValidAsync()
        {
            try { 
            var result = new BoolReturn(){Result = false};
            var creds = _DbContext.KeycloakCredentials.FirstOrDefault(x => x.CredentialType == CredentialType.Submission);
            if (creds != null)
            {
                var token = await _keycloakTokenHelper.GetTokenForUser(creds.UserName,
                    _encDecHelper.Decrypt(creds.PasswordEnc), "dare-tre-admin");
                result.Result = !string.IsNullOrWhiteSpace(token);
                    
            }

            return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "CheckCredentialsAreValid");
                throw;
            }
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpPost("UpdateCredentials")]
        public async Task<KeycloakCredentials> UpdateCredentials(KeycloakCredentials creds)
        {
            try
            {
                creds.Valid = true;
                var token = await _keycloakTokenHelper.GetTokenForUser(creds.UserName, creds.PasswordEnc, "dare-tre-admin");
                if (string.IsNullOrWhiteSpace(token))
                {
                    creds.Valid = false;
                    return creds;
                }
                else
                {

                    var dbcred =
                        _DbContext.KeycloakCredentials.FirstOrDefault(
                            x => x.CredentialType == CredentialType.Submission);
                    if (dbcred == null)
                        dbcred = new KeycloakCredentials();

                    dbcred.CredentialType = CredentialType.Submission;
                    dbcred.UserName = creds.UserName;
                    dbcred.PasswordEnc = _encDecHelper.Encrypt(creds.PasswordEnc);

                    if (dbcred.Id == 0)
                        _DbContext.KeycloakCredentials.Add(dbcred);
                    else
                        _DbContext.KeycloakCredentials.Update(dbcred);

                    await _DbContext.SaveChangesAsync();

                    Log.Information("{Function} Credentials Successfully update", "UpdateCredentials");
                    return creds;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "UpdateCredentials");
                throw;
            }
        }
    }
}
