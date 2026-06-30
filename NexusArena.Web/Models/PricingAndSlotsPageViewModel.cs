using System.Collections.Generic;

namespace NexusArena.MVC.Models
{
    // Yeh class ek hi page par Dropdown (Resources) aur Table (Slots) dono dikhane ke kaam aayegi
    public class PricingAndSlotsPageViewModel
    {
        public TimeSlotViewModel NewSlot { get; set; } = new TimeSlotViewModel();
        public List<ResourceViewModel> Resources { get; set; } = new List<ResourceViewModel>();
        public List<TimeSlotViewModel> TimeSlots { get; set; } = new List<TimeSlotViewModel>();
    }
}