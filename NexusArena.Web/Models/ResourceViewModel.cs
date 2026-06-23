using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace NexusArena.MVC.Models
{
    public class ResourceViewModel
    {
        public string ResourceName { get; set; } = string.Empty;

        public string ResourceType { get; set; } = string.Empty;

        public List<string> SelectedSports { get; set; } = new List<string>();

        public decimal BasePricePerHour { get; set; }

        public int Capacity { get; set; }

        public string Dimensions { get; set; } = string.Empty;

        public string IncludedEquipment { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}