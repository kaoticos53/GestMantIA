using GestMantIA.Core.Entities.Identity; // Namespace de UserProfile
using GestMantIA.Core.Interfaces;       // Para IRepository

namespace GestMantIA.Core.Repositories // El namespace se mantiene
{
    public interface IUserProfileRepository : IRepository<UserProfile>
    {
        // Aquí se pueden añadir métodos específicos para UserProfile si son necesarios en el futuro.
        // Por ejemplo:
        // Task<UserProfile?> GetByUserIdAsync(Guid userId);
    }
}
