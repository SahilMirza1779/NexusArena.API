using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticAssets;
using System.Text.Json;
using System.Net.Http;
using NexusArena.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using NexusArena.API.Models;
using System.Net.Mail;
using System.Net;

namespace NexusArena.Web.Controllers
{
    public class SuperAdminDashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NexusArenaDbContext _context;

        public SuperAdminDashboardController(IHttpClientFactory httpClientFactory, NexusArenaDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var dashboardData = new DashboardStatsViewModel();
            dashboardData.PendingApprovals = new List<PendingArenaViewModel>();

            string statsApiUrl = "http://localhost:5092/api/Dashboard/Stats";

            try
            {
                HttpResponseMessage statsResponse = await client.GetAsync(statsApiUrl);
                if (statsResponse.IsSuccessStatusCode)
                {
                    string jsonData = await statsResponse.Content.ReadAsStringAsync();
                    var parsedData = JsonSerializer.Deserialize<DashboardStatsViewModel>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (parsedData != null)
                    {
                        dashboardData = parsedData;
                    }

                    if (dashboardData.PendingApprovals == null)
                    {
                        dashboardData.PendingApprovals = new List<PendingArenaViewModel>();
                    }
                }
                else
                {
                    ViewBag.Error = "Failed to fetch stats from the API. Status code: " + statsResponse.StatusCode;
                    SetDefaultStats(dashboardData);
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "The API is completely inaccessible.";
                SetDefaultStats(dashboardData);
            }

            try
            {
                var pendingListDb = _context.PendingArenas
                                            .Where(a => a.Status == "Pending")
                                            .OrderByDescending(a => a.AppliedOn)
                                            .ToList();

                if (pendingListDb != null && pendingListDb.Count > 0)
                {
                    dashboardData.PendingApprovals = pendingListDb.Select(p => new PendingArenaViewModel
                    {
                        Id = p.Id,
                        ArenaName = p.ArenaName,
                        OwnerName = p.OwnerName,
                        Status = p.Status,

                        Category = "Sports Turf",
                        Address = p.Address,
                        Latitude = p.Latitude,
                        Longitude = p.Longitude,
                        ImagePaths = p.ImagePaths
                    }).ToList();
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Failed to load Pending Approvals from database.";
            }

            return View(dashboardData);
        }

        private void SetDefaultStats(DashboardStatsViewModel model)
        {
            model.TotalPlayers = 0;
            model.RegisteredOwners = 0;
            model.TotalReceptionists = 0;
            model.ActiveArenas = 0;
            model.PlatformRevenue = "₹0";
        }

        [HttpPost]
        public async Task<IActionResult> ApproveArena(int id)
        {
            var pendingArena = await _context.PendingArenas.FindAsync(id);
            if (pendingArena == null)
                return Json(new { success = false, message = "Arena not found." });

            string rawPassword = "Nex" + new Random().Next(1000, 9999).ToString() + "@Ar";

            var newOwner = new User
            {
                RoleId = 2,
                FullName = pendingArena.OwnerName,
                Email = pendingArena.Email,
                PasswordHash = rawPassword,
                Phone = "9999999999",
                IsActive = true
            };

            _context.Users.Add(newOwner);
            await _context.SaveChangesAsync();

            var newArena = new Arena
            {
                OwnerId = newOwner.UserId,
                Name = pendingArena.ArenaName,
                City = "Surat",
                Location = pendingArena.Address,
                IsActive = true
            };
            _context.Arenas.Add(newArena);

            pendingArena.Status = "Approved";
            await _context.SaveChangesAsync();

            bool mailSent = SendApprovalEmail(pendingArena.Email, pendingArena.OwnerName, pendingArena.Email, rawPassword);

            return Json(new { success = true, emailSent = mailSent });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteArena(int id)
        {
            var arena = await _context.PendingArenas.FindAsync(id);
            if (arena == null) return Json(new { success = false });
            _context.PendingArenas.Remove(arena);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        private bool SendApprovalEmail(string toEmail, string ownerName, string userId, string password)
        {
            try
            {
                string senderEmail = "sahilmirza01779@gmail.com";
                string senderAppPassword = "xumb xpgu rrbd aimt";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(senderEmail, "Nexus Arena Admin");
                mail.To.Add(toEmail);
                mail.Subject = "🎉 Nexus Arena - Partnership Approved!";

                mail.Body = $@"
            <div style='font-family: Arial, sans-serif; background-color: #111; color: #fff; padding: 30px; border-radius: 12px; border: 1px solid #333; max-width: 600px; margin: auto;'>
                <h2 style='color: #00ff7f; margin-top: 0;'>Welcome to Nexus Arena, {ownerName}!</h2>
                <p style='color: #ccc; font-size: 15px;'>Your turf application has been successfully approved by the SuperAdmin.</p>
                <div style='background: #1a1a1a; padding: 20px; border-radius: 8px; border-left: 4px solid #00ff7f;'>
                    <p style='margin: 0 0 10px 0; color: #fff; font-weight: bold;'>Your Dashboard Login Credentials:</p>
                    <p style='margin: 5px 0;'>Login ID: <strong style='color: #00ff7f;'>{userId}</strong></p>
                    <p style='margin: 5px 0;'>Password: <strong style='color: #00ff7f;'>{password}</strong></p>
                </div>
                <p style='font-size: 13px; color: #888; margin-top: 20px;'>Please login and change your password immediately for security.</p>
                <p style='color: #aaa; font-size: 12px;'>Best Regards,<br/>Nexus Arena Team</p>
            </div>";

                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(senderEmail, senderAppPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Arenas()
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5092/api/Arena/GetAll";
            var arenaList = new List<ArenaListViewModel>();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var fetchedList = JsonSerializer.Deserialize<List<ArenaListViewModel>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (fetchedList != null)
                    {
                        arenaList = fetchedList;
                    }
                }
            }
            catch (Exception)
            {
            }

            return View(arenaList);
        }

        [HttpGet]
        public async Task<IActionResult> Manage(int id)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/Arena/GetDetails/{id}";
            var arenaDetails = new ArenaDetailsViewModel();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    arenaDetails = JsonSerializer.Deserialize<ArenaDetailsViewModel>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    return RedirectToAction("Arenas");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Arenas");
            }

            return View(arenaDetails);
        }

        [HttpPost]
        public async Task<IActionResult> SuspendArena(int id)
        {
            try
            {
                var arena = await _context.Arenas.FindAsync(id);

                if (arena != null)
                {
                    arena.IsActive = false;
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Arena not found." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Server error occurred." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActivateArena(int id)
        {
            try
            {
                var arena = await _context.Arenas.FindAsync(id);

                if (arena != null)
                {
                    arena.IsActive = true;
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Arena not found." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Server error occurred." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5092/api/SportCategory/GetAll";
            var categoryList = new List<CategoryViewModel>();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var fetchedList = System.Text.Json.JsonSerializer.Deserialize<List<CategoryViewModel>>(jsonData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (fetchedList != null)
                    {
                        categoryList = fetchedList;
                    }
                }
            }
            catch (Exception)
            {
            }

            return View(categoryList);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryViewModel newCategory)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5092/api/SportCategory/Create";
            var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(newCategory), System.Text.Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(apiUrl, jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
            }
            catch (Exception)
            {
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/SportCategory/Delete/{id}";

            try
            {
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
            }
            catch (Exception)
            {
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, [FromBody] CategoryViewModel upadatedCategory)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/SportCategory/Update/{id}";
            var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(upadatedCategory), System.Text.Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PutAsync(apiUrl, jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
            }
            catch (Exception)
            {
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> Owners()
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5092/api/ManageOwners/GetAll";
            var ownerList = new List<ManageOwnerViewModel>();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var fetchedList = System.Text.Json.JsonSerializer.Deserialize<List<ManageOwnerViewModel>>(jsonData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (fetchedList != null)
                    {
                        ownerList = fetchedList;
                    }
                }
            }
            catch (Exception)
            {
            }

            return View(ownerList);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleOwnerStatus(int id)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/ManageOwners/ToggleStatus/{id}";
            var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
            }
            catch (Exception)
            {
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5092/api/ManageUsers/GetAll";
            var userList = new List<ManageUserViewModel>();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var fetchedList = System.Text.Json.JsonSerializer.Deserialize<List<ManageUserViewModel>>(jsonData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (fetchedList != null)
                    {
                        userList = fetchedList;
                    }
                }
            }
            catch (Exception)
            {
            }

            return View(userList);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/ManageUsers/ToggleStatus/{id}";
            var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
            }
            catch (Exception)
            {
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> Receptionists()
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5092/api/ManageReceptionists/GetAll";
            var receptionistList = new List<ManageReceptionistViewModel>();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var fetchedList = System.Text.Json.JsonSerializer.Deserialize<List<ManageReceptionistViewModel>>(jsonData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (fetchedList != null)
                    {
                        receptionistList = fetchedList;
                    }
                }
            }
            catch (Exception)
            {
            }
            return View(receptionistList);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleReceptionistStatus(int id)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/ManageReceptionists/ToggleStatus/{id}";
            var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
            }
            catch (Exception)
            {
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> Reviews()
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5092/api/ManageReviews/GetAll";
            var reviewList = new List<ManageReviewViewModel>();

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var fetchedList = System.Text.Json.JsonSerializer.Deserialize<List<ManageReviewViewModel>>(jsonData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (fetchedList != null)
                    {
                        reviewList = fetchedList;
                    }
                }
            }
            catch (Exception)
            {
            }

            return View(reviewList);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePlatformReview(int id)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = $"http://localhost:5092/api/ManageReviews/Delete/{id}";

            try
            {
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
            }
            catch (Exception)
            {
            }

            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> Revenue()
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5092/api/ManageRevenue/GetAll";
            var transactionList = new List<ManageRevenueViewModel>();
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var fetchedList = System.Text.Json.JsonSerializer.Deserialize<List<ManageRevenueViewModel>>(jsonData, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (fetchedList != null)
                    {
                        transactionList = fetchedList;
                    }
                }
            }
            catch (Exception)
            {
            }
            return View(transactionList);
        }

        [HttpGet]
        public IActionResult Broadcast()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendPlatformBroadcast(string targetAudience, string message)
        {
            var client = _httpClientFactory.CreateClient();
            string apiUrl = "http://localhost:5092/api/ManageNotifications/SendBroadcast";
            var payload = new
            {
                TargetAudience = targetAudience,
                Message = message
            };
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Broadcast sent Successfully!" });
                }
            }
            catch (Exception)
            {
            }

            return Json(new { success = false, message = "Failed to send broadcast." });
        }

