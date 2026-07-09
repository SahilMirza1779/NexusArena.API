using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexusArena.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExploreController : ControllerBase
    {
        private readonly NexusArenaDbContext _context;

        public ExploreController(NexusArenaDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<IActionResult> SearchTurfs(
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var totalInDb = await _context.Arenas.CountAsync();
                Console.WriteLine($"DEBUG: Total Arenas in DB: {totalInDb}");

                var arenasQuery = _context.Arenas
                    .Include(a => a.Resources)
                        .ThenInclude(r => r.Category)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    var search = query.ToLower().Trim();
                    arenasQuery = arenasQuery.Where(a =>
                        a.Name.ToLower().Contains(search) ||
                        (a.Location != null && a.Location.ToLower().Contains(search)) ||
                        a.City.ToLower().Contains(search) ||
                        a.Resources.Any(r => r.Category != null && r.Category.Name.ToLower().Contains(search))
                    );
                }

                var totalRecords = await arenasQuery.CountAsync();
                Console.WriteLine($"DEBUG: Arenas after filter: {totalRecords}");

                var arenas = await arenasQuery
                    .OrderBy(a => a.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new ArenaExploreDto
                    {
                        ArenaId = a.ArenaId,
                        Name = a.Name,
                        City = a.City,
                        Location = a.Location ?? "Not Specified",
                        HourlyRegularPrice = a.HourlyRegularPrice,
                        HourlyPeakPrice = a.HourlyPeakPrice,

                        ImagePaths = null,

                        SupportedSports = a.Resources
                                            .Where(r => r.Category != null)
                                            .Select(r => r.Category.Name)
                                            .Distinct()
                                            .ToList()
                    })
                    .ToListAsync();

                Console.WriteLine($"DEBUG: Arenas sent to Frontend: {arenas.Count}");

                return Ok(new ExploreResponseDto
                {
                    Success = true,
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                    CurrentPage = page,
                    Data = arenas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class ArenaExploreDto
    {
        public int ArenaId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal HourlyRegularPrice { get; set; }
        public decimal HourlyPeakPrice { get; set; }
        public string? ImagePaths { get; set; }
        public List<string> SupportedSports { get; set; } = new List<string>();
    }

    public class ExploreResponseDto
    {
        public bool Success { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public List<ArenaExploreDto> Data { get; set; } = new List<ArenaExploreDto>();
    }
}