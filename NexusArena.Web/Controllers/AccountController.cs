using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace NexusArena.Web.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            string userRole = GetRoleFromApiDummy(email, password);

            if (userRole == "Invalid")
            {
                ViewBag.Error = "Invalid Email or Password!";
                return View();
            }

            switch (userRole)
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
                    return RedirectToAction("Login");
            }
        }

        private string GetRoleFromApiDummy(string email, string password)
        {
            if (email == "admin@nexus.com" && password == "123") return "SuperAdmin";
            if (email == "owner@nexus.com" && password == "123") return "Owner";
            if (email == "reception@nexus.com" && password == "123") return "Receptionist";
            if (email == "player@nexus.com" && password == "123") return "User";

            return "Invalid";
        }
    }
}