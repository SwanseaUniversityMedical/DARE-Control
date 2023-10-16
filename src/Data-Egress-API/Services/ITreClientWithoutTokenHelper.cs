using BL.Services;

namespace Data_Egress_API.Services
{
    public interface ITreClientWithoutTokenHelper: IBaseClientHelper
    {

        bool CheckCredsAreAvailable();


    }
}
