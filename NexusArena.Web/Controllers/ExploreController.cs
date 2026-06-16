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
            _httpClient.BaseAddress = new Uri("http://localhost:5092/");
        }

        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("api/Explore/arenas");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var apiResult = JsonSerializer.Deserialize<ExploreApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return View(apiResult?.data ?? new List<ArenaViewModel>());
                    }
                    catch (JsonException) // jsonEx hata diya
                    {
                        ViewBag.Error = $"JSON Format Error: Data convert nahi ho paya. API ne ye bheja tha: {jsonString}";
                        return View(new List<ArenaViewModel>());
                    }
                }
                else
                {
                    ViewBag.Error = $"API Error: Status Code {response.StatusCode}";
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

    // Models me '?' laga diya nullable errors hatane ke liye
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