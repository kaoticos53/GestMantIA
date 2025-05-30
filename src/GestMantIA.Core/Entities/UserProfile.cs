using System.ComponentModel.DataAnnotations;
using GestMantIA.Core.Identity.Entities; // Para ApplicationUser

namespace GestMantIA.Core.Entities.Identity // El namespace se mantiene
{
    public class UserProfile : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; } // Clave foránea a ApplicationUser.Id
        public virtual ApplicationUser User { get; set; } = null!; // Propiedad de navegación

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? PhoneNumber { get; set; }

        // Campos de dirección
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? StateProvince { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        public UserProfile(Guid userId)
        {
            UserId = userId;
        }

        // Constructor sin parámetros para EF Core y serialización
        protected UserProfile()
        {
            UserId = Guid.Empty;
        }
    }
}
