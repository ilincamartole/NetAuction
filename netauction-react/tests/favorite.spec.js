import { test, expect } from '@playwright/test';

test.describe('Modul React: Produse Favorite (Bază de date REALE)', () => {

    const FAVORITE_URL = 'https://localhost:7256/Licitatii/Favorite';

    test('Ar trebui să verifice structura paginii și să se adapteze la datele reale din cont', async ({ page }) => {

        // 1. Navigăm pe pagina de favorite (Autentificarea se face automat prin auth.setup.js)
        await page.goto(FAVORITE_URL);

        // 2. Așteptăm ca toate cererile asincrone (fetch/XHR) către baza de date să fie finalizate
        await page.waitForLoadState('networkidle');

        // 3. Verificăm că elementul de bază al paginii (titlul) s-a încărcat corect
        const titluPagina = page.getByRole('heading', { name: 'Produse Favorite' });
        await expect(titluPagina).toBeVisible({ timeout: 15000 });

        // --- LOGICĂ DINAMICĂ ÎN FUNCȚIE DE CE AI ÎN BAZA DE DATE ---
        // Numărăm câte carduri de produse au fost randate efectiv pe ecran
        const numarCarduri = await page.locator('.card:has-text("Miză Curentă")').count();

        if (numarCarduri === 0) {
            // 🟢 CAZUL A: Baza de date nu are produse favorite pentru acest cont
            console.log('ℹ️ Contul de test nu are produse favorite în baza de date. Se verifică starea goală.');

            await expect(page.getByRole('heading', { name: 'Lista ta este goală' })).toBeVisible({ timeout: 5000 });
            await expect(page.getByText('Explorează piața și folosește inimioara')).toBeVisible();

        } else {
            // 🔵 CAZUL B: Contul are produse salvate în mod real
            console.log(`\u2705 S-au găsit ${numarCarduri} produse reale în baza de date.`);

            // Verificăm că primul card din listă are elementele structurale la locul lor
            const primulCard = page.locator('.card').first();
            await expect(primulCard).toBeVisible();

            // Verificăm prezența categoriei (badge-ul de sus) și a prețului real din SQL
            await expect(primulCard.locator('.badge.bg-light')).toBeVisible();
            await expect(primulCard.locator('text=RON')).toBeVisible();

            // Verificăm că butoanele de acțiune specifice fiecărui card funcționează structural
            const butonDeschide = primulCard.getByRole('link', { name: /Deschide/i });
            await expect(butonDeschide).toBeVisible();
            await expect(butonDeschide).toHaveAttribute('href', /\/Licitatii\/Details\//);

            // --- TESTARE FILTRARE (DOAR DACĂ EXISTĂ PASTILE DE CATEGORII) ---
            const pastilaToate = page.getByRole('button', { name: 'Toate', exact: true });
            if (await pastilaToate.isVisible()) {
                await expect(pastilaToate).toBeVisible();
                // Dăm click pe ea ca să ne asigurăm că componenta React nu crapă la interacțiune
                await pastilaToate.click();
            }
        }
    });

    test('Ar trebui să verifice existența butonului de navigare înapoi la licitații', async ({ page }) => {
        await page.goto(FAVORITE_URL);
        await page.waitForLoadState('networkidle');

        // Verificăm butonul „Adaugă produse” care te trimite înapoi la Index-ul de licitații real
        const butonAdauga = page.getByRole('link', { name: /Adaugă produse/i });
        await expect(butonAdauga).toBeVisible();
        await expect(butonAdauga).toHaveAttribute('href', '/Licitatii/Index');
    });

});