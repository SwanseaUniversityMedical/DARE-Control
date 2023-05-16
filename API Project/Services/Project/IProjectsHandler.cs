using BL.Models;
using System.Linq.Expressions;

namespace API_Project.Services.Project
{
    public interface IProjectsHandler
    {
        Task<User> GetUserSettings(int id);

        Task<Projects> GetProject(int id);
        Task<User> AddUser(int id);
        Task<Projects> CreateProject(Projects model);

        IEnumerable<BL.Models.Projects> Get(Expression<Func<BL.Models.Projects, bool>> filter = null, Func<IQueryable<BL.Models.Projects>, IOrderedQueryable<BL.Models.Projects>> orderBy = null);
        BL.Models.Projects? Get(int id);
        IEnumerable<BL.Models.Projects>? GetAll();
        Task<bool> AddAsync(BL.Models.Projects ProjectModel);
        Task<bool> Update(BL.Models.Projects ProjectModel);
        Task<bool> Remove(int id);
        Task<bool> Remove(BL.Models.Projects p);
        

    }
}