        [HttpGet]
        public async Task<IActionResult> AdminDetails()
        {
            int currentAdminId = GetLoggedInUserId();
            if (currentAdminId == 0) return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"http://localhost:5092/api/AdminProfile/GetAdmin/{currentAdminId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var profile = System.Text.Json.JsonSerializer.Deserialize<NexusArena.Web.Models.AdminProfileViewModel>(
                    jsonString, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View(profile);
            }

            return View(new NexusArena.Web.Models.AdminProfileViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> UpdateDetails()
        {
            int currentAdminId = GetLoggedInUserId();
            if (currentAdminId == 0) return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"http://localhost:5092/api/AdminProfile/GetAdmin/{currentAdminId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var profile = System.Text.Json.JsonSerializer.Deserialize<NexusArena.Web.Models.AdminProfileViewModel>(
                    jsonString, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View(profile);
            }

            return View(new NexusArena.Web.Models.AdminProfileViewModel());
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateDetails(NexusArena.Web.Models.AdminProfileViewModel model)
        {
            int currentAdminId = GetLoggedInUserId();
            if (currentAdminId == 0) return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"http://localhost:5092/api/AdminProfile/UpdateAdmin/{currentAdminId}", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("AdminDetails");
            }

            return View(model);
        }

        private int GetLoggedInUserId()
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token))
            {
                return 0;
            }
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }
    }
}