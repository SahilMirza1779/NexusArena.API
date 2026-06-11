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

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
