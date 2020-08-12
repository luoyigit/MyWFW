using System.Threading.Tasks;
using Project.Domain.SeedWork;

namespace Project.Domain.AggregatesModel
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<Project> GetAsync(int id);
        Task<Project> AddAsync(Project project);
        void Update(Project project);
    }
}
