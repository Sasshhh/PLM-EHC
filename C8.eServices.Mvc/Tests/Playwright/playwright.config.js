const { defineConfig, devices } = require('@playwright/test');

module.exports = defineConfig({
  testDir: './',
  fullyParallel: false, // Set to false to ensure predictable sequential execution for E2E
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: 1, // Ensure single worker for state-dependent tests
  reporter: [
    ['html', { open: 'never' }],
    ['list']
  ],
  use: {
    baseURL: 'http://localhost:3450/',
    trace: 'retain-on-failure', // Pro feature: Keeps a DOM snapshot trace if test fails
    screenshot: 'only-on-failure',
    video: 'retain-on-failure', // Pro feature: Records video automatically
    actionTimeout: 15000,
    navigationTimeout: 30000,
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
