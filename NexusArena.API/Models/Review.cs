#nullable enable

using System;
using System.Collections.Generic;

namespace NexusArena.API.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public int ArenaId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Arena Arena { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
