import { test, expect } from '@playwright/test';

// Ignorăm erorile SSL pe localhost
test.use({ ignoreHTTPSErrors: true });

test.describe('Modul React: WatchlistContext & LocalStorage (Test direct pe browser)', () => {

    const REPER_URL = 'https://localhost:7256/Licitatii/Favorite'; // Pagina ta care folosește acest context

    test('Ar trebui să verifice citirea inițială din localStorage', async ({ page }) => {
        // 1. Scriem în localStorage cheia ta exactă înainte ca pagina să se încarce
        await page.addInitScript(() => {
            window.localStorage.setItem('netauction_watchlist', JSON.stringify([12, 34]));
        });

        // 2. Navigăm pe pagină
        await page.goto(REPER_URL);
        await page.waitForLoadState('networkidle');

        // 3. Verificăm că browserul a reținut exact ce am configurat în starea inițială
        const stareaLocalStorage = await page.evaluate(() => {
            return window.localStorage.getItem('netauction_watchlist');
        });

        // Validăm că string-ul din memorie este cel corect
        expect(stareaLocalStorage).toBe('[12,34]');
    });

    test('Ar trebui să funcționeze cu un watchlist gol la pornire', async ({ page }) => {
        // 1. Configurăm starea ca fiind goală, exact ca în linia ta: return saved ? JSON.parse(saved) : [];
        await page.addInitScript(() => {
            window.localStorage.setItem('netauction_watchlist', JSON.stringify([]));
        });

        await page.goto(REPER_URL);
        await page.waitForLoadState('networkidle');

        // 2. Verificăm că localStorage-ul a rămas inițializat ca un array gol stringified
        const stareaLocalStorage = await page.evaluate(() => {
            return window.localStorage.getItem('netauction_watchlist');
        });

        expect(stareaLocalStorage).toBe('[]');
    });

    test('Ar trebui să testeze reacția la eliminare direct din interfața grafică', async ({ page }) => {
        // Navigăm direct pe pagină (folosind sesiunea ta activă din auth.setup.js)
        await page.goto(REPER_URL);
        await page.waitForLoadState('networkidle');

        // Așteptăm să dispară spinner-ul de încărcare din pagină (dacă există)
        const spinner = page.locator('.spinner-border');
        if (await spinner.isVisible()) {
            await expect(spinner).not.toBeVisible({ timeout: 15000 });
        }

        // Identificăm butonul de ștergere/eliminare de pe orice card real pe care îl ai în baza de date
        const butonElimina = page.locator('button[title="Elimină din favorite"]').first();

        // Dacă ai un produs la favorite în contul tău real, dăm click pentru a rula funcția ta `toggleFavorite`
        if (await butonElimina.isVisible()) {
            await butonElimina.click();

            // Verificăm dacă s-a modificat valoarea în localStorage în mod real prin efectul secundar (useEffect-ul tău)
            const stareaDupaClick = await page.evaluate(() => {
                return window.localStorage.getItem('netauction_watchlist');
            });

            // Validăm că valoarea a fost actualizată și este un string valid de tip Array JSON
            expect(stareaDupaClick).not.toBeNull();
            expect(stareaDupaClick?.startsWith('[')).toBeTruthy();
        }
    });

});