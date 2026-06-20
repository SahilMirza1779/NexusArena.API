using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using NexusArena.Web.Models;
using System.IdentityModel.Tokens.Jwt; // <-- Yeh naya namespace add kiya hai

namespace NexusArena.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProfileController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5092/api/"); // Apna API port same rakhna
        }

        // Token se real User ID nikalne ka Helper Method
        private int GetUserIdFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                // API se claim name check karta hai
                var idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id" || c.Type == "nameid");
                return idClaim != null ? int.Parse(idClaim.Value) : 1;
            }
            catch
            {
                return 1; // Fallback agar token read na ho paye
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 🔥 DYNAMIC USER ID FETCH HO RAHI HAI 🔥
            int dynamicUserId = GetUserIdFromToken(token);

            try
            {
                var response = await _httpClient.GetAsync($"Profile/GetUser/{dynamicUserId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var profileData = JsonSerializer.Deserialize<ProfileViewModel>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return View(profileData);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Connection Error: {ex.Message}";
            }

            return View(new ProfileViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // 🔥 DYNAMIC USER ID 🔥
            int dynamicUserId = GetUserIdFromToken(token);

            try
            {
                var updatePayload = new
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                var content = new StringContent(JsonSerializer.Serialize(updatePayload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"Profile/UpdateProfile/{dynamicUserId}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Profile successfully update ho gayi!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Profile update karne me API error aayi.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return View("Index", model);
        }
    }
}