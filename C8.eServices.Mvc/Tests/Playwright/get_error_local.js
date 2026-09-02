const { chromium } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  try {
    console.log('Logging in as re_finance_officer...');
    await page.goto('http://localhost:3450/Account/Login');
    await page.locator('#UserName').fill('re_finance_officer');
    await page.locator('#Password').fill('Arsenal5@');
    
    const btn = page.locator('button[type="submit"], input[type="submit"], button:has-text("SIGN IN"), .btn-success, .btn');
    await btn.first().click();
    
    await page.waitForTimeout(5000); // Wait for login redirect
    console.log('Logged in, URL is:', page.url());

    console.log('Navigating to ApplicationFeePayments...');
    await page.goto('http://localhost:3450/RealEstateAdmin/ApplicationFeePayments');
    await page.waitForTimeout(5000); // Wait for page error
    
    // Capture screenshot of the error page
    await page.screenshot({ path: path.join(__dirname, 'error_page.png') });
    
    const bodyText = await page.evaluate(() => document.body.innerText);
    fs.writeFileSync(path.join(__dirname, 'error_output.txt'), bodyText, 'utf8');
    console.log('Wrote error page body text to error_output.txt');
  } catch (err) {
    console.error('Error during execution:', err);
  } finally {
    await browser.close();
  }
})();
