using System;
using System.Collections.Generic;

namespace NexusArena.API.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int UserId { get; set; }

    public int ResourceId { get; set; }

    public DateOnly BookingDate { get; set; }

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public decimal AmountPaid { get; set; }

    public string? PaymentMode { get; set; }

    public string? PaymentStatus { get; set; }

    public string? TransactionId { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string BookingMode { get; set; } = null!;

    public string? TournamentPackage { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Resource Resource { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
