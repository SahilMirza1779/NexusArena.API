using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NexusArena.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserDashboardController(NexusArenaDbContext context) : ControllerBase
{
    private readonly NexusArenaDbContext _context = context;

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized(new { message = "Invalid Token." });

            var allBookings = await _context.Bookings
                .Include(b => b.Resource).ThenInclude(r => r.Arena)
                .Where(b => b.UserId == userId && b.Status != "Cancelled")
                .ToListAsync();

            int totalMatches = allBookings.Count;
            var currentDateTime = DateTime.Now;

            decimal totalSpent = allBookings
                .Where(b => b.PaymentStatus == "Paid")
                .Sum(b => b.AmountPaid);

            var upcomingBookings = allBookings
                .Where(b => {
                    DateTime matchStart = b.StartTime.HasValue
                        ? b.BookingDate.ToDateTime(b.StartTime.Value)
                        : b.BookingDate.ToDateTime(new TimeOnly(8, 0));

                    return matchStart.AddHours(1) >= currentDateTime &&
                           b.Status != "Completed" &&
                           b.Status != "Expired";
                })
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.StartTime ?? new TimeOnly(0, 0))
                .ToList();

            int upcomingMatches = upcomingBookings.Count;
            int loyaltyPoints = 150 + (totalMatches * 25);

            var nextGames = upcomingBookings.Select(b => new
            {
                b.BookingId,
                ArenaName = b.Resource?.Arena?.Name ?? "Nexus Turf",
                PlayDate = b.BookingDate.ToString("dd MMM yyyy"),
                TimeSlot = b.StartTime != null ? $"{b.StartTime.Value:hh\\:mm tt}" : b.TournamentPackage ?? "Full Day",
                Status = b.Status ?? "Confirmed",
                TargetDateTime = b.StartTime.HasValue
                    ? b.BookingDate.ToDateTime(b.StartTime.Value).ToString("yyyy-MM-ddTHH:mm:ss")
                    : b.BookingDate.ToDateTime(new TimeOnly(8, 0)).ToString("yyyy-MM-ddTHH:mm:ss")
            }).ToList();

            return Ok(new { message = "Success", data = new { totalMatches, upcomingMatches, loyaltyPoints, totalSpent, nextGames } });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal Server Error: " + ex.Message });
        }
    }
}