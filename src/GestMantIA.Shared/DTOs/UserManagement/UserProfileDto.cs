using System;
using System.Collections.Generic;

namespace GestMantIA.Shared.DTOs.UserManagement
{
    public class UserProfileDto
    {
        public required string Id { get; set; } // Proviene de ApplicationUser.Id (es string por defecto)
        public required string UserName { get; set; } // Proviene de ApplicationUser
        public required string Email { get; set; } // Proviene de ApplicationUser
        
        public string? FirstName { get; set; } // Proviene de UserProfile
        public string? LastName { get; set; } // Proviene de UserProfile
        public string? PhoneNumber { get; set; } // Proviene de UserProfile (o ApplicationUser si se prefiere allí)
        
        public bool EmailConfirmed { get; set; } // Proviene de ApplicationUser
        public bool TwoFactorEnabled { get; set; } // Proviene de ApplicationUser
        public DateTimeOffset? LockoutEnd { get; set; } // Proviene de ApplicationUser
        
        public DateTime CreatedAt { get; set; } // Proviene de UserProfile (a través de BaseEntity) o ApplicationUser si fue extendido
        public IEnumerable<string> Roles { get; set; } = new List<string>();

        // Campos adicionales de UserProfile que podrían ser útiles para mostrar
        public DateTime? DateOfBirth { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? StateProvince { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
    }
}
