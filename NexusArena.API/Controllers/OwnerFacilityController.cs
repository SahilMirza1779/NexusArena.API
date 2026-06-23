using Microsoft.AspNetCore.Mvc;
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
        public IActionResult AddFacility([FromBody] Resource formData)
        {
            try
            {
                formData.ArenaId = 1;

                formData.CategoryId = 2;

                _context.Resources.Add(formData);
                _context.SaveChanges();

                return Ok(new { success = true, Message = "The new facility has been successfully added!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.InnerException?.Message ?? ex.Message });
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
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = "Data fetch fail: " + ex.Message });
            }
        }
    }
}