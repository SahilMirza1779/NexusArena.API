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

    public decimal HourlyRegularPrice { get; set; }

    public decimal HourlyPeakPrice { get; set; }

    public decimal HalfDayMorningPrice { get; set; }

    public decimal HalfDayEveningPrice { get; set; }

    public decimal FullDayPrice { get; set; }

    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
