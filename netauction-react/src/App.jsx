import { test, expect } from '@playwright/test';

// 👑 SOLUȚIE CONTEXT: Activăm ignorarea erorilor SSL la nivel global în fișier (evită eroarea din imagine_96a0a7.png)
test.use({ ignoreHTTPSErrors: true });

test.describe('Modul AI: Ghid de Cumpărături Inteligent', () => {

    test.beforeEach(async ({ page }) => {
        // Injectăm cookie-ul de sesiune valid pentru .NET Identity
        const context = page.context();
        await context.addCookies([
            {
                name: '.AspNetCore.Identity.Application',
                value: 'CfDJ8NJG...', // ⚠️ Pune aici valoarea activă din browser-ul tău
                domain: 'localhost',
                path: '/',
                httpOnly: true,
                secure: true,
                sameSite: 'Lax'
            }
        ]);

        // Navigăm direct pe pagina asistentului AI (actualizează ruta dacă diferă pe local)
        await page.goto('https://localhost:7256/Home/Asistent');
        await page.waitForLoadState('networkidle');
    });

    test('Ar trebui să afișeze o eroare toast dacă preferințele sunt goale', async ({ page }) => {
        // Găsim butonul în mod stabil după clasa de bază definită în React (.hover-up-btn)
        const butonGenereaza = page.locator('.hover-up-btn');

        await expect(butonGenereaza).toBeVisible({ timeout: 5000 });
        await butonGenereaza.click();

        // Validăm apariția toast-ului de eroare declanșat de handleTrimite când preferinte.trim() este gol
        const toastEroare = page.locator('text=Te rog să descrii ce anume te interesează!');
        await expect(toastEroare).toBeVisible({ timeout: 5000 });
    });

    test('Ar trebui să completeze formularul și să primească cu succes raportul AI', async ({ page }) => {
        // 1. Selectăm categoria din dropdown (Valoarea "1" corespunde opțiunii "Auto")
        const dropdownCategorie = page.locator('select.form-select');
        await dropdownCategorie.selectOption('1');

        // 2. Completăm bugetul alocat (RON)
        const inputBuget = page.locator('input[type="number"]');
        await inputBuget.fill('45000');

        // 3. Completăm specificațiile cerute în textarea
        const textPreferinte = page.locator('textarea.form-control');
        await textPreferinte.fill('Caut o mașină compactă, an după 2018, consum redus, exclus cutie manuală.');

        // 4. Apăsăm butonul stabil din componentă
        const butonGenereaza = page.locator('.hover-up-btn');
        await butonGenereaza.click();

        // 5. Verificăm schimbarea textului din buton (loading === true)
        await expect(butonGenereaza).toHaveText('Se generează raportul...');

        // Verificăm apariția mesajului de procesare din panoul drept al raportului
        const textLoadingRaport = page.locator('text=Se parsează datele din piață...');
        await expect(textLoadingRaport).toBeVisible();

        // 6. Sincronizare cu API-ul: Așteptăm mai întâi toast-ul care anunță finalizarea fetch-ului.
        // Îi oferim un timeout extins (45 secunde) pentru a acoperi timpul de calcul al LLM-ului
        const toastSucces = page.locator('text=Ghidul de achiziție a fost finalizat!');
        await expect(toastSucces).toBeVisible({ timeout: 45000 });

        // 7. Validăm că containerul `.leading-relaxed` care găzduiește componenta <TypewriterHtml /> 
        // s-a montat și este vizibil pe ecran
        const zonaRezultatRaport = page.locator('.leading-relaxed');
        await expect(zonaRezultatRaport).toBeVisible({ timeout: 5000 });
    });

});