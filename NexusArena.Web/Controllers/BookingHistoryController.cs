using Microsoft.AspNetCore.Mvc;
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

            List<BookingHistoryViewModel> viewModel = new List<BookingHistoryViewModel>();

            try
            {
                var response = await _httpClient.GetAsync("api/BookingHistory/my-history");
                var jsonString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResult = JsonSerializer.Deserialize<BookingHistoryApiResponse>(jsonString, _jsonOptions);
                    viewModel = apiResult?.Data ?? new List<BookingHistoryViewModel>();
                }
                else
                {
                    TempData["Error"] = $"API Error ({response.StatusCode}): {jsonString}";
                }
            }
            catch (JsonException jsonEx)
            {
                TempData["Error"] = $"Data Mapping Error: {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Connection Error: {ex.Message}";
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

    public class BookingHistoryApiResponse
    {
        public string? Message { get; set; }
        public List<BookingHistoryViewModel>? Data { get; set; }
    }

    public class BookingHistoryViewModel
    {
        public int BookingId { get; set; }
        public int ArenaId { get; set; }
        public string? ArenaName { get; set; }
        public string? City { get; set; }
        public string? PlayDate { get; set; }
        public string? TimeSlot { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal PendingAmount { get; set; }
        public string? PaymentStatus { get; set; }
        public string? Status { get; set; }
        public bool CanCancel { get; set; }
        public bool IsRated { get; set; }
    }
}