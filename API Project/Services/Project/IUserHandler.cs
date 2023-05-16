using BL.Models;

namespace API_Project.Services.Project
{
    public interface IUserHandler
    {

        Task<User> GetUserSettings(int id);

        Task<User> CreateUser(User user);

    }
}
