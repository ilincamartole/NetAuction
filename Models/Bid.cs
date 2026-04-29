using System.ComponentModel.DataAnnotations;
namespace WebApplication1.Models
{
    public class Bid
    {   
        public int id { get; set; }


        [Required]
        public decimal suma { get; set; }

        public DateTime data { get; set; }

        public int licitatieId { get; set; }

        public Licitatie licitatie { get; set; }

        public string userId { get; set; }

    }
}
