using GestMantIA.Core.Entities.Identity;    // Para UserProfile
using GestMantIA.Core.Repositories;         // Para IUserProfileRepository
using GestMantIA.Infrastructure.Data;       // Para ApplicationDbContext

namespace GestMantIA.Infrastructure.Features.UserManagement.Repositories
{
    public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
    {
        public UserProfileRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        // Ejemplo de implementación de un método específico si se añadiera a IUserProfileRepository:
        // public async Task<UserProfile?> GetByUserIdAsync(Guid userId)
        // {
        //     return await _dbContext.Set<UserProfile>()
        //                            .FirstOrDefaultAsync(up => up.UserId == userId);
        // }
    }
}
