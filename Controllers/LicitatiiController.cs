using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Controllers
{
    
    public class LicitatiiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructorul primește acum și UserManager pentru a putea găsi datele userilor
        public LicitatiiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string categorie, decimal? pretMax, string searchString, string filter)
        {
            // 1. Începem cu toate licitațiile active
            var licitatiiQuery = _context.Licitatii.AsQueryable();
            // 2. FILTRARE DUPĂ PROPRIETAR (Licitatiile Mele)
            if (filter == "mine" && User.Identity.IsAuthenticated)
            {
                // Luăm ID-ul utilizatorului care este logat acum
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Filtrăm doar licitațiile unde seller_id este egal cu ID-ul meu
                licitatiiQuery = licitatiiQuery.Where(l => l.seller_id == userId);
            }

            // 2. Filtrare după Categorie
            if (!string.IsNullOrEmpty(categorie))
            {
                // Convertim string-ul primit din URL în tipul Enum
                if (Enum.TryParse<CategorieLicitatie>(categorie, out var catEnum))
                {
                    licitatiiQuery = licitatiiQuery.Where(x => x.Categorie == catEnum);
                }
            }

            // 3. Filtrare după Preț Maxim
            if (pretMax.HasValue)
            {
                licitatiiQuery = licitatiiQuery.Where(x => x.PretCurent <= pretMax.Value);
            }

            // 4. Filtrare după căutare text (Opțional, dar util)
            if (!string.IsNullOrEmpty(searchString))
            {
                licitatiiQuery = licitatiiQuery.Where(x => x.titlu.Contains(searchString));
            }

            // Trimitem categoriile către View pentru a popula dropdown-ul din nou
            ViewBag.Categorii = Enum.GetValues(typeof(CategorieLicitatie));

            return View(await licitatiiQuery.ToListAsync());
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("titlu,descriere,PretPornire,data_finalizare,Categorie")] Licitatie licitatie, IFormFile? fisierImagine)
        {
            if (licitatie.data_finalizare <= DateTime.Now)
            {
                ModelState.AddModelError("data_finalizare", "Data de expirare trebuie să fie în viitor.");
            }

            if (fisierImagine != null)
            {
                if (fisierImagine.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Imaginea nu trebuie să depășească 10MB.");
                }
            }

            ModelState.Remove("seller_id");
            ModelState.Remove("PretCurent");
            ModelState.Remove("ImaginePath");

            if (ModelState.IsValid)
            {
                if (fisierImagine != null && fisierImagine.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + fisierImagine.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fisierImagine.CopyToAsync(fileStream);
                    }

                    licitatie.ImaginePath = uniqueFileName;
                }

                licitatie.seller_id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                licitatie.PretCurent = licitatie.PretPornire;

                _context.Add(licitatie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(licitatie);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var licitatie = await _context.Licitatii.FirstOrDefaultAsync(m => m.id == id);
            if (licitatie == null) return NotFound();

            // Căutăm userul în baza de date folosind ID-ul salvat în licitație
            var seller = await _userManager.FindByIdAsync(licitatie.seller_id);

            // Trimitem numele (email-ul) către View. Dacă nu-l găsește, punem un text generic.
            ViewBag.SellerName = seller != null ? seller.UserName : "Utilizator necunoscut";

            return View(licitatie);
        }
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var licitatie = await _context.Licitatii.FirstOrDefaultAsync(m => m.id == id);
            if (licitatie == null) return NotFound();

            // Verificare: Doar cel care a creat licitația poate ajunge la pagina de ștergere
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (licitatie.seller_id != currentUserId)
            {
                return Forbid();
            }

            // Aduce numele și aici pentru afișare în confirmare
            var seller = await _userManager.FindByIdAsync(licitatie.seller_id);
            ViewBag.SellerName = seller != null ? seller.UserName : "Utilizator necunoscut";

            return View(licitatie);
        }
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var licitatie = await _context.Licitatii.FindAsync(id);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (licitatie == null) return NotFound();

            if (licitatie.seller_id != currentUserId)
            {
                return Forbid();
            }

            if (!string.IsNullOrEmpty(licitatie.ImaginePath))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", licitatie.ImaginePath);
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _context.Licitatii.Remove(licitatie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}