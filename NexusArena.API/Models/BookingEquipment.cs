using System;
using System.Collections.Generic;

namespace NexusArena.API.Models;

public partial class BookingEquipment
{
    public int BookingEqId { get; set; }

    public int BookingId { get; set; }

    public int EquipmentId { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }
    public bool IsReturned { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Equipment Equipment { get; set; } = null!;
}
