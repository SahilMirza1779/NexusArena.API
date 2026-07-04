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
            _httpClient.BaseAddress = new Uri("http://localhost:5092/");
        }

        public async Task<IActionResult> Index(string? searchTerm, string? sport, string? area, int page = 1)
        {
            var token = Request.Cookies["JWToken"];
            // Guest mode: Token na bhi ho toh chalega kyunki Explore page API AllowAnonymous hai
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                // API Call with Pagination & Omni-Search filters
                var apiUrl = $"api/Explore/search?page={page}&pageSize=12";
                if (!string.IsNullOrEmpty(searchTerm)) apiUrl += $"&query={Uri.EscapeDataString(searchTerm)}";
                if (!string.IsNullOrEmpty(area)) apiUrl += $"&query={Uri.EscapeDataString(area)}"; // Using same query parameter for area search
                if (!string.IsNullOrEmpty(sport)) apiUrl += $"&query={Uri.EscapeDataString(sport)}"; // Using same query parameter for sport search

                var response = await _httpClient.GetAsync(apiUrl);

                // ViewBags for UI State
                ViewBag.CurrentSearch = searchTerm;
                ViewBag.CurrentArea = area;
                ViewBag.CurrentSport = sport;

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<ExploreApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Pagination info send to View
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

    // 🌟 API MAPPING MODELS (Fixed Exact Casing for CSHTML)
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

        // 🌟 FIX: Capital letters lagaye hain taaki CSHTML se match ho jaye
        public decimal HourlyRegularPrice { get; set; }
        public decimal HourlyPeakPrice { get; set; }
        public List<string> SupportedSports { get; set; } = new List<string>();

        public double averageRating { get; set; }
        public int totalReviews { get; set; }
    }
}