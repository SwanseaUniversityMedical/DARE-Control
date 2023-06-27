using BL.DTO;
using BL.Models;
using System.Linq.Expressions;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Nodes;
using static DARE_FrontEnd.Controllers.FormsController;

namespace DARE_FrontEnd.Services.Project
{
    public interface IProjectsHandler
    {

        //Task<User> GetUserSettings(int id);
        Task<BL.Models.Project> CreateProject(data model);
        //Task<Projects> CreateProject(JsonObject model);

        Task<BL.Models.Project> GetProjectSettings(int id);

        Task<BL.Models.Project> GetAllProjects();

        Task<BL.Models.Endpoint> GetAllEndPoints(int projectId);


        //Task<User> AddAUser(User model);
        //Task<User> AddAUser1(JsonObject model);

        Task<User> AddAUser(FormIoData model);
        //Task<User> AddAUser1(JsonObject model);
        Task<User> GetAUser(int id);
        Task<User> GetAllUsers();

        //Task<ProjectMembership> AddMembership(ProjectMembership membership);


        //IEnumerable<Models.Projects> Get(Expression<Func<Models.Projects, bool>> filter = null, Func<IQueryable<Models.Projects>, IOrderedQueryable<Models.Projects>> orderBy = null);
        //Models.Projects? Get(int id);
        //IEnumerable<Models.Projects>? GetAll();
        Task<bool> AddAsync(BL.Models.Project ProjectModel);
        Task<bool> Update(BL.Models.Project ProjectModel);
        //Task<bool> Remove(int id);
        //Task<bool> Remove(Models.Projects p);
        Task<T> GenericGetData<T>(string endPoint, StringContent jsonString = null, bool usePut = false) where T : class, new();
        Task<T> GenericGetData<T>(string v, Task<T> stringContent);
    }
}
