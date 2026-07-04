namespace NexusArena.Web.Models
{
    public class ManageReceptionistViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // 🚨 NAYA FIELD: Table aur Backend me Business Name handle karne ke liye
        public string? BusinessName { get; set; }
    }
}