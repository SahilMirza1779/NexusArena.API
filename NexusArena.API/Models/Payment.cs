using System;
using System.Collections.Generic;

namespace NexusArena.API.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int BookingId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal AdvancePaid { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public decimal PendingAmount { get; set; }

    public string? GatewayTransactionId { get; set; }

    public string? RefundStatus { get; set; }

    public decimal? RefundAmount { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
