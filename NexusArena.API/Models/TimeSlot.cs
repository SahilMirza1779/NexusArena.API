#nullable disable
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

<<<<<<< HEAD
    public string? FestivalName { get; set; }
    public int? DiscountPercent { get; set; } = 0;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
=======
    // 🚨 THE GHOST KILLER: Is line ko comment/delete kar diya hai taaki EF Core Booking aur TimeSlot ka rishta bhool jaye!
    // public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
>>>>>>> aba64065e454c18b5fad2d990cc58c64aa397b4b

    public virtual Resource Resource { get; set; } = null!;
}