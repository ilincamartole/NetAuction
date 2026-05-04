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

        public LicitatiiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string categorie, decimal? pretMax, string searchString, string filter)
        {
            var licitatiiQuery = _context.Licitatii.AsQueryable();

            if (filter == "mine" && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                licitatiiQuery = licitatiiQuery.Where(l => l.seller_id == userId);
            }

            if (!string.IsNullOrEmpty(categorie) && Enum.TryParse<CategorieLicitatie>(categorie, out var catEnum))
            {
                licitatiiQuery = licitatiiQuery.Where(x => x.Categorie == catEnum);
            }

            if (pretMax.HasValue)
            {
                licitatiiQuery = licitatiiQuery.Where(x => x.PretCurent <= pretMax.Value);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                licitatiiQuery = licitatiiQuery.Where(x => x.titlu.Contains(searchString));
            }

            ViewBag.Categorii = Enum.GetValues(typeof(CategorieLicitatie));
            return View(await licitatiiQuery.ToListAsync());
        }

        [Authorize]
        public IActionResult Create() => View();

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("titlu,descriere,PretPornire,data_finalizare,Categorie")] Licitatie licitatie, IFormFile? fisierImagine)
        {
            if (licitatie.data_finalizare <= DateTime.Now)
                ModelState.AddModelError("data_finalizare", "Data de expirare trebuie să fie în viitor.");

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
                    using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
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

            var seller = await _userManager.FindByIdAsync(licitatie.seller_id);
            ViewBag.SellerName = seller?.UserName ?? "Utilizator necunoscut";

            return View(licitatie);
        }

        // --- IMPLEMENTARE NOUĂ: LOGICA DE LICITARE ---
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Licitare(int id, decimal sumaLicitata)
        {
            var licitatie = await _context.Licitatii.FindAsync(id);
            if (licitatie == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validări conform criteriilor de acceptare
            if (licitatie.data_finalizare <= DateTime.Now)
            {
                TempData["Error"] = "Această licitație s-a încheiat.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            if (licitatie.seller_id == currentUserId)
            {
                TempData["Error"] = "Nu poți licita la propriul tău produs.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            if (sumaLicitata <= licitatie.PretCurent)
            {
                TempData["Error"] = $"Oferta trebuie să fie strict mai mare decât prețul actual ({licitatie.PretCurent} RON).";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            // Actualizare bază de date
            var bid = new Bid
            {
                suma = sumaLicitata,
                data = DateTime.Now,
                licitatieId = id,
                userId = currentUserId
            };

            licitatie.PretCurent = sumaLicitata;

            try
            {
                _context.Bids.Add(bid);
                _context.Update(licitatie);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Oferta ta a fost înregistrată cu succes! Ești noul lider.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Eroare la procesarea ofertei.";
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var licitatie = await _context.Licitatii.FindAsync(id);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (licitatie != null && licitatie.seller_id == currentUserId)
            {
                if (!string.IsNullOrEmpty(licitatie.ImaginePath))
                {
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", licitatie.ImaginePath);
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }
                _context.Licitatii.Remove(licitatie);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}