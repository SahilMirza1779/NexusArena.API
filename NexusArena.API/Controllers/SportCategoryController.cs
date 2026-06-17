using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SportCategoryController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public SportCategoryController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.SportCategories
                .Select(c => new
                {
                    Id = c.CategoryId,
                    Name = c.Name,
                    Icon = c.Icon
                })
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _context.SportCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = " Category not found! " });
            }
            return Ok(new
            {
                Id = category.CategoryId,
                Name = category.Name,
                Icon = category.Icon
            });
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateCategory([FromBody] SportCategory newCategory)
        {
            if (string.IsNullOrEmpty(newCategory.Name) || string.IsNullOrEmpty(newCategory.Icon))
            {
                return BadRequest(new { message = " Both the name and the icon are essential! " });
            }

            _context.SportCategories.Add(newCategory);
            await _context.SaveChangesAsync();

            return Ok(new { message = " Category Successfully added! " });
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] SportCategory updateCategory)
        {
            var existingCategory = await _context.SportCategories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound(new { message = " Category not found! " });
            }

            existingCategory.Name = updateCategory.Name;
            existingCategory.Icon = updateCategory.Icon;
            await _context.SaveChangesAsync();

            return Ok(new { message = " Category Successfully Added! " });
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.SportCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = " Category not found! " });
            }

            _context.SportCategories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = " Category Deleted! " });
        }
    }
}
