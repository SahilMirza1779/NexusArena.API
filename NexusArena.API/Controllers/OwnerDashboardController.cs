using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Authorize(Roles = "Owner")]
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerDashboardController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;
        public OwnerDashboardController(NexusArenaDbContext context) => _context = context;

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var currentTime = TimeOnly.FromDateTime(DateTime.UtcNow);

            
            var todayRevenue = await _context.Bookings
                .Where(b => b.BookingDate == todayDate && b.Status != "Cancelled")
                .SelectMany(b => b.Payments)
                .SumAsync(p => p.TotalAmount);

            
            var totalResources = await _context.Resources.CountAsync();
            var activeBookings = await _context.Bookings
                .Include(b => b.Slot)
                .Where(b => b.BookingDate == todayDate &&
                            b.Slot.StartTime <= currentTime &&
                            b.Slot.EndTime >= currentTime &&
                            b.Status == "Confirmed")
                .CountAsync();

            double occupancyPercent = totalResources > 0 ? ((double)activeBookings / totalResources) * 100 : 0;

            
            var topSports = await _context.Bookings
                .Include(b => b.Resource)
                    .ThenInclude(r => r.Category)
                .SelectMany(b => b.Payments, (b, p) => new { b.Resource.Category.Name, p.TotalAmount })
                .GroupBy(x => x.Name)
                .Select(g => new {
                    SportName = g.Key,
                    TotalEarnings = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.TotalEarnings)
                .ToListAsync();

            
            var upcomingBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Resource)
                .Include(b => b.Slot)
                .Where(b => b.BookingDate == todayDate && b.Slot.StartTime > currentTime)
                .OrderBy(b => b.Slot.StartTime)
                .Select(b => new {
                    b.BookingId,
                    CustomerName = b.User.FullName,
                    FacilityName = b.Resource.ResourceName, 
                    TimeSlot = b.Slot.StartTime.ToString() + " - " + b.Slot.EndTime.ToString(),
                    Status = b.Status
                })
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                TodayRevenue = todayRevenue,
                LiveOccupancy = $"{Math.Round(occupancyPercent, 1)}% Booked",
                TopSports = topSports,
                UpcomingBookings = upcomingBookings
            });
        }

        [HttpGet("financial-report")]
        public async Task<IActionResult> GetReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var start = DateOnly.FromDateTime(fromDate);
            var end = DateOnly.FromDateTime(toDate);

            var reportData = await _context.Bookings
                .Include(b => b.Resource)
                .Include(b => b.Payments)
                .Where(b => b.BookingDate >= start && b.BookingDate <= end)
                .Select(b => new {
                    b.BookingId,
                    Date = b.BookingDate,
                    Facility = b.Resource.ResourceName,
                    TotalCollected = b.Payments.Sum(p => p.TotalAmount)
                })
                .ToListAsync();

            return Ok(reportData);
        }
    }
}