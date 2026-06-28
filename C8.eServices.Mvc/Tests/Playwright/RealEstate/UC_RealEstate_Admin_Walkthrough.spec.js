const { test, expect } = require('@playwright/test');

const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

// Helper to safely apply zoom to page
async function applyZoom(page) {
  try {
    await page.waitForSelector('body', { timeout: 5000 });
    await page.evaluate(() => {
      if (document.body) {
        document.body.style.zoom = '1.75';
      }
    });
  } catch (err) {
    console.warn('Failed to apply zoom:', err.message);
  }
}

test.use({
  viewport: { width: 3840, height: 2160 },
  video: {
    mode: 'on',
    size: { width: 3840, height: 2160 }
  }
});

test('Real Estate Admin Config Walkthrough - 4K UHD Presentation', async ({ page }) => {
  test.setTimeout(120000); // 2 minutes

  console.log('STEP 1: Logging in as Administrator...');
  await page.context().clearCookies();
  await page.goto('http://localhost:3450/Account/Login');
  await applyZoom(page);
  await delay(3000);

  await page.locator('#UserName').fill('BOSystemAdminstrator');
  await delay(1000);
  await page.locator('#Password').fill('Arsenal5@');
  await delay(1500);
  await page.getByRole('button', { name: 'Sign In' }).click();

  await expect(page).toHaveURL(/.*\/AreaManager\/AdminInbox/, { timeout: 20000 });
  await applyZoom(page);
  console.log('Logged in successfully!');
  await delay(2000);

  console.log('STEP 2: Navigating to Real Estate Admin Dashboard...');
  await page.goto('http://localhost:3450/RealEstateAdmin/Index');
  await applyZoom(page);
  await delay(4000); // Pause to see the main dashboard headers and KPIs

  // Slowly scroll down the Tariff Matrix (first tab)
  console.log('Scrolling Tariff Matrix...');
  await page.evaluate(() => {
    window.scrollTo({ top: 300, behavior: 'smooth' });
  });
  await delay(3000);
  await page.evaluate(() => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  });
  await delay(2000);

  // Switch to Facilities tab
  console.log('Clicking Facilities Tab...');
  const facTab = page.locator('.admin-tab-item').filter({ hasText: 'Facilities' });
  await facTab.click();
  await delay(3000);
  await page.evaluate(() => {
    window.scrollTo({ top: 300, behavior: 'smooth' });
  });
  await delay(3000);
  await page.evaluate(() => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  });
  await delay(2000);

  // Switch to Unit Configurations tab
  console.log('Clicking Unit Configurations Tab...');
  const unitTab = page.locator('.admin-tab-item').filter({ hasText: 'Unit Configurations' });
  await unitTab.click();
  await delay(3000);
  await page.evaluate(() => {
    window.scrollTo({ top: 400, behavior: 'smooth' });
  });
  await delay(4000);
  await page.evaluate(() => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  });
  await delay(3000);

  // Return to Tariff Matrix
  console.log('Returning to Tariff Matrix Tab...');
  const tariffTab = page.locator('.admin-tab-item').filter({ hasText: 'Tariff Matrix' });
  await tariffTab.click();
  await delay(2000);

  console.log('Admin walkthrough presentation complete.');
});
