using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using WebApplication1.Hubs;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace WebApplication1.Controllers
{
    public class LicitatiiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly IEmailSender _emailSender;

        public LicitatiiController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<AuctionHub> hubContext,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
            _emailSender = emailSender;
        }

        // Afișează piața de licitații cu filtre și notificări de câștig
        public async Task<IActionResult> Index(string categorie, decimal? pretMax, string searchString, string filter)
        {
            var licitatiiQuery = _context.Licitatii.AsQueryable();

            // Filtrare după propriile licitații (Seller)
            if (filter == "mine" && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                licitatiiQuery = licitatiiQuery.Where(l => l.seller_id == userId);
            }

            // Filtrare după Categorie
            if (!string.IsNullOrEmpty(categorie) && Enum.TryParse<CategorieLicitatie>(categorie, out var catEnum))
            {
                licitatiiQuery = licitatiiQuery.Where(x => x.Categorie == catEnum);
            }

            // Filtrare după Preț
            if (pretMax.HasValue)
            {
                licitatiiQuery = licitatiiQuery.Where(x => x.PretCurent <= pretMax.Value);
            }

            // Filtrare după Titlu
            if (!string.IsNullOrEmpty(searchString))
            {
                licitatiiQuery = licitatiiQuery.Where(x => x.titlu.Contains(searchString));
            }

            // --- LOGICĂ NOTIFICARE CÂȘTIGĂTOR (Pop-up WOW) ---
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // Căutăm o licitație câștigată de el, încheiată, dar pentru care nu a primit felicitări
                var licitatieCastigata = await _context.Licitatii
                    .FirstOrDefaultAsync(l => l.CastigatorId == userId && l.EsteIncheiata && !l.NotificareTrimisa);

                if (licitatieCastigata != null)
                {
                    ViewBag.ShowWinnerModal = true;
                    ViewBag.ProductName = licitatieCastigata.titlu;

                    // Marcăm ca trimisă ca să nu apară la fiecare refresh
                    licitatieCastigata.NotificareTrimisa = true;
                    _context.Update(licitatieCastigata);
                    await _context.SaveChangesAsync();
                }
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
                ModelState.AddModelError("data_finalizare", "Data trebuie să fie în viitor.");

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

        // Detalii Licitație - Include Istoric, Mapare Usernames și Securitate Date Contact
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var licitatie = await _context.Licitatii.FirstOrDefaultAsync(m => m.id == id);
            if (licitatie == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Preluăm istoricul ofertelor
            var istoricoferte = await _context.Bids
                .Where(b => b.licitatieId == id)
                .OrderByDescending(b => b.data)
                .ToListAsync();

            ViewBag.OferteleMeleAici = istoricoferte.Where(b => b.userId == currentUserId).ToList();

            // 2. Mapăm ID-urile la UserNames pentru afișare
            var userIds = istoricoferte.Select(b => b.userId).Distinct();
            var usernames = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            ViewBag.IstoricOferte = istoricoferte;
            ViewBag.Usernames = usernames;

            var seller = await _userManager.FindByIdAsync(licitatie.seller_id);
            ViewBag.SellerName = seller?.UserName ?? "Utilizator necunoscut";

            // 3. LOGICĂ DE SECURITATE: Date contact câștigător (Doar pentru Admin)
            if (licitatie.EsteIncheiata && !string.IsNullOrEmpty(licitatie.CastigatorId))
            {
                var winner = await _userManager.FindByIdAsync(licitatie.CastigatorId);
                ViewBag.WinnerName = winner?.UserName;

                // Datele private sunt trimise DOAR dacă utilizatorul curent este Admin
                if (User.IsInRole("Admin"))
                {
                    ViewBag.WinnerFullName = winner?.prenume + " " + winner?.nume;
                    ViewBag.WinnerEmail = winner?.Email;
                    ViewBag.WinnerAddress = winner?.adresa;
                }
            }

            return View(licitatie);
        }

        // Dashboard Cumpărător (Toate ofertele mele)
        [Authorize]
        public async Task<IActionResult> DashboardCumparator()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var oferteleMele = await _context.Bids
                .Where(b => b.userId == userId)
                .Include(b => b.licitatie)
                .OrderByDescending(b => b.data)
                .ToListAsync();

            return View(oferteleMele);
        }

        // Logică Licitare cu Notificări în timp real (SignalR) și Email
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Licitare(int id, decimal sumaLicitata)
        {
            var licitatie = await _context.Licitatii.FindAsync(id);
            if (licitatie == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Identificăm cine deținea oferta anterioară pentru a-l notifica
            var ultimaOferta = await _context.Bids
                .Where(b => b.licitatieId == id)
                .OrderByDescending(b => b.suma)
                .FirstOrDefaultAsync();

            string userDepasitId = ultimaOferta?.userId;

            // Verificare stare licitație
            if (licitatie.EsteIncheiata || licitatie.data_finalizare <= DateTime.Now)
            {
                TempData["Error"] = "Această licitație s-a încheiat și nu mai acceptă oferte.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            if (licitatie.seller_id == currentUserId)
            {
                TempData["Error"] = "Nu poți licita la propriul tău produs.";
            }
            else if (sumaLicitata <= licitatie.PretCurent)
            {
                TempData["Error"] = $"Oferta trebuie să fie mai mare de {licitatie.PretCurent} RON.";
            }
            else
            {
                var bid = new Bid
                {
                    suma = sumaLicitata,
                    data = DateTime.Now,
                    licitatieId = id,
                    userId = currentUserId
                };
                licitatie.PretCurent = sumaLicitata;

                _context.Bids.Add(bid);
                _context.Update(licitatie);
                await _context.SaveChangesAsync();

                // --- TRIMITERE NOTIFICĂRI ---
                if (!string.IsNullOrEmpty(userDepasitId) && userDepasitId != currentUserId)
                {
                    var productUrl = Url.Action("Details", "Licitatii", new { id = licitatie.id }, Request.Scheme);

                    // 1. SignalR (Notificare instantă în interfață)
                    await _hubContext.Clients.User(userDepasitId).SendAsync("ReceiveOutbidNotification", licitatie.titlu, productUrl);

                    // 2. Email (Notificare externă)
                    var userDepasit = await _userManager.FindByIdAsync(userDepasitId);
                    if (userDepasit != null)
                    {
                        string emailBody = $@"
                            <div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                                <h2 style='color: #dc3545;'>⚠️ Ai fost depășit!</h2>
                                <p>Cineva a oferit o sumă mai mare pentru obiectul: <strong>{licitatie.titlu}</strong></p>
                                <p>Noua sumă curentă: <strong>{sumaLicitata} RON</strong></p>
                                <br />
                                <a href='{productUrl}' style='display: inline-block; padding: 10px 20px; background: #0d6efd; color: white; text-decoration: none; border-radius: 5px;'>Licitează din nou acum</a>
                            </div>";
                        await _emailSender.SendEmailAsync(userDepasit.Email, "Alertă Licitație: Ai fost depășit!", emailBody);
                    }
                }

                TempData["Success"] = "Oferta ta a fost înregistrată!";
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var licitatie = await _context.Licitatii.FindAsync(id);
            // Permite ștergerea doar dacă este seller-ul sau Admin (opțional, poți adăuga || User.IsInRole("Admin"))
            if (licitatie != null && licitatie.seller_id == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                _context.Licitatii.Remove(licitatie);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}