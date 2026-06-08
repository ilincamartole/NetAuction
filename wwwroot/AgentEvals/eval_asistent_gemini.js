require('dotenv').config();
const fs = require('fs');
const path = require('path');

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const URL_ASISTENT_LOCAL = 'https://localhost:7256/api/ai/consultanta-cumparaturi';

// 🔒 Extragem cheia Gemini din secrete
const CHEIE_API_GEMINI = process.env.GEMINI_API_KEY;

if (!CHEIE_API_GEMINI) {
    console.error("❌ EROARE: Cheia GEMINI_API_KEY nu a fost găsită în fișierul .env!");
    process.exit(1);
}

async function apeleazaJudecatorulGemini(mesajUser, raspunsAsistent, rubrica) {
    const promptAudit = `
    Esti un expert in Asigurarea Calitatii (QA) pentru aplicatii AI.
    Rolul tau este sa evaluezi daca asistentul nostru a raspuns corect cerintelor utilizatorului.

    CONTEXT EVALUARE:
    1. Mesajul utilizatorului: "${mesajUser}"
    2. Raspunsul generat de asistentul nostru: "${raspunsAsistent}"
    3. Regula stricta pe care trebuia sa o respecte (Rubrica): "${rubrica}"

    Te rog sa analizezi textul si sa raspunzi STRICT in urmatorul format JSON (fara alte cuvinte ajutatoare sau blocuri de cod tip markdown):
    {
      "trecut": true sau false,
      "explicatie_critica": "Detaliaza argumentat de ce a trecut sau de ce a picat testul"
    }
  `;

    // URL-ul oficial pentru modelul gratuit și rapid Gemini 1.5 Flash
    const urlGemini = `https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=${CHEIE_API_GEMINI}`;

    try {
        const response = await fetch(urlGemini, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                contents: [{ parts: [{ text: promptAudit }] }],
                generationConfig: {
                    responseMimeType: "application/json" // Îi ordonăm lui Gemini să scoată doar JSON
                }
            })
        });

        const data = await response.json();

        // Extragem textul brut din structura de răspuns a Google API
        const textJsonBrut = data.candidates[0].content.parts[0].text;
        return JSON.parse(textJsonBrut.trim());
    } catch (e) {
        return { trecut: false, explicatie_critica: "Eroare la comunicarea cu Google Gemini API: " + e.message };
    }
}

async function pornesteEvaluareGemini() {
    const caleaDate = path.join(__dirname, 'dataset_asistent.json');
    const dataset = JSON.parse(fs.readFileSync(caleaDate, 'utf-8').trim().replace(/^\uFEFF/, ''));

    console.log('👑 Pornire Agent Evals cu Google Gemini (Gratuit în Cloud)...');
    let succese = 0;

    for (const caz of dataset) {
        try {
            console.log(`\n--------------------------------------------------`);
            console.log(`⏳ Se trimite scenariul: [${caz.scenariu}] către asistentul local .NET...`);

            // 1. Preluăm răspunsul de la asistentul tău local
            const resAsistent = await fetch(URL_ASISTENT_LOCAL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ mesaj: caz.mesaj_utilizator })
            });

            if (!resAsistent.ok) {
                console.log(`❌ Eroare la asistentul .NET (Status: ${resAsistent.status})`);
                continue;
            }

            const dataAsistent = await resAsistent.json();
            const textGeneratDeAsistent = dataAsistent.raspuns || dataAsistent.text;

            // 2. Trimitem totul la judecătorul Gemini din cloud-ul Google
            console.log(`🧠 Gemini 1.5 Flash analizează răspunsul...`);
            const verdict = await apeleazaJudecatorulGemini(caz.mesaj_utilizator, textGeneratDeAsistent, caz.rubrica_evaluare);

            console.log(`🎬 Scenariu: ${caz.scenariu}`);
            console.log(`📊 Verdict AI-Judecător: ${verdict.trecut ? '✅ TRECUT' : '❌ PICAT'}`);
            console.log(`📝 Motivație: ${verdict.explicatie_critica}`);

            if (verdict.trecut) succese++;

        } catch (err) {
            console.error(`💥 Eroare pe parcursul testului:`, err.message);
        }
    }

    console.log(`\n==================================================`);
    console.log(`🏆 SCOR FINAL EVALS GEMINI: ${succese} din ${dataset.length} scenarii trecute.`);
    console.log(`==================================================`);
}

pornesteEvaluareGemini();