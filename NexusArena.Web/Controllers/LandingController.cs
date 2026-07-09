using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using NexusArena.Web.Models;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;
using NexusArena.API.Models;
using System.Net.Http;
using System.Text.Json;
using System.Linq;

namespace NexusArena.Web.Controllers
{
    [AllowAnonymous]
    public class LandingController : Controller
    {
        private readonly NexusArenaDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly HttpClient _httpClient;

        public LandingController(NexusArenaDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5092/") };
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? area, string? sport, string? date)
        {
            var featuredTurfs = new List<ExploreArenaViewModel>();
            var searchResults = new List<ExploreArenaViewModel>();

            try
            {
                var response = await _httpClient.GetAsync("api/Explore/search?page=1&pageSize=10");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<ExploreApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResult != null && apiResult.data != null)
                    {
                        featuredTurfs = apiResult.data.Take(3).ToList();
                    }
                }

                bool isSearchActive = !string.IsNullOrEmpty(area) || !string.IsNullOrEmpty(sport);
                ViewBag.IsSearchActive = isSearchActive;
                ViewBag.SelectedArea = area;
                ViewBag.SelectedSport = sport;
                ViewBag.SelectedDate = date;

                if (isSearchActive)
                {
                    string combinedQuery = $"{area} {sport}".Trim();
                    var searchUrl = $"api/Explore/search?page=1&pageSize=20";

                    if (!string.IsNullOrEmpty(combinedQuery))
                    {
                        searchUrl += $"&query={Uri.EscapeDataString(combinedQuery)}";
                    }

                    var searchResponse = await _httpClient.GetAsync(searchUrl);
                    if (searchResponse.IsSuccessStatusCode)
                    {
                        var searchJson = await searchResponse.Content.ReadAsStringAsync();
                        var searchApiResult = JsonSerializer.Deserialize<ExploreApiResponse>(searchJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (searchApiResult != null && searchApiResult.data != null)
                        {
                            searchResults = searchApiResult.data;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Connection Failed: {ex.Message}";
            }

            ViewBag.SearchResults = searchResults;
            return View(featuredTurfs);
        }

        [HttpGet]
        public IActionResult OwnerApplication()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitApplication(OwnerApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("OwnerApplication", model);
            }

            List<string> uploadedFilePaths = new List<string>();

            if (model.Photos != null && model.Photos.Count > 0)
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "arenas");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                foreach (var file in model.Photos)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    uploadedFilePaths.Add("/uploads/arenas/" + uniqueFileName);
                }
            }

            var pendingApplication = new PendingArena
            {
                OwnerName = model.Name,
                ArenaName = model.ArenaName,
                Email = model.Email,
                Address = model.Address,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                ImagePaths = string.Join(",", uploadedFilePaths),
                Status = "Pending",
                AppliedOn = DateTime.Now
            };

            _context.PendingArenas.Add(pendingApplication);
            await _context.SaveChangesAsync();

            return View("ApplicationSuccess");
        }
    }
}