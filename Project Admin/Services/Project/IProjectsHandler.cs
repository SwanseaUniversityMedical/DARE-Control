using Project_Admin.Models;
using System.Linq.Expressions;

namespace Project_Admin.Services.Project
{
    public interface IProjectsHandler
    {
        
        //Task<User> GetUserSettings(int id);

        Task<Projects> GetProjectSettings(int id);
        //Task<User> AddUser(int id);
        Task<Projects> CreateProject(Projects model);

        //IEnumerable<Models.Projects> Get(Expression<Func<Models.Projects, bool>> filter = null, Func<IQueryable<Models.Projects>, IOrderedQueryable<Models.Projects>> orderBy = null);
        //Models.Projects? Get(int id);
        //IEnumerable<Models.Projects>? GetAll();
        Task<bool> AddAsync(Models.Projects ProjectModel);
        Task<bool> Update(Models.Projects ProjectModel);
        //Task<bool> Remove(int id);
        //Task<bool> Remove(Models.Projects p);
        Task<T> GenericGetData<T>(string endPoint, StringContent jsonString = null, bool usePut = false) where T : class, new();
        Task<T> GenericGetData<T>(string v, Task<T> stringContent);
    }
}
