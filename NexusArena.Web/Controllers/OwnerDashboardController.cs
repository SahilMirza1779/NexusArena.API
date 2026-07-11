using Microsoft.AspNetCore.Mvc;
using NexusArena.MVC.Models;
using NexusArena.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Mail;
using System.Net;

namespace NexusArena.Web.Controllers
{
    public class OwnerDashboardController : Controller
    {
        private readonly string _baseApiUrl = "[http://localhost:5092](http://localhost:5092)/api/";

        private HttpClient GetAuthenticatedClient()
        {
            var client = new HttpClient();
            string? token = Request.Cookies["JWToken"];
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private string GetLoggedInUserEmail()
        {
            var token = Request.Cookies["JWToken"];
            if (string.IsNullOrEmpty(token)) return "";
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                return jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? "";
            }
            catch { return ""; }
        }

        private bool SendStaffWelcomeEmail(string toEmail, string fullName, string password, string businessName)
        {
            try
            {
                string senderEmail = "sahilmirza01779@gmail.com";
                string senderAppPassword = "xumb xpgu rrbd aimt";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(senderEmail, "Nexus Arena");
                mail.To.Add(toEmail);
                mail.Subject = "🎉 Welcome to Nexus Arena - Your Staff Account!";
                mail.IsBodyHtml = true;

                mail.Body = $@"
                <div style='font-family: Arial, sans-serif; background-color: #111; color: #fff; padding: 30px; border-radius: 12px; border: 1px solid #333; max-width: 600px; margin: auto;'>
                    <h2 style='color: #3498db; margin-top: 0;'>Welcome, {fullName}! 🏢</h2>
                    <p style='color: #ccc; font-size: 15px;'>Your staff account has been created successfully for <strong>{businessName}</strong>. You can now manage operations instantly!</p>
                    <div style='background: #1a1a1a; padding: 20px; border-radius: 8px; border-left: 4px solid #3498db;'>
                        <p style='margin: 0 0 10px 0; color: #fff; font-weight: bold;'>Your Login Credentials:</p>
                        <p style='margin: 5px 0;'>Email ID: <strong style='color: #3498db;'>{toEmail}</strong></p>
                        <p style='margin: 5px 0;'>Password: <strong style='color: #3498db;'>{password}</strong></p>
                    </div>
                    <p style='font-size: 13px; color: #888; margin-top: 20px;'>Please login and change your password for security.</p>
                </div>";

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(senderEmail, senderAppPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email Error: " + ex.Message);
                return false;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new OwnerDashboardViewModel();

            using (var client = GetAuthenticatedClient())
            {
                try
                {
                    var response = await client.GetAsync($"{_baseApiUrl}OwnerDashboard/stats");

                    if (response.IsSuccessStatusCode)
                    {
                        viewModel = await response.Content.ReadFromJsonAsync<OwnerDashboardViewModel>();
                    }
                    else
                    {
                        TempData["Error"] = "Failed to load real-time dashboard stats!";
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Connection Error: " + ex.Message;
                }
            }

            return View(viewModel ?? new OwnerDashboardViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking(int bookingId, string cancelReason)
        {
            TempData["Success"] = $"Booking #{bookingId} cancelled! Reason: {cancelReason}";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            string email = GetLoggedInUserEmail();
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}OwnerProfile/GetByEmail/{email}");
                if (response.IsSuccessStatusCode)
                {
                    var model = await response.Content.ReadFromJsonAsync<OwnerProfileViewModel>();
                    return View(model);
                }
            }
            return View(new OwnerProfileViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            string email = GetLoggedInUserEmail();
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}OwnerProfile/GetByEmail/{email}");
                if (response.IsSuccessStatusCode)
                {
                    var model = await response.Content.ReadFromJsonAsync<OwnerProfileViewModel>();
                    return View(model);
                }
            }
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(OwnerProfileViewModel model)
        {
            string email = GetLoggedInUserEmail();
            using (var client = GetAuthenticatedClient())
            {
                var payload = new { FullName = model.FullName, Phone = model.Phone };
                var response = await client.PutAsJsonAsync($"{_baseApiUrl}OwnerProfile/update/{email}", payload);

                if (response.IsSuccessStatusCode) TempData["Success"] = "Profile Updated Successfully!";
                else TempData["Error"] = "Failed to update profile!";
            }
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> ManageResources()
        {
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}ResourceManager/GetAllFacilities");
                var list = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<List<ResourceViewModel>>() : new List<ResourceViewModel>();
                ViewBag.ResourceList = list ?? new List<ResourceViewModel>();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddResource(ResourceViewModel model)
        {
            var sports = Request.Form["SelectedSports"].ToList();
            var payload = new { ResourceName = model.ResourceName, ResourceType = string.Join(", ", sports) };
            using (var client = GetAuthenticatedClient()) { await client.PostAsJsonAsync($"{_baseApiUrl}ResourceManager/add", payload); }
            return RedirectToAction("ManageResources");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteResource(string resourceName)
        {
            using (var client = GetAuthenticatedClient()) { await client.DeleteAsync($"{_baseApiUrl}ResourceManager/Delete/{resourceName}"); }
            return RedirectToAction("ManageResources");
        }

        [HttpGet]
        public async Task<IActionResult> EditResource(string resourceName)
        {
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}ResourceManager/GetByName/{resourceName}");
                if (response.IsSuccessStatusCode)
                {
                    var model = await response.Content.ReadFromJsonAsync<ResourceViewModel>();
                    ViewBag.OriginalName = resourceName;
                    return View(model);
                }
            }
            return RedirectToAction("ManageResources");
        }

        [HttpPost]
        public async Task<IActionResult> EditResource(string originalName, ResourceViewModel model)
        {
            var payload = new { ResourceName = model.ResourceName, ResourceType = string.Join(", ", Request.Form["SelectedSports"].ToList()) };
            using (var client = GetAuthenticatedClient()) { await client.PutAsJsonAsync($"{_baseApiUrl}ResourceManager/update/{originalName}", payload); }
            return RedirectToAction("ManageResources");
        }

        [HttpGet]
        public async Task<IActionResult> PricingAndSlots()
        {
            var viewModel = new PricingAndSlotsPageViewModel();
            using (var client = GetAuthenticatedClient())
            {
                var resResponse = await client.GetAsync($"{_baseApiUrl}ResourceManager/GetAllFacilities");
                if (resResponse.IsSuccessStatusCode) viewModel.Resources = await resResponse.Content.ReadFromJsonAsync<List<ResourceViewModel>>() ?? new List<ResourceViewModel>();

                var slotResponse = await client.GetAsync($"{_baseApiUrl}TimeSlot/GetAll");
                if (slotResponse.IsSuccessStatusCode) viewModel.TimeSlots = await slotResponse.Content.ReadFromJsonAsync<List<TimeSlotViewModel>>() ?? new List<TimeSlotViewModel>();
            }
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddTimeSlot(TimeSlotViewModel model)
        {
            model.IsPremium = Request.Form["IsPremium"] == "true";
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.PostAsJsonAsync($"{_baseApiUrl}TimeSlot/add", model);
                if (response.IsSuccessStatusCode) TempData["Success"] = "Time Slot added successfully!";
                else TempData["Error"] = "Error! Time Slot add nahi ho paya. Database ya API method check karo.";
            }
            return RedirectToAction("PricingAndSlots");
        }

        [HttpGet]
        public async Task<IActionResult> ManageArenas()
        {
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}Arena/GetMyArenas");
                if (response.IsSuccessStatusCode)
                {
                    var list = await response.Content.ReadFromJsonAsync<List<ArenaViewModel>>() ?? new List<ArenaViewModel>();
                    ViewBag.ArenasList = list;
                }
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Staff()
        {
            var viewModel = new StaffPageViewModel();
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.GetAsync($"{_baseApiUrl}Staff/GetAll");
                if (response.IsSuccessStatusCode)
                    viewModel.StaffList = await response.Content.ReadFromJsonAsync<List<ManageReceptionistViewModel>>() ?? new List<ManageReceptionistViewModel>();

                var resResponse = await client.GetAsync($"{_baseApiUrl}ResourceManager/GetAllFacilities");
                if (resResponse.IsSuccessStatusCode)
                    ViewBag.BusinessList = await resResponse.Content.ReadFromJsonAsync<List<ResourceViewModel>>() ?? new List<ResourceViewModel>();
            }
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterStaff(ManageReceptionistViewModel model)
        {
            model.Password = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            using (var client = GetAuthenticatedClient())
            {
                var response = await client.PostAsJsonAsync($"{_baseApiUrl}Staff/add", model);

                if (response.IsSuccessStatusCode)
                {
                    bool isEmailSent = SendStaffWelcomeEmail(model.Email, model.FullName, model.Password, model.BusinessName ?? "Nexus Arena");

                    if (isEmailSent) TempData["Success"] = $"Staff '{model.FullName}' registered successfully! Email sent to {model.Email}.";
                    else TempData["Error"] = $"Staff '{model.FullName}' saved, BUT Email sending failed. Auto-generated Password is: {model.Password}";
                }
                else TempData["Error"] = "Error! API ne data save nahi kiya. Database check karo.";
            }
            return RedirectToAction("Staff");
        }

        [HttpGet]
        public async Task<IActionResult> EditStaff(string email)
        {
            using (var client = GetAuthenticatedClient())
            {
                var resResponse = await client.GetAsync($"{_baseApiUrl}ResourceManager/GetAllFacilities");
                if (resResponse.IsSuccessStatusCode)
                    ViewBag.BusinessList = await resResponse.Content.ReadFromJsonAsync<List<ResourceViewModel>>() ?? new List<ResourceViewModel>();

                var response = await client.GetAsync($"{_baseApiUrl}Staff/GetByEmail/{email}");
                if (response.IsSuccessStatusCode)
                {
                    var model = await response.Content.ReadFromJsonAsync<ManageReceptionistViewModel>();
                    ViewBag.OriginalEmail = email;
                    return View(model);
                }

                TempData["Error"] = "Error! Staff member not found in database.";
                return RedirectToAction("Staff");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditStaff(string originalEmail, ManageReceptionistViewModel model)
        {
            using (var client = GetAuthenticatedClient())
            {
                var payload = new { FullName = model.FullName, Phone = model.Phone, BusinessName = model.BusinessName };
                var response = await client.PutAsJsonAsync($"{_baseApiUrl}Staff/update/{originalEmail}", payload);

                if (response.IsSuccessStatusCode) TempData["Success"] = "Staff Updated Successfully!";
                else TempData["Error"] = "Update failed! Check database/API.";
            }
            return RedirectToAction("Staff");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStaff(string email)
        {
            using (var client = GetAuthenticatedClient())
            {
                var response = await client.DeleteAsync($"{_baseApiUrl}Staff/Delete/{email}");

                if (response.IsSuccessStatusCode) TempData["Success"] = "Staff Deleted Successfully!";
                else TempData["Error"] = "Failed to delete staff!";
            }
            return RedirectToAction("Staff");
        }
    }
}