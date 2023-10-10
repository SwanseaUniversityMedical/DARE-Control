
using BL.Models;
using BL.Models.APISimpleTypeReturns;
using BL.Models.Settings;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Data_Egress_API.Repositories.DbContexts;


namespace Data_Egress_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "data-egress-admin")]
    [ApiController]
    public class TreCredentialsController : Controller
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly IEncDecHelper _encDecHelper;
        private readonly ITREClientHelper _treClientHelper;
        public KeycloakTokenHelper _keycloakTokenHelper { get; set; }


        public TreCredentialsController(ApplicationDbContext applicationDbContext, IEncDecHelper encDec, TreKeyCloakSettings keycloakSettings)
        {
            _encDecHelper = encDec;
            _DbContext = applicationDbContext;
            _keycloakTokenHelper = new KeycloakTokenHelper(keycloakSettings.BaseUrl, keycloakSettings.ClientId,
                keycloakSettings.ClientSecret, keycloakSettings.Proxy, keycloakSettings.ProxyAddresURL);

        }


        [HttpGet("CheckCredentialsAreValid")]
        public async Task<BoolReturn> CheckCredentialsAreValidAsync()
        {
            var result = new BoolReturn() { Result = false };
            var creds = _DbContext.SubmissionCredentials.FirstOrDefault();
            if (creds != null)
            {
                var token = await _keycloakTokenHelper.GetTokenForUser(creds.UserName,
                    _encDecHelper.Decrypt(creds.PasswordEnc), "data-egress-admin");
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
                    creds.PasswordEnc, "data-egress-admin");
                if (string.IsNullOrWhiteSpace(token))
                {
                    creds.Valid = false;
                    return creds;
                }

                var add = true;
                var dbcred = _DbContext.SubmissionCredentials.FirstOrDefault();
                if (dbcred != null)
                {
                    creds.Id = dbcred.Id;
                    add = false;
                }

                creds.PasswordEnc = _encDecHelper.Encrypt(creds.PasswordEnc);
                if (add)
                {
                    _DbContext.SubmissionCredentials.Add(creds);

                }
                else
                {
                    _DbContext.SubmissionCredentials.Update(creds);
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
