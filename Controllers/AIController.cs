using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _context;

        public AiController(IConfiguration config, IHttpClientFactory httpClientFactory, ApplicationDbContext context)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        // =======================================================
        // 🤖 AGENT 1: EVALUATOR PREȚ (Gemini - Mod Rigid, Temp 0.1)
        // =======================================================
        [HttpPost("sugereaza-pret")]
        public async Task<IActionResult> SugereazaPret([FromBody] SugereazaPretRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Titlu))
                return BadRequest("Titlul este gol.");

            var apiKey = _config["Google:GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return StatusCode(500, "Cheia API Gemini nu a fost găsită în configurație (Google:GeminiApiKey).");

            var prompt = $@"CONTEXT: Ești un algoritm rigid de evaluare financiară.
Sarcina ta este să analizezi titlul produsului: ""{request.Titlu}"" și să estimezi un preț de pornire rezonabil în RON.
REGULĂ ABSOLUTĂ: Răspunde DOAR cu numărul întreg, fără niciun alt cuvânt în plus, fără simbolul RON, fără puncte sau virgule. Exemplu: 150";

            try
            {
                string aiResponse = await ApeleazaGeminiCoreAsync(prompt, temperatura: 0.1);

                string textCurat = aiResponse.Trim();
                textCurat = Regex.Replace(textCurat, @"```[a-zA-Z]*", "");
                textCurat = textCurat.Replace("```", "");
                var match = Regex.Match(textCurat, @"\d+");

                if (match.Success && decimal.TryParse(match.Value, out var pret))
                {
                    return Ok(new { pret });
                }

                return BadRequest("Agentul Evaluator nu a putut genera un preț numeric valid.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Eroare Agent Evaluator Gemini: " + ex.Message);
            }
        }

        // =======================================================
        // 🧠 AGENT 2: GHID ACHIZIȚII (Gemini - Mod Analitic, Temp 0.7)
        // =======================================================
        [HttpPost("consultanta-cumparaturi")]
        public async Task<IActionResult> ConsultantaCumparaturi([FromBody] AiConsultingRequest request)
        {
            var apiKey = _config["Google:GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return StatusCode(500, "Cheia API Gemini nu a fost găsită în configurație (Google:GeminiApiKey).");

            var acum = DateTime.UtcNow;
            var licitatiiQuery = _context.Licitatii
                .Where(l => l.Categorie == request.Categorie && !l.EsteIncheiata && l.data_finalizare > acum);

            if (request.BugetMaxim.HasValue && request.BugetMaxim.Value > 0)
            {
                licitatiiQuery = licitatiiQuery.Where(l => l.PretCurent <= request.BugetMaxim.Value);
            }

            var produseDisponibile = await licitatiiQuery
                .Select(l => new { l.id, l.titlu, l.descriere, l.PretCurent })
                .ToListAsync();

            if (!produseDisponibile.Any())
            {
                return Ok(new { raspuns = "<p class='text-warning text-center my-4'><i class='bi bi-exclamation-triangle fs-2'></i><br>Momentan nu am găsit licitații active în această categorie care să se încadreze în bugetul tău.</p>" });
            }

            var sbProduse = new StringBuilder();
            foreach (var p in produseDisponibile)
            {
                sbProduse.AppendLine($"- [ID: {p.id}] \"{p.titlu}\" | Descriere: {p.descriere} | Preț Curent: {p.PretCurent} RON");
            }

            var prompt = $@"
ROLE: Ești un asistent inteligent de cumpărături, un consultant personal integrat pe platforma NetAuction.
Misiunea ta este să analizezi dorințele utilizatorului și să-i generezi un ghid de achiziție pe baza oportunităților reale transmise mai jos.

Preferințele cumpărătorului: ""{request.PreferinteUser}""
Buget maxim specificat: {(request.BugetMaxim.HasValue ? request.BugetMaxim.Value + " RON" : "Fără limită")}
Categoria selectată: {request.Categorie}

Iată produsele disponibile acum pe platformă:
{sbProduse.ToString()}

Instrucțiuni obligatorii de formatare:
1. Limba și Tonul: Răspunde direct în limba română. Fii extrem de analitic, oferă argumente pragmatice și folosește emoji-uri native pentru design.
2. Analiză Calitate-Preț: Compară produsele trimise în listă și explică clar de ce un anumit obiect reprezintă o afacere excelentă sau o potrivire bună pentru nevoile exprimate.
3. Strategie de Licitație: Oferă sfaturi de acțiune specifice și tactice (ex: când să plaseze oferta ca să nu crească prețul artificial).
4. Regula HTML: Pentru fiecare produs pe care îl recomanzi, generează un link HTML valid stilizat Bootstrap utilizând exact acest format: <a href='/Licitatii/Details/ID_PRODUS' class='btn btn-sm btn-primary text-white my-1'><i class='bi bi-gavel'></i> Vezi Licitația pentru TITLU_PRODUS</a>. Înlocuiește corect ID_PRODUS și TITLU_PRODUS cu datele reale din listă.
5. FĂRĂ MARKDOWN: Nu folosi absolut deloc formatare de tip Markdown (fără **, fără ###). Folosește doar taguri HTML simple (<strong>, <p>, <ul>, <li>).";

            try
            {
                string raspunsAi = await ApeleazaGeminiCoreAsync(prompt, temperatura: 0.7);
                return Ok(new { raspuns = raspunsAi });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Eroare la generarea ghidului de către Agentul Gemini: " + ex.Message);
            }
        }

        // =======================================================
        // ⚙️ ENGINE REUTILIZABIL (Controlul Temperaturii)
        // =======================================================
        private async Task<string> ApeleazaGeminiCoreAsync(string prompt, double temperatura)
        {
            var apiKey = _config["Google:GeminiApiKey"];
            var client = _httpClientFactory.CreateClient();
            var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={apiKey}";

            var body = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { temperature = temperatura }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Gemini API Error [Status {response.StatusCode}]: {responseJson}");
            }

            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? string.Empty;
        }
    }

    public class SugereazaPretRequest { public string Titlu { get; set; } }
    public class AiConsultingRequest { public CategorieLicitatie Categorie { get; set; } public decimal? BugetMaxim { get; set; } public string PreferinteUser { get; set; } }
}