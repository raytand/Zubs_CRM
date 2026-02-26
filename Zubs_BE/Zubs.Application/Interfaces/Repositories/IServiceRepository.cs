using Zubs.Domain.Entities;

namespace Zubs.Application.Interfaces.Repositories
{
    public interface IServiceRepository : IRepository<Service>
    {
        Task<bool> ExistsByCodeAsync(string code);
    }
}
