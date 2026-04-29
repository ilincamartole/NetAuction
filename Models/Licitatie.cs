using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Licitatie
    {
        public int id { get; set; }

        [Required]
        public String titlu;
        public String descriere;


        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal pret_pornire { get; set; }
        public decimal pret_curent { get; set; }

        public DateTime data_finalizare { get; set; }

        public String seller_id { get; set; }




    }
}
