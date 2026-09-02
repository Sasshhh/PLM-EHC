const { chromium } = require('@playwright/test');
const fs = require('fs');
const path = require('path');
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

async function run() {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  const tempDir = path.join(__dirname, 'temp_debug_html');
  if (!fs.existsSync(tempDir)) {
    fs.mkdirSync(tempDir);
  }
  const testFilePath = path.join(tempDir, 'test.pdf');
  fs.writeFileSync(testFilePath, 'Dummy debug content');

  try {
    console.log('Logging in...');
    await page.goto('http://localhost:3450/Account/Login');
    await page.locator('#UserName').fill('RealEstateCustomer');
    await page.locator('#Password').fill('Arsenal5@');
    await page.getByRole('button', { name: 'Sign In' }).click();
    await delay(2000);
    
    console.log('Navigating to Capture...');
    await page.goto('http://localhost:3450/RealEstate/Capture');
    await delay(2000);
    
    console.log('Selecting Company...');
    await page.locator('#ddlApplicantType').selectOption({ label: 'Company (PTY LTD) / Partnership' });
    await delay(1000);
    
    console.log('Setting file...');
    await page.setInputFiles('#file_Id', testFilePath);
    await delay(1000);
    
    console.log('Saving HTML...');
    const content = await page.content();
    fs.writeFileSync('after_upload.html', content);
    console.log('Saved page content to after_upload.html');

  } finally {
    if (fs.existsSync(testFilePath)) {
      fs.unlinkSync(testFilePath);
    }
    if (fs.existsSync(tempDir)) {
      fs.rmdirSync(tempDir);
    }
    await browser.close();
  }
}

run().catch(console.error);
