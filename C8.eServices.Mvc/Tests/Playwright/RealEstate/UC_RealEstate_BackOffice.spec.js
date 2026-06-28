const { test, expect } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

// Helper to safely apply zoom to page for clear 4K readability
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

test('Real Estate Lease Back-Office Flow (UC 06 - UC 08) - 4K Video Presentation', async ({ page }) => {
  test.setTimeout(180000); // 3 minutes timeout for human-paced flow

  // Listen to browser console logs and errors
  page.on('console', msg => console.log('BROWSER CONSOLE:', msg.type(), msg.text()));
  page.on('pageerror', err => console.log('BROWSER ERROR:', err.message));

  // Create temporary mock upload directory and files
  const tempDir = path.join(__dirname, 'temp_backoffice_mock_uploads');
  if (!fs.existsSync(tempDir)) {
    fs.mkdirSync(tempDir);
  }

  const fileNames = [
    'id.pdf', 'address.pdf', 'cipc.pdf', 'sars.pdf', 'profile.pdf',
    'references.pdf', 'letter.pdf', 'locality.pdf', 'zoning.pdf',
    'income.pdf', 'fee.pdf', 'evidence.pdf'
  ];
  const filePaths = {};
  for (const name of fileNames) {
    const filePath = path.join(tempDir, name);
    fs.writeFileSync(filePath, `Dummy mock data for ${name}`);
    filePaths[name] = filePath;
  }

  try {
    // ==========================================
    // PART 1: CUSTOMER SUBMITS LEASE APPLICATION
    // ==========================================
    console.log('STEP 1: Logging in as Real Estate Customer...');
    await page.context().clearCookies();
    await page.goto('http://localhost:3450/Account/Login');
    await applyZoom(page);
    await delay(3000);

    // Login inputs
    await page.locator('#UserName').fill('RealEstateCustomer');
    await delay(1000);
    await page.locator('#Password').fill('Arsenal5@');
    await delay(1500);
    await page.getByRole('button', { name: 'Sign In' }).click();

    // Verify redirect to customer inbox
    await expect(page).toHaveURL(/.*\/RealEstate\/Inbox/, { timeout: 25000 });
    await applyZoom(page);
    console.log('Logged in successfully as Customer!');
    await delay(3000);

    // Navigate to Capture application page
    console.log('STEP 2: Navigating to Capture page...');
    const reMenu = page.locator('ul.nav-list a:has-text("Real Estate Applications")');
    await reMenu.click();
    await delay(1500);

    const captureMenu = page.locator('ul.nav-list a:has-text("Capture")');
    await captureMenu.click();

    await expect(page).toHaveURL(/.*\/RealEstate\/Capture/, { timeout: 15000 });
    await applyZoom(page);
    console.log('Navigated to capture page.');
    await delay(3000);

    // Fill application form (Company PTY LTD type)
    console.log('STEP 3: Selecting applicant type and filling details...');
    await page.locator('#ddlApplicantType').selectOption({ label: 'Company (PTY LTD) / Partnership' });
    await delay(1500); // Wait for toggle animation

    await page.locator('input[name="Application.EntityName"]').fill('Sasha Properties (Pty) Ltd');
    await page.locator('input[name="Application.CompanyRegistrationNumber"]').fill('2026/001122/07');
    await page.locator('input[name="Application.VatRegistrationNumber"]').fill('4998877665');
    await page.locator('input[name="Application.TaxReferenceNumber"]').fill('9998887776');
    await page.locator('input[name="Application.AuthorizedRepresentativeName"]').fill('Sasha Director');
    await page.locator('input[name="Application.AuthorizedRepresentativeCapacity"]').fill('Chief Executive Officer');
    await page.locator('input[name="Application.EntityRegisteredAddress"]').fill('45 Glimmering Heights, Germiston West');
    await page.locator('input[name="Application.EntityRegisteredPostalCode"]').fill('1401');
    await page.locator('input[name="Application.EntityTelephone"]').fill('0115551234');
    await page.locator('input[name="Application.EntityMobile"]').fill('0835559876');
    await page.locator('input[name="Application.EntityFax"]').fill('0115551235');
    await page.locator('input[name="Application.EntityEmail"]').fill('info@sashaproperties.co.za');
    await delay(1500);

    // Banking details
    console.log('STEP 4: Filling banking details...');
    await page.locator('input[name="Application.BankName"]').fill('First National Bank');
    await page.locator('select[name="Application.BankAccountType"]').selectOption({ label: 'Cheque / Current Account' });
    await page.locator('input[name="Application.BankAccountName"]').fill('Sasha Properties Main Acc');
    await page.locator('input[name="Application.BankAccountNumber"]').fill('62123456789');
    await page.locator('input[name="Application.BankBranchCode"]').fill('250655');
    await delay(1500);

    // Premises selections (using Tokoza CCC ID = 10 to ensure we have configured facilities and pricing)
    console.log('STEP 5: Specifying leasing premises details...');
    await page.locator('select[name="Application.PurposeOfLease"]').selectOption({ label: 'Offices/Professional Units' });
    await page.locator('select[name="Application.CCCId"]').selectOption({ value: '10' }); // Tokoza CCC
    await page.locator('input[name="Application.ErfFarmNumber"]').fill('Erf 1024 Tokoza Ext 2');
    await page.locator('input[name="Application.PropertyAddress"]').fill('1024 Kumalo Street, Tokoza');
    await page.locator('input[name="Application.TownshipSuburbFarmName"]').fill('Tokoza Suburb');
    await page.locator('input[name="Application.PropertyPostalCode"]').fill('1426');
    await delay(2000);

    // Cascading selects
    console.log('Selecting Facility, Unit and Count dynamically...');
    await page.waitForSelector('#ddlSelectedFacility:not([disabled])', { timeout: 10000 });
    await page.locator('#ddlSelectedFacility').selectOption({ index: 1 });
    await delay(1500);

    await page.waitForSelector('#ddlSelectedUnit:not([disabled])', { timeout: 10000 });
    await page.locator('#ddlSelectedUnit').selectOption({ index: 1 });
    await delay(1500);

    await page.waitForSelector('#txtUnitCount:not([disabled])', { timeout: 10000 });
    await page.locator('#txtUnitCount').click();
    await page.locator('#txtUnitCount').fill('1');
    await delay(2000); // Wait for pricing lookup to display

    // Upload 11 files
    console.log('STEP 6: Uploading all 11 pre-qualification files...');
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
    await delay(3000); // Visual pause showing files list

    // Submit Application
    console.log('STEP 7: Submitting form and confirming SweetAlert...');
    await page.locator('#btnSubmitForm').click();
    await delay(2000);

    // Confirm sweetalert
    const confirmBtn = page.locator('.swal-button--confirm');
    await expect(confirmBtn).toBeVisible();
    await confirmBtn.click();

    // Verify redirect to MyApplications
    await expect(page).toHaveURL(/.*\/RealEstate\/MyApplications/, { timeout: 35000 });
    await applyZoom(page);
    console.log('Application submitted successfully! Redirected to MyApplications.');
    await delay(3000);

    // Extract reference number
    const refNumber = await page.locator('table.re-table tbody tr:last-child span.ref-badge').innerText();
    console.log(`Captured Reference Number: ${refNumber}`);
    await delay(2000);

    // Logout
    console.log('Logging out Customer...');
    await page.evaluate(() => {
      const form = document.getElementById('logoutForm');
      if (form) {
        form.submit();
      } else {
        window.location.href = '/Account/Login';
      }
    });
    await page.waitForURL(/.*\/Account\/Login/, { timeout: 15000 });
    await delay(3000);

    // ==========================================
    // PART 2: ADMIN PAYMENT VALIDATION (UC 06)
    // ==========================================
    console.log('STEP 8: Logging in as Back Office System Administrator...');
    await page.goto('http://localhost:3450/Account/Login');
    await applyZoom(page);
    await delay(3000);

    await page.locator('#UserName').fill('BOSystemAdminstrator');
    await delay(1000);
    await page.locator('#Password').fill('Arsenal5@');
    await delay(1500);
    await page.getByRole('button', { name: 'Sign In' }).click();

    await expect(page).toHaveURL(/.*\/AreaManager\/AdminInbox/, { timeout: 25000 });
    await applyZoom(page);
    console.log('Logged in successfully as Back Office Administrator!');
    await delay(3000);

    // Navigate to Financials -> Application Fee Payments via sidebar
    console.log('STEP 9: Navigating to Application Fee Payments menu...');
    const financialsMenu = page.locator('ul.nav-list span:has-text("Financials")');
    await financialsMenu.click();
    await delay(1500);

    const paymentsMenu = page.locator('ul.nav-list a:has-text("Application Fee Payments")');
    await paymentsMenu.click();

    await expect(page).toHaveURL(/.*\/RealEstateAdmin\/ApplicationFeePayments/, { timeout: 15000 });
    await applyZoom(page);
    console.log('Navigated to Application Fee Payments page.');
    await delay(4000);

    // Find the row containing our reference number and click Verify Payment
    console.log(`Locating row with reference number: ${refNumber}`);
    const paymentRow = page.locator('table.admin-table tbody tr', { hasText: refNumber });
    await expect(paymentRow).toBeVisible();
    await paymentRow.locator('a.btn-re-gold').click();

    // Verify payment details page loaded
    await expect(page).toHaveURL(/.*\/RealEstateAdmin\/VerifyPayment/, { timeout: 15000 });
    await applyZoom(page);
    console.log('Verify Payment details page loaded successfully.');
    
    // Smooth scroll down the page to show the details and documents
    await page.evaluate(() => {
      window.scrollTo({ top: 300, behavior: 'smooth' });
    });
    await delay(3000);
    await page.evaluate(() => {
      window.scrollTo({ top: 500, behavior: 'smooth' });
    });
    await delay(3000);

    // Capture comment and Approve
    console.log('Capturing verification comments and approving payment...');
    await page.locator('#txtComment').fill('Proof of payment checked. Standard R150 lease application fee verified. Amount matches the bank deposit.');
    await delay(2000);

    // Click Approve Payment
    await page.locator('#btn-approve-submit').click();
    await delay(2000);

    // Confirm submission in custom modal
    const paymentConfirmModal = page.locator('#confirmModal');
    await expect(paymentConfirmModal).toBeVisible();
    await delay(1500);
    await paymentConfirmModal.locator('button:has-text("Yes, I am sure")').click();

    // Verify redirect and success alert
    await expect(page).toHaveURL(/.*\/RealEstateAdmin\/ApplicationFeePayments/, { timeout: 20000 });
    await applyZoom(page);
    await expect(page.locator('.alert-success')).toBeVisible();
    console.log('Payment validated successfully! Status moved to Awaiting Risk Assessment.');
    await delay(4000);

    // ==========================================
    // PART 3: ADMIN RISK ASSESSMENT (UC 07)
    // ==========================================
    console.log('STEP 10: Navigating to Risk Assessment via sidebar...');
    const assessmentMenu = page.locator('ul.nav-list span:has-text("Assessment")').first();
    await assessmentMenu.click();
    await delay(1500);

    const riskMenu = page.locator('ul.nav-list a:has-text("Risk Assessment")');
    await riskMenu.click();

    await expect(page).toHaveURL(/.*\/RealEstateAdmin\/RiskAssessments/, { timeout: 15000 });
    await applyZoom(page);
    console.log('Risk Assessments list page loaded successfully.');
    await delay(4000);

    // Locate application row and click Assess
    console.log(`Locating row with reference number: ${refNumber} for assessment...`);
    const riskRow = page.locator('table.admin-table tbody tr', { hasText: refNumber });
    await expect(riskRow).toBeVisible();
    await riskRow.locator('a.btn-re-gold').click();

    // Verify conduct assessment page loaded
    await expect(page).toHaveURL(/.*\/RealEstateAdmin\/ConductAssessment/, { timeout: 15000 });
    await applyZoom(page);
    console.log('Conduct Risk Assessment page loaded successfully.');
    await delay(3000);

    // Scroll down to the risk forms
    await page.evaluate(() => {
      window.scrollTo({ top: 300, behavior: 'smooth' });
    });
    await delay(3000);

    // Fill vetting parameters
    console.log('Filling in Vetting agency check results...');
    await page.locator('#txtCredit').fill('Cleared - Score: 720 (Low Risk)');
    await delay(800);
    await page.locator('#txtHomeAffairs').fill('Verified - Active ID, Sasha Director confirmed');
    await delay(800);
    await page.locator('#txtDeeds').fill('Cleared - No municipal property conflict flags');
    await delay(800);
    await page.locator('#txtSassa').fill('Not Registered / Active Employee');
    await delay(800);
    await page.locator('#txtCipc').fill('Active - Registration number is fully compliant');
    await delay(1500);

    // Select recommended outcome
    console.log('Selecting Recommended outcome and uploading evidence...');
    await page.locator('#ddlRecommendation').selectOption({ value: 'Recommended' });
    await delay(1500);

    // Upload evidence document
    await page.setInputFiles('input[name="evidenceFile"]', filePaths['evidence.pdf']);
    await delay(2000);

    // Reason
    await page.locator('#txtReason').fill('All background checks through credit, home affairs, deeds, SASSA, and CIPC are verified and clear. Highly recommended for departmental review.');
    await delay(2500);

    // Scroll more to see submit button clearly
    await page.evaluate(() => {
      window.scrollTo({ top: 600, behavior: 'smooth' });
    });
    await delay(2000);

    // Submit risk assessment
    await page.locator('#btn-submit-assessment').click();
    await delay(2000);

    // Confirm in custom modal
    const riskConfirmModal = page.locator('#confirmModal');
    await expect(riskConfirmModal).toBeVisible();
    await delay(1500);
    await riskConfirmModal.locator('button:has-text("Yes, I am sure")').click();

    // Verify redirect and success alert
    await expect(page).toHaveURL(/.*\/RealEstateAdmin\/RiskAssessments/, { timeout: 20000 });
    await applyZoom(page);
    await expect(page.locator('.alert-success')).toBeVisible();
    console.log('Risk assessment saved successfully! Status moved to Verified.');
    await delay(4000);

    // ==========================================
    // PART 4: INITIATE DEPARTMENTAL REVIEW (UC 08)
    // ==========================================
    console.log('STEP 11: Navigating to Departmental Reviews list page...');
    const assessmentMenu2 = page.locator('ul.nav-list span:has-text("Assessment")').first();
    await assessmentMenu2.click();
    await delay(1500);

    const deptReviewsMenu = page.locator('ul.nav-list a:has-text("Departmental Reviews")');
    await deptReviewsMenu.click();

    await expect(page).toHaveURL(/.*\/RealEstateAdmin\/DepartmentalReviews/, { timeout: 15000 });
    await applyZoom(page);
    console.log('Departmental Reviews list page loaded successfully.');
    await delay(4000);

    // Locate application row and click Initiate Review
    console.log(`Locating row with reference number: ${refNumber} to initiate review...`);
    const deptRow = page.locator('table.admin-table tbody tr', { hasText: refNumber });
    await expect(deptRow).toBeVisible();
    await deptRow.locator('a.btn-re-gold').click();

    // Verify initiate review page loaded
    await expect(page).toHaveURL(/.*\/RealEstateAdmin\/InitiateReview/, { timeout: 15000 });
    await applyZoom(page);
    console.log('Initiate Review page loaded successfully.');
    await delay(3000);

    // Scroll to see reviewing departments list
    await page.evaluate(() => {
      window.scrollTo({ top: 400, behavior: 'smooth' });
    });
    await delay(4000);

    // Click Send for Review
    console.log('Clicking Send for Review...');
    await page.locator('#btn-send-review').click();
    await delay(2000);

    // Confirm in custom modal
    const deptConfirmModal = page.locator('#confirmModal');
    await expect(deptConfirmModal).toBeVisible();
    await delay(1500);
    await deptConfirmModal.locator('button:has-text("Yes, I am sure")').click();

    // Verify redirect and success alert
    await expect(page).toHaveURL(/.*\/RealEstateAdmin\/DepartmentalReviews/, { timeout: 20000 });
    await applyZoom(page);
    await expect(page.locator('.alert-success')).toBeVisible();
    console.log('Departmental review initiated successfully! Status moved to In Circulation for Evaluation.');
    await delay(4000);

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
