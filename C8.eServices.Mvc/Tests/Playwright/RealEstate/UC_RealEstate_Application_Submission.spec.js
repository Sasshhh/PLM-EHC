const { test, expect } = require('@playwright/test');
const fs = require('fs');
const path = require('path');
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));



test('Submit Real Estate Lease Application (UC 05) - Full Submission & Details Modal Verification', async ({ page }) => {
  test.setTimeout(120000); // 2 minute timeout for form fill and slow visual delay

  // Listen to browser console logs and errors
  page.on('console', msg => console.log('BROWSER CONSOLE:', msg.type(), msg.text()));
  page.on('pageerror', err => console.log('BROWSER ERROR:', err.message));

  const tempDir = path.join(__dirname, 'temp_mock_uploads');
  if (!fs.existsSync(tempDir)) {
    fs.mkdirSync(tempDir);
  }

  // Create 11 mock text files to upload
  const fileNames = [
    'id.pdf', 'address.pdf', 'cipc.pdf', 'sars.pdf', 'profile.pdf',
    'references.pdf', 'letter.pdf', 'locality.pdf', 'zoning.pdf',
    'income.pdf', 'fee.pdf'
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

    // STEP 3: Select Corporate/Entity type and wait for toggle
    console.log('STEP 3: Filling out Applicant and Entity details...');
    await page.locator('#ddlApplicantType').selectOption({ label: 'Company (PTY LTD) / Partnership' });
    await delay(1000); // Allow toggle JS animation to complete

    // Fill Entity details
    await page.locator('input[name="Application.EntityName"]').fill('Acme Corporate Solutions (Pty) Ltd');
    await page.locator('input[name="Application.CompanyRegistrationNumber"]').fill('2025/987654/07');
    await page.locator('input[name="Application.VatRegistrationNumber"]').fill('4510293847');
    await page.locator('input[name="Application.TaxReferenceNumber"]').fill('9019283746');
    await page.locator('input[name="Application.AuthorizedRepresentativeName"]').fill('Sasha Representative');
    await page.locator('input[name="Application.AuthorizedRepresentativeCapacity"]').fill('Managing Director');
    await page.locator('input[name="Application.EntityRegisteredAddress"]').fill('12 Primrose Lane, Germiston Industrial');
    await page.locator('input[name="Application.EntityRegisteredPostalCode"]').fill('1401');
    await page.locator('input[name="Application.EntityTelephone"]').fill('0117778888');
    await page.locator('input[name="Application.EntityMobile"]').fill('0721234567');
    await page.locator('input[name="Application.EntityFax"]').fill('0117778889');
    await page.locator('input[name="Application.EntityEmail"]').fill('sasha@acmecorporate.co.za');
    await delay(1000);

    // STEP 4: Fill Banking Details
    console.log('STEP 4: Filling out Banking details...');
    await page.locator('input[name="Application.BankName"]').fill('Standard Bank');
    await page.locator('select[name="Application.BankAccountType"]').selectOption({ label: 'Cheque / Current Account' });
    await page.locator('input[name="Application.BankAccountName"]').fill('Acme Corporate Solutions');
    await page.locator('input[name="Application.BankAccountNumber"]').fill('1029384756');
    await page.locator('input[name="Application.BankBranchCode"]').fill('051001');
    await delay(1000);

    // STEP 5: Fill Space / Lease Specifications
    console.log('STEP 5: Filling out Space and Lease specifications...');
    await page.locator('select[name="Application.PurposeOfLease"]').selectOption({ label: 'Offices/Professional Units' });
    // Select first valid CCC care centre
    await page.locator('select[name="Application.CCCId"]').selectOption({ index: 1 });
    await page.locator('input[name="Application.ErfFarmNumber"]').fill('Erf 890 Germiston');
    await page.locator('input[name="Application.PropertyAddress"]').fill('101 Corporate Boulevard, Germiston');
    await page.locator('input[name="Application.TownshipSuburbFarmName"]').fill('Germiston South');
    await page.locator('input[name="Application.PropertyPostalCode"]').fill('1401');

    // Select facilities (check checkboxes)
    await page.locator('#facOutdoor').check();
    await page.locator('#facBusiness').check();
    await delay(1000);

    // STEP 6: Upload the 11 Pre-Qualification documents
    console.log('STEP 6: Uploading all 11 mandatory documents inline...');
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
    await delay(2000); // Visual pause showing files successfully loaded in list

    // STEP 7: Submit the form
    console.log('STEP 7: Submitting form and confirming SweetAlert...');
    await page.locator('#btnSubmitForm').click();
    await delay(1000);

    // Assert SweetAlert is visible and click confirm
    const confirmBtn = page.locator('.swal-button--confirm');
    await expect(confirmBtn).toBeVisible();
    await confirmBtn.click();

    // Verify redirected to MyApplications page
    await expect(page).toHaveURL(/.*\/RealEstate\/MyApplications/, { timeout: 30000 });
    console.log('Application submitted successfully! Redirected to MyApplications.');
    await delay(2000);

    // Verify success banner is present
    await expect(page.locator('.alert-success')).toContainText('Application submitted successfully!');

    // STEP 8: Verify MyApplications table list & details modal
    console.log('STEP 8: Verifying details modal on MyApplications...');
    // Click View details button for the newly submitted application (should be the first or in the table)
    const viewBtn = page.locator('button.btn-view-details').first();
    await expect(viewBtn).toBeVisible();
    await viewBtn.click();

    // Wait for modal content to load and verify correct details
    const modalContent = page.locator('#modalContent');
    await expect(modalContent).toBeVisible({ timeout: 10000 });
    await expect(page.locator('#valEntityName')).toContainText('Acme Corporate Solutions (Pty) Ltd');
    await expect(page.locator('#valRegNo')).toContainText('2025/987654/07');
    await expect(page.locator('#valBankName')).toContainText('Standard Bank');
    await expect(page.locator('#valPurposeOfLease')).toContainText('Office/Professional');
    
    // Verify facility checkboxes rendered correctly
    await expect(page.locator('#modalFacilities')).toContainText('Outdoor Advertising');
    await expect(page.locator('#modalFacilities')).toContainText('Business Hub');

    console.log('Details verified successfully inside modal!');
    await delay(3000); // Pause so the modal values can be visually checked in the recorded video
    
    // Close modal
    await page.locator('#detailsModal button:has-text("Close")').click();
    await delay(2000);
    console.log('UC 05 E2E Lease Application Capture, File Upload and Details verification passed successfully!');

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
