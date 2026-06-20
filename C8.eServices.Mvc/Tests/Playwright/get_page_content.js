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
  
  const content = await page.content();
  fs.writeFileSync('C:\\Users\\sashe\\.gemini\\antigravity-ide\\brain\\acde49bc-895f-4c36-a156-be1e81ddcfc1\\scratch\\terminations_page.html', content, 'utf8');
  console.log('Saved page content to terminations_page.html');
  
  // Let's print out all rows in the table
  const rows = await page.evaluate(() => {
    const trs = Array.from(document.querySelectorAll('#RCSTable tbody tr'));
    return trs.map(tr => tr.textContent.trim().replace(/\s+/g, ' '));
  });
  console.log('--- TABLE ROWS ---');
  console.log(rows);
  console.log('------------------');
  
  // Take screenshot
  await page.screenshot({ path: 'C:\\Users\\sashe\\.gemini\\antigravity-ide\\brain\\acde49bc-895f-4c36-a156-be1e81ddcfc1\\scratch\\terminations_dashboard.png', fullPage: true });
  console.log('Saved screenshot to terminations_dashboard.png');
  
  await browser.close();
}

run().catch(console.error);
