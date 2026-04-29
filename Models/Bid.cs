using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WebApplication1.Models
{
    public class Bid
    {   
        public int id { get; set; }


        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal suma { get; set; }

        public DateTime data { get; set; }

        public int licitatieId { get; set; }

        public Licitatie licitatie { get; set; }

        public string userId { get; set; }

    }
}
