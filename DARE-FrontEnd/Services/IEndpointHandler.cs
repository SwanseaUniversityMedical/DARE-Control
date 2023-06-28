using BL.Models;
using static DARE_FrontEnd.Controllers.FormsController;

namespace DARE_FrontEnd.Services
{
    public interface IEndpointHandler
    {
        Task<Endpoints> CreateEndpoint(data model);

        Task<Endpoints> ListOfAllEndpoints(List<Endpoints> endpoints);


    }
}
