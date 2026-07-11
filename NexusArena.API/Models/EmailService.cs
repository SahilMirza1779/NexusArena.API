using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

// 🌟 FIX: Namespace IDE0130 - Tune file Models folder mein banayi hai, toh namespace wahi rakha hai
namespace NexusArena.API.Models
{
    public interface IEmailService
    {
        Task SendBookingConfirmationAsync(string toEmail, string userName, string arenaName, string date, string time, string bookingId);
    }

    public class EmailService : IEmailService
    {
        public async Task SendBookingConfirmationAsync(string toEmail, string userName, string arenaName, string date, string time, string bookingId)
        {
            // 🌟 ENTERPRISE EMAIL CONFIGURATION
            // Tujhe apna real Gmail aur "App Password" yahan dalna hoga (Google Account -> Security -> 2-Step Verification -> App Passwords)
            // 🌟 FIX: IDE0090 - Simplified 'new' expression
            MailAddress fromAddress = new("sahilmirza01779@gmail.com", "Nexus Arena");
            MailAddress toAddress = new(toEmail);
            const string fromPassword = "xumb xpgu rrbd aimt"; // 16-digit App Password yahan daal

            // 🌟 FIX: IDE0090 - Simplified 'new' expression
            using SmtpClient smtp = new()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            // HTML Email Template
            using MailMessage message = new(fromAddress, toAddress)
            {
                Subject = $"🎟️ Booking Confirmed: {arenaName}",
                Body = $@"
                    <div style='font-family: Arial, sans-serif; background-color: #111; color: #fff; padding: 20px; border-radius: 10px; border: 1px solid #00ff66;'>
                        <h2 style='color: #00ff66;'>Booking Confirmed! ⚡</h2>
                        <p>Hello <b>{userName}</b>,</p>
                        <p>Your turf pitch is locked and ready. Here are your digital ticket details:</p>
                        <div style='background-color: #222; padding: 15px; border-radius: 8px;'>
                            <ul style='list-style: none; padding: 0;'>
                                <li>⚽ <b>Turf:</b> {arenaName}</li>
                                <li>📅 <b>Date:</b> {date}</li>
                                <li>⏰ <b>Time:</b> {time}</li>
                                <li>🎫 <b>Booking ID:</b> #{bookingId}</li>
                            </ul>
                        </div>
                        <p>See you on the pitch!</p>
                        <p style='color: #888; font-size: 12px;'>Team Nexus Arena</p>
                    </div>",
                IsBodyHtml = true
            };

            await smtp.SendMailAsync(message);
        }
    }
}