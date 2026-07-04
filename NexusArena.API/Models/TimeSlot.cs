using System;
using System.Collections.Generic;

namespace NexusArena.API.Models;

public partial class TimeSlot
{
    public int SlotId { get; set; }

    public int ResourceId { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public decimal BasePrice { get; set; }

    public bool? IsPremium { get; set; }

    public string? FestivalName { get; set; }
    public int? DiscountPercent { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Resource Resource { get; set; } = null!;
}
