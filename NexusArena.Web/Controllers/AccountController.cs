using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace NexusArena.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccountController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5092/");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email aur Password dono daalna zaroori hai!";
                return View();
            }

            var loginData = new { Email = email, Password = password };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/Auth/Login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(responseData);

                    string? token = result.GetProperty("token").GetString();
                    string? role = result.GetProperty("role").GetString();

                    // Null check lagaya taaki warning na aaye
                    if (!string.IsNullOrEmpty(token))
                    {
                        Response.Cookies.Append("JWToken", token, new CookieOptions { HttpOnly = true, Secure = true });
                    }

                    switch (role)
                    {
                        case "SuperAdmin":
                            return RedirectToAction("Index", "SuperAdminDashboard");
                        case "Owner":
                            return RedirectToAction("Index", "OwnerDashboard");
                        case "Receptionist":
                            return RedirectToAction("Index", "ReceptionistDashboard");
                        case "User":
                            return RedirectToAction("Index", "Home");
                        default:
                            ViewBag.Error = $"System ko ye role samajh nahi aaya: '{role}'";
                            return View();
                    }
                }
                else
                {
                    ViewBag.Error = "Galat Email ya Password! Kripya sahi details daalein.";
                    return View();
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "API Server se connect nahi ho paya. Kya aapne API project run kiya hai?";
                return View();
            }
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("JWToken");
            return RedirectToAction("Login");
        }
    }
}