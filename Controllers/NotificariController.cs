using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class NotificariController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificariController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Pagina principală de notificări
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var notificari = await _context.Notificari
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.DataCreare)
                .ToListAsync();

            return View(notificari);
        }

        // Marcare ca citită (apelată când dă click pe notificare)
        [HttpPost]
        public async Task<IActionResult> MarcheazaCitita(int id)
        {
            var notificare = await _context.Notificari.FindAsync(id);
            if (notificare != null)
            {
                notificare.EsteCitita = true;
                await _context.SaveChangesAsync();
                
                if (!string.IsNullOrEmpty(notificare.Link))
                {
                    return Redirect(notificare.Link);
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}