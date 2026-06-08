const fs = require('fs');
const path = require('path');

// Ignorăm certificatele SSL nesemnate de pe localhost-ul de .NET
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

// Ruta către AIController-ul tău din imagine
const URL_API = 'https://localhost:7256/api/ai/sugereaza-pret';

async function ruleazaEvaluareAI() {
    const caleaDate = path.join(__dirname, 'dataset_preturi.json');
    let textBrut = fs.readFileSync(caleaDate, 'utf-8');

    // Elimină caracterele BOM și spațiile ascunse de la început/sfârșit
    textBrut = textBrut.trim().replace(/^\uFEFF/, '');

    const dataset = JSON.parse(textBrut);

    console.log('🚀 Pornire Agent Evals nativ pentru AIController .NET...');
    let testeTrecute = 0;

    for (const caz of dataset) {
        try {
            const response = await fetch(URL_API, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ titlu: caz.nume_produs })
            });

            if (!response.ok) {
                console.log(`❌ [EROARE API] Pentru: "${caz.nume_produs}" - Status HTTP: ${response.status}`);
                continue;
            }

            const data = await response.json();
            const pretAI = parseFloat(data.pret);
            const esteCorect = pretAI >= caz.pret_minim && pretAI <= caz.pret_maxim;

            console.log(`\n--------------------------------------------------`);
            console.log(`📦 Produs: "${caz.nume_produs}"`);
            console.log(`🤖 Preț sugerat de AI: ${pretAI} RON`);
            console.log(`📊 Interval acceptat: [${caz.pret_minim} - ${caz.pret_maxim}] RON`);

            if (esteCorect) {
                console.log('✅ REZULTAT: TRECUT');
                testeTrecute++;
            } else {
                console.log('❌ REZULTAT: PICAT (Halucinație de preț / Prompt instabil)');
            }

        } catch (error) {
            console.error(`💥 Eroare conexiune pentru "${caz.nume_produs}":`, error.message);
        }
    }

    console.log(`\n==================================================`);
    console.log(`🏆 EVALUARE FINALIZATĂ: ${testeTrecute} din ${dataset.length} teste trecute.`);
    console.log(`📈 Rata de succes a modelului: ${(testeTrecute / dataset.length) * 100}%`);
    console.log(`==================================================`);
}

ruleazaEvaluareAI();