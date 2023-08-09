
using BL.Models;
using BL.Models.APISimpleTypeReturns;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TRE_API.Models;
using TRE_API.Repositories.DbContexts;
using TRE_API.Services;

namespace TRE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControlCredentialsController : ControllerBase
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly IEncDecHelper _encDecHelper;
        private readonly IKeycloakTokenHelper _keycloakTokenHelper;

        public ControlCredentialsController(ApplicationDbContext applicationDbContext, IEncDecHelper encDec, IKeycloakTokenHelper keycloakTokenHelper)
        {
            _encDecHelper = encDec;
            _DbContext = applicationDbContext;
            _keycloakTokenHelper = keycloakTokenHelper;
        }

        [AllowAnonymous]
        [HttpGet("CheckCredentialsAreValid")]
        public async Task<BoolReturn> CheckCredentialsAreValidAsync()
        {
            var result = new BoolReturn(){Result = false};
            var creds = _DbContext.ControlCredentials.FirstOrDefault();
            if (creds != null)
            {
                var token = await _keycloakTokenHelper.GetTokenForUser(creds.UserName,
                    _encDecHelper.Decrypt(creds.PasswordEnc));
                result.Result = !string.IsNullOrWhiteSpace(token);
                    
            }

            return result;
        }

        [HttpPost("UpdateCredentials")]
        public async Task<ControlCredentials> UpdateCredentials(ControlCredentials creds)
        {
            try
            {
                creds.Valid = true;
                var token = await _keycloakTokenHelper.GetTokenForUser(creds.UserName,
                    creds.PasswordEnc);
                if (string.IsNullOrWhiteSpace(token))
                {
                    creds.Valid = false;
                    return creds;
                }
                
                var add = true;
                var dbcred = _DbContext.ControlCredentials.FirstOrDefault();
                if (dbcred != null)
                {
                    creds.Id = dbcred.Id;
                    add = false;
                }

                creds.PasswordEnc = _encDecHelper.Encrypt(creds.PasswordEnc);
                if (add)
                {
                    _DbContext.ControlCredentials.Add(creds);
                    
                }
                else
                {
                    _DbContext.ControlCredentials.Update(creds);
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
