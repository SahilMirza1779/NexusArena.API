using System.Text.Json.Serialization; // 🌟 Yeh upar add karna mat bhoolna

namespace NexusArena.Web.Models
{
    public class BookingHistoryViewModel
    {
        public int BookingId { get; set; }
        public int ArenaId { get; set; }
        public string ArenaName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PlayDate { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal PendingAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool CanCancel { get; set; }

        // 🌟 ENTERPRISE FIX: Ab C# ko pata chalega ki API ka 'isRated' yahan save karna hai
        [JsonPropertyName("isRated")]
        public bool IsRated { get; set; }
    }
}