using System.Collections.Generic;

namespace NexusArena.MVC.Models
{
    public class OwnerDashboardViewModel
    {
        public decimal TodayRevenue { get; set; }
        public string LiveOccupancy { get; set; } = "0%";
        public List<UpcomingBookingViewModel> UpcomingBookings { get; set; } = new List<UpcomingBookingViewModel>();
    }

    public class UpcomingBookingViewModel
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}