using Microsoft.AspNetCore.SignalR;

namespace WebApplication1.Hubs
{
    // Hub-ul este punctul central prin care trec mesajele Real-time
    public class AuctionHub : Hub
    {
        // Nu este nevoie de metode aici pentru notificările de tip "Outbid", 
        // deoarece serverul (Controller-ul) va trimite mesaje direct către clienți.
    }
}