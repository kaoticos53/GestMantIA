using GestMantIA.Core.Identity.Entities;

namespace GestMantIA.Core.Identity.Interfaces
{
    /// <summary>
    /// Contrato para el acceso y manipulación de entidades de usuario.
    /// </summary>
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(Guid userId);
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        Task AddAsync(ApplicationUser user);
        Task UpdateAsync(ApplicationUser user);
        Task DeleteAsync(Guid userId);
        Task<bool> ExistsAsync(Guid userId);
        // Métodos adicionales según reglas de negocio
    }
}
