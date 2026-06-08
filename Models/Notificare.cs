using System;

namespace WebApplication1.Models
{
    public class Notificare
    {
        public int Id { get; set; }

        // ID-ul utilizatorului care va primi notificarea (Vânzător sau Cumpărător)
        public string UserId { get; set; }

        public string Mesaj { get; set; }

        public DateTime DataCreare { get; set; } = DateTime.Now;

        public bool EsteCitita { get; set; } = false;

        public string? Link { get; set; }
    }
}