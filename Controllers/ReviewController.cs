using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. POST: api/Reviews (Adaugă un review nou)
        [HttpPost]
        [Authorize] // Doar utilizatorii logați pot lăsa review-uri
        public async Task<IActionResult> PostReview([FromBody] CreateReviewDto dto)
        {
            // Preluăm ID-ul utilizatorului logat curent (Cumpărătorul)
            var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized("Trebuie să fii autentificat pentru a lăsa un review.");

            if (buyerId == dto.SellerId)
                return BadRequest("Nu îți poți lăsa review singur.");

            // Validare notă
            if (dto.Nota < 1 || dto.Nota > 5)
                return BadRequest("Nota trebuie să fie între 1 și 5.");

            var review = new Review
            {
                Nota = dto.Nota,
                Comentariu = dto.Comentariu,
                SellerId = dto.SellerId,
                BuyerId = buyerId,
                DataPublicarii = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Review-ul a fost adăugat cu succes!" });
        }

        // 2. GET: api/Reviews/seller/{sellerId} (Returnează review-urile unui vânzător - ANONIM)
        [HttpGet("seller/{sellerId}")]
        public async Task<IActionResult> GetSellerReviews(string sellerId)
        {
            // Selectăm doar câmpurile sigure, lăsând BuyerId și obiectul Buyer deoparte pentru anonimat!
            var reviews = await _context.Reviews
                .Where(r => r.SellerId == sellerId)
                .OrderByDescending(r => r.DataPublicarii)
                .Select(r => new
                {
                    r.Id,
                    r.Nota,
                    r.Comentariu,
                    r.DataPublicarii
                })
                .ToListAsync();

            return Ok(reviews);
        }
    }

    // Un obiect simplu (DTO) pentru a primi datele din React curat
    public class CreateReviewDto
    {
        public int Nota { get; set; }
        public string Comentariu { get; set; }
        public string SellerId { get; set; }
    }
}