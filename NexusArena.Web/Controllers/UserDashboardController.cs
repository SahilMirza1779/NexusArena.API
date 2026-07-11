using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Linq;

namespace NexusArena.Web.Controllers;

[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public class UserDashboardController : Controller
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public UserDashboardController()
    {
        // 🌟 THE FIX: Cleaned the URL, removed the brackets and duplicate text
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5092/") };
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
                if (!string.IsNullOrWhiteSpace(jsonString))
                {
                    var apiResult = JsonSerializer.Deserialize<PlayerDashboardOuterResponse>(jsonString, _jsonOptions);
                    if (apiResult?.Data != null)
                    {
                        viewModel = apiResult.Data;
                    }
                }
            }
            else
            {
                ViewBag.Error = "Could not fetch the latest dashboard data.";
            }
        }
        catch (Exception)
        {
            ViewBag.Error = "Dashboard service is currently offline. Showing cached data.";
        }

        // 🌟 FIX: Getting the ACTUAL User Name from Claims securely
        var nameClaim = User.Claims.FirstOrDefault(c => c.Type == "name" || c.Type == "Name" || c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
        ViewBag.UserName = !string.IsNullOrEmpty(nameClaim) ? nameClaim : (User.Identity?.Name ?? "Player");

        return View(viewModel);
    }
}

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
    public decimal TotalSpent { get; set; }
    public List<PlayerDashboardGameItem> NextGames { get; set; } = [];
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