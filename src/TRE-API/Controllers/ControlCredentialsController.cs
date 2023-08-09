
using BL.Models;
using BL.Models.APISimpleTypeReturns;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TRE_API.Models;
using TRE_API.Repositories.DbContexts;

namespace TRE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControlCredentialsController : ControllerBase
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly IEncDecHelper _encDecHelper;

        public ControlCredentialsController(ApplicationDbContext applicationDbContext, IEncDecHelper encDec)
        {
            _encDecHelper = encDec;
            _DbContext = applicationDbContext;
        }

        [AllowAnonymous]
        [HttpGet("AreCredentialsSet")]
        public BoolReturn AreCredentialsSet()
        {
            return new BoolReturn() { Result = _DbContext.ControlCredentials.Any() };
        }

        [HttpPost("UpdateCredentials")]
        public async Task<ControlCredentials> UpdateCredentials(ControlCredentials creds)
        {
            try
            {
                var add = false;
                var dbcred = _DbContext.ControlCredentials.FirstOrDefault();
                if (dbcred != null)
                {
                    creds.Id = dbcred.Id;
                    add = true;
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
