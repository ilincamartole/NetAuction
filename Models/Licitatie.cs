using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Licitatie
    {
        public int id { get; set; }

        [Required]
        public String titlu;
        public String descriere;


        [Column(TypeName = "decimal(18,2)")] // 18 cifre în total, 2 după virgulă
        public decimal PretPornire { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PretCurent { get; set; }

        public DateTime data_finalizare { get; set; }

        public String seller_id { get; set; }




    }
}
