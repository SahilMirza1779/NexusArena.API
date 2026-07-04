using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NexusArena.API.Controllers
{
    // 🚨 1. Request model update kiya taaki front-end se data receive ho sake
    public class AddSlotRequest
    {
        public int ResourceId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsPremium { get; set; }

        public string? FestivalName { get; set; }
        public int? DiscountPercent { get; set; }
    }

    [Authorize(Roles = "Owner")]
    [Route("api/[controller]")]
    [ApiController]
    public class TimeSlotController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;
        public TimeSlotController(NexusArenaDbContext context) => _context = context;

        [HttpPost("add")]
        public async Task<IActionResult> AddSlot([FromBody] AddSlotRequest input)
        {
            try
            {
                var slot = new TimeSlot
                {
                    ResourceId = input.ResourceId,
                    StartTime = TimeOnly.FromTimeSpan(input.StartTime),
                    EndTime = TimeOnly.FromTimeSpan(input.EndTime),
                    BasePrice = input.BasePrice,
                    IsPremium = input.IsPremium,

                    // 🚨 2. Database mein save hone ke liye map kiya
                    FestivalName = input.FestivalName,
                    DiscountPercent = input.DiscountPercent
                };

                _context.TimeSlots.Add(slot);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Slot successfully added!" });
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllSlots()
        {
            try
            {
                var slots = await _context.TimeSlots
                    .Include(t => t.Resource)
                    .Select(t => new {
                        t.SlotId,
                        t.ResourceId,
                        ResourceName = t.Resource != null ? t.Resource.ResourceName : "N/A",
                        t.StartTime,
                        t.EndTime,
                        t.BasePrice,
                        t.IsPremium,

                        // 🚨 3. Database se waapas frontend me bhejne ke liye map kiya
                        t.FestivalName,
                        t.DiscountPercent
                    }).ToListAsync();

                return Ok(slots);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}