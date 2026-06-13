using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System.Security.Claims;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class UserDashboardController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public UserDashboardController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpGet("widgets")]
        public async Task<IActionResult> GetDashboardWidgets()
        {
            try
            {
                // Token se User ID nikalna
                var userIdString = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized(new { message = "Invalid Token. User ID not found." });
                }

                int userId = int.Parse(userIdString);

                // Naye model ke hisaab se DateOnly ka use
                var today = DateOnly.FromDateTime(DateTime.Today);

                // Upcoming Matches: Direct 'Resource' aur 'Slot' ko include kiya hai
                var upcomingMatches = await _context.Bookings
                    .Include(b => b.Resource)
                        .ThenInclude(r => r.Arena)
                    .Include(b => b.Slot)
                    .Where(b => b.UserId == userId && b.Status == "Confirmed" && b.BookingDate >= today)
                    .OrderBy(b => b.BookingDate)
                    .Select(b => new
                    {
                        BookingId = b.BookingId,
                        ArenaName = b.Resource.Arena.Name,
                        Sport = b.Resource.ResourceName,
                        PlayDate = b.BookingDate.ToString(), // DateOnly to String
                        StartTime = b.Slot.StartTime.ToString(), // TimeOnly to String
                        Status = b.Status
                    })
                    .Take(3)
                    .ToListAsync();

                // Past Matches Count
                var totalMatchesPlayed = await _context.Bookings
                    .Where(b => b.UserId == userId && b.Status == "Confirmed" && b.BookingDate < today)
                    .CountAsync();

                return Ok(new
                {
                    message = "Dashboard data fetched successfully",
                    stats = new
                    {
                        totalMatchesPlayed = totalMatchesPlayed,
                        loyaltyPoints = 0 // Static for now
                    },
                    upcomingMatches = upcomingMatches
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}