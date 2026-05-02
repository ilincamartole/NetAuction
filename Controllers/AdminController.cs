using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Admin")] // Doar cei cu rolul Admin pot intra aici
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Pagina unde vezi toți utilizatorii
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleSuspension(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Inversăm starea de suspendare
            user.IsSuspended = !user.IsSuspended;

            if (user.IsSuspended)
            {
                // Îl blocăm să nu se mai poată loga
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                // Îl dăm afară din sesiune dacă e online
                await _userManager.UpdateSecurityStampAsync(user);
            }
            else
            {
                // Îl deblocăm
                await _userManager.SetLockoutEndDateAsync(user, null);
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Users));
        }
        [HttpPost]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Verificăm dacă userul selectat este deja Admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                // Dacă este deja Admin, îi scoatem rolul (demote)
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            else
            {
                // Dacă nu este Admin, îi adăugăm rolul (promote)
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return RedirectToAction(nameof(Users));
        }
    }
}