using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using Microsoft.AspNetCore.Http;
using System.Web.Http;

namespace TREAuthenticationAndManagement.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class AuthenticationController : ApiController
    {
       
        //public static Dictionary<string, string> TokenToRole 


        public HashSet<string> CoolCodes = new HashSet<string>()
        {
            "COOL", "COOL2"
        };

        [HttpGet("")]
        public string Index([FromHeader] string MYCOOLToken)
        {
            if (string.IsNullOrEmpty(MYCOOLToken))
            {
                return "";
            }


            if (CoolCodes.Contains(MYCOOLToken))
            {
                var hasuraVariables = new Dictionary<string, string> {
                        { "X-Hasura-Role", "COOLSchemas2" },
                        { "X-Hasura-User-Ide", "1" },
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

                //StatusCode = 401;
                return "";
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
