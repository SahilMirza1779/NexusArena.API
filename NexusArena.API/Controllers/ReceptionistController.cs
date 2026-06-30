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
    [Authorize(Roles = "Receptionist")]
    public class ReceptionistController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public ReceptionistController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetLiveDashboard")]
        public async Task<IActionResult> GetLiveDashboard()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                var todaysBookings = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Resource)
                    .Include(b => b.Slot)
                    .Where(b => b.BookingDate == today
                             && b.Status != "Cancelled"
                             && b.Status != "Completed")
                    .ToListAsync();

                var totalPendingCash = todaysBookings.Sum(b => b.TotalAmount - b.AmountPaid);

                int checkedInCount = todaysBookings.Count(b => b.Status == "CheckedIn");
                int totalTurfs = await _context.Resources.CountAsync();

                var liveBookingsList = todaysBookings.Select(b => new
                {
                    BookingId = b.BookingId,
                    CustomerName = b.User?.FullName ?? "Walk-in Customer",
                    TurfName = b.Resource?.ResourceName ?? "Unknown Turf",
                    TimeSlot = b.Slot != null ? $"{b.Slot.StartTime} - {b.Slot.EndTime}" : "N/A",
                    PendingAmount = Math.Max(0, b.TotalAmount - b.AmountPaid),
                    IsTimeUpWarning = false
                }).ToList();

                var response = new
                {
                    TotalPendingCash = totalPendingCash,
                    TodayBookingsCount = todaysBookings.Count,
                    AvailableTurfsCount = totalTurfs - checkedInCount,
                    LiveBookings = liveBookingsList
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Server Error: {ex.Message}" });
            }
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
            var slot = _context.TimeSlots.FirstOrDefault(s => s.SlotId == request.SlotId);

            decimal actualPrice = slot != null ? slot.BasePrice : 0;

            var newBooking = new Booking
            {
                UserId = request.CustomerId,
                ResourceId = request.ResourceId,
                SlotId = request.SlotId,
                BookingDate = request.BookingDate,
                Status = "Confirmed",
                TotalAmount = actualPrice,
                AmountPaid = 0
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

            booking.Status = newStatus;
            _context.SaveChanges();

            return Ok(new { message = $"Booking ka status ab '{newStatus}' ho gaya hai." });
        }

        [HttpGet("booking-history")]
        public async Task<IActionResult> GetBookingHistory()
        {
            try
            {
                var history = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Resource)
                    .Include(b => b.Slot)
                    .OrderByDescending(b => b.BookingDate) 
                    .Select(b => new {
                        BookingId = b.BookingId,
                        CustomerName = b.User != null ? b.User.FullName : "Walk-in",
                        TurfName = b.Resource != null ? b.Resource.ResourceName : "-",
                        BookingDate = b.BookingDate,
                        TimeSlot = b.Slot != null ? $"{b.Slot.StartTime} - {b.Slot.EndTime}" : "-",
                        Status = b.Status,
                        TotalAmount = b.TotalAmount,
                        AmountPaid = b.AmountPaid
                    })
                    .ToListAsync();

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Server Error: {ex.Message}" });
            }
        }

        [HttpPut("collect-payment/{bookingId}")]
        public IActionResult CollectPayment(int bookingId)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return NotFound(new { message = "The booking was not found in the database." });
            }

            booking.AmountPaid = booking.TotalAmount;
            _context.SaveChanges();

            return Ok(new { message = $"Payment collected successfully for Booking #{bookingId}" });
        }

        [HttpGet("available-turfs")]
        public async Task<IActionResult> GetAvailableTurfs()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var now = TimeOnly.FromDateTime(DateTime.Now);

            var activeBookings = await _context.Bookings
                .Where(b => b.BookingDate == today && b.Status == "CheckedIn")
                .Select(b => b.ResourceId)
                .ToListAsync();

            var available = await _context.Resources
                .Where(r => !activeBookings.Contains(r.ResourceId) && r.IsActive == true)
                .Select(r => new { r.ResourceId, r.ResourceName, r.ResourceType, r.Capacity })
                .ToListAsync();

            return Ok(available);
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