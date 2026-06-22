using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
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
                var userIdString = User.FindFirst("UserId")?.Value ?? User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

                int userId = int.Parse(userIdString);

                var history = await _context.Bookings
                    .Include(b => b.Resource)
                    .Include(b => b.Slot)
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.BookingDate)
                    .Select(b => new
                    {
                        bookingId = b.BookingId,
                        arenaName = "Arena #" + b.Resource.ArenaId,
                        sport = "Turf / Ground",
                        playDate = b.BookingDate.ToString("dd MMM yyyy"),
                        startTime = b.Slot.StartTime.ToString("hh:mm tt") + " - " + b.Slot.EndTime.ToString("hh:mm tt"),
                        status = b.Status
                    }).ToListAsync();

                return Ok(new { message = "History fetched", data = history });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("cancel/{bookingId}")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            try
            {
                var userIdString = User.FindFirst("UserId")?.Value ?? User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
                int userId = int.Parse(userIdString);

                var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

                if (booking == null) return NotFound(new { message = "Booking nahi mili." });
                if (booking.Status == "Cancelled") return BadRequest(new { message = "Booking pehle se cancel ho chuki hai." });

                booking.Status = "Cancelled";
                await _context.SaveChangesAsync();

                return Ok(new { message = "Booking successfully cancelled." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}