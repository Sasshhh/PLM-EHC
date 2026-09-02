const { test, expect } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

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

test('Submit Real Estate Lease Application (UC 05) - Slow Visual Submission', async ({ page }) => {
  test.setTimeout(180000); // 3 minutes timeout for human-paced flow

  // Listen to browser console logs
  page.on('console', msg => console.log('BROWSER CONSOLE:', msg.type(), msg.text()));
  page.on('pageerror', err => console.log('BROWSER ERROR:', err.message));

  // Create temporary mock upload directory and files
  const tempDir = path.join(__dirname, 'temp_mock_uploads_slow');
  if (!fs.existsSync(tempDir)) {
    fs.mkdirSync(tempDir);
  }

  const fileNames = [
    'id.pdf', 'address.pdf', 'cipc.pdf', 'sars.pdf', 'profile.pdf',
    'references.pdf', 'letter.pdf', 'locality.pdf', 'zoning.pdf',
    'income.pdf', 'fee.pdf', 'experience.pdf', 'financials.pdf', 'bizplan.pdf', 'mbd4.pdf'
  ];
  const filePaths = {};
  for (const name of fileNames) {
    const filePath = path.join(tempDir, name);
    fs.writeFileSync(filePath, `Dummy mock data for ${name}`);
    filePaths[name] = filePath;
  }

  try {
    console.log('STEP 1: Logging in as Real Estate Customer...');
    await page.context().clearCookies();
    await page.goto('http://localhost:3450/Account/Login');
    await applyZoom(page);
    await delay(3000); // Pause on login screen

    // Click and type username
    await page.locator('#UserName').click();
    await delay(1000);
    await page.locator('#UserName').fill('RealEstateCustomer');
    await delay(1500);

    // Click and type password
    await page.locator('#Password').click();
    await delay(1000);
    await page.locator('#Password').fill('Arsenal5@');
    await delay(2000);

    // Click Sign In
    await page.getByRole('button', { name: 'Sign In' }).click();

    // Verify redirected to Inbox
    await expect(page).toHaveURL(/.*\/RealEstate\/Inbox/, { timeout: 25000 });
    await applyZoom(page);
    console.log('Logged in successfully!');
    await delay(3000); // Pause to look at dashboard

    // STEP 2: Navigate to Capture
    console.log('STEP 2: Navigating to Capture page...');
    const reMenu = page.locator('ul.nav-list a:has-text("Real Estate Applications")');
    await reMenu.click();
    await delay(2000); // Wait for dropdown animation

    const captureMenu = page.locator('ul.nav-list a:has-text("Capture")');
    await captureMenu.click();

    await expect(page).toHaveURL(/.*\/RealEstate\/Capture/, { timeout: 15000 });
    await applyZoom(page);
    console.log('Navigated to capture page.');
    await delay(3000); // Pause to see page headers

    // STEP 3: Fill applicant and entity details
    console.log('STEP 3: Selecting applicant type and filling entity details...');
    
    // Choose Company (PTY LTD)
    await page.locator('#ddlApplicantType').click();
    await delay(1000);
    await page.locator('#ddlApplicantType').selectOption({ label: 'Company (PTY LTD) / Partnership' });
    await delay(2000); // Wait for form toggle animation

    // Entity Name
    await page.locator('input[name="Application.EntityName"]').click();
    await delay(800);
    await page.locator('input[name="Application.EntityName"]').fill('Acme Corporate Solutions (Pty) Ltd');
    await delay(1500);

    // Company Registration Number
    await page.locator('input[name="Application.CompanyRegistrationNumber"]').click();
    await delay(800);
    await page.locator('input[name="Application.CompanyRegistrationNumber"]').fill('2025/987654/07');
    await delay(1500);

    // VAT Registration Number
    await page.locator('input[name="Application.VatRegistrationNumber"]').click();
    await delay(800);
    await page.locator('input[name="Application.VatRegistrationNumber"]').fill('4510293847');
    await delay(1500);

    // Tax Reference Number
    await page.locator('input[name="Application.TaxReferenceNumber"]').click();
    await delay(800);
    await page.locator('input[name="Application.TaxReferenceNumber"]').fill('9019283746');
    await delay(1500);

    // Authorized Representative Name
    await page.locator('input[name="Application.AuthorizedRepresentativeName"]').click();
    await delay(800);
    await page.locator('input[name="Application.AuthorizedRepresentativeName"]').fill('Sasha Representative');
    await delay(1500);

    // Authorized Representative Capacity
    await page.locator('input[name="Application.AuthorizedRepresentativeCapacity"]').click();
    await delay(800);
    await page.locator('input[name="Application.AuthorizedRepresentativeCapacity"]').fill('Managing Director');
    await delay(1500);

    // Registered Address
    await page.locator('input[name="Application.EntityRegisteredAddress"]').click();
    await delay(800);
    await page.locator('input[name="Application.EntityRegisteredAddress"]').fill('12 Primrose Lane, Germiston Industrial');
    await delay(1500);

    // Postal Code
    await page.locator('input[name="Application.EntityRegisteredPostalCode"]').click();
    await delay(800);
    await page.locator('input[name="Application.EntityRegisteredPostalCode"]').fill('1401');
    await delay(1500);

    // Telephone
    await page.locator('input[name="Application.EntityTelephone"]').click();
    await delay(800);
    await page.locator('input[name="Application.EntityTelephone"]').fill('0117778888');
    await delay(1500);

    // Mobile
    await page.locator('input[name="Application.EntityMobile"]').click();
    await delay(800);
    await page.locator('input[name="Application.EntityMobile"]').fill('0721234567');
    await delay(1500);

    // Fax
    await page.locator('input[name="Application.EntityFax"]').click();
    await delay(800);
    await page.locator('input[name="Application.EntityFax"]').fill('0117778889');
    await delay(1500);

    // Email
    await page.locator('input[name="Application.EntityEmail"]').click();
    await delay(800);
    await page.locator('input[name="Application.EntityEmail"]').fill('sasha@acmecorporate.co.za');
    await delay(2000);



    // STEP 5: Space/Lease Details
    console.log('STEP 5: Space/Lease specifications...');
    await page.locator('select[name="Application.PurposeOfLease"]').click();
    await delay(800);
    await page.locator('select[name="Application.PurposeOfLease"]').selectOption({ label: 'Offices/Professional Units' });
    await delay(1500);

    await page.locator('select[name="Application.CCCId"]').click();
    await delay(800);
    await page.locator('select[name="Application.CCCId"]').selectOption({ value: '10' }); // Tokoza CCC (has seeded facilities)
    await delay(1500);

    await page.locator('input[name="Application.ErfFarmNumber"]').click();
    await delay(800);
    await page.locator('input[name="Application.ErfFarmNumber"]').fill('Erf 890 Germiston');
    await delay(1500);

    await page.locator('input[name="Application.PropertyAddress"]').click();
    await delay(800);
    await page.locator('input[name="Application.PropertyAddress"]').fill('101 Corporate Boulevard, Germiston');
    await delay(1500);

    await page.locator('input[name="Application.TownshipSuburbFarmName"]').click();
    await delay(800);
    await page.locator('input[name="Application.TownshipSuburbFarmName"]').fill('Germiston South');
    await delay(1500);

    await page.locator('input[name="Application.PropertyPostalCode"]').click();
    await delay(800);
    await page.locator('input[name="Application.PropertyPostalCode"]').fill('1401');
    await delay(1500);

    // Select Facility, Unit and Count dynamically (cascading dropdowns)
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

    // Verify calculator panel displays
    await expect(page.locator('#pricingPanel')).toBeVisible();
    await delay(2000);

    // STEP 6: Document Uploads
    const uploadHelper = async (labelId, inputName, fileName) => {
      console.log(`Uploading ${fileName} via label ${labelId}...`);
      await page.locator(labelId).click();
      await delay(1000);
      await page.setInputFiles(`input[name="${inputName}"]`, filePaths[fileName]);
      await delay(1500);
    };

    console.log('STEP 6: Clicking upload slots and uploading 11 pre-qualification files...');
    await uploadHelper('#lblFile_Id', 'file_Id', 'id.pdf');
    await uploadHelper('#lblFile_Address', 'file_Address', 'address.pdf');
    await uploadHelper('#lblFile_Cipc', 'file_Cipc', 'cipc.pdf');
    await uploadHelper('#lblFile_Sars', 'file_Sars', 'sars.pdf');
    await uploadHelper('#lblFile_Profile', 'file_Profile', 'profile.pdf');
    await uploadHelper('#lblFile_Experience', 'file_Experience', 'experience.pdf');
    await uploadHelper('#lblFile_References', 'file_References', 'references.pdf');
    await uploadHelper('#lblFile_Letters', 'file_Letters', 'letter.pdf');
    await uploadHelper('#lblFile_Locality', 'file_Locality', 'locality.pdf');
    await uploadHelper('#lblFile_Zoning', 'file_Zoning', 'zoning.pdf');
    await uploadHelper('#lblFile_Financials', 'file_Financials', 'financials.pdf');
    await uploadHelper('#lblFile_BusinessPlan', 'file_BusinessPlan', 'bizplan.pdf');
    await uploadHelper('#lblFile_Mbd4', 'file_Mbd4', 'mbd4.pdf');
    await uploadHelper('#lblFile_Fee', 'file_Fee', 'fee.pdf');

    await delay(3000); // Visual pause to verify all 11 files are uploaded in the list

    // STEP 7: Submit form
    console.log('STEP 7: Submitting form and confirming SweetAlert...');
    await page.locator('#btnSubmitForm').click();
    await delay(2000);

    // Assert SweetAlert is visible and click confirm
    const confirmBtn = page.locator('.swal-button--confirm');
    await expect(confirmBtn).toBeVisible();
    await confirmBtn.click();

    // Verify redirected to MyApplications page
    await expect(page).toHaveURL(/.*\/RealEstate\/MyApplications/, { timeout: 35000 });
    await applyZoom(page);
    console.log('Redirected to MyApplications page.');
    await delay(3000);

    // Verify success banner is present
    await expect(page.locator('.alert-success')).toContainText('Application submitted successfully!');
    await delay(2000);

    // STEP 8: Verify details modal
    console.log('STEP 8: Opening details modal for verification...');
    const viewBtn = page.locator('button.btn-view-details').first();
    await expect(viewBtn).toBeVisible();
    await viewBtn.click();

    // Wait for modal content to load and verify correct details
    const modalContent = page.locator('#modalContent');
    await expect(modalContent).toBeVisible({ timeout: 15000 });
    await delay(2000);

    // Slowly scroll inside the modal to show the values
    await page.evaluate(() => {
      const modal = document.querySelector('#detailsModal .modal-body');
      if (modal) {
        modal.scrollTo({ top: 300, behavior: 'smooth' });
      }
    });
    await delay(3000);

    await page.evaluate(() => {
      const modal = document.querySelector('#detailsModal .modal-body');
      if (modal) {
        modal.scrollTo({ top: 0, behavior: 'smooth' });
      }
    });
    await delay(2000);

    // Close modal
    await page.locator('#detailsModal button:has-text("Close")').click();
    await delay(3000);

    console.log('Slow visual presentation test completed successfully.');

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
