using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NexusArena.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly HttpClient _httpClient;

        public BookingController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5092/");
        }

        [HttpGet]
        public async Task<IActionResult> Index(int arenaId, string? date = null)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            string selectedDate = string.IsNullOrEmpty(date) ? DateTime.Today.ToString("yyyy-MM-dd") : date;

            ViewBag.ArenaId = arenaId;
            ViewBag.SelectedDate = selectedDate;

            try
            {
                var response = await _httpClient.GetAsync($"api/Booking/available-slots?arenaId={arenaId}&date={selectedDate}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var apiResult = JsonSerializer.Deserialize<SlotApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return View(apiResult?.data ?? new List<SlotViewModel>());
                    }
                    catch (JsonException)
                    {
                        ViewBag.Error = "JSON Format Error.";
                        return View(new List<SlotViewModel>());
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return View(new List<SlotViewModel>());
                }
                else
                {
                    ViewBag.Error = $"API Error: Status Code {response.StatusCode}";
                    return View(new List<SlotViewModel>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Connection Error: {ex.Message}";
            }

            return View(new List<SlotViewModel>());
        }

        [HttpGet]
        public IActionResult Review(int arenaId, string date, int slotId, string startTime, string endTime, decimal price)
        {
            ViewBag.ArenaId = arenaId;
            ViewBag.Date = date;
            ViewBag.SlotId = slotId;
            ViewBag.StartTime = startTime;
            ViewBag.EndTime = endTime;
            ViewBag.Price = price;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(int arenaId, int slotId, string playDate)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var bookingPayload = new
            {
                ResourceId = arenaId,
                SlotId = slotId,
                PlayDate = playDate
            };

            var content = new StringContent(JsonSerializer.Serialize(bookingPayload), System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/Booking/create", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Home"); // Success hone par Dashboard pe jayega
                }
                else
                {
                    TempData["Error"] = "Sorry, booking nahi ho payi. Shayad ye slot book ho chuka hai.";
                    return RedirectToAction("Index", new { arenaId = arenaId, date = playDate });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"API Error: {ex.Message}";
                return RedirectToAction("Index", new { arenaId = arenaId, date = playDate });
            }
        }
    }

    public class SlotApiResponse
    {
        public string? message { get; set; }
        public List<SlotViewModel>? data { get; set; }
    }

    public class SlotViewModel
    {
        public int slotId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("startTime")]
        public string? startTime { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("endTime")]
        public string? endTime { get; set; }

        public decimal price { get; set; }
        public bool isAvailable { get; set; }
    }
}