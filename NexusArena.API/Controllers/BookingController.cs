using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class BookingController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public BookingController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpGet("available-slots")]
        public async Task<IActionResult> GetAvailableSlots(int arenaId, string date)
        {
            try
            {
                if (!DateOnly.TryParse(date, out DateOnly playDate))
                    return BadRequest(new { message = "Invalid date format." });

                var resource = await _context.Resources.FirstOrDefaultAsync(r => r.ArenaId == arenaId);
                if (resource == null) return NotFound(new { message = "Turf resource not found." });

                var allSlots = await _context.TimeSlots
                    .Where(ts => ts.ResourceId == resource.ResourceId)
                    .ToListAsync();

                var bookedSlotIds = await _context.Bookings
                    .Where(b => b.ResourceId == resource.ResourceId && b.BookingDate == playDate && b.Status != "Cancelled")
                    .Select(b => b.SlotId)
                    .ToListAsync();

                var availabilityList = allSlots.Select(slot => new SlotAvailabilityDto
                {
                    SlotId = slot.SlotId,
                    StartTime = slot.StartTime.ToString("hh:mm tt"),
                    EndTime = slot.EndTime.ToString("hh:mm tt"),
                    Price = slot.BasePrice,
                    IsAvailable = !bookedSlotIds.Contains(slot.SlotId)
                }).ToList();

                return Ok(new { message = "Slots fetched", data = availabilityList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                var userIdString = User.FindFirst("UserId")?.Value ?? User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

                int userId = int.Parse(userIdString);

                if (!DateOnly.TryParse(request.PlayDate, out DateOnly playDate))
                    return BadRequest(new { message = "Invalid date format." });

                var resource = await _context.Resources.FirstOrDefaultAsync(r => r.ArenaId == request.ArenaId);
                if (resource == null) return NotFound(new { message = "Turf not found." });

                var isAlreadyBooked = await _context.Bookings
                    .AnyAsync(b => b.ResourceId == resource.ResourceId && b.SlotId == request.SlotId && b.BookingDate == playDate && b.Status != "Cancelled");

                if (isAlreadyBooked) return BadRequest(new { message = "Slot already booked." });

                var newBooking = new Booking
                {
                    UserId = userId,
                    ResourceId = resource.ResourceId,
                    SlotId = request.SlotId,
                    BookingDate = playDate,
                    Status = "Confirmed"
                };

                _context.Bookings.Add(newBooking);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Booking successful!", bookingId = newBooking.BookingId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}