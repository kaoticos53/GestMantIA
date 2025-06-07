using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestMantIA.Shared.Identity.DTOs.Requests
{
    public class UpdateUserRolesRequestDTO
    {
        [Required]
        public List<string> RoleNames { get; set; } = new List<string>();
    }
}
