using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Services
{
    public class AuctionWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
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
                    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    var acum = DateTime.Now;

                    var licitatiiExpirate = await context.Licitatii
                        .Where(l => l.data_finalizare <= acum && (!l.EsteIncheiata || !l.NotificareTrimisa))
                        .ToListAsync(stoppingToken);

                    foreach (var licitatie in licitatiiExpirate)
                    {
                        if (string.IsNullOrEmpty(licitatie.CastigatorId))
                        {
                            var ultimaOferta = await context.Bids
                                .Where(b => b.licitatieId == licitatie.id)
                                .OrderByDescending(b => b.suma)
                                .FirstOrDefaultAsync(stoppingToken);

                            if (ultimaOferta != null)
                            {
                                licitatie.CastigatorId = ultimaOferta.userId;
                            }
                        }

                        licitatie.EsteIncheiata = true;

                        if (!string.IsNullOrEmpty(licitatie.CastigatorId) && !licitatie.NotificareTrimisa)
                        {
                            var castigator = await userManager.FindByIdAsync(licitatie.CastigatorId);
                            var vanzator = await userManager.FindByIdAsync(licitatie.seller_id);

                            bool emailCumpatorTrimis = false;
                            bool emailVanzatorTrimis = false;

                            // 1. NOTIFICARE + EMAIL CUMPĂRĂTOR
                            if (castigator != null && !string.IsNullOrEmpty(castigator.Email))
                            {
                                try
                                {
                                    string subiectCumparator = $"Felicitări! Ai câștigat licitația: {licitatie.titlu}";
                                    string mesajCumparator = $@"<h2>Felicitări!</h2><p>Ai câștigat licitația pentru <strong>{licitatie.titlu}</strong> la prețul de {licitatie.PretCurent} RON.</p>";
                                    await emailSender.SendEmailAsync(castigator.Email, subiectCumparator, mesajCumparator);
                                    emailCumpatorTrimis = true;
                                }
                                catch (Exception ex) { Console.WriteLine(ex.Message); }

                                // ADĂUGARE ÎN SECȚIUNEA WEB
                                context.Notificari.Add(new Notificare
                                {
                                    UserId = licitatie.CastigatorId,
                                    Mesaj = $"🎉 Felicitări! Ai câștigat licitația pentru '{licitatie.titlu}' cu suma de {licitatie.PretCurent} RON.",
                                    Link = $"/Licitatii/Details/{licitatie.id}"
                                });
                            }

                            // 2. NOTIFICARE + EMAIL VÂNZĂTOR
                            if (vanzator != null && !string.IsNullOrEmpty(vanzator.Email))
                            {
                                try
                                {
                                    string subiectVanzator = $"Licitația ta s-a încheiat cu succes! 🎉 {licitatie.titlu}";
                                    string mesajVanzator = $@"<h2>Felicitări!</h2><p>Produsul tău <strong>{licitatie.titlu}</strong> a fost vândut cu succes pentru {licitatie.PretCurent} RON.</p>";
                                    await emailSender.SendEmailAsync(vanzator.Email, subiectVanzator, mesajVanzator);
                                    emailVanzatorTrimis = true;
                                }
                                catch (Exception ex) { Console.WriteLine(ex.Message); }

                                // ADĂUGARE ÎN SECȚIUNEA WEB
                                context.Notificari.Add(new Notificare
                                {
                                    UserId = licitatie.seller_id,
                                    Mesaj = $"💰 Licitația ta pentru '{licitatie.titlu}' s-a încheiat cu succes! Produsul a fost vândut pentru {licitatie.PretCurent} RON.",
                                    Link = $"/Licitatii/Details/{licitatie.id}"
                                });
                            }

                            if (emailVanzatorTrimis || emailCumpatorTrimis)
                            {
                                licitatie.NotificareTrimisa = true;
                            }
                        }
                        else if (string.IsNullOrEmpty(licitatie.CastigatorId))
                        {
                            licitatie.NotificareTrimisa = true;
                        }
                    }

                    if (licitatiiExpirate.Any())
                    {
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}