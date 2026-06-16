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
                        // YAHAN CHANGE KIYA HAI: Direct List ki jagah humne nayi 'ExploreApiResponse' class use ki hai
                        var apiResult = JsonSerializer.Deserialize<ExploreApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        // View ko sirf 'data' ke andar wali list bhej rahe hain
                        return View(apiResult?.data ?? new List<ArenaViewModel>());
                    }
                    catch (JsonException jsonEx)
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

    // 1. NAYA WRAPPER CLASS: Jo API ke { message, data } wale structure ko handle karega
    public class ExploreApiResponse
    {
        public string message { get; set; }
        public List<ArenaViewModel> data { get; set; }
    }

    // 2. MODEL UPDATE KIYA: API me description nahi 'city' aa raha hai, toh wahi lagaya
    public class ArenaViewModel
    {
        public int arenaId { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public string city { get; set; }
    }
}