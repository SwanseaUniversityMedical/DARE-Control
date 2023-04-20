using Project_Admin.Models;
using System.Linq.Expressions;

namespace Project_Admin.Services.Project
{
    public interface IProjectsHandler
    {
        
        IEnumerable<ProjectModel> Get(Expression<Func<ProjectModel, bool>> filter = null, Func<IQueryable<ProjectModel>, IOrderedQueryable<ProjectModel>> orderBy = null);
        ProjectModel? Get(int id);
        IEnumerable<ProjectModel>? GetAll();
        Task<bool> AddAsync(ProjectModel ProjectModel);
        Task<bool> Update(ProjectModel ProjectModel);
        Task<bool> Remove(int id);
        Task<bool> Remove(ProjectModel p);
        

    }
}
