const { chromium } = require('@playwright/test');
const fs = require('fs');
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
  
  console.log('Navigating to terminations list...');
  await page.goto('http://localhost:3450/PropertyLeaseApplication/PropertyLeaseApplicationTerminations');
  await delay(3000);
  
  const href = await page.evaluate(() => {
    const link = Array.from(document.querySelectorAll('a')).find(a => a.textContent.trim() === 'Serve Eviction Notice');
    return link ? link.href : null;
  });
  
  if (!href) {
    console.error('Serve Eviction Notice link not found!');
    await browser.close();
    return;
  }
  
  console.log(`Navigating to: ${href}`);
  await page.goto(href);
  await delay(3000);
  
  console.log(`Page title is: ${await page.title()}`);
  
  const content = await page.content();
  fs.writeFileSync('C:\\Users\\sashe\\.gemini\\antigravity-ide\\brain\\acde49bc-895f-4c36-a156-be1e81ddcfc1\\scratch\\serve_page_debug.html', content, 'utf8');
  console.log('Saved page content to serve_page_debug.html');
  
  await browser.close();
}

run().catch(console.error);
