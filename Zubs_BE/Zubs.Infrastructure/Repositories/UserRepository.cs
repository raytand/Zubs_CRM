using Microsoft.EntityFrameworkCore;
using Zubs.Application.Interfaces.Repositories;
using Zubs.Domain.Entities;
using Zubs.Infrastructure.Persistence;

namespace Zubs.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);
    }
    public override async Task DeleteAsync(Guid id)
    {
        var user = await _dbSet.FindAsync(id);
        if (user != null)
        {
            _dbSet.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
    
}
