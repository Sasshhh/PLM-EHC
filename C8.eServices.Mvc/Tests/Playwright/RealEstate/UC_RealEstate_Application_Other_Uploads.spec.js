const { test, expect } = require('@playwright/test');
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

// Helper to run database queries via a temporary file
function runQuery(query) {
  try {
    const tempFile = path.join(__dirname, 'temp_query_other.sql');
    fs.writeFileSync(tempFile, query, 'utf8');
    const cmd = `sqlcmd -S localhost -E -d CRMPLMDEV_2025 -i "${tempFile}" -W`;
    const output = execSync(cmd, { encoding: 'utf8' });
    try { fs.unlinkSync(tempFile); } catch (e) {}
    return output.trim();
  } catch (err) {
    console.error('SQL Execution Error:', err);
    return '';
  }
}

test('Submit Real Estate Application with Tabbed Multiple Other Uploads', async ({ page }) => {
  test.setTimeout(120000); // 2 minutes timeout

  // Listen to browser console logs and errors
  page.on('console', msg => console.log('BROWSER CONSOLE:', msg.type(), msg.text()));
  page.on('pageerror', err => console.log('BROWSER ERROR:', err.message));

  const tempDir = path.join(__dirname, 'temp_mock_uploads_other');
  if (!fs.existsSync(tempDir)) {
    fs.mkdirSync(tempDir);
  }

  // Create 11 mock text files to upload + 3 other files
  const fileNames = [
    'id.pdf', 'address.pdf', 'cipc.pdf', 'sars.pdf', 'profile.pdf',
    'references.pdf', 'letter.pdf', 'locality.pdf', 'zoning.pdf',
    'income.pdf', 'fee.pdf', 'extra1.pdf', 'extra2.pdf', 'extra3.pdf'
  ];
  const filePaths = {};
  for (const name of fileNames) {
    const filePath = path.join(tempDir, name);
    fs.writeFileSync(filePath, `Dummy mock data for ${name}`);
    filePaths[name] = filePath;
  }

  try {
    console.log('STEP 1: Logging in as Real Estate Customer...');
    await page.goto('http://localhost:3450/Account/Login');
    await delay(1000);

    await page.locator('#UserName').fill('RealEstateCustomer');
    await page.locator('#Password').fill('Arsenal5@');
    await page.getByRole('button', { name: 'Sign In' }).click();

    // Verify redirected to Inbox
    await expect(page).toHaveURL(/.*\/RealEstate\/Inbox/, { timeout: 20000 });
    console.log('Logged in successfully!');
    await delay(1500);

    // STEP 2: Navigate to Capture Lease Application
    console.log('STEP 2: Navigating to Capture page...');
    const reMenu = page.locator('ul.nav-list a:has-text("Real Estate Applications")');
    await reMenu.click();
    await delay(1000);

    const captureMenu = page.locator('ul.nav-list a:has-text("Capture")');
    await captureMenu.click();

    // Verify redirected to Capture page
    await expect(page).toHaveURL(/.*\/RealEstate\/Capture/, { timeout: 15000 });
    console.log('Navigated to /RealEstate/Capture successfully!');
    await delay(1500);

    // STEP 3: Select Corporate/Entity type
    await page.locator('#ddlApplicantType').selectOption({ label: 'Company (PTY LTD) / Partnership' });
    await delay(1000);

    // Fill Entity details
    await page.locator('input[name="Application.EntityName"]').fill('Multi-Upload Test Corporation');
    await page.locator('input[name="Application.CompanyRegistrationNumber"]').fill('2026/777888/07');
    await page.locator('input[name="Application.VatRegistrationNumber"]').fill('4510293847');
    await page.locator('input[name="Application.TaxReferenceNumber"]').fill('9019283746');
    await page.locator('input[name="Application.AuthorizedRepresentativeName"]').fill('Sasha Multi Rep');
    await page.locator('input[name="Application.AuthorizedRepresentativeCapacity"]').fill('Director');
    await page.locator('input[name="Application.EntityRegisteredAddress"]').fill('100 Digital Boulevard');
    await page.locator('input[name="Application.EntityRegisteredPostalCode"]').fill('1401');
    await page.locator('input[name="Application.EntityTelephone"]').fill('0117778888');
    await page.locator('input[name="Application.EntityMobile"]').fill('0721234567');
    await page.locator('input[name="Application.EntityEmail"]').fill('sasha@multi-upload.co.za');
    await delay(1000);

    // STEP 4: Fill Banking Details
    await page.locator('input[name="Application.BankName"]').fill('Standard Bank');
    await page.locator('select[name="Application.BankAccountType"]').selectOption({ label: 'Cheque / Current Account' });
    await page.locator('input[name="Application.BankAccountName"]').fill('Multi-Upload Corp');
    await page.locator('input[name="Application.BankAccountNumber"]').fill('1029384756');
    await page.locator('input[name="Application.BankBranchCode"]').fill('051001');
    await delay(1000);

    // STEP 5: Fill Space / Lease Specifications
    await page.locator('select[name="Application.PurposeOfLease"]').selectOption({ label: 'Offices/Professional Units' });
    await page.locator('select[name="Application.CCCId"]').selectOption({ value: '10' }); // Tokoza CCC (has seeded facilities)
    await page.locator('input[name="Application.ErfFarmNumber"]').fill('Erf 777 Germiston');
    await page.locator('input[name="Application.PropertyAddress"]').fill('777 Cloud Avenue, Germiston');
    await page.locator('input[name="Application.TownshipSuburbFarmName"]').fill('Germiston West');
    await page.locator('input[name="Application.PropertyPostalCode"]').fill('1401');
    await delay(1500);

    // Select Facility, Unit and Count dynamically
    console.log('Selecting Facility, Unit Type and Quantity...');
    await page.waitForSelector('#ddlSelectedFacility:not([disabled])', { timeout: 10000 });
    await page.locator('#ddlSelectedFacility').selectOption({ index: 1 });
    await delay(1500);

    await page.waitForSelector('#ddlSelectedUnit:not([disabled])', { timeout: 10000 });
    await page.locator('#ddlSelectedUnit').selectOption({ index: 1 });
    await delay(1500);

    await page.waitForSelector('#txtUnitCount:not([disabled])', { timeout: 10000 });
    await page.locator('#txtUnitCount').click();
    await delay(500);
    await page.locator('#txtUnitCount').fill('2');
    await delay(1500);

    // STEP 6: Upload the 11 Pre-Qualification documents (under Required Documents tab)
    console.log('STEP 6: Uploading 11 mandatory documents under the default Required Documents tab...');
    await page.setInputFiles('input[name="file_Id"]', filePaths['id.pdf']);
    await page.setInputFiles('input[name="file_Address"]', filePaths['address.pdf']);
    await page.setInputFiles('input[name="file_Cipc"]', filePaths['cipc.pdf']);
    await page.setInputFiles('input[name="file_Sars"]', filePaths['sars.pdf']);
    await page.setInputFiles('input[name="file_Profile"]', filePaths['profile.pdf']);
    await page.setInputFiles('input[name="file_References"]', filePaths['references.pdf']);
    await page.setInputFiles('input[name="file_Letters"]', filePaths['letter.pdf']);
    await page.setInputFiles('input[name="file_Locality"]', filePaths['locality.pdf']);
    await page.setInputFiles('input[name="file_Zoning"]', filePaths['zoning.pdf']);
    await page.setInputFiles('input[name="file_Income"]', filePaths['income.pdf']);
    await page.setInputFiles('input[name="file_Fee"]', filePaths['fee.pdf']);
    await delay(2000);

    // STEP 7: Switch to the "Other Documents" tab
    console.log('STEP 7: Switching to Other Documents tab...');
    const otherTab = page.locator('ul.re-nav-tabs a[href="#tab-other"]');
    await expect(otherTab).toBeVisible();
    await otherTab.click();
    await delay(1500);

    // STEP 8: Upload multiple files under "Other Supporting Documents"
    console.log('STEP 8: Uploading 3 other documents...');
    const otherInput = page.locator('input[name="file_Other"]');
    await otherInput.setInputFiles([
      filePaths['extra1.pdf'],
      filePaths['extra2.pdf'],
      filePaths['extra3.pdf']
    ]);
    await delay(1500);

    // Assert UI shows "3 files selected"
    const otherFilenameSpan = page.locator('#name_Other span');
    await expect(otherFilenameSpan).toContainText('3 files selected');
    console.log('UI filename verified for multiple uploads!');
    await delay(2000);

    // STEP 9: Submit the form
    console.log('STEP 9: Submitting form...');
    await page.locator('#btnSubmitForm').click();
    await delay(1000);

    // Confirm SweetAlert
    const confirmBtn = page.locator('.swal-button--confirm');
    await expect(confirmBtn).toBeVisible();
    await confirmBtn.click();

    // Verify redirected to MyApplications page
    await expect(page).toHaveURL(/.*\/RealEstate\/MyApplications/, { timeout: 30000 });
    console.log('Application submitted successfully!');
    await delay(2000);

    // Fetch the new application reference number from database and check uploaded documents count
    console.log('Querying DB to verify document linkages...');
    const queryResult = runQuery(`
      SELECT COUNT(*)
      FROM Documents d
      INNER JOIN RE_Applications app ON d.RealEstateApplicationId = app.Id
      WHERE app.EntityName = 'Multi-Upload Test Corporation'
        AND d.DocumentName LIKE 'Other Supporting Document:%';
    `);
    
    console.log('DB query result for other supporting documents count:', queryResult);
    
    // We expect 3 records (for extra1.pdf, extra2.pdf, extra3.pdf)
    expect(queryResult).toContain('3');
    console.log('E2E verify for tabbed multiple uploads passed successfully!');

  } finally {
    // Cleanup temporary files
    try {
      for (const name of fileNames) {
        const filePath = path.join(tempDir, name);
        if (fs.existsSync(filePath)) {
          fs.unlinkSync(filePath);
        }
      }
      if (fs.existsSync(tempDir)) {
        fs.rmdirSync(tempDir);
      }
    } catch (err) {
      console.error('Failed to cleanup temp upload files:', err);
    }
  }
});
