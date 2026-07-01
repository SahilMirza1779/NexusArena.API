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

        [HttpGet("get-customers")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var customers = await _context.Users
                    .Where(u => u.Role.RoleName == "Customer")
                    .Select(u => new
                    {
                        id = u.UserId,
                        name = u.FullName,
                        phone = u.Phone
                    })
                    .ToListAsync();

                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
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
                        id = r.ResourceId,
                        name = r.ResourceName,
                        type = r.ResourceType,
                        pricePerHour = r.BasePricePerHour
                    })
                    .ToListAsync();

                return Ok(turfs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("get-available-slots/{resourceId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSlots(int resourceId)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                var slots = await _context.TimeSlots
                    .Where(s => s.ResourceId == resourceId
                            && !_context.Bookings.Any(b => 
                                b.SlotId == s.SlotId 
                                && b.BookingDate == today 
                                && b.Status != "Cancelled"))
                    .Select(s => new
                    {
                        slotId = s.SlotId,
                        startTime = s.StartTime,
                        endTime = s.EndTime,
                        displayTime = $"{s.StartTime} - {s.EndTime}",
                        endTimeDisplay = s.EndTime,
                        available = true
                    })
                    .ToListAsync();

                return Ok(slots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
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
            var reqStart = TimeOnly.FromTimeSpan(request.StartTime);
            var reqEnd = TimeOnly.FromTimeSpan(request.EndTime);

            bool isOverlap = _context.Bookings
                .Include(b => b.Slot)
                .Any(b => b.ResourceId == request.ResourceId
                       && b.BookingDate == request.BookingDate
                       && b.Status != "Cancelled"
                       && b.Slot.StartTime < reqEnd
                       && b.Slot.EndTime > reqStart);

            if (isOverlap)
            {
                return BadRequest("Oops! Someone has just booked this time slot online, or it conflicts with another booking. Please choose a different time.");
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
                    Role = new Role { RoleName = "Customer" }
                };
                _context.Users.Add(user);
                _context.SaveChanges();
            }

            double totalHours = (request.EndTime - request.StartTime).TotalHours;
            if (totalHours <= 0) return BadRequest("The end time must be after the start time!");

            decimal calculatedPrice = (decimal)totalHours * 500;

            var newSlot = new TimeSlot
            {
                StartTime = reqStart,
                EndTime = reqEnd,
                BasePrice = calculatedPrice,
                ResourceId = request.ResourceId
            };
            _context.TimeSlots.Add(newSlot);
            _context.SaveChanges();

            var newBooking = new Booking
            {
                UserId = user.UserId,
                ResourceId = request.ResourceId,
                SlotId = newSlot.SlotId,
                BookingDate = request.BookingDate,
                Status = "Confirmed",
                TotalAmount = calculatedPrice,
                AmountPaid = 0
            };

            _context.Bookings.Add(newBooking);
            _context.SaveChanges();

            return Ok(new { message = "Custom Walk-in booking successfully done!", bookingId = newBooking.BookingId });
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
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; } 
        public int ResourceId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateOnly BookingDate { get; set; }
    }

}