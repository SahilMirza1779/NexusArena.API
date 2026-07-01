using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System;

namespace NexusArena.Web.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class UserDashboardController : Controller
    {
        private readonly HttpClient _httpClient;

        public UserDashboardController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5092/"); // API base port
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var viewModel = new PlayerDashboardMainViewModel();

            try
            {
                var response = await _httpClient.GetAsync("api/UserDashboard/stats");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<PlayerDashboardOuterResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (apiResult?.Data != null)
                    {
                        viewModel = apiResult.Data;
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Dashboard connection error: " + ex.Message;
            }

            return View(viewModel);
        }
    }

    // 🌟 UNIQUE DTOs TO PREVENT AMBIGUITY COMPILATION ERROR
    public class PlayerDashboardOuterResponse
    {
        public string? Message { get; set; }
        public PlayerDashboardMainViewModel? Data { get; set; }
    }

    public class PlayerDashboardMainViewModel
    {
        public int TotalMatches { get; set; }
        public int UpcomingMatches { get; set; }
        public int LoyaltyPoints { get; set; }
        public List<PlayerDashboardGameItem>? NextGames { get; set; }
    }

    public class PlayerDashboardGameItem
    {
        public int BookingId { get; set; }
        public string? ArenaName { get; set; }
        public string? PlayDate { get; set; }
        public string? TimeSlot { get; set; }
        public string? Status { get; set; }
        public string? TargetDateTime { get; set; }
    }
}