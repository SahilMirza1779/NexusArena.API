namespace NexusArena.MVC.Models
{
    public class ResourceViewModel
    {
        // YEH PROPERTY MISSING THI
        public int ResourceId { get; set; }

        public string ResourceName { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal BasePricePerHour { get; set; }
        public string Dimensions { get; set; } = string.Empty;
        public string IncludedEquipment { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}