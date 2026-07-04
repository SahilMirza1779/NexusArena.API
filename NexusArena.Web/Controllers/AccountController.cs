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
                    if (role == "User" || role == "4" || role == "Customer") return RedirectToAction("Index", "UserDashboard");

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

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string phone)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone))
            {
                ViewBag.Error = "Please fill all the required fields.";
                return View();
            }

            string rawPassword = "Pla" + new Random().Next(1000, 9999).ToString() + "@Nx";

            var registerData = new
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                Password = rawPassword,
                RoleName = "Customer" 
            };

            var content = new StringContent(JsonSerializer.Serialize(registerData), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/Auth/Register", content);

                if (response.IsSuccessStatusCode)
                {
                    SendPlayerEmail(email, fullName, rawPassword);

                    TempData["SuccessMessage"] = "Player account created successfully! We have sent the password to your email.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.Error = "Registration failed! This email or phone might already exist.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "API Connection Error: " + ex.Message;
                return View();
            }
        }

        private bool SendPlayerEmail(string toEmail, string playerName, string password)
        {
            try
            {
                string senderEmail = "sahilmirza01779@gmail.com";
                string senderAppPassword = "xumb xpgu rrbd aimt";

                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                mail.From = new System.Net.Mail.MailAddress(senderEmail, "Nexus Arena");
                mail.To.Add(toEmail);
                mail.Subject = "🎉 Welcome to Nexus Arena - Your Player Account!";

                mail.Body = $@"
                <div style='font-family: Arial, sans-serif; background-color: #111; color: #fff; padding: 30px; border-radius: 12px; border: 1px solid #333; max-width: 600px; margin: auto;'>
                    <h2 style='color: #3498db; margin-top: 0;'>Welcome, {playerName}! ⚽🏏</h2>
                    <p style='color: #ccc; font-size: 15px;'>Your player account has been created successfully. You can now book turfs instantly!</p>
                    <div style='background: #1a1a1a; padding: 20px; border-radius: 8px; border-left: 4px solid #3498db;'>
                        <p style='margin: 0 0 10px 0; color: #fff; font-weight: bold;'>Your Login Credentials:</p>
                        <p style='margin: 5px 0;'>Email ID: <strong style='color: #3498db;'>{toEmail}</strong></p>
                        <p style='margin: 5px 0;'>Password: <strong style='color: #3498db;'>{password}</strong></p>
                    </div>
                    <p style='font-size: 13px; color: #888; margin-top: 20px;'>Please login and change your password for security.</p>
                </div>";

                mail.IsBodyHtml = true;

                using (System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new System.Net.NetworkCredential(senderEmail, senderAppPassword);
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
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return View();

            var requestData = new { Email = email };
            var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Auth/VerifyEmail", content);

            if (response.IsSuccessStatusCode)
            {
                string otp = new Random().Next(100000, 999999).ToString();
                SendOtpEmail(email, otp);

                TempData["OTP"] = otp;
                TempData["ResetEmail"] = email;
                TempData["OTPExpiry"] = DateTime.Now.AddSeconds(90).ToString("O");

                return RedirectToAction("VerifyOTP");
            }

            ViewBag.Error = "We couldn't find any account with this email address.";
            return View();
        }

        [HttpGet]
        public IActionResult VerifyOTP()
        {
            if (TempData["ResetEmail"] == null) return RedirectToAction("ForgotPassword");

            TempData.Keep("OTP");
            TempData.Keep("ResetEmail");
            TempData.Keep("OTPExpiry");

            ViewBag.Email = TempData["ResetEmail"].ToString();
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOTP(string enteredOtp)
        {
            TempData.Keep("ResetEmail");
            TempData.Keep("OTP");
            TempData.Keep("OTPExpiry");

            string realOtp = TempData["OTP"]?.ToString();
            string expiryString = TempData["OTPExpiry"]?.ToString();

            if (!string.IsNullOrEmpty(expiryString))
            {
                DateTime expiryTime = DateTime.Parse(expiryString);
                if (DateTime.Now > expiryTime)
                {
                    ViewBag.Error = "OTP has expired! Please click on 'Resend OTP' to get a new one.";
                    ViewBag.Email = TempData["ResetEmail"].ToString();
                    return View();
                }
            }

            if (realOtp == enteredOtp)
            {
                TempData["VerifiedEmail"] = TempData["ResetEmail"];
                return RedirectToAction("ResetPassword");
            }
            else
            {
                ViewBag.Error = "Invalid OTP! Please check your email and try again.";
                ViewBag.Email = TempData["ResetEmail"].ToString();
                return View();
            }
        }

        [HttpPost]
        public IActionResult ResendOTP([FromBody] JsonElement data)
        {
            string email = data.GetProperty("email").GetString();

            if (!string.IsNullOrEmpty(email))
            {
                string newOtp = new Random().Next(100000, 999999).ToString();
                SendOtpEmail(email, newOtp);

                TempData["OTP"] = newOtp;
                TempData["ResetEmail"] = email;
                TempData["OTPExpiry"] = DateTime.Now.AddSeconds(90).ToString("O");

                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["VerifiedEmail"] == null) return RedirectToAction("ForgotPassword");

            ViewBag.VerifiedEmail = TempData["VerifiedEmail"].ToString();
            return View();
        }

        private void SendOtpEmail(string toEmail, string otp)
        {
            try
            {
                string senderEmail = "sahilmirza01779@gmail.com";
                string senderAppPassword = "xumb xpgu rrbd aimt";

                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                mail.From = new System.Net.Mail.MailAddress(senderEmail, "Nexus Arena Security");
                mail.To.Add(toEmail);
                mail.Subject = "🔑 Password Reset OTP - Nexus Arena";

                mail.Body = $@"
                <div style='font-family: Arial, sans-serif; background-color: #111; color: #fff; padding: 30px; border-radius: 12px; border: 1px solid #333; max-width: 600px; margin: auto;'>
                    <h2 style='color: #00ff7f; margin-top: 0;'>Verification Code</h2>
                    <p style='color: #ccc; font-size: 15px;'>You requested a password reset. Here is your 6-digit OTP:</p>
                    <div style='background: #1a1a1a; padding: 20px; border-radius: 8px; border-left: 4px solid #00ff7f; text-align: center; letter-spacing: 5px;'>
                        <h1 style='margin: 0; color: #00ff7f; font-size: 32px;'>{otp}</h1>
                    </div>
                    <p style='font-size: 13px; color: #888; margin-top: 20px;'>This OTP is valid for your current session. Do not share it with anyone.</p>
                </div>";

                mail.IsBodyHtml = true;

                using (System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new System.Net.NetworkCredential(senderEmail, senderAppPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
            catch (Exception)
            {

            }
        }


        [HttpPost]
        public async Task<IActionResult> SaveNewPassword(string email, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match!";
                ViewBag.VerifiedEmail = email;
                return View("ResetPassword");
            }

            var requestData = new { Email = email, NewPassword = newPassword };
            var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/Auth/ResetUserPassword", content);

                if (response.IsSuccessStatusCode)
                {
                    SendPasswordChangedEmail(email, newPassword);

                    ViewBag.Success = "Your password has been changed successfully!";
                    ViewBag.VerifiedEmail = email;
                    return View("ResetPassword");
                }

                ViewBag.Error = "Something went wrong while resetting the password.";
                ViewBag.VerifiedEmail = email;
                return View("ResetPassword");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "API Connection Error: " + ex.Message;
                ViewBag.VerifiedEmail = email;
                return View("ResetPassword");
            }
        }

        private void SendPasswordChangedEmail(string toEmail, string newPassword)
        {
            try
            {
                string senderEmail = "sahilmirza01779@gmail.com";
                string senderAppPassword = "xumb xpgu rrbd aimt";

                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                mail.From = new System.Net.Mail.MailAddress(senderEmail, "Nexus Arena Support");
                mail.To.Add(toEmail);
                mail.Subject = "✅ Password Changed Successfully - Nexus Arena";

                mail.Body = $@"
                <div style='font-family: Arial, sans-serif; background-color: #111; color: #fff; padding: 30px; border-radius: 12px; border: 1px solid #333; max-width: 600px; margin: auto;'>
                    <h2 style='color: #00ff7f; margin-top: 0;'>Password Updated! 🔒</h2>
                    <p style='color: #ccc; font-size: 15px;'>Your password has been successfully changed as per your request.</p>
                    <div style='background: #1a1a1a; padding: 20px; border-radius: 8px; border-left: 4px solid #00ff7f;'>
                        <p style='margin: 0 0 10px 0; color: #fff; font-weight: bold;'>Your New Password is:</p>
                        <p style='margin: 5px 0; font-size: 18px;'><strong style='color: #00ff7f;'>{newPassword}</strong></p>
                    </div>
                    <p style='font-size: 13px; color: #888; margin-top: 20px;'>Shukriya for updating your security details. You can now login to your account.</p>
                </div>";

                mail.IsBodyHtml = true;

                using (System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new System.Net.NetworkCredential(senderEmail, senderAppPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
            catch (Exception)
            {

            }
        }

    }
}