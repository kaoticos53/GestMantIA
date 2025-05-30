using GestMantIA.Core.Identity.Entities;

namespace GestMantIA.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetByIdAsync(Guid id);
        Task<IReadOnlyList<ApplicationUser>> ListAllAsync();
        Task<ApplicationUser> AddAsync(ApplicationUser entity);
        Task UpdateAsync(ApplicationUser entity);
        Task DeleteAsync(ApplicationUser entity);

    }
}
