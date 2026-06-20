using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageNotificationsController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public ManageNotificationsController(NexusArenaDbContext context)
        {
            _context = context;
        }

        public class BroadcastRequest
        {
            public string TargetAudience { get; set; } = null!;
            public string Message { get; set; } = null!;
        }

        [HttpPost("SendBroadcast")]
        public async Task<IActionResult> SendBroadcast([FromBody] BroadcastRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new {success = false, message = "The message cannot be empty!" });
            }
            var targetUsersQuery = _context.Users.AsQueryable();
            if (request.TargetAudience == "Users")
            {
                targetUsersQuery = targetUsersQuery.Where(u => u.RoleId == 3);
            }
            else if (request.TargetAudience == "Owners")
            {
                targetUsersQuery = targetUsersQuery.Where(u => u.RoleId == 2);
            }
            else if (request.TargetAudience == "All")
            {
                targetUsersQuery = targetUsersQuery.Where(u => u.RoleId != 1);
            }

            var userToNotify = await targetUsersQuery.ToListAsync();
            if (!userToNotify.Any())
            {
                return NotFound(new { success = false, message = "No user found in this category." });
            }
            var notificationsList = new List<Notification>();
            var currentTime = DateTime.Now;

            foreach (var user in userToNotify)
            {
                notificationsList.Add(new Notification
                {
                    UserId = user.UserId,
                    Message = request.Message,
                    Type = "System Broadcast",
                    IsSent = true,
                    CreatedAt = currentTime
                });
            }

            _context.Notifications.AddRange(notificationsList);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Awesome! The broadcast was successfully sent to {userToNotify.Count} people." });
        }
    }
}
