using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    // Swagger me lamba JSON na aaye uske liye sirf 5 fields wali choti class
    public class AddEquipmentRequest
    {
        public int ArenaId { get; set; }
        public int CategoryId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal PricePerItem { get; set; }
        public int TotalStock { get; set; }
    }

    [Authorize(Roles = "Owner")]
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public InventoryController(NexusArenaDbContext context) => _context = context;

        [HttpPost("add-equipment")]
        public async Task<IActionResult> AddEquipment([FromBody] AddEquipmentRequest input)
        {
            if (input == null) return BadRequest("Invalid equipment data.");

            // Input data ko tumhare actual Database Model me map kar rahe hain
            var newEquipment = new Equipment
            {
                ArenaId = input.ArenaId,
                CategoryId = input.CategoryId,
                ItemName = input.ItemName,
                PricePerItem = input.PricePerItem,
                TotalStock = input.TotalStock
            };

            _context.Set<Equipment>().Add(newEquipment);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{newEquipment.ItemName} added to inventory successfully!" });
        }

        [HttpPut("update-stock/{id}")]
        public async Task<IActionResult> UpdateStock(int id, [FromQuery] int stock, [FromQuery] decimal pricePerItem)
        {
            var equipment = await _context.Set<Equipment>().FindAsync(id);

            if (equipment == null) return NotFound("Equipment not found.");

            equipment.TotalStock = stock;
            equipment.PricePerItem = pricePerItem;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Inventory stock and pricing updated successfully!" });
        }
    }
}