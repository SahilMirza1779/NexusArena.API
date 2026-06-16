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
            // Yahan API ka Base URL hai (Swagger wala port check kar lein)
            _httpClient.BaseAddress = new Uri("http://localhost:5092/");
        }

        public async Task<IActionResult> Index()
        {
            // 1. Browser ki cookie se JWT token nikalna
            var token = Request.Cookies["JWToken"];

            // Agar token nahi mila, toh wapas Login page par bhej do
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Request header me Token ko "Bearer" format me add karna
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // 3. User Dashboard API ko call karna
                var response = await _httpClient.GetAsync("api/UserDashboard/widgets");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    // JSON string ko C# object me convert karna
                    var dashboardData = JsonSerializer.Deserialize<DashboardApiResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return View(dashboardData); // View ko asli data bhej diya
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Agar token expire ho gaya toh wapas login par bhej do
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception)
            {
                // Agar API band hui toh ye chalega
                ViewBag.Error = "Backend API se connect nahi ho paya.";
            }

            return View(new DashboardApiResponse());
        }
    }

    // --- Ye classes API se aane wale data ko hold karne ke liye hain ---
    public class DashboardApiResponse
    {
        public string message { get; set; }
        public DashboardStats stats { get; set; } = new DashboardStats();
        public List<UpcomingMatch> upcomingMatches { get; set; } = new List<UpcomingMatch>();
    }

    public class DashboardStats
    {
        public int totalMatchesPlayed { get; set; }
        public int loyaltyPoints { get; set; }
    }

    public class UpcomingMatch
    {
        public int bookingId { get; set; }
        public string arenaName { get; set; }
        public string sport { get; set; }
        public string playDate { get; set; }
        public string startTime { get; set; }
        public string status { get; set; }
    }
}