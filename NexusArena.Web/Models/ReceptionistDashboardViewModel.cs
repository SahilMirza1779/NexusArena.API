using System.Collections.Generic;

namespace NexusArena.Web.Models 
{
    public class ReceptionistDashboardViewModel
    {
        public decimal TotalPendingCash { get; set; }
        public int TodayBookingsCount { get; set; }
        public int AvailableTurfsCount { get; set; }
        public List<LiveBookingViewModel> LiveBookings { get; set; } = new List<LiveBookingViewModel>();
    }

    public class LiveBookingViewModel
    {
        public int BookingId { get; set; }
        public string? CustomerName { get; set; } 
        public string? TurfName { get; set; }     
        public string? TimeSlot { get; set; }    
        public decimal PendingAmount { get; set; }
        public bool IsTimeUpWarning { get; set; }
    }
}