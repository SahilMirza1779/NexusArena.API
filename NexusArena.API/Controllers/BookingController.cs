using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")] // Sirf login kiya hua player hi book kar sakta hai
    public class BookingController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public BookingController(NexusArenaDbContext context)
        {
            _context = context;
        }

        // 1. API: Kisi specific date aur turf ke slots check karna
        [HttpGet("available-slots")]
        public async Task<IActionResult> GetAvailableSlots(int resourceId, string date)
        {
            try
            {
                // String date ko DateOnly me convert karna ("2026-06-15")
                if (!DateOnly.TryParse(date, out DateOnly playDate))
                {
                    return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd." });
                }

                // Us turf (resource) ke saare slots nikalna
                var allSlots = await _context.TimeSlots
                    .Where(ts => ts.ResourceId == resourceId)
                    .ToListAsync();

                if (!allSlots.Any())
                {
                    return NotFound(new { message = "Is turf ke liye koi time slots set nahi hain." });
                }

                // Us din ki saari active bookings nikalna
                var bookedSlotIds = await _context.Bookings
                    .Where(b => b.ResourceId == resourceId && b.BookingDate == playDate && b.Status != "Cancelled")
                    .Select(b => b.SlotId)
                    .ToListAsync();

                // DTO map karna: Har slot ko check karna ki wo book hai ya available
                var availabilityList = allSlots.Select(slot => new SlotAvailabilityDto
                {
                    SlotId = slot.SlotId,
                    StartTime = slot.StartTime.ToString(), // TimeOnly to string
                    EndTime = slot.EndTime.ToString(),
                    Price = slot.BasePrice,
                    IsAvailable = !bookedSlotIds.Contains(slot.SlotId) // Agar booked list me nahi hai, toh available hai
                }).ToList();

                return Ok(new { message = "Slots fetched successfully", data = availabilityList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // 2. API: Nayi Booking Create Karna
        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                // Token se User ID nikalna
                var userIdString = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
                int userId = int.Parse(userIdString);

                if (!DateOnly.TryParse(request.PlayDate, out DateOnly playDate))
                {
                    return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd." });
                }

                // Check: Kya ye slot pehle se book toh nahi ho gaya? (Double Booking Prevention)
                var isAlreadyBooked = await _context.Bookings
                    .AnyAsync(b => b.ResourceId == request.ResourceId
                                && b.SlotId == request.SlotId
                                && b.BookingDate == playDate
                                && b.Status != "Cancelled");

                if (isAlreadyBooked)
                {
                    return BadRequest(new { message = "Sorry, ye slot already book ho chuka hai." });
                }

                // Slot ki details fetch karna bill calculate karne ke liye
                var slotInfo = await _context.TimeSlots.FindAsync(request.SlotId);
                if (slotInfo == null) return NotFound(new { message = "Slot nahi mila." });

                // Nayi Booking database me save karna
                var newBooking = new Booking
                {
                    UserId = userId,
                    ResourceId = request.ResourceId,
                    SlotId = request.SlotId,
                    BookingDate = playDate, // Khelne ki date
                    Status = "Confirmed"
                };

                _context.Bookings.Add(newBooking);
                await _context.SaveChangesAsync();

                // Note: Payments table aur Equipments logic hum aage payment module me add karenge

                return Ok(new { message = "Booking successful!", bookingId = newBooking.BookingId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}