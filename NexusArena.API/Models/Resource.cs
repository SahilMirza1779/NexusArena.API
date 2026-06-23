using System;
using System.Collections.Generic;
using System.Text.Json.Serialization; 

namespace NexusArena.API.Models;

public partial class Resource
{
    public int ResourceId { get; set; }

    public int ArenaId { get; set; }

    public int CategoryId { get; set; }

    public string ResourceName { get; set; } = null!;

    public int? Capacity { get; set; }

    [JsonIgnore]
    public virtual Arena? Arena { get; set; }

    [JsonIgnore]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    [JsonIgnore]
    public virtual SportCategory? Category { get; set; }

    [JsonIgnore]
    public virtual ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
}