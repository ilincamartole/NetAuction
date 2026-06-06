using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public AiController(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("sugereaza-pret")]
        public async Task<IActionResult> SugereazaPret([FromBody] SugereazaPretRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Titlu))
                return BadRequest("Titlul este gol.");

// var apiKey = _config["Google:GeminiApiKey"];
            var apiKey = _config["Google:GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return StatusCode(500, "Cheia API nu a fost găsită în configurație.");
            var client = _httpClientFactory.CreateClient();

            var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={apiKey}";
            var prompt = $@"Ești un expert în evaluarea prețurilor pentru licitații online din România.
Bazat pe denumirea produsului ""{request.Titlu}"", estimează un preț de pornire rezonabil în RON (lei românești).
Răspunde DOAR cu un număr întreg, fără text, fără simbolul RON, fără puncte sau virgule. Exemplu: 150";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            //using var doc = JsonDocument.Parse(responseJson);
            //var text = doc.RootElement
            //    .GetProperty("candidates")[0]
            //    .GetProperty("content")
            //    .GetProperty("parts")[0]
            //    .GetProperty("text")
            //    .GetString();

            //if (decimal.TryParse(text?.Trim(), out var pret))
            //    return Ok(new { pret });

            //return BadRequest("AI-ul nu a putut genera un preț valid.");

            using var doc = JsonDocument.Parse(responseJson);

            // Returnează răspunsul brut dacă ceva nu merge, ca să vedem exact eroarea
            if (!doc.RootElement.TryGetProperty("candidates", out var candidates))
                return StatusCode(500, "Raspuns neasteptat de la Gemini: " + responseJson);

            var text = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (decimal.TryParse(text?.Trim(), out var pret))
                return Ok(new { pret });

            return BadRequest("AI-ul nu a putut genera un preț valid. Răspuns: " + text);
        }
    }

    public class SugereazaPretRequest
    {
        public string Titlu { get; set; }
    }
}