import { test, expect } from '@playwright/test';

// Ignorăm erorile de certificat SSL locale specifice pentru .NET în faza de dezvoltare
test.use({ ignoreHTTPSErrors: true });

test.describe('Testare Sistem Review-uri - Navigare Directă E2E', () => {

    test('Ar trebui să se logheze, să meargă direct la tabloul 1017 și să lase un review', async ({ page }) => {

        // ==========================================
        // PASUL 1: AUTENTIFICAREA ROBOTULUI
        // ==========================================
        // Mergem pe pagina de Login generată de .NET
        await page.goto('https://localhost:7256/Identity/Account/Login');

        // Completăm datele cu un cont real din baza ta de date locală
        // ⚠️ REAMINTIRE: Contul acesta nu trebuie să fie cel care a adăugat tabloul!
        await page.locator('input[type="email"], input[name*="Email"]').fill('ilinca@test.com');
        await page.locator('input[type="password"], input[name*="Password"]').fill('ParolaTest123!');

        // Apăsăm butonul de Login și așteptăm ca rețeaua să se liniștească după redirecționare
        await Promise.all([
            page.waitForLoadState('networkidle'),
            page.locator('button:has-text("Log in"), button[type="submit"]').click()
        ]);

        // ==========================================
        // PASUL 2 & 3: EVITĂM LISTA ȘI MERGEM DIRECT LA PRODUSE
        // ==========================================
        // În loc să dăm click pe "Licitații" și apoi pe card, mergem DIRECT pe link-ul paginii de detalii!
        // Schimbă URL-ul de mai jos exact cu cel care se deschide când ești pe pagina tabloului.
        await page.goto('https://localhost:7256/Licitatii/Details/1017');

        // Îi dăm paginii un moment scurt să se încarce complet și să asambleze componentele React
        await page.waitForLoadState('networkidle');

        // ==========================================
        // PASUL 4: PORNIREA MODALEI DE RECENZIE
        // ==========================================
        // Căutăm hyperlink-ul dinamic ce folosește href="#review" (emailul vânzătorului)
        const emailHyperlink = page.locator('a[href="#review"]');

        // Verificăm dacă hyperlink-ul apare pe ecran. Dacă nu apare, înseamnă că ești logată pe contul proprietarului!
        await expect(emailHyperlink).toBeVisible({ timeout: 7000 });
        await emailHyperlink.click();

        // Validăm vizual că fereastra modala de tip Bootstrap s-a deschis pe ecran
        const modalContainer = page.locator('.modal.show');
        await expect(modalContainer).toBeVisible({ timeout: 5000 });

        // ==========================================
        // PASUL 5: COMPLETAREA ȘI TRIMITEREA FORMULARULUI
        // ==========================================
        // Schimbăm nota din dropdown (selectăm valoarea "4" din opțiuni)
        const dropdownNota = page.locator('select.form-select');
        await dropdownNota.selectOption('4');

        // Completăm textul recenziei în caseta de comentarii
        const textComentariu = page.locator('textarea.form-control');
        await textComentariu.fill('Test Automat E2E: Tranzacție excelentă cu tabloul, recomand artistul!');

        // Trimitem formularul apăsând pe butonul „Trimite Review”
        const submitButton = page.locator('button[type="submit"]:has-text("Trimite Review")');
        await submitButton.click();

        // ==========================================
        // PASUL 6: VALIDAREA SALVĂRII FINALE
        // ==========================================
        // Verificăm dacă notificarea Toast din 'react-hot-toast' confirmă succesul operațiunii
        const succesToast = page.locator('text=Review adăugat cu succes!');
        await expect(succesToast).toBeVisible({ timeout: 7000 });
    });
});