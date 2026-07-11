using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Http;

namespace NexusArena.Web.Controllers
{
    public class ExploreController : Controller
    {
        private readonly HttpClient _httpClient;

        public ExploreController()
        {
            _httpClient = new HttpClient();
            // 🌟 THE FIX: Cleaned the URL, removed the brackets
            _httpClient.BaseAddress = new Uri("http://localhost:5092/");
        }

        public async Task<IActionResult> Index(string? searchTerm, string? sport, string? area, int page = 1)
        {
            var token = Request.Cookies["JWToken"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var apiUrl = $"api/Explore/search?page={page}&pageSize=12";
                if (!string.IsNullOrEmpty(searchTerm)) apiUrl += $"&query={Uri.EscapeDataString(searchTerm)}";
                if (!string.IsNullOrEmpty(area)) apiUrl += $"&query={Uri.EscapeDataString(area)}";
                if (!string.IsNullOrEmpty(sport)) apiUrl += $"&query={Uri.EscapeDataString(sport)}";

                var response = await _httpClient.GetAsync(apiUrl);

                ViewBag.CurrentSearch = searchTerm;
                ViewBag.CurrentArea = area;
                ViewBag.CurrentSport = sport;

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<ExploreApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResult != null)
                    {
                        ViewBag.TotalPages = apiResult.totalPages;
                        ViewBag.CurrentPage = apiResult.currentPage;
                        ViewBag.TotalRecords = apiResult.totalRecords;
                    }

                    return View(apiResult?.data ?? new List<ExploreArenaViewModel>());
                }
                else
                {
                    ViewBag.Error = "No arenas found matching your search.";
                    return View(new List<ExploreArenaViewModel>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Connection Error: {ex.Message}";
            }

            return View(new List<ExploreArenaViewModel>());
        }
    }

    public class ExploreApiResponse
    {
        public bool success { get; set; }
        public int totalRecords { get; set; }
        public int totalPages { get; set; }
        public int currentPage { get; set; }
        public List<ExploreArenaViewModel>? data { get; set; }
    }

    public class ExploreArenaViewModel
    {
        public int arenaId { get; set; }
        public string name { get; set; } = string.Empty;
        public string city { get; set; } = string.Empty;
        public string location { get; set; } = string.Empty;

        public decimal HourlyRegularPrice { get; set; }
        public decimal HourlyPeakPrice { get; set; }
        public List<string> SupportedSports { get; set; } = new List<string>();

        public double averageRating { get; set; }
        public int totalReviews { get; set; }
        public string? ImagePaths { get; set; }
    }
}