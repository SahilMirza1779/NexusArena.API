using System;

namespace NexusArena.Web.Models
{
    public class ReceptionistBookingHistoryViewModel
    {
        public int BookingId { get; set; }
        public string? CustomerName { get; set; }
        public string? TurfName { get; set; }
        public DateOnly BookingDate { get; set; }
        public string? TimeSlot { get; set; }
        public string? Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
    }
}
