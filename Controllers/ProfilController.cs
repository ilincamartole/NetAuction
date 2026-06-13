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
        public async Task<IActionResult> Index(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                // Dacă nu se trimite ID, înseamnă că utilizatorul își vede propriul profil
                id = _userManager.GetUserId(User);
            }

            var user = await _context.Users
                .Include(u => u.ReviewsPrimite) // Include recenziile primite
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            // CORECTAT: am schimbat l.SellerId în l.seller_id (așa cum e în modelul tău de Licitatie)
            ViewBag.LicitatiiUser = await _context.Licitatii.Where(l => l.seller_id == id).ToListAsync();
            ViewBag.EsteProfilPropriu = (id == _userManager.GetUserId(User));

            return View(user);
        }

        // 2. Pagina de detalii profil (accesată prin link de la licitații)
        public async Task<IActionResult> Details(string username)
        {
            if (string.IsNullOrEmpty(username)) return NotFound();

            // CORECTAT: În loc de FindByNameAsync, interogăm cu contextul pentru a putea folosi .Include() pe review-uri!
            var user = await _context.Users
                .Include(u => u.ReviewsPrimite) // Adus review-urile și pe profilul public!
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esteProfilPropriu = (user.Id == currentUserId);

            var licitatiiQuery = _context.Licitatii.Where(l => l.seller_id == user.Id);

            if (!esteProfilPropriu)
            {
                var acum = DateTime.UtcNow;
                licitatiiQuery = licitatiiQuery.Where(l => !l.EsteIncheiata && l.data_finalizare > acum);
            }

            var licitatii = await licitatiiQuery.ToListAsync();

            ViewBag.LicitatiiUser = licitatii;
            ViewBag.EsteProfilPropriu = esteProfilPropriu;

            return View("Index", user);
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