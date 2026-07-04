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
    // 🌟 THE FIX: IDE0290 - Use Primary Constructor
    public class BookingHistoryController(NexusArenaDbContext context) : ControllerBase
    {
        private readonly NexusArenaDbContext _context = context;

        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyHistory()
        {
            try
            {
                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                    return Unauthorized(new { Message = "Invalid Token." }); // 🌟 FIX: IDE1006 (Message)

                var rawBookings = await _context.Bookings
                    .Include(b => b.Resource).ThenInclude(r => r.Arena)
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.BookingDate)
                    .ThenByDescending(b => b.StartTime)
                    .ToListAsync();

                DateOnly today = DateOnly.FromDateTime(DateTime.Now);

                var history = rawBookings.Select(b => {
                    bool isPast = b.BookingDate < today;
                    string currentStatus = b.Status ?? "Confirmed";

                    if (isPast && currentStatus != "Cancelled")
                    {
                        currentStatus = (b.PaymentStatus == "Paid") ? "Completed" : "Expired";
                    }

                    return new
                    {
                        // 🌟 THE FIX: IDE0037 - Member names simplified
                        b.BookingId,
                        ArenaId = b.Resource?.ArenaId ?? 0,
                        ArenaName = b.Resource?.Arena?.Name ?? "Nexus Turf",
                        City = b.Resource?.Arena?.City ?? "Surat",
                        PlayDate = b.BookingDate.ToString("dd MMM yyyy"),
                        TimeSlot = FormatTimeSlot(b.StartTime, b.EndTime, b.BookingMode, b.TournamentPackage),
                        b.TotalAmount,
                        b.AmountPaid,
                        PendingAmount = (b.PaymentMode == "Advance50" && b.PaymentStatus == "Paid")
                                        ? (b.TotalAmount - b.AmountPaid) : 0,
                        PaymentStatus = b.PaymentStatus ?? "Pending",
                        Status = currentStatus,
                        CanCancel = !isPast && currentStatus != "Cancelled" && currentStatus != "Completed" && currentStatus != "Expired"
                    };
                }).ToList();

                // 🌟 THE FIX: IDE1006 - Uppercase Message & Data
                return Ok(new { Message = "Success", Data = history });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error: " + ex.Message });
            }
        }

        // 🌟 THE FIX: CA1822 (Marked as Static) & IDE0071 (Simplified Interpolation)
        private static string FormatTimeSlot(TimeOnly? start, TimeOnly? end, string bookingMode, string? package)
        {
            if (bookingMode == "Tournament") return $"Tournament ({package})";
            if (start == null || end == null) return "N/A";
            return $"{start.Value:hh\\:mm tt} - {end.Value:hh\\:mm tt}";
        }

        [HttpPut("cancel/{bookingId}")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null) return NotFound();
            if (booking.Status == "Cancelled") return BadRequest(new { Message = "Already cancelled." });

            if (booking.BookingDate < DateOnly.FromDateTime(DateTime.Now))
                return BadRequest(new { Message = "Past bookings cannot be cancelled." });

            booking.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Cancelled successfully." });
        }
    }
}