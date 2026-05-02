using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models; // Necesar pentru ApplicationUser și Licitatie
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ProfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfilController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. Pagina personală (accesată din meniu)
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            var licitatiiProprii = await _context.Licitatii
                .Where(l => l.seller_id == userId)
                .ToListAsync();

            ViewBag.LicitatiiUser = licitatiiProprii;
            ViewBag.EsteProfilPropriu = true;

            return View(user);
        }

        // 2. Pagina de detalii profil (accesată prin link de la licitații)
        // URL: /Profil/Details?username=nume@email.com
        public async Task<IActionResult> Details(string username)
        {
            if (string.IsNullOrEmpty(username)) return NotFound();

            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return NotFound();

            var licitatii = await _context.Licitatii
                .Where(l => l.seller_id == user.Id)
                .ToListAsync();

            ViewBag.LicitatiiUser = licitatii;
            ViewBag.EsteProfilPropriu = (user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            return View("Index", user); // Refolosim vederea Index pentru simplitate
        }

        [HttpPost]
        public async Task<IActionResult> IncarcaPoza(IFormFile pozaFile)
        {
            if (pozaFile != null && pozaFile.Length > 0)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(pozaFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await pozaFile.CopyToAsync(stream);
                    }

                    user.ProfilePicture = fileName;
                    await _userManager.UpdateAsync(user);
                }
            }
            return RedirectToAction("Index");
        }
    }
}