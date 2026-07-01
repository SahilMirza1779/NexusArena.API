using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserNotificationsController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public UserNotificationsController(NexusArenaDbContext context)
        {
            _context = context;
        }

        // 🌟 FULL UPDATED: Personal + Broadcast Notifications dono fetch karega
        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications()
        {
            try
            {
                // Token se User ID nikalo
                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                    return Unauthorized(new { message = "Invalid Token." });

                // 🌟 THE LOGIC: UserID match ho YA phir UserId NULL ho (Broadcast message)
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId || n.UserId == null)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(10)
                    .Select(n => new
                    {
                        id = n.NotificationId,
                        title = n.Type ?? "System Alert",
                        message = n.Message,
                        date = n.CreatedAt.HasValue ? n.CreatedAt.Value.ToString("dd MMM, hh:mm tt") : "Just now"
                    })
                    .ToListAsync();

                return Ok(new { data = notifications, count = notifications.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }
    }
}