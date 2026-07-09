using System;
using System.Collections.Generic;

namespace NexusArena.API.Models;

public partial class Resource
{
    public int ResourceId { get; set; }

    public int ArenaId { get; set; }

    public int CategoryId { get; set; }

    public string ResourceName { get; set; } = null!;

    public int? Capacity { get; set; }

    public decimal BasePricePerHour { get; set; }

    public string ResourceType { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual Arena Arena { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual SportCategory Category { get; set; } = null!;

    public virtual ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
}
