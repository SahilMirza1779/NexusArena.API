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
                ViewBag.Error = "It is necessary to enter both the email and the password!";
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

                    using JsonDocument doc = JsonDocument.Parse(responseData);
                    JsonElement root = doc.RootElement;

                    string token = root.TryGetProperty("token", out JsonElement t1) ? t1.GetString() ?? "" :
                                   (root.TryGetProperty("Token", out JsonElement t2) ? t2.GetString() ?? "" : "");

                    string role = root.TryGetProperty("role", out JsonElement r1) ? r1.GetString() ?? "" :
                                  (root.TryGetProperty("Role", out JsonElement r2) ? r2.GetString() ?? "" : "");

                    if (!string.IsNullOrEmpty(token))
                    {
                        Response.Cookies.Append("JWToken", token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = false,
                            SameSite = SameSiteMode.Lax
                        });
                    }

                    if (role == "SuperAdmin" || role == "1") return RedirectToAction("Index", "SuperAdminDashboard");
                    if (role == "Owner" || role == "2" || role == "Turf Owner") return RedirectToAction("Index", "OwnerDashboard");
                    if (role == "Receptionist" || role == "3") return RedirectToAction("Index", "ReceptionistDashboard");
                    if (role == "User" || role == "4") return RedirectToAction("Index", "Home");

                    ViewBag.Error = "The system did not understand this role.";
                    return View();
                }
                else
                {
                    ViewBag.Error = "Incorrect email or password, or the account is inactive!";
                    return View();
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Could not connect to the API server. Have you run the API project?";
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