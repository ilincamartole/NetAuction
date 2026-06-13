using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System;

namespace WebApplication1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public String nume { get; set; }
        public String prenume { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal balance { get; set; }
        public String adresa { get; set; }

        public DateTime data_inregistrarii { get; set; } = DateTime.UtcNow;

        public ApplicationUser()
        {
            balance = 0;
            ReviewsPrimite = new List<Review>(); // Inițializare corectă în constructor
        }

        // Păstrăm DOAR această declarare (am șters-o pe cea duplicată de jos)
        public virtual ICollection<Review> ReviewsPrimite { get; set; }

        public string? ProfilePicture { get; set; }

        public bool IsSuspended { get; set; } = false;
    }
}