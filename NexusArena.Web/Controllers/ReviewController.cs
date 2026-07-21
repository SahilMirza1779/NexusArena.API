using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NexusArena.Web.Controllers;

[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public class ReviewController : Controller
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ReviewController()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5092/") };
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var token = Request.Cookies["JWToken"];
        if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var viewModel = new ReviewPageViewModel();

        try
        {
            var reviewResp = await _httpClient.GetAsync("api/Review/my-reviews");
            if (reviewResp.IsSuccessStatusCode)
            {
                var json = await reviewResp.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var data = JsonSerializer.Deserialize<ReviewApiResponse>(json, _jsonOptions);
                    viewModel.MyReviews = data?.Data ?? [];
                }
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Connection Error: " + ex.Message;
        }

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int arenaId, int bookingId, int rating, string comment)
    {
        var token = Request.Cookies["JWToken"];
        if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var reviewData = new { ArenaId = arenaId, BookingId = bookingId, Rating = rating, Comment = comment };
        var content = new StringContent(JsonSerializer.Serialize(reviewData), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/Review/add", content);

        if (response.IsSuccessStatusCode) TempData["Success"] = "Your match review was submitted successfully! ⭐";
        else TempData["Error"] = "Failed to add review.";

        return RedirectToAction("Index", "BookingHistory");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int reviewId, int arenaId, int rating, string comment)
    {
        var token = Request.Cookies["JWToken"];
        if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var reviewData = new { ArenaId = arenaId, Rating = rating, Comment = comment };
        var content = new StringContent(JsonSerializer.Serialize(reviewData), Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"api/Review/update/{reviewId}", content);

        if (response.IsSuccessStatusCode) TempData["Success"] = "Review successfully updated! ✅";
        else TempData["Error"] = "Failed to update review.";

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int reviewId)
    {
        var token = Request.Cookies["JWToken"];
        if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.DeleteAsync($"api/Review/delete/{reviewId}");

        if (response.IsSuccessStatusCode) TempData["Success"] = "Review successfully deleted! 🗑️";
        else TempData["Error"] = "Failed to delete review.";

        return RedirectToAction("Index");
    }
}

public class ReviewPageViewModel
{
    public List<ReviewItemViewModel>? MyReviews { get; set; } = [];
}

public class ReviewApiResponse
{
    [JsonPropertyName("data")]
    public List<ReviewItemViewModel>? Data { get; set; }
}

public class ReviewItemViewModel
{
    [JsonPropertyName("reviewId")] public int ReviewId { get; set; }
    [JsonPropertyName("arenaId")] public int ArenaId { get; set; }
    [JsonPropertyName("arenaName")] public string? ArenaName { get; set; }
    [JsonPropertyName("rating")] public int Rating { get; set; }
    [JsonPropertyName("comment")] public string? Comment { get; set; }
    [JsonPropertyName("date")] public DateTime Date { get; set; }
    [JsonPropertyName("bookingId")] public int? BookingId { get; set; }
}