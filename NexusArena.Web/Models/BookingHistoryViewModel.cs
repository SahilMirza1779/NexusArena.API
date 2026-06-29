namespace NexusArena.API.Models
{
    public class BookingHistoryViewModel
    {
        public int BookingId { get; set; }
        public string? ArenaName { get; set; }
        public string? City { get; set; }
        public string? PlayDate { get; set; }
        public string? TimeSlot { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal PendingAmount { get; set; }
        public string? PaymentStatus { get; set; }
        public string? Status { get; set; }
    }
}