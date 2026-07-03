#nullable disable

namespace NexusArena.Web.Models
{
    public class ManageUserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }

        public int TotalBookings { get; set; }
    }
}