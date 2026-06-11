using System;
using System.Collections.Generic;

namespace NexusArena.API.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int UserId { get; set; }

    public int ResourceId { get; set; }

    public int SlotId { get; set; }

    public DateOnly BookingDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<BookingEquipment> BookingEquipments { get; set; } = new List<BookingEquipment>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Resource Resource { get; set; } = null!;

    public virtual TimeSlot Slot { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
