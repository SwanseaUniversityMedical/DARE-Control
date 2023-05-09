using Project_Admin.Models;
using System.Linq.Expressions;

namespace API_Project.Services
{
    public interface IProjectsHandler
    {
        Task<User> GetUserSettings(int id);

        Task<Projects> GetProjectSettings(int id);
        Task<User> AddUser(int id);
        Task<Projects> CreateProjectSettings(Projects model);

        IEnumerable<Project_Admin.Models.Projects> Get(Expression<Func<Project_Admin.Models.Projects, bool>> filter = null, Func<IQueryable<Project_Admin.Models.Projects>, IOrderedQueryable<Project_Admin.Models.Projects>> orderBy = null);
        Project_Admin.Models.Projects? Get(int id);
        IEnumerable<Project_Admin.Models.Projects>? GetAll();
        Task<bool> AddAsync(Project_Admin.Models.Projects ProjectModel);
        Task<bool> Update(Project_Admin.Models.Projects ProjectModel);
        Task<bool> Remove(int id);
        Task<bool> Remove(Project_Admin.Models.Projects p);
        

    }
}
