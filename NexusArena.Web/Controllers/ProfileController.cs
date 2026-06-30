using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using NexusArena.Web.Models; // 🌟 Yeh line Model folder se connect karegi

namespace NexusArena.Web.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class ProfileController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProfileController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5092/"); // Make sure ye port API ka ho
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var model = new ProfileViewModel();

            try
            {
                var response = await _httpClient.GetAsync("api/Profile/me");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    model = JsonSerializer.Deserialize<ProfileViewModel>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new ProfileViewModel();
                }
            }
            // 🌟 THE FIX: Yahan humne 'ex.Message' use kar liya, toh ab warning nahi aayegi!
            catch (Exception ex)
            {
                ViewBag.Error = "Could not load profile details. Error: " + ex.Message;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("api/Profile/update", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Your profile details have been updated successfully! 🏆";
            }
            else
            {
                TempData["Error"] = "Failed to update profile details. Please try again.";
            }

            return RedirectToAction("Index");
        }
    }
}