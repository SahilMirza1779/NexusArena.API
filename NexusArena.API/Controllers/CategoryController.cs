using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusArena.API.Models;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class CategoryController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public CategoryController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [HttpPost("add-category")]
        public IActionResult AddCategory([FromBody] CategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "The category name cannot be empty." });

            if (_context.SportCategories.Any(c => c.Name.ToLower() == request.Name.ToLower()))
                return BadRequest(new { message = "This category already exists in the system!" });

            var newCategory = new SportCategory
            {
                Name = request.Name
            };

            _context.SportCategories.Add(newCategory);
            _context.SaveChanges();

            return Ok(new { message = "The new category has been successfully added!", categoryId = newCategory.CategoryId });
        }

        [HttpGet("all-categories")]
        public IActionResult GetAllCategories()
        {
            var categories = _context.SportCategories
                .Select(c => new
                {
                    c.CategoryId,
                    CategoryName = c.Name
                }).ToList();

            return Ok(categories);
        }

        [HttpDelete("delete-category/{categoryId}")]
        public IActionResult DeleteCategory(int categoryId)
        {
            var category = _context.SportCategories.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null) return NotFound(new { message = "Category not found in the database." });

            // Disable karne ki jagah seedha delete kar rahe hain
            _context.SportCategories.Remove(category);
            _context.SaveChanges();

            return Ok(new { message = "The category has been successfully deleted." });
        }
    }

    public class CategoryRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}