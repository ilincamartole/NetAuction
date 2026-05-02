using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
using WebApplication1.Models;
using WebApplication1.Areas.Identity.Pages.Account.Manage;

namespace WebApplication1.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {

            [Display(Name = "Prenume")]
            public string? FirstName { get; set; }

            [Display(Name = "Nume")]
            public string? LastName { get; set; }
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();


            if (string.IsNullOrWhiteSpace(user.prenume))
            {
                Username = "Utilizator fara nume";
            }


            Input = new InputModel
            {

                FirstName = user.prenume, // Presupunând c? ai aceste propriet??i în ApplicationUser
                LastName = user.nume,

                PhoneNumber = await _userManager.GetPhoneNumberAsync(user)
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();






            // 2. Salvare Prenume ?i Nume (Personalizat)
            // Verific?m dac? datele din formular sunt diferite de cele din baza de date
            if (Input.FirstName != user.prenume || Input.LastName != user.nume)
            {
                user.prenume = Input.FirstName;
                user.nume = Input.LastName;

                // Actualiz?m utilizatorul în baza de date
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return Page();
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    return Page();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage();
        }
    }
}