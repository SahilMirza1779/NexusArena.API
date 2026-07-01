namespace NexusArena.Web.Models
{
    public class AvailableTurfViewModel
    {
        public int ResourceId { get; set; }
        public string? ResourceName { get; set; }
        public string? ResourceType { get; set; }
        public int Capacity { get; set; }
    }
}