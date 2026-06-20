using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NexusArena.Web.Controllers
{
    public class ExploreController : Controller
    {
        private readonly HttpClient _httpClient;

        public ExploreController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5092/"); // API ka port
        }

        // Search aur Area filters ke sath complete Index method
        public async Task<IActionResult> Index(string? searchTerm, string? area)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // API ka dynamic URL ban raha hai
                var apiUrl = "api/Explore/arenas?";
                if (!string.IsNullOrEmpty(searchTerm)) apiUrl += $"searchTerm={Uri.EscapeDataString(searchTerm)}&";
                if (!string.IsNullOrEmpty(area)) apiUrl += $"area={Uri.EscapeDataString(area)}";

                var response = await _httpClient.GetAsync(apiUrl);

                // UI form mein value wapas dikhane ke liye
                ViewBag.CurrentSearch = searchTerm;
                ViewBag.CurrentArea = area;

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var apiResult = JsonSerializer.Deserialize<ExploreApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return View(apiResult?.data ?? new List<ArenaViewModel>());
                    }
                    catch (JsonException)
                    {
                        ViewBag.Error = "JSON Format Error: Data convert nahi ho paya.";
                        return View(new List<ArenaViewModel>());
                    }
                }
                else
                {
                    ViewBag.Error = "No arenas found matching your search.";
                    return View(new List<ArenaViewModel>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Connection Error: {ex.Message}";
            }

            return View(new List<ArenaViewModel>());
        }
    }

    // API ka response map karne ke liye Models
    public class ExploreApiResponse
    {
        public string? message { get; set; }
        public List<ArenaViewModel>? data { get; set; }
    }

    public class ArenaViewModel
    {
        public int arenaId { get; set; }
        public string? name { get; set; }
        public string? location { get; set; }
        public string? city { get; set; }
    }
}