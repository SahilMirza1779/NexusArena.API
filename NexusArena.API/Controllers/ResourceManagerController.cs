using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    public class AddResourceRequest
    {
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

        [HttpPost("add")]
        public async Task<IActionResult> AddResource([FromBody] AddResourceRequest input)
        {
            try
            {
                var newResource = new Resource
                {
                    ArenaId = 1,
                    CategoryId = 1,
                    ResourceName = input.ResourceName,
                    Capacity = input.Capacity
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