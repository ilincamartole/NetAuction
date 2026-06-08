import { test as setup, expect } from '@playwright/test';

// Locul unde Playwright va salva cookie-ul tău .NET Identity
const authFile = 'playwright/.auth/user.json';

setup('Autentificare în aplicație', async ({ page }) => {
    // 1. Mergem pe pagina ta de Login din .NET
    await page.goto('https://localhost:7256/Identity/Account/Login');

    // 2. Completăm datele în interfață (ajustează valorile cu un cont real din DB-ul tău local)
    await page.locator('input[type="email"], input[name*="Email"]').fill('ilinca@test.com');
    await page.locator('input[type="password"]').fill('ParolaTest123!');

    // 3. Apăsăm butonul de Login
    await page.locator('button[type="submit"]').click();

    // 4. Așteptăm ca URL-ul să se schimbe (semn că logarea a reușit și am plecat de pe pagina de Login)
    await page.waitForURL('https://localhost:7256/**');

    // 5. Salvăm sesiunea completă în fișierul user.json
    await page.context().storageState({ path: authFile });
});