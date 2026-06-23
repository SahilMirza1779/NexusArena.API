using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using NexusArena.API.Models; 

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerFacilityController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public OwnerFacilityController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpPost("AddFacility")]
        public IActionResult AddFacility([FromBody] Resource model)
        {
            try
            {
                var categoryExists = _context.SportCategories.Any(c => c.CategoryId == model.CategoryId);
                if (!categoryExists)
                {
                    model.CategoryId = _context.SportCategories.FirstOrDefault()?.CategoryId ?? 1;
                }

                model.ArenaId = 1;

                _context.Resources.Add(model);
                _context.SaveChanges();

                return Ok(new { success = true, Message = "The new facility has been successfully added!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, Message = "Database error: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        [HttpGet("GetAllFacilities")]
        public IActionResult GetAllFacilities()
        {
            try
            {
                var realList = _context.Resources.ToList();

                return Ok(realList);
            }
            catch (System.Exception)
            {
                return BadRequest(new { success = false, Message = "There was a problem fetching the data." });
            }
        }
    }
}