using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System;
using System.Threading.Tasks;

namespace NexusArena.API.Controllers
{
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
                    ArenaId = 1,
                    CategoryId = 1,
                    ResourceName = input.ResourceName,
                    ResourceType = input.ResourceType,
                    BasePricePerHour = input.BasePricePerHour,
                    Capacity = input.Capacity,
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

        [HttpDelete("Delete/{name}")]
        public async Task<IActionResult> DeleteResource(string name)
        {
            try
            {
                var resource = await _context.Resources.FirstOrDefaultAsync(r => r.ResourceName == name);
                if (resource == null) return NotFound(new { message = "Resource not found" });

                _context.Resources.Remove(resource);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Deleted successfully" });
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("GetByName/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            try
            {
                var resource = await _context.Resources.FirstOrDefaultAsync(r => r.ResourceName == name);
                if (resource == null) return NotFound(new { message = "Resource not found" });

                return Ok(resource);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        // 🚨 NAYA: EDIT/UPDATE LOGIC
        [HttpPut("update/{originalName}")]
        public async Task<IActionResult> UpdateResource(string originalName, [FromBody] AddResourceRequest input)
        {
            try
            {
                var resource = await _context.Resources.FirstOrDefaultAsync(r => r.ResourceName == originalName);
                if (resource == null) return NotFound(new { message = "Resource not found" });

                resource.ResourceName = input.ResourceName;
                resource.ResourceType = input.ResourceType;
                resource.BasePricePerHour = input.BasePricePerHour;
                resource.Capacity = input.Capacity;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Updated successfully" });
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }
    }
}