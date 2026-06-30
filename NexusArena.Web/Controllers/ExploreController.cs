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

        public async Task<IActionResult> Index(string? searchTerm, string? area)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var apiUrl = "api/Explore/arenas?";
                if (!string.IsNullOrEmpty(searchTerm)) apiUrl += $"searchTerm={Uri.EscapeDataString(searchTerm)}&";
                if (!string.IsNullOrEmpty(area)) apiUrl += $"area={Uri.EscapeDataString(area)}";

                var response = await _httpClient.GetAsync(apiUrl);

                ViewBag.CurrentSearch = searchTerm;
                ViewBag.CurrentArea = area;

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<ExploreApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

    // 🌟 THE FIX: Sabkuch lower camelCase kar diya hai
    public class ExploreApiResponse
    {
        public string? message { get; set; }
        public List<ExploreArenaViewModel>? data { get; set; }
    }

    public class ExploreArenaViewModel
    {
        public int arenaId { get; set; }
        public string name { get; set; } = string.Empty;
        public string? location { get; set; }
        public string city { get; set; } = string.Empty;
        public double averageRating { get; set; }
        public int totalReviews { get; set; }
    }
}