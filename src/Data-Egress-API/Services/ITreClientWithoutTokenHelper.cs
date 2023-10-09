using BL.Services;

namespace TRE_API.Services
{
    public interface ITreClientWithoutTokenHelper: IBaseClientHelper
    {

        bool CheckCredsAreAvailable();


    }
}
