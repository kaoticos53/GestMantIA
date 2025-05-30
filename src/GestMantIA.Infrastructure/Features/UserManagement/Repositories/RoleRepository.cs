using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces;
using GestMantIA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestMantIA.Infrastructure.Features.UserManagement.Repositories
{
    /// <summary>
    /// Implementación de IRoleRepository usando ApplicationDbContext.
    /// </summary>
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ApplicationRole role)
        {
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid roleId)
        {
            var entity = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (entity != null)
            {
                _context.Roles.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ApplicationRole?> GetByIdAsync(Guid roleId)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        }

        public async Task<ApplicationRole?> GetByNameAsync(string roleName)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task<IEnumerable<ApplicationRole>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task UpdateAsync(ApplicationRole role)
        {
            _context.Entry(role).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid roleId)
        {
            return await _context.Roles.AnyAsync(r => r.Id == roleId);
        }
    }
}
