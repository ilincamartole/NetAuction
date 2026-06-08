using System.ComponentModel.DataAnnotations;
namespace WebApplication1.Models


{
    public enum CategorieLicitatie
    {
        Electronice,
        Auto,
        Imobiliare,
        Moda,
        Sport,
        Bijuterii,
        Arta,
        Casa,
        Altele
    }
    public class Licitatie
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Titlul trebuie să aibă între 5 și 100 de caractere.")]
        public string titlu { get; set; }

        [Required(ErrorMessage = "Descrierea este obligatorie.")]
        public string descriere { get; set; }

        [Required(ErrorMessage = "Prețul este obligatoriu.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Prețul de pornire nu poate fi negativ sau zero.")]
        public decimal PretPornire { get; set; }

        public decimal PretCurent { get; set; }

        [Required(ErrorMessage = "Data de finalizare este obligatorie.")]
        public DateTime data_finalizare { get; set; }

        public bool IsPaymentConfirmed { get; set; } = false;
        public string seller_id { get; set; }

        // Câmp pentru stocarea numelui imaginii în DB
        public string? ImaginePath { get; set; }

        [Required(ErrorMessage = "Selectarea unei categorii este obligatorie.")]
        public CategorieLicitatie Categorie { get; set; }

        public string? CastigatorId { get; set; } // ID-ul userului care a câștigat
        public bool EsteIncheiata { get; set; } = false; // Flag pentru status

        public bool NotificareTrimisa { get; set; } = false;
    }
}