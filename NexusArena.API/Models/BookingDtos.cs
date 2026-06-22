namespace NexusArena.API.Models
{
    public class CreateBookingRequest
    {
        public int ArenaId { get; set; }
        public int SlotId { get; set; }
        public string PlayDate { get; set; } = null!; // Format: "yyyy-MM-dd"

        // 🌟 NAYA FIELD: Payment ka tarika
        public string PaymentMode { get; set; } = "PayAtTurf";
    }

    public class SlotAvailabilityDto
    {
        public int SlotId { get; set; }
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}