namespace NexusArena.Web.Models
{
    public class OwnerProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? BusinessName { get; set; }
    }
}