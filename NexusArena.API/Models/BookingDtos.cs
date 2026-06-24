namespace NexusArena.API.Models
{
    public class CreateBookingRequest
    {
        public int ArenaId { get; set; }
        public int SlotId { get; set; }
        public string PlayDate { get; set; } = null!; // Format: "yyyy-MM-dd"

        // Default mode ab 'Advance50' rahega (PayAtTurf hatane ke baad)
        public string PaymentMode { get; set; } = "Advance50";
    }

    public class SlotAvailabilityDto
    {
        public int SlotId { get; set; }
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }

    // PHASE 4 DTO: Verification endpoint ke liye data model
    public class PaymentVerificationDto
    {
        public int BookingId { get; set; }
        public string RazorpayPaymentId { get; set; } = null!;
        public string RazorpayOrderId { get; set; } = null!;
        public string RazorpaySignature { get; set; } = null!;
    }

    // 🌟 NAYA DTO: Booking History ke real data ke liye
    public class UserBookingHistoryDto
    {
        public int BookingId { get; set; }
        public string ArenaName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PlayDate { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal PendingAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}