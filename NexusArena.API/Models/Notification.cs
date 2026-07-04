#nullable disable

using System;
using System.Collections.Generic;

namespace NexusArena.API.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int? UserId { get; set; }

    public string Message { get; set; } = null!;

    public string Type { get; set; } = null!;

    public bool? IsSent { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
