using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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

            // 1. Today's Revenue
            var todayRevenue = await _context.Bookings
                .Where(b => b.BookingDate == todayDate && b.Status != "Cancelled")
                .SelectMany(b => b.Payments)
                .SumAsync(p => p.TotalAmount);

            // 2. Live Occupancy
            var totalResources = await _context.Resources.CountAsync();

            // 🌟 FIX: Purana Include(b => b.Slot) hata diya aur direct StartTime/EndTime use kiya
            var activeBookings = await _context.Bookings
                .Where(b => b.BookingDate == todayDate &&
                            b.StartTime <= currentTime &&
                            b.EndTime >= currentTime &&
                            b.Status == "Confirmed")
                .CountAsync();

            double occupancyPercent = totalResources > 0 ? ((double)activeBookings / totalResources) * 100 : 0;

            // 3. Top Sports
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

            // 4. Upcoming Bookings
            // 🌟 FIX: Purana Slot hata kar direct StartTime/EndTime use kiya
            var upcomingBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Resource)
                .Where(b => b.BookingDate == todayDate && b.StartTime > currentTime)
                .OrderBy(b => b.StartTime)
                .Select(b => new {
                    b.BookingId,
                    CustomerName = b.User.FullName,
                    FacilityName = b.Resource.ResourceName,
                    TimeSlot = b.StartTime != null ? b.StartTime.ToString() + " - " + b.EndTime.ToString() : "N/A",
                    Status = b.Status
                })
                .Take(5)
                .ToListAsync();

            // 5. Active Receptionists 
            var activeReceptionists = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleName == "Receptionist" && u.IsActive == true)
                .Select(u => new {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone
                })
                .ToListAsync();

            // Final Response
            return Ok(new
            {
                TodayRevenue = todayRevenue,
                LiveOccupancy = $"{Math.Round(occupancyPercent, 1)}% Booked",
                TopSports = topSports,
                UpcomingBookings = upcomingBookings,
                Receptionists = activeReceptionists
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