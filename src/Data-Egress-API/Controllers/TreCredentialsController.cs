
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
    
    
    [ApiController]
    public class TreCredentialsController : Controller
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly IEncDecHelper _encDecHelper;
        
        public KeycloakTokenHelper _keycloakTokenHelper { get; set; }


        public TreCredentialsController(ApplicationDbContext applicationDbContext, IEncDecHelper encDec, TreKeyCloakSettings keycloakSettings)
        {
            _encDecHelper = encDec;
            _DbContext = applicationDbContext;
            _keycloakTokenHelper = new KeycloakTokenHelper(keycloakSettings.BaseUrl, keycloakSettings.ClientId,
                keycloakSettings.ClientSecret, keycloakSettings.Proxy, keycloakSettings.ProxyAddresURL, keycloakSettings.IgnoreHttps);

        }


        [Authorize(Roles = "data-egress-admin")]
        [HttpGet("CheckCredentialsAreValid")]
        public async Task<BoolReturn> CheckCredentialsAreValidAsync()
        {
            try
            {
                var result = new BoolReturn() { Result = false };
                var creds = _DbContext.KeycloakCredentials.FirstOrDefault(x => x.CredentialType == CredentialType.Tre);
                if (creds != null)
                {
                    var token = await _keycloakTokenHelper.GetTokenForUser(creds.UserName,
                        _encDecHelper.Decrypt(creds.PasswordEnc), "data-egress-admin");
                    result.Result = !string.IsNullOrWhiteSpace(token);

                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "CheckCredentialsAreValid");
                throw;
            }
        }

        [Authorize(Roles = "data-egress-admin")]
        [HttpGet("EgressCheckCredentialsAreValid")]
        public async Task<BoolReturn> EgressCheckCredentialsAreValidAsync()
        {
            try { 
            var result = new BoolReturn() { Result = false };
            var creds = _DbContext.KeycloakCredentials.FirstOrDefault(x => x.CredentialType == CredentialType.Egress);
            if (creds != null)
            {
                var token = await _keycloakTokenHelper.GetTokenForUser(creds.UserName,
                    _encDecHelper.Decrypt(creds.PasswordEnc), "data-egress-admin");
                result.Result = !string.IsNullOrWhiteSpace(token);

            }

            return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "EgressCheckCredentialsAreValid");
                throw;
            }
        }

        [Authorize(Roles = "data-egress-admin")]
        [HttpPost("EgressUpdateCredentials")]
        public async Task<KeycloakCredentials> EgressUpdateCredentials(KeycloakCredentials creds)
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
                var dbcred = _DbContext.KeycloakCredentials.FirstOrDefault(x => x.CredentialType == CredentialType.Egress);
                if (dbcred != null)
                {
                    creds.Id = dbcred.Id;
                    creds.CredentialType = CredentialType.Egress;
                    add = false;
                }
                creds.CredentialType = CredentialType.Egress;
                creds.PasswordEnc = _encDecHelper.Encrypt(creds.PasswordEnc);
                if (add)
                {
                    _DbContext.KeycloakCredentials.Add(creds);

                }
                else
                {
                    _DbContext.KeycloakCredentials.Update(creds);
                }

                await _DbContext.SaveChangesAsync();

                Log.Information("{Function} Credentials Successfully update", "EgressUpdateCredentials");
                return creds;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "EgressUpdateCredentials");
                throw;
            }
        }

        [Authorize(Roles = "data-egress-admin")]
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
                var dbcred = _DbContext.KeycloakCredentials.FirstOrDefault(x => x.CredentialType == CredentialType.Tre);
                if (dbcred != null)
                {
                    creds.Id = dbcred.Id;
                    creds.CredentialType = CredentialType.Tre;
                    add = false;
                }
                creds.CredentialType = CredentialType.Tre;
                creds.PasswordEnc = _encDecHelper.Encrypt(creds.PasswordEnc);
                if (add)
                {
                    _DbContext.KeycloakCredentials.Add(creds);

                }
                else
                {
                    _DbContext.KeycloakCredentials.Update(creds);
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
