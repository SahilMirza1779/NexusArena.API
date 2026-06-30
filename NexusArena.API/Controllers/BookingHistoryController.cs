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
    public class BookingHistoryController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public BookingHistoryController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyHistory()
        {
            try
            {
                var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                    return Unauthorized(new { message = "Invalid Token." });

                var rawBookings = await _context.Bookings
                    .Include(b => b.Resource).ThenInclude(r => r.Arena)
                    .Include(b => b.Slot)
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.BookingDate)
                    .ThenByDescending(b => b.BookingId)
                    .ToListAsync();

                // 🌟 FIX: Aaj ki date nikali
                DateOnly today = DateOnly.FromDateTime(DateTime.Now);

                var history = rawBookings.Select(b => {
                    bool isPast = b.BookingDate < today;
                    string currentStatus = b.Status ?? "Confirmed";

                    // 🌟 FIX: Agar match ki date nikal gayi hai, toh status Completed ya Expired hoga
                    if (isPast && currentStatus != "Cancelled")
                    {
                        currentStatus = (b.PaymentStatus == "Paid") ? "Completed" : "Expired";
                    }

                    return new
                    {
                        BookingId = b.BookingId,
                        ArenaId = b.Resource?.ArenaId ?? 0,
                        ArenaName = b.Resource?.Arena?.Name ?? "Nexus Turf",
                        City = b.Resource?.Arena?.City ?? "Surat",
                        PlayDate = b.BookingDate.ToString("dd MMM yyyy"),
                        TimeSlot = FormatTimeSlot(b.Slot?.StartTime, b.Slot?.EndTime),
                        TotalAmount = b.Slot?.BasePrice ?? 0,
                        AmountPaid = b.AmountPaid,
                        PendingAmount = (b.PaymentMode == "Advance50" && b.PaymentStatus == "Paid")
                                        ? ((b.Slot?.BasePrice ?? 0) - b.AmountPaid) : 0,
                        PaymentStatus = b.PaymentStatus ?? "Pending",
                        Status = currentStatus,
                        // 🌟 FIX: UI ko batao ki yeh cancel ho sakta hai ya nahi
                        CanCancel = !isPast && currentStatus != "Cancelled" && currentStatus != "Completed" && currentStatus != "Expired"
                    };
                }).ToList();

                return Ok(new { message = "Success", data = history });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        private string FormatTimeSlot(TimeOnly? start, TimeOnly? end)
        {
            if (start == null || end == null) return "N/A";
            return $"{start.Value.ToString("hh:mm tt")} - {end.Value.ToString("hh:mm tt")}";
        }

        [HttpPut("cancel/{bookingId}")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var userIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null) return NotFound();
            if (booking.Status == "Cancelled") return BadRequest(new { message = "Already cancelled." });

            // 🌟 FIX: Koi past booking cancel karna chahe API ke through toh rok do
            if (booking.BookingDate < DateOnly.FromDateTime(DateTime.Now))
                return BadRequest(new { message = "Past bookings cannot be cancelled." });

            booking.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cancelled successfully." });
        }
    }
}