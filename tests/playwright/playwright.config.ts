import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for testing the Privatekonomi application
 */
export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:5274',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  webServer: {
    command: 'cd ../../src/Privatekonomi.Web && dotnet run',
    url: 'http://localhost:5274',
    reuseExistingServer: true,
    timeout: 120 * 1000,
  },
});
