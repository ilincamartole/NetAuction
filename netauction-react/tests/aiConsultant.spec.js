import { test, expect } from '@playwright/test';

test.describe('Modul AI: Ghid de Cumpărături Inteligent', () => {

    // Înainte de fiecare test, configurăm interceptările de rețea și navigăm pe pagină
    test.beforeEach(async ({ page }) => {
        // Înlocuiește URL-ul de mai jos cu cel pe care rulează aplicația ta local (ex: http://localhost:3000)
        await page.goto('https://localhost:7256/Licitatii/Asistent');
        await page.waitForLoadState('networkidle');
    });

    test('Ar trebui să verifice starea inițială a componentelor', async ({ page }) => {
        // Verificăm dacă titlul panoului din stânga și cel al raportului sunt vizibile
        await expect(page.locator('h3:has-text("Ghid de Cumpărături Inteligent")')).toBeVisible();
        await expect(page.locator('text=Raportul Tău Personalizat')).toBeVisible();

        // Verificăm dacă textul de tip placeholder ("Sistem pregătit") este pe ecran
        await expect(page.locator('h5:has-text("Sistem pregătit")')).toBeVisible();
        await expect(page.locator('text=Completează criteriile din panoul din stânga')).toBeVisible();
    });

    test('Ar trebui să afișeze o eroare toast dacă textarea pentru preferințe este goală', async ({ page }) => {
        const butonGenereaza = page.locator('.hover-up-btn');

        // Apăsăm direct pe buton fără să completăm preferințele
        await butonGenereaza.click();

        // Verificăm mesajul de eroare generat de toast.error() din handleTrimite
        const toastEroare = page.locator('text=Te rog să descrii ce anume te interesează!');
        await expect(toastEroare).toBeVisible({ timeout: 5000 });
    });

    test('Ar trebui să completeze formularul și să randeze cu succes raportul primit de la API', async ({ page }) => {
        // Macheta (Mock) pentru răspunsul de la API-ul AI
        const mockResponse = {
            raspuns: '<h3>Recomandare Auto</h3><p>Pe baza bugetului de <strong>45000 RON</strong>, îți recomandăm o mașină compactă din anul 2019...</p>'
        };

        // Interceptăm ruta POST către API și întoarcem un răspuns controlat de noi
        await page.route('**/api/ai/consultanta-cumparaturi', async route => {
            // Verificăm dacă payload-ul trimis către server este cel corect
            const postData = route.request().postDataJSON();
            expect(postData).toEqual({
                Categorie: 1, // Valoarea întreagă pentru "Auto" (parseInt("1"))
                BugetMaxim: 45000, // Transformat în float
                PreferinteUser: 'Caut o mașină compactă, an după 2018, consum redus, cutie automată.'
            });

            // Trimitem răspunsul mockat cu un mic delay artificial pentru a prinde starea de loading
            setTimeout(async () => {
                await route.fulfill({
                    status: 200,
                    contentType: 'application/json',
                    body: JSON.stringify(mockResponse)
                });
            }, 1000);
        });

        // 1. Selectăm categoria din dropdown (Valoarea "1" corespunde opțiunii "Auto")
        const dropdownCategorie = page.locator('select.form-select');
        await dropdownCategorie.selectOption('1');

        // 2. Completăm bugetul alocat (RON)
        const inputBuget = page.locator('input[type="number"]');
        await inputBuget.fill('45000');

        // 3. Completăm specificațiile cerute în textarea
        const textPreferinte = page.locator('textarea.form-control');
        await textPreferinte.fill('Caut o mașină compactă, an după 2018, consum redus, cutie automată.');

        // 4. Apăsăm butonul de generare
        const butonGenereaza = page.locator('.hover-up-btn');
        await butonGenereaza.click();

        // 5. Verificăm apariția textelor specifice stării de LOADING
        await expect(butonGenereaza).toHaveText('Se generează raportul...');
        await expect(page.locator('text=Se parsează datele din piață...')).toBeVisible();

        // 6. Sincronizare cu finalizarea API-ului: Așteptăm toast-ul de succes
        const toastSucces = page.locator('text=Ghidul de achiziție a fost finalizat!');
        await expect(toastSucces).toBeVisible({ timeout: 10000 });

        // 7. Validăm că componenta TypewriterHtml a randat corect textul HTML în interiorul containerului raportului
        const zonaRezultatRaport = page.locator('.leading-relaxed');
        await expect(zonaRezultatRaport).toBeVisible();

        // Verificăm dacă textul randat în mod dinamic prin dangerouslySetInnerHTML conține datele noastre mockate
        await expect(zonaRezultatRaport).toContainText('Recomandare Auto');
        await expect(zonaRezultatRaport).toContainText('45000 RON');
    });

    test('Ar trebui să trateze corect cazul în care apelul API eșuează', async ({ page }) => {
        // Interceptăm ruta și returnăm o eroare HTTP 500 (Server Error)
        await page.route('**/api/ai/consultanta-cumparaturi', async route => {
            await route.fulfill({
                status: 500,
                contentType: 'application/json',
                body: JSON.stringify({ message: "Eroare internă de server la procesarea algoritmilor." })
            });
        });

        // Completăm datele minime obligatorii
        await page.locator('textarea.form-control').fill('Doresc recomandări pentru un laptop de gaming.');
        await page.locator('.hover-up-btn').click();

        // Verificăm dacă a apărut toast-ul de eroare din blocul catch (configurat cu ID-ul loadToast-ului)
        const toastEroareServer = page.locator('text=A eșuat conexiunea cu serverul.');
        await expect(toastEroareServer).toBeVisible();

        // Verificăm dacă interfața a afișat mesajul de eroare în panoul din dreapta: {!loading && eroare && ...}
        const textEroareEcran = page.locator('text=Conexiunea a eșuat. Reîncearcă.');
        await expect(textEroareEcran).toBeVisible();
    });

});