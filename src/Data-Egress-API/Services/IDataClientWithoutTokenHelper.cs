using BL.Services;

namespace Data_Egress_API.Services
{
    public interface IDataClientWithoutTokenHelper : IBaseClientHelper
    {

        bool CheckCredsAreAvailable();


    }
}
