// @ts-check
import { defineConfig, devices } from '@playwright/test';

/**
 * @see https://playwright.dev/docs/test-configuration
 */
export default defineConfig({
    testDir: './tests',
    /* Run tests in files in parallel */
    fullyParallel: true,
    /* Fail the build on CI if you accidentally left test.only in the source code. */
    forbidOnly: !!process.env.CI,
    /* Retry on CI only */
    retries: process.env.CI ? 2 : 0,
    /* Opt out of parallel tests on CI. */
    workers: process.env.CI ? 1 : undefined,
    /* Reporter to use. See https://playwright.dev/docs/test-reporters */
    reporter: 'html',
    /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
    use: {
        /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
        trace: 'on-first-retry',
        ignoreHTTPSErrors: true
    },

    /* Configure projects for major browsers */
    projects: [
        {
            name: 'setup',
            testMatch: /auth\.setup\.js/,
        },

        // 2. PROIECTUL CHROMIUM CONFIGURAT PENTRU PAGINI PROTEJATE
        {
            name: 'chromium',
            use: {
                ...devices['Desktop Chrome'],
                // Îi spunem browserului să se deschidă direct logat folosind fișierul generat de setup
                storageState: 'playwright/.auth/user.json',
            },
            // IMPORTANT: Acest proiect va porni DOAR după ce proiectul 'setup' s-a terminat cu succes
            dependencies: ['setup'],
        },

        {
            name: 'firefox',
            use: {
                ...devices['Desktop Firefox'],
                storageState: 'playwright/.auth/user.json',
            },
            dependencies: ['setup'],
        },

        {
            name: 'webkit',
            use: {
                ...devices['Desktop Safari'],
                storageState: 'playwright/.auth/user.json',
            },
            dependencies: ['setup'],
        },
    ],
});