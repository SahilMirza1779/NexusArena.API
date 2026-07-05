using System;

namespace NexusArena.MVC.Models
{
    public class TimeSlotViewModel
    {
        public int SlotId { get; set; }
        public int ResourceId { get; set; }
        public string ResourceName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsPremium { get; set; }

        // 🚨 Naye Festival aur Discount ke fields (Nullable banaye hain taaki purana data crash na ho)
        public string? FestivalName { get; set; }
        public int? DiscountPercent { get; set; }
    }
}