using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")] // Sirf login player ke liye
    public class BookingHistoryController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public BookingHistoryController(NexusArenaDbContext context)
        {
            _context = context;
        }

        // 1. API: Player ki saari bookings fetch karna (Past + Upcoming)
        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyHistory()
        {
            try
            {
                var userIdString = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
                int userId = int.Parse(userIdString);

                var history = await _context.Bookings
                    .Include(b => b.Resource)
                        .ThenInclude(r => r.Arena)
                    .Include(b => b.Slot)
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.BookingDate) // Sabse latest upar aayegi
                    .Select(b => new
                    {
                        BookingId = b.BookingId,
                        ArenaName = b.Resource.Arena.Name,
                        Sport = b.Resource.ResourceName,
                        PlayDate = b.BookingDate.ToString(),
                        StartTime = b.Slot.StartTime.ToString(),
                        Status = b.Status
                    })
                    .ToListAsync();

                if (!history.Any())
                {
                    return Ok(new { message = "Aapne ab tak koi booking nahi ki hai.", data = history });
                }

                return Ok(new { message = "History fetched successfully", data = history });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 2. API: Booking Cancel karna
        [HttpPut("cancel/{bookingId}")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            try
            {
                var userIdString = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
                int userId = int.Parse(userIdString);

                // Database me check karna ki kya ye booking is player ki hai
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

                if (booking == null)
                {
                    return NotFound(new { message = "Booking nahi mili." });
                }

                if (booking.Status == "Cancelled")
                {
                    return BadRequest(new { message = "Ye booking pehle se cancel ho chuki hai." });
                }

                // Check karna ki booking past ki toh nahi hai (Purani booking cancel nahi ho sakti)
                var today = DateOnly.FromDateTime(DateTime.Today);
                if (booking.BookingDate < today)
                {
                    return BadRequest(new { message = "Aap purani (past) bookings ko cancel nahi kar sakte." });
                }

                // Status update karna
                booking.Status = "Cancelled";
                await _context.SaveChangesAsync();

                return Ok(new { message = "Aapki booking successfully cancel ho gayi hai!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}