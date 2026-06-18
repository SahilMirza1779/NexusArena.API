using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
// using NexusArena.API.Data; // Apne DbContext ka namespace add kar lena

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Owner")] 
    public class OwnerFacilityController : ControllerBase
    {
        // private readonly NexusArenaDbContext _context;
        // public OwnerFacilityController(NexusArenaDbContext context) { _context = context; }

        [HttpPost("AddFacility")]
        public IActionResult AddFacility([FromBody] object model)
        {
            // TODO: EF Core logic to save in database
            // _context.Facilities.Add(model);
            // _context.SaveChanges();

            return Ok(new { Message = "Nayi facility successfully add ho gayi!" });
        }

        [HttpGet("GetAllFacilities")]
        public IActionResult GetAllFacilities()
        {
            // TODO: Yahan database se list fetch karne ka code aayega
            // var list = _context.Facilities.ToList();

            // Abhi testing ke liye dummy data bhej rahe hain
            var dummyList = new List<object>
            {
                new { Id = 1, ResourceName = "Dream Box Cricket 1", ResourceType = "Box Cricket", BasePricePerHour = 1000, Capacity = 12, IsActive = true },
                new { Id = 2, ResourceName = "Pro Pool Table", ResourceType = "Pool", BasePricePerHour = 300, Capacity = 4, IsActive = true }
            };

            return Ok(dummyList);
        }
    }
}