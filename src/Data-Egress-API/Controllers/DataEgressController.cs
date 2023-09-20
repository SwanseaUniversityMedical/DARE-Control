using BL.Models.APISimpleTypeReturns;
using Microsoft.AspNetCore.Mvc;

namespace Data_Egress_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataEgressController : ControllerBase
    {
        

        public DataEgressController()
        {
           
        }

        [HttpPost(Name = "AddNewDataEgress")]
        public async Task<BoolReturn> AddNewDataEgress(int submissionId, List<string> files)
        {
            return new BoolReturn() { Result = true };
        }
    }
}