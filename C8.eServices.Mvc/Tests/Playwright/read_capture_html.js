const { chromium } = require('@playwright/test');
const fs = require('fs');
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

async function run() {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  console.log('Logging in as Real Estate Customer...');
  await page.goto('http://localhost:3450/Account/Login');
  await page.locator('#UserName').fill('RealEstateCustomer');
  await page.locator('#Password').fill('Arsenal5@');
  await page.getByRole('button', { name: 'Sign In' }).click();
  await delay(2000);
  
  console.log('Navigating to Capture page...');
  await page.goto('http://localhost:3450/RealEstate/Capture');
  await delay(2000);
  
  console.log('Current URL:', page.url());
  const content = await page.content();
  fs.writeFileSync('capture_page_load.html', content);
  console.log('Saved page content to capture_page_load.html');
  
  await browser.close();
}

run().catch(console.error);
