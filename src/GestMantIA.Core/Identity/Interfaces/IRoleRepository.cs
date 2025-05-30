using GestMantIA.Core.Identity.Entities;

namespace GestMantIA.Core.Identity.Interfaces
{
    /// <summary>
    /// Contrato para el acceso y manipulación de entidades de rol.
    /// </summary>
    public interface IRoleRepository
    {
        Task<ApplicationRole?> GetByIdAsync(Guid roleId);
        Task<ApplicationRole?> GetByNameAsync(string roleName);
        Task<IEnumerable<ApplicationRole>> GetAllAsync();
        Task AddAsync(ApplicationRole role);
        Task UpdateAsync(ApplicationRole role);
        Task DeleteAsync(Guid roleId);
        Task<bool> ExistsAsync(Guid roleId);
        // Métodos adicionales según reglas de negocio
    }
}
