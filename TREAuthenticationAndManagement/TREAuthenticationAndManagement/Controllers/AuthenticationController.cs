using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using Microsoft.AspNetCore.Http;
using System.Web.Http;

namespace TRE_TESK.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class AuthenticationController : ApiController
    {

        public class RoleData { 
            public string Name { get; set; }
            public int ID { get; set; }

        }


        //TODO save Tokens  To disk?
        //TODO Easy way to grab all the generated roles , even if they had been generated already 

        public static Dictionary<string, RoleData> TokenToRole = new Dictionary<string, RoleData>();

        public static int freeRoleID = 0;

        public static List<string> GenRoles = new List<string>()
        {
            "COOLSchemas2",
            "COOLSchemas1"
        };


        public HashSet<string> CoolCodes = new HashSet<string>()
        {
            "COOL", "COOL2"
        };

        [HttpGet("GetNewToken/{role}")]
        public string GetNewToken(string role)
        {
            if (GenRoles.Contains(role))
            {
                var Token = GenToken();

                TokenToRole[Token] = new RoleData()
                {
                    Name = role,
                    ID = freeRoleID,
                };
                freeRoleID++;
                return Token;
            }

            return "";
        }




        [HttpGet("")]
        public string Index([FromHeader] string MYCOOLToken)
        {
            if (string.IsNullOrEmpty(MYCOOLToken))
            {
                return null;
            }


            if (TokenToRole.ContainsKey(MYCOOLToken))
            {
                var hasuraVariables = new Dictionary<string, string> {
                        { "X-Hasura-Role", TokenToRole[MYCOOLToken].Name },
                        { "X-Hasura-User-Ide", TokenToRole[MYCOOLToken].ID.ToString() },
                };
                //cool.StatusCode = 200;

                var _jsonSerializerOptions = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                };

                var dta = System.Text.Json.JsonSerializer.Serialize(hasuraVariables, _jsonSerializerOptions);
                return dta;
            }
            else
            {
                return null;
            }
        }

        private string GenToken()
        {
            string code = "";

            Random RNG = new Random();
            bool Duplicate = true;
            for (int i = 0; i < 100; i++)
            {
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

                code = new string(Enumerable.Repeat(chars, 128).Select(s => s[RNG.Next(s.Length)]).ToArray());

                if (TokenToRole.ContainsKey(code) == false)
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
