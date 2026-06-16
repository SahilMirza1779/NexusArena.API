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

        // Ye action arenaId (Turf ka ID) aur date lega. string? ka matlab hai ye null bhi ho sakta hai.
        public async Task<IActionResult> Index(int arenaId, string? date = null)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Agar user ne koi date select nahi ki hai, toh aaj ki date (Today) set kar do
            string selectedDate = string.IsNullOrEmpty(date) ? DateTime.Today.ToString("yyyy-MM-dd") : date;

            // View me data bhejne ke liye ViewBag use kar rahe hain
            ViewBag.ArenaId = arenaId;
            ViewBag.SelectedDate = selectedDate;

            try
            {
                // Query parameter banakar API call kar rahe hain
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
                        // JSON fail hone par error catch karega
                        ViewBag.Error = $"JSON Format Error: Data convert nahi ho paya. API ne ye bheja tha: {jsonString}";
                        return View(new List<SlotViewModel>());
                    }
                }
                // Agar 404 Not Found aaye toh error mat dikhao, bas empty list bhej do
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
    }

    // Wrapper Class API Response ke liye (Nullable warnings hatane ke liye '?' lagaya hai)
    public class SlotApiResponse
    {
        public string? message { get; set; }
        public List<SlotViewModel>? data { get; set; }
    }

    // Har ek slot ki details
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