using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System;
using System.Linq;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Receptionist")] 
    public class ReceptionistController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public ReceptionistController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpGet("todays-bookings/{arenaId}")]
        public IActionResult GetTodaysBookings(int arenaId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var bookings = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Resource) 
                .Where(b => b.Resource.ArenaId == arenaId && b.BookingDate == today)
                .Select(b => new
                {
                    b.BookingId,
                    CustomerName = b.User.FullName,
                    ResourceName = b.Resource.ResourceName,
                    b.BookingDate,
                    b.Status
                }).ToList();

            return Ok(bookings);
        }

        [HttpPost("walk-in-booking")]
        public IActionResult WalkInBooking([FromBody] WalkInBookingRequest request)
        {
            var newBooking = new Booking
            {
                UserId = request.CustomerId,
                ResourceId = request.ResourceId, 
                SlotId = request.SlotId,
                BookingDate = request.BookingDate,
                Status = "Confirmed" 
            };

            _context.Bookings.Add(newBooking);
            _context.SaveChanges();

            return Ok(new { message = "Walk-in booking successfully done!", bookingId = newBooking.BookingId });
        }

        [HttpPut("update-status/{bookingId}")]
        public IActionResult UpdateStatus(int bookingId, [FromBody] string newStatus)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return NotFound(new { message = "Booking database me nahi mili." });
            }

            // Status update karega (Jaise: "Completed")
            booking.Status = newStatus;
            _context.SaveChanges();

            return Ok(new { message = $"Booking ka status ab '{newStatus}' ho gaya hai." });
        }
    }

    public class WalkInBookingRequest
    {
        public int CustomerId { get; set; }
        public int ResourceId { get; set; }
        public int SlotId { get; set; }
        public DateOnly BookingDate { get; set; }
    }
}