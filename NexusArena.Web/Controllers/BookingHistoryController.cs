using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NexusArena.Web.Controllers
{
    public class BookingHistoryController : Controller
    {
        private readonly HttpClient _httpClient;

        public BookingHistoryController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5092/");
        }

        // 1. Saari bookings dikhane ke liye
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var viewModel = new List<BookingHistoryViewModel>();

            try
            {
                var response = await _httpClient.GetAsync("api/BookingHistory/my-history");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<BookingHistoryApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    viewModel = apiResult?.data ?? new List<BookingHistoryViewModel>();
                }
                else
                {
                    ViewBag.Error = "History fetch karne me problem aayi.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Connection Error: {ex.Message}";
            }

            return View(viewModel);
        }

        // 2. Booking Cancel karne ke liye
        [HttpPost]
        public async Task<IActionResult> Cancel(int bookingId)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // API me Cancel ka route PUT hai, isliye PutAsync use kiya
                var response = await _httpClient.PutAsync($"api/BookingHistory/cancel/{bookingId}", null);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Booking successfully cancel ho gayi.";
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = "Booking cancel nahi ho payi.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }

    // --- VIEW MODELS ---
    public class BookingHistoryApiResponse
    {
        public string? message { get; set; }
        public List<BookingHistoryViewModel>? data { get; set; }
    }

    public class BookingHistoryViewModel
    {
        public int bookingId { get; set; }
        public string? arenaName { get; set; }
        public string? sport { get; set; }
        public string? playDate { get; set; }
        public string? startTime { get; set; }
        public string? status { get; set; }
    }
}