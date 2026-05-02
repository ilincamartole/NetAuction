namespace WebApplication1.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int Nota { get; set; } // de la 1 la 5
        public string Comentariu { get; set; }
        public DateTime DataPublicarii { get; set; } = DateTime.Now;

        // Cine primește rating-ul (Vânzătorul)
        public string SellerId { get; set; }
        public ApplicationUser Seller { get; set; }

        // Cine lasă rating-ul (Cumpărătorul)
        public string BuyerId { get; set; }
        public ApplicationUser Buyer { get; set; }
    }
}
