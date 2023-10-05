using Newtonsoft.Json.Linq;
using System.Text.Json;
using TRE_API.Models;
using TRE_API.Repositories.DbContexts;
using TRE_TESK.Models;

namespace TRE_API.Services
{
    public interface IHasuraAuthenticationService
    {
        public string GetNewToken(string role);
        public string CheckeTokenAndGetRoles(string Token);

        public bool ExpirerToken(string Token);
    }

    public class HasuraAuthenticationService : IHasuraAuthenticationService
    {
        private readonly AuthenticationSettings _authenticationSettings;
        private readonly ApplicationDbContext _applicationDbContext;


        public HasuraAuthenticationService(AuthenticationSettings AuthenticationSettings, ApplicationDbContext applicationDbContext)
        {
            _authenticationSettings = AuthenticationSettings;
            _applicationDbContext = applicationDbContext;
        }

        public string GetNewToken(string role)
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


        public string CheckeTokenAndGetRoles(string Token)
        {
            if (string.IsNullOrEmpty(Token))
            {
                return null;
            }

            var role = _applicationDbContext.DataToRoles.FirstOrDefault(x => x.Token == Token);
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

        public bool ExpirerToken(string Token)
        {
            var role = _applicationDbContext.DataToRoles.FirstOrDefault(x => x.Token == Token);
            if (role == null) return false;


            _applicationDbContext.DataToRoles.Remove(role);
            _applicationDbContext.SaveChanges();
            return true;

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
