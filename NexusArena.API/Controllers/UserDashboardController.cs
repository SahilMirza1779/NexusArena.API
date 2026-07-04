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

                // 1. User ki saari bookings nikalo (No Slot Include)
                var allBookings = await _context.Bookings
                    .Include(b => b.Resource).ThenInclude(r => r.Arena)
                    .Where(b => b.UserId == userId && b.Status != "Cancelled")
                    .ToListAsync();

                int totalMatches = allBookings.Count;
                var currentDateTime = DateTime.Now;

                // 2. Sirf wo matches filter karo jo abhi shuru nahi hue hain (Strictly Future)
                var upcomingBookings = allBookings
                    .Where(b => {
                        // 🌟 FIX: StartTime use kiya, agar full day hai toh subah 8 baje fallback liya
                        DateTime matchStart = b.StartTime.HasValue
                            ? b.BookingDate.ToDateTime(b.StartTime.Value)
                            : b.BookingDate.ToDateTime(new TimeOnly(8, 0));

                        // Agar match start time + 1 ghanta future mein hai, tabhi aage bhejo
                        return matchStart.AddHours(1) >= currentDateTime;
                    })
                    .OrderBy(b => b.BookingDate)
                    .ThenBy(b => b.StartTime ?? new TimeOnly(0, 0))
                    .ToList();

                int upcomingMatchesCount = upcomingBookings.Count;
                // 🌟 As it is purana logic
                int loyaltyPoints = 150 + (totalMatches * 25);

                var nextGamesList = upcomingBookings.Select(b => new
                {
                    BookingId = b.BookingId,
                    ArenaName = b.Resource?.Arena?.Name ?? "Nexus Turf",
                    PlayDate = b.BookingDate.ToString("dd MMM yyyy"),
                    // 🌟 Format update kiya
                    TimeSlot = b.StartTime != null ? $"{b.StartTime.Value:hh\\:mm tt}" : b.TournamentPackage ?? "Full Day",
                    Status = b.Status ?? "Confirmed",
                    TargetDateTime = b.StartTime.HasValue
                        ? b.BookingDate.ToDateTime(b.StartTime.Value).ToString("yyyy-MM-ddTHH:mm:ss")
                        : b.BookingDate.ToDateTime(new TimeOnly(8, 0)).ToString("yyyy-MM-ddTHH:mm:ss")
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