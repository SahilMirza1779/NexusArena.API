namespace NexusArena.Web.Models
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }

        // string ke aage '?' lagane se nullable error chala jayega
        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
    }
}