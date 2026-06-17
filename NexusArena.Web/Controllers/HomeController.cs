using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NexusArena.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5092/"); // Apna port verify kar lena
        }

        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var viewModel = new DashboardApiResponse(); // Naya model

            try
            {
                // SAHIL KI API CALL KAR RAHE HAIN
                var response = await _httpClient.GetAsync("api/UserDashboard/widgets");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    viewModel = JsonSerializer.Deserialize<DashboardApiResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Dashboard data load nahi ho paya: " + ex.Message;
            }

            // Agar API fail hui toh null exception na aaye isliye default values
            if (viewModel == null) viewModel = new DashboardApiResponse();
            if (viewModel.stats == null) viewModel.stats = new DashboardStats();
            if (viewModel.upcomingMatches == null) viewModel.upcomingMatches = new List<UpcomingMatchDto>();

            return View(viewModel);
        }
    }

    // --- SAHIL KE JSON KE HISAAB SE NAYE VIEW MODELS ---
    public class DashboardApiResponse
    {
        public string? message { get; set; }
        public DashboardStats? stats { get; set; }
        public List<UpcomingMatchDto>? upcomingMatches { get; set; }
    }

    public class DashboardStats
    {
        public int totalMatchesPlayed { get; set; }
        public int loyaltyPoints { get; set; }
    }

    public class UpcomingMatchDto
    {
        public int bookingId { get; set; }
        public string? arenaName { get; set; }
        public string? sport { get; set; }
        public string? playDate { get; set; }
        public string? startTime { get; set; }
        public string? status { get; set; }
    }
}