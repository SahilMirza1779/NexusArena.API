using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    // Swagger me lamba JSON na aaye uske liye sirf 4 zaroori fields wali choti class
    public class AddResourceRequest
    {
        public int ArenaId { get; set; }
        public int CategoryId { get; set; }
        public string ResourceName { get; set; } = string.Empty;
        public int Capacity { get; set; }
    }

    [Authorize(Roles = "Owner")]
    [Route("api/[controller]")]
    [ApiController]
    public class ResourceManagerController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;
        public ResourceManagerController(NexusArenaDbContext context) => _context = context;

        [HttpPost("add-facility")]
        public async Task<IActionResult> AddResource([FromBody] AddResourceRequest input)
        {
            if (input == null) return BadRequest("Invalid data.");

            // Input data ko actual Database Model me map kar rahe hain
            var newResource = new Resource
            {
                ArenaId = input.ArenaId,
                CategoryId = input.CategoryId,
                ResourceName = input.ResourceName,
                Capacity = input.Capacity
            };

            _context.Set<Resource>().Add(newResource);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{newResource.ResourceName} added successfully!" });
        }

        [HttpPut("update-slot-pricing/{slotId}")]
        public async Task<IActionResult> UpdateSlotPricing(int slotId, [FromQuery] decimal basePrice, [FromQuery] bool isPremium)
        {
            var slot = await _context.Set<TimeSlot>().FindAsync(slotId);
            if (slot == null) return NotFound("Time slot not found.");

            slot.BasePrice = basePrice;
            slot.IsPremium = isPremium;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Slot pricing updated successfully!" });
        }
    }
}