#nullable enable
using System;
using System.Collections.Generic;

namespace NexusArena.API.Models;

public partial class Arena
{
    public int ArenaId { get; set; }
    public int OwnerId { get; set; }
    public string Name { get; set; } = null!;
    public string? Location { get; set; }
    public string City { get; set; } = null!;
    public bool? IsActive { get; set; }

    // 🌟 NAYE COLUMNS (Prices) YAHAN ADD HUI HAIN
    public decimal HourlyRegularPrice { get; set; }
    public decimal HourlyPeakPrice { get; set; }
    public decimal HalfDayMorningPrice { get; set; }
    public decimal HalfDayEveningPrice { get; set; }
    public decimal FullDayPrice { get; set; }

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
    public virtual User Owner { get; set; } = null!;
    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    // 🌟 MAPPING RELATION YAHAN ADD HUA HAI
    public virtual ICollection<ArenaSport> ArenaSports { get; set; } = new List<ArenaSport>();
}