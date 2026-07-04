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
    public class ReceptionistController(NexusArenaDbContext context) : ControllerBase
    {
        private readonly NexusArenaDbContext _context = context;

        [HttpGet("GetLiveDashboard")]
        public async Task<IActionResult> GetLiveDashboard()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                var todaysBookings = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Resource)
                    .Where(b => b.BookingDate == today
                             && b.Status != "Cancelled"
                             && b.Status != "Completed")
                    .ToListAsync();

                var totalPendingCash = todaysBookings.Sum(b => b.TotalAmount - b.AmountPaid);

                int checkedInCount = todaysBookings.Count(b => b.Status == "CheckedIn");
                int totalTurfs = await _context.Resources.CountAsync();

                var liveBookingsList = todaysBookings.Select(b => new
                {
                    b.BookingId,
                    CustomerName = b.User?.FullName ?? "Walk-in Customer",
                    TurfName = b.Resource?.ResourceName ?? "Unknown Turf",
                    TimeSlot = b.StartTime != null && b.EndTime != null ? $"{b.StartTime} - {b.EndTime}" : "N/A",
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
                return StatusCode(500, new { Message = $"Server Error: {ex.Message}" });
            }
        }

        [HttpGet("get-customers")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var customers = await _context.Users
                    .Where(u => u.Role != null && u.Role.RoleName == "Customer")
                    .Select(u => new
                    {
                        Id = u.UserId,
                        Name = u.FullName,
                        u.Phone
                    })
                    .ToListAsync();

                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("get-turfs")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTurfs()
        {
            try
            {
                var turfs = await _context.Resources
                    .Select(r => new
                    {
                        Id = r.ResourceId,
                        Name = r.ResourceName,
                        Type = r.ResourceType,
                        PricePerHour = r.BasePricePerHour
                    })
                    .ToListAsync();

                return Ok(turfs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("get-available-slots/{resourceId}")]
        [AllowAnonymous]
        public IActionResult GetAvailableSlots(int resourceId)
        {
            _ = resourceId; // Warning hatane ke liye discard kiya
            return Ok(new[] { new { Message = "Dynamic slots enabled. Please select start and end time." } });
        }

        [HttpGet("todays-bookings/{arenaId}")]
        public IActionResult GetTodaysBookings(int arenaId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var bookings = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Resource)
                .Where(b => b.Resource != null && b.Resource.ArenaId == arenaId && b.BookingDate == today)
                .Select(b => new
                {
                    b.BookingId,
                    CustomerName = b.User!.FullName,
                    b.Resource!.ResourceName,
                    b.BookingDate,
                    b.Status
                }).ToList();

            return Ok(bookings);
        }

        [HttpPost("walk-in-booking")]
        public IActionResult WalkInBooking([FromBody] WalkInBookingRequest request)
        {
            var reqStart = TimeOnly.FromTimeSpan(request.StartTime);
            var reqEnd = TimeOnly.FromTimeSpan(request.EndTime);

            // 🌟 THE FIX: Hussain ka SlotId wala kachra hata kar tera naya StartTime/EndTime lagaya
            bool isOverlap = _context.Bookings
                .Any(b => b.ResourceId == request.ResourceId
                       && b.BookingDate == request.BookingDate
                       && b.Status != "Cancelled"
                       && b.StartTime < reqEnd
                       && b.EndTime > reqStart);

            if (isOverlap)
            {
                return BadRequest(new { Message = "Oops! Someone has just booked this time slot online, or it conflicts with another booking. Please choose a different time." });
            }

            var user = _context.Users.FirstOrDefault(u => u.Phone == request.CustomerPhone);
            if (user == null)
            {
                user = new User
                {
                    FullName = request.CustomerName,
                    Phone = request.CustomerPhone,
                    Email = $"walkin_{DateTime.Now.Ticks}@nexus.com",
                    PasswordHash = "WalkIn123!",
                    RoleId = _context.Roles.FirstOrDefault(r => r.RoleName == "Customer")?.RoleId ?? 2
                };
                _context.Users.Add(user);
                _context.SaveChanges();
            }

            double totalHours = (request.EndTime - request.StartTime).TotalHours;
            if (totalHours <= 0) return BadRequest(new { Message = "The end time must be after the start time!" });

            decimal calculatedPrice = (decimal)totalHours * 500;

            var newBooking = new Booking
            {
                UserId = user.UserId,
                ResourceId = request.ResourceId,
                BookingDate = request.BookingDate,
                StartTime = reqStart,
                EndTime = reqEnd,
                Status = "Confirmed",
                TotalAmount = calculatedPrice,
                AmountPaid = 0,
                BookingMode = "Hourly"
            };

            _context.Bookings.Add(newBooking);
            _context.SaveChanges();

            return Ok(new { Message = "Custom Walk-in booking successfully done!", BookingId = newBooking.BookingId });
        }

        [HttpPut("update-status/{bookingId}")]
        public IActionResult UpdateStatus(int bookingId, [FromBody] string newStatus)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return NotFound(new { Message = "Booking database me nahi mili." });
            }

            booking.Status = newStatus;
            _context.SaveChanges();

            return Ok(new { Message = $"Booking ka status ab '{newStatus}' ho gaya hai." });
        }

        [HttpGet("booking-history")]
        public async Task<IActionResult> GetBookingHistory()
        {
            try
            {
                var history = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Resource)
                    .OrderByDescending(b => b.BookingDate)
                    .Select(b => new {
                        b.BookingId,
                        CustomerName = b.User != null ? b.User.FullName : "Walk-in",
                        TurfName = b.Resource != null ? b.Resource.ResourceName : "-",
                        b.BookingDate,
                        TimeSlot = b.StartTime != null && b.EndTime != null ? $"{b.StartTime} - {b.EndTime}" : "-",
                        b.Status,
                        b.TotalAmount,
                        b.AmountPaid
                    })
                    .ToListAsync();

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Server Error: {ex.Message}" });
            }
        }

        [HttpPut("collect-payment/{bookingId}")]
        public IActionResult CollectPayment(int bookingId)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return NotFound(new { Message = "The booking was not found in the database." });
            }

            booking.AmountPaid = booking.TotalAmount;
            _context.SaveChanges();

            return Ok(new { Message = $"Payment collected successfully for Booking #{bookingId}" });
        }

        [HttpGet("available-turfs")]
        public async Task<IActionResult> GetAvailableTurfs()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

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
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public int ResourceId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateOnly BookingDate { get; set; }
    }
}