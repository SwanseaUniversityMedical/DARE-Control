using BL.Models;
using static DARE_FrontEnd.Controllers.FormsController;

namespace DARE_FrontEnd.Services
{
    public interface IEndpointHandler
    {
        Task<BL.Models.Endpoint> CreateEndpoint(data model);
    }
}
