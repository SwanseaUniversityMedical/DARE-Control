using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using Microsoft.AspNetCore.Http;
using System.Web.Http;
using System;

using TRE_TESK.Models;
using Newtonsoft.Json.Linq;

namespace TRE_TESK.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class AuthenticationController : ApiController
    {
        private readonly AuthenticationSettings _authenticationSettings;
        private readonly ApplicationDbContext _applicationDbContext;
        

        public AuthenticationController(AuthenticationSettings AuthenticationSettings, ApplicationDbContext applicationDbContext)
        {
            _authenticationSettings = AuthenticationSettings;
            _applicationDbContext = applicationDbContext;
        }

        [HttpGet("GetNewToken/{role}")]
        public string GetNewToken(string role)
        {
            if (_applicationDbContext.GeneratedRole.Any(x => x.RoleName == role))
            {
                var Token = GenToken();

                _applicationDbContext.DataToRoles.Add(new RoleData()
                {
                    Token = Token,
                    Name = role,
                });
                _applicationDbContext.SaveChanges();
                return Token;
            }

            return "";
        }

        [Microsoft.AspNetCore.Mvc.HttpPost("ExpirerToken/{Token}")]
        public bool ExpirerToken(string Token)
        {
            var role = _applicationDbContext.DataToRoles.FirstOrDefault(x => x.Token == Token);
            if (role == null) return false;


            _applicationDbContext.DataToRoles.Remove(role);
            _applicationDbContext.SaveChanges();
            return true;

        }


        [HttpGet("")]
        public string Index([FromHeader] string MYCOOLToken)
        {
            if (string.IsNullOrEmpty(MYCOOLToken))
            {
                return null;
            }

            var role = _applicationDbContext.DataToRoles.FirstOrDefault(x => x.Token == MYCOOLToken);
            if (role == null) return null;


            if ((DateTime.UtcNow - role.DateTime).Days > _authenticationSettings.TokenExpireDays)
            {
                _applicationDbContext.DataToRoles.Remove(role);
                _applicationDbContext.SaveChanges();
                return null;
            }

            var hasuraVariables = new Dictionary<string, string> {
                        { "X-Hasura-Role", role.Name },
                        { "X-Hasura-User-Ide", role.Id.ToString() },
                };
            //cool.StatusCode = 200;

            var _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };

            var dta = System.Text.Json.JsonSerializer.Serialize(hasuraVariables, _jsonSerializerOptions);
            return dta;

        }

        private string GenToken()
        {
            string code = "";

            Random RNG = new Random();
            bool Duplicate = true;
            for (int i = 0; i < 3; i++)
            {
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

                code = new string(Enumerable.Repeat(chars, 128).Select(s => s[RNG.Next(s.Length)]).ToArray());

                if (_applicationDbContext.DataToRoles.Any(x => x.Token == code) == false)
                {
                    Duplicate = false;
                    break;
                }
            }

            if (Duplicate)
            {
                throw new Exception("Was unable to generate unique code");
            }
            return code;
        }
    }
}
