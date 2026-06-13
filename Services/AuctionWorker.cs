using WebApplication1.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Services
{
    public class AuctionWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        // Setăm intervalul de verificare (ex: la fiecare 10 secunde pentru a nu omorî baza de date)
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);

        public AuctionWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var acum = DateTime.UtcNow;

                    // 1. Căutăm licitațiile care au expirat dar nu sunt încă marcate ca "Încheiate"
                    var licitatiiExpirate = await context.Licitatii
                        .Where(l => l.data_finalizare <= acum && !l.EsteIncheiata)
                        .ToListAsync();

                    foreach (var licitatie in licitatiiExpirate)
                    {
                        // 2. Căutăm cea mai mare ofertă (ultima depusă)
                        var ultimaOferta = await context.Bids
                            .Where(b => b.licitatieId == licitatie.id)
                            .OrderByDescending(b => b.suma)
                            .FirstOrDefaultAsync();

                        if (ultimaOferta != null)
                        {
                            // Criteriu: Marcăm câștigătorul
                            licitatie.CastigatorId = ultimaOferta.userId;
                        }

                        // Criteriu: Marcăm ca încheiată (cu sau fără câștigător)
                        licitatie.EsteIncheiata = true;
                    }

                    if (licitatiiExpirate.Any())
                    {
                        await context.SaveChangesAsync();
                    }
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}