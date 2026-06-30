using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    // Yahan humne API ko bata diya ki frontend se ab kya-kya aayega
    public class AddResourceRequest
    {
        public string ResourceName { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
        public decimal BasePricePerHour { get; set; }
        public int Capacity { get; set; }
    }

    [Authorize(Roles = "Owner")]
    [Route("api/[controller]")]
    [ApiController]
    public class ResourceManagerController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;
        public ResourceManagerController(NexusArenaDbContext context) => _context = context;

        [HttpPost("add")]
        public async Task<IActionResult> AddResource([FromBody] AddResourceRequest input)
        {
            try
            {
                var newResource = new Resource
                {
                    ArenaId = 1, // Abhi ke liye hardcoded
                    CategoryId = 1, // Abhi ke liye hardcoded
                    ResourceName = input.ResourceName,

                    // Naye fields jo ab seedha database mein jayenge
                    ResourceType = input.ResourceType,
                    BasePricePerHour = input.BasePricePerHour,
                    Capacity = input.Capacity,

                    // Naya facility add karte hi default active rahega
                    IsActive = true
                };

                _context.Resources.Add(newResource);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Success" });
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("GetAllFacilities")]
        public async Task<IActionResult> GetAllFacilities()
        {
            return Ok(await _context.Resources.ToListAsync());
        }
    }
}