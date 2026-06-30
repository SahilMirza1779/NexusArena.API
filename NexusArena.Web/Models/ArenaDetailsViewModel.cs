namespace NexusArena.Web.Models
{
    public class ArenaDetailsViewModel
    {
        public int Id { get; set; }
        public string ArenaName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}