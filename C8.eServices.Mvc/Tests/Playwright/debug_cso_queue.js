const { chromium } = require('@playwright/test');
const fs = require('fs');
const path = require('path');
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

async function run() {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  console.log('Logging in as AshKay...');
  await page.goto('http://localhost:3450/Account/Login');
  await page.locator('#UserName').fill('AshKay');
  await page.locator('#Password').fill('Arsenal5@');
  await page.evaluate(() => {
    document.querySelector('form').submit();
  });
  await delay(3000);
  
  console.log(`Current page URL: ${page.url()}`);
  
  console.log('Navigating to terminations list...');
  await page.goto('http://localhost:3450/PropertyLeaseApplication/PropertyLeaseApplicationTerminations');
  await delay(3000);
  
  console.log(`Terminations queue page URL: ${page.url()}`);
  
  // Let's print out all rows in the table
  const rows = await page.evaluate(() => {
    const trs = Array.from(document.querySelectorAll('#RCSTable tbody tr'));
    return trs.map(tr => tr.textContent.trim().replace(/\s+/g, ' '));
  });
  console.log('--- TABLE ROWS ---');
  console.log(rows);
  console.log('------------------');
  
  // Take screenshot
  const screenshotPath = 'C:\\Users\\sashe\\.gemini\\antigravity-ide\\brain\\2bc01b71-b987-4dd3-9e18-9484ed61e758\\cso_terminations_dashboard.png';
  await page.screenshot({ path: screenshotPath, fullPage: true });
  console.log('Saved screenshot to: ' + screenshotPath);
  
  await browser.close();
}

run().catch(console.error);
