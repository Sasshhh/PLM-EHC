const { chromium } = require('@playwright/test');
const fs = require('fs');
const path = require('path');
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

async function run() {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  console.log('Logging in as sash38...');
  await page.goto('http://localhost:3450/Account/Login');
  await page.locator('#UserName').fill('sash38');
  await page.locator('#Password').fill('Arsenal5@');
  await page.evaluate(() => {
    document.querySelector('form').submit();
  });
  await delay(4000);
  
  console.log(`Current page URL: ${page.url()}`);
  
  console.log('Navigating to RegisterLeaseDispute...');
  await page.goto('http://localhost:3450/PropertyLeaseApplication/RegisterLeaseDispute');
  await delay(4000);
  
  console.log(`RegisterLeaseDispute URL: ${page.url()}`);
  
  const content = await page.content();
  fs.writeFileSync('C:\\Users\\sashe\\.gemini\\antigravity-ide\\brain\\acde49bc-895f-4c36-a156-be1e81ddcfc1\\scratch\\dispute_view_error.html', content, 'utf8');
  console.log('Saved page content to dispute_view_error.html');
  
  await browser.close();
}

run().catch(console.error);
