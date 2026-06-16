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
            // Yahan hum aapki API ka URL de rahe hain
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
            // 1. Agar field khali hai toh error dikhao
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email aur Password dono daalna zaroori hai!";
                return View();
            }

            // 2. Data ko JSON me convert karna API ke liye
            var loginData = new { Email = email, Password = password };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            try
            {
                // 3. API par POST request bhejna
                var response = await _httpClient.PostAsync("api/Auth/Login", content);

                if (response.IsSuccessStatusCode)
                {
                    // 4. Response se Token aur Role nikalna
                    var responseData = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(responseData);

                    string token = result.GetProperty("token").GetString();
                    string role = result.GetProperty("role").GetString();

                    // 5. Token ko Browser ki Cookie me save karna (Aage dashboard me kaam aayega)
                    Response.Cookies.Append("JWToken", token, new CookieOptions { HttpOnly = true, Secure = true });

                    // 6. Role ke hisaab se Dashboard par bhejna
                    switch (role)
                    {
                        case "SuperAdmin":
                            return RedirectToAction("Index", "SuperAdminDashboard");
                        case "Owner":
                            return RedirectToAction("Index", "OwnerDashboard");
                        case "Receptionist":
                            return RedirectToAction("Index", "ReceptionistDashboard");
                        case "User":
                            return RedirectToAction("Index", "Home"); // Aapka naya Player Dashboard
                        default:
                            ViewBag.Error = $"System ko ye role samajh nahi aaya: '{role}'";
                            return View();
                    }
                }
                else
                {
                    // Agar Email ya Password database me match nahi hua
                    ViewBag.Error = "Galat Email ya Password! Kripya sahi details daalein.";
                    return View();
                }
            }
            catch (Exception)
            {
                // Agar API wala project chalu nahi hai
                ViewBag.Error = "API Server se connect nahi ho paya. Kya aapne API project run kiya hai?";
                return View();
            }
        }

        public IActionResult Logout()
        {
            // Logout par cookie delete kar dena
            Response.Cookies.Delete("JWToken");
            return RedirectToAction("Login");
        }
    }
}