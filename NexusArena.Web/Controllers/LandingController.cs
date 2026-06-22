using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using NexusArena.Web.Models; 
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;
using NexusArena.API.Models;

namespace NexusArena.Web.Controllers
{
    [AllowAnonymous]
    public class LandingController : Controller
    {
        private readonly NexusArenaDbContext _context; 
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LandingController(NexusArenaDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult OwnerApplication()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitApplication(OwnerApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("OwnerApplication", model);
            }

            List<string> uploadedFilePaths = new List<string>();

            if (model.Photos != null && model.Photos.Count > 0)
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "arenas");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                foreach (var file in model.Photos)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    uploadedFilePaths.Add("/uploads/arenas/" + uniqueFileName);
                }
            }

            var pendingApplication = new PendingArena
            {
                OwnerName = model.Name,
                ArenaName = model.ArenaName,
                Email = model.Email,
                Address = model.Address,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                ImagePaths = string.Join(",", uploadedFilePaths), 
                Status = "Pending",
                AppliedOn = DateTime.Now
            };

            _context.PendingArenas.Add(pendingApplication);
            await _context.SaveChangesAsync();

            return View("ApplicationSuccess");
        }
    }
}