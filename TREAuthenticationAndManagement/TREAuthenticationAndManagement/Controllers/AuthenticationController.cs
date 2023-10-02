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

        //TODO save Tokens  To disk?
        //TODO Easy way to grab all the generated roles , even if they had been generated already 

        public static Dictionary<string, string> TokenToRole = new Dictionary<string, string>();

        public static List<string> GenRoles = new List<string>()
        {
            "COOLSchemas2",
            "COOLSchemas1"
        };


        public HashSet<string> CoolCodes = new HashSet<string>()
        {
            "COOL", "COOL2"
        };

        [HttpGet("GetNewToken")]
        public string GetNewToken(string role)
        {
            if (GenRoles.Contains(role))
            {
                var Token = "AAAAAAAAAAAAA"; //TODO The have a better system for token 
                TokenToRole[Token] = role;
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
                        { "X-Hasura-Role", TokenToRole[MYCOOLToken] },
                        { "X-Hasura-User-Ide", "1" }, //TOOD ID
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

        private StringContent GetStringContent<T>(T datasetObj) where T : class
        {
            var  _jsonSerializerOptions = new JsonSerializerOptions()
            {
                //AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
            };
            var jsonString = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(datasetObj, _jsonSerializerOptions),
                Encoding.UTF8,
                "application/json");
            return jsonString;
        }
    }

   
}
