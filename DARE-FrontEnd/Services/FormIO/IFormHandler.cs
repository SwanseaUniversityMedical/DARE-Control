using BL.Models;

namespace DARE_FrontEnd.Services.FormIO
{
    public interface IFormHandler
    {

        Task<FormData> GetFormDataById(int id);
    }
}
