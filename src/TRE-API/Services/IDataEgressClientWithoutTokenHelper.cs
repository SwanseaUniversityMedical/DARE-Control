using BL.Services;

namespace TRE_API.Services
{
    public interface IDataEgressClientWithoutTokenHelper: IBaseClientHelper
    {

        bool CheckCredsAreAvailable();


    }
}
