using Microsoft.AspNetCore.Mvc;
using NexusArena.API.Models;
using NexusArena.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace NexusArena.Web.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class BookingHistoryController : Controller
    {
        private readonly HttpClient _httpClient;

        // 🌟 THE FIX: CA1869 - Cache JsonSerializerOptions taaki memory bache
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public BookingHistoryController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5092/");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 🌟 THE FIX: IDE0028 - Naya C# 12 empty collection syntax
            List<BookingHistoryViewModel> viewModel = [];

            try
            {
                var response = await _httpClient.GetAsync("api/BookingHistory/my-history");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<BookingHistoryApiResponse>(jsonString, _jsonOptions);
                    viewModel = apiResult?.Data ?? []; // 🌟 THE FIX: Use uppercase Data & []
                }
                else
                {
                    ViewBag.Error = "Failed to fetch booking history.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Connection Error: {ex.Message}";
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int bookingId)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.PutAsync($"api/BookingHistory/cancel/{bookingId}", null);
                if (response.IsSuccessStatusCode)
                    TempData["Success"] = "Your booking has been successfully cancelled.";
                else
                    TempData["Error"] = "Failed to cancel the booking. It might be too late or already processed.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Ticket(int id)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("api/BookingHistory/my-history");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<BookingHistoryApiResponse>(jsonString, _jsonOptions);

                    var ticketData = apiResult?.Data?.FirstOrDefault(b => b.BookingId == id);
                    if (ticketData == null) return NotFound("Ticket details not found in your account!");

                    return View(ticketData);
                }
            }
            catch (Exception ex)
            {
                return Content($"System Error Loading Ticket: {ex.Message}");
            }

            return RedirectToAction("Index");
        }
    }

    // 🌟 THE FIX: IDE1006 - Naming Rules for JSON deserialization object
    public class BookingHistoryApiResponse
    {
        public string? Message { get; set; }
        public List<BookingHistoryViewModel>? Data { get; set; }
    }
}