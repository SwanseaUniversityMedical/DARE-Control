using Project_Admin.Models;
using System.Linq.Expressions;

namespace Project_Admin.Services.Project
{
    public interface IProjectsHandler
    {
        
        IEnumerable<Models.Project> Get(Expression<Func<Models.Project, bool>> filter = null, Func<IQueryable<Models.Project>, IOrderedQueryable<Models.Project>> orderBy = null);
        Models.Project? Get(int id);
        IEnumerable<Models.Project>? GetAll();
        Task<bool> AddAsync(Models.Project ProjectModel);
        Task<bool> Update(Models.Project ProjectModel);
        Task<bool> Remove(int id);
        Task<bool> Remove(Models.Project p);
        

    }
}
