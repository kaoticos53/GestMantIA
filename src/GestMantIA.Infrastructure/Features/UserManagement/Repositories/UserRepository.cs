using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestMantIA.Infrastructure.Features.UserManagement.Repositories
{
    /// <summary>
    /// Implementación de IUserRepository usando ApplicationDbContext.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ApplicationUser user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid userId)
        {
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (entity != null)
            {
                _context.Users.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ApplicationUser?> GetByIdAsync(Guid userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }
    }
}
