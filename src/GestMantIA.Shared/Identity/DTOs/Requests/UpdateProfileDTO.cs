namespace GestMantIA.Shared.Identity.DTOs.Requests
{
    public class UpdateProfileDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        // Consider adding other updatable profile fields here if needed in the future,
        // e.g., ProfilePictureUrl, Bio, etc.
    }
}
