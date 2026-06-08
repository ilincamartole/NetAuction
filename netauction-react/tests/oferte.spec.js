import { test, expect } from '@playwright/test';

// 👑 SOLUȚIE GLOBALĂ: Ignorăm erorile SSL globale pe localhost pentru a permite încărcarea corectă a resurselor
test.use({ ignoreHTTPSErrors: true });

test.describe('Modul React: Dashboard Oferte (Bază de date REALE)', () => {

    const DASHBOARD_URL = 'https://localhost:7256/Licitatii/DashboardCumparator';

    test('Ar trebui să încarce istoricul și graficele direct din baza de date reală', async ({ page }) => {

        // 1. Navigăm pe pagina de oferte (autentificarea se aplică automat din auth.setup.js)
        await page.goto(DASHBOARD_URL);

        // 2. Așteptăm ca spinner-ul de loading din React să dispară și rețeaua să se liniștească
        await page.waitForLoadState('networkidle');

        // 3. Verificăm că titlul principal al panoului este vizibil
        const titluPagina = page.getByRole('heading', { name: 'Activitatea Mea' });
        await expect(titluPagina).toBeVisible({ timeout: 15000 });

        // --- LOGICĂ DINAMICĂ ÎN FUNCȚIE DE DATELE REALE DIN SQL ---
        // Verificăm dacă există textul de „stare goală” pe ecran
        const esteStareGoala = await page.getByRole('heading', { name: 'Nu ai nicio activitate încă' }).isVisible();

        if (esteStareGoala) {
            // 🟢 CAZUL A: Utilizatorul nu are nicio miză/licitație în baza de date
            console.log('ℹ️ Contul nu are mize în SQL. Se verifică textul de fallback.');

            // 👑 REPARARE EFICIENTĂ: Folosim regex parțial pentru a evita erorile de spațiere sau punctuație
            await expect(page.getByText(/Începe să licitezi pe site/i)).toBeVisible({ timeout: 5000 });

        } else {
            // 🔵 CAZUL B: Utilizatorul are istoric real în baza de date
            console.log('✅ S-au detectat mize reale în baza de date. Se verifică componentele.');

            // Verificăm că contoarele de sus s-au încărcat (nu mai sunt goale sau pe starea de loading)
            await expect(page.locator('h3.text-primary')).toBeVisible();
            await expect(page.locator('h3.text-success')).toBeVisible();

            // Verificăm dacă tabelul conține cel puțin o înregistrare (un rând de date)
            const primulRandTabel = page.locator('tbody tr').first();
            await expect(primulRandTabel).toBeVisible();

            // Verificăm că butonul „Fișă produs” conține link-ul corect către ruta de .NET
            const butonFisa = primulRandTabel.getByRole('link', { name: /Fișă produs/i });
            await expect(butonFisa).toBeVisible();
            await expect(butonFisa).toHaveAttribute('href', /\/Licitatii\/Details\//);

            // Verificăm dacă graficul circular (PieChart din Recharts) s-a randat cu succes
            const containerGrafic = page.locator('.recharts-responsive-container');
            await expect(containerGrafic).toBeVisible();

            // --- TESTARE INTERACȚIUNE CU BUTOANELE DE FILTRARE ---
            // Apăsăm pe pastila „Toate” ca să ne asigurăm că interfața React răspunde la click
            const butonToate = page.getByRole('button', { name: /Toate/i }).first();
            await expect(butonToate).toBeVisible();
            await butonToate.click();
        }
    });

});