using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserDashboardController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public UserDashboardController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                    return Unauthorized(new { message = "Invalid Token." });

                // 1. User ki saari bookings nikalo
                var allBookings = await _context.Bookings
                    .Include(b => b.Resource).ThenInclude(r => r.Arena)
                    .Include(b => b.Slot)
                    .Where(b => b.UserId == userId && b.Status != "Cancelled")
                    .ToListAsync();

                int totalMatches = allBookings.Count;
                var currentDateTime = DateTime.Now; // 🌟 NAYA: Ab hum real exact time le rahe hain

                // 2. Sirf wo matches filter karo jo abhi shuru nahi hue hain (Strictly Future)
                var upcomingBookings = allBookings
                    .Where(b => {
                        DateTime matchStart = b.Slot != null
                            ? b.BookingDate.ToDateTime(b.Slot.StartTime)
                            : b.BookingDate.ToDateTime(new TimeOnly(18, 0));

                        // 🌟 FIX: Agar match start time + 1 ghanta future mein hai, tabhi aage bhejo
                        return matchStart.AddHours(1) >= currentDateTime;
                    })
                    .OrderBy(b => b.BookingDate)
                    .ThenBy(b => b.Slot != null ? b.Slot.StartTime : new TimeOnly(0, 0))
                    .ToList();

                int upcomingMatchesCount = upcomingBookings.Count;
                int loyaltyPoints = 150 + (totalMatches * 25);

                var nextGamesList = upcomingBookings.Select(b => new
                {
                    BookingId = b.BookingId,
                    ArenaName = b.Resource?.Arena?.Name ?? "Nexus Turf",
                    PlayDate = b.BookingDate.ToString("dd MMM yyyy"),
                    TimeSlot = b.Slot != null ? $"{b.Slot.StartTime:hh\\:mm tt}" : "6:00 PM",
                    Status = b.Status ?? "Confirmed",
                    TargetDateTime = b.Slot != null
                        ? b.BookingDate.ToDateTime(b.Slot.StartTime).ToString("yyyy-MM-ddTHH:mm:ss")
                        : b.BookingDate.ToDateTime(new TimeOnly(18, 0)).ToString("yyyy-MM-ddTHH:mm:ss")
                }).ToList();

                var dashboardData = new
                {
                    TotalMatches = totalMatches,
                    UpcomingMatches = upcomingMatchesCount,
                    LoyaltyPoints = loyaltyPoints,
                    NextGames = nextGamesList
                };

                return Ok(new { message = "Success", data = dashboardData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error: " + ex.Message });
            }
        }
    }
}