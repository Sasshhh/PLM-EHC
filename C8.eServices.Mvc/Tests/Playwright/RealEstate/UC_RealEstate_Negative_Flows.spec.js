const { test, expect } = require('@playwright/test');
const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

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

// Helper to run database queries via a temporary file
function runQuery(query) {
  try {
    const tempFile = path.join(__dirname, 'temp_query_negative.sql');
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

// Luhn-valid South African ID generator
function generateValidRSAID() {
  const year = Math.floor(Math.random() * 20) + 80;
  const month = Math.floor(Math.random() * 12) + 1;
  const day = Math.floor(Math.random() * 28) + 1;
  
  const yy = String(year).padStart(2, '0');
  const mm = String(month).padStart(2, '0');
  const dd = String(day).padStart(2, '0');
  const gggg = String(Math.floor(Math.random() * 5000) + 5000).padStart(4, '0');
  const partialID = `${yy}${mm}${dd}${gggg}08`;
  
  let sum = 0;
  for (let i = 0; i < 12; i++) {
    let digit = parseInt(partialID.charAt(i));
    if (i % 2 === 1) {
      digit *= 2;
      if (digit > 9) {
        digit = Math.floor(digit / 10) + (digit % 10);
      }
    }
    sum += digit;
  }
  const z = (10 - (sum % 10)) % 10;
  return `${partialID}${z}`;
}

test.use({
  viewport: { width: 3840, height: 2160 },
  video: {
    mode: 'on',
    size: { width: 3840, height: 2160 }
  }
});

test('Real Estate Negative and Alternative Flows', async ({ page }) => {
  test.setTimeout(240000); // 4 minutes timeout

  // Generate unique credentials for this run
  const randomSuffix = Math.floor(Math.random() * 90000) + 10000;
  const username = `RENegUser${randomSuffix}`;
  const email = `reneguser_${randomSuffix}@siyakhokha-test.gov.za`;
  const mobile = `072${Math.floor(Math.random() * 90000) + 10000}${Math.floor(Math.random() * 90) + 10}`;
  const saID = generateValidRSAID();

  // Create temporary mock upload directory and files
  const tempDir = path.join(__dirname, 'temp_mock_uploads_neg');
  if (!fs.existsSync(tempDir)) {
    fs.mkdirSync(tempDir);
  }

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
    // ========================================================
    // SCENARIO 1: Create and Verify Customer Profile
    // ========================================================
    console.log('Registering a new Real Estate user...');
    await page.context().clearCookies();
    await page.goto('http://localhost:3450/Account/Register');
    await applyZoom(page);

    await page.locator('#ddlDepartment').selectOption({ label: 'Real Estate Development' });
    await page.locator('input[name="FirstName"]').fill('Sasha');
    await page.locator('input[name="LastName"]').fill('Negative');
    await page.locator('#IdentificationNumber').fill(saID);
    await page.locator('#ddlNotificationType').selectOption({ label: 'Email' });
    await page.locator('#UserName').fill(username);
    await page.locator('input[name="Password"]').fill('Arsenal5@');
    await page.locator('input[name="ConfirmPassword"]').fill('Arsenal5@');
    await page.locator('#MobileNumber').fill(mobile);
    await page.locator('#ConfirmMobileNumber').fill(mobile);
    await page.locator('#EmailAddress').fill(email);
    await page.locator('#ConfirmEmailAddress').fill(email);
    await delay(1000);

    await page.evaluate(() => {
      document.getElementById('btnTandC').disabled = false;
    });
    await page.locator('#btnTandC').click();
    await delay(1000);

    await page.evaluate(() => {
      const terms = document.getElementById('terms');
      if (terms) terms.scrollTop = 5000;
    });
    await delay(1000);
    await page.locator('#box').click();

    await page.locator('#confirmEmailModal button:has-text("OK")').waitFor({ state: 'visible', timeout: 15000 });
    await page.locator('#confirmEmailModal button:has-text("OK")').click();
    await page.waitForURL(/.*\/Account\/Login/, { timeout: 15000 });

    // Bypass server confirmation locks in database
    runQuery(`
      UPDATE AspNetUsers
      SET EmailConfirmed = 1, PhoneNumberConfirmed = 1
      WHERE UserName = '${username}';
    `);

    // Log in to complete profile
    await page.locator('#UserName').fill(username);
    await page.locator('#Password').fill('Arsenal5@');
    await page.getByRole('button', { name: 'Sign In' }).click();
    await page.waitForURL(/.*\/Profile\/Index3.*/, { timeout: 15000 });

    await page.locator('a:has-text("here")').click();
    await page.waitForURL(/.*\/Profile\/ManageProfile.*/, { timeout: 15000 });

    await page.locator('#ddlCustomerTypes').selectOption({ label: 'Individuals' });
    await page.locator('#ddlTitleTypeId').selectOption({ label: 'Mr' });
    await page.locator('#txtHomeNumber').fill('0115551234');
    await page.locator('input[name="Customer.WorkPhoneNumber"]').fill('0115555678');
    await page.locator('#PhysicalAddress1').fill('45');
    await page.locator('#PhysicalAddress2').fill('Albertina Sisulu Road');
    await page.locator('#PhysicalAddress3').fill('3');
    await page.locator('#PhysicalAddress4').fill('Germiston');
    await page.locator('#PhysicalAddress5').fill('Gauteng');
    await page.locator('#PhysicalAddressCode').fill('1401');
    await page.locator('#isSameAddressChk').click();
    await delay(1000);

    await page.evaluate(() => {
      document.getElementById('btnSave').disabled = false;
    });
    await page.locator('#btnSave').click();
    await page.waitForURL(/.*\/Document\/Register.*/, { timeout: 15000 });

    // Log off customer
    await page.locator('#burgerbutton').click();
    await page.locator('#logoutForm a:has-text("Log off")').click();
    await page.waitForURL(/.*\/Account\/Login/, { timeout: 15000 });

    // Activate Profile
    runQuery(`
      UPDATE Customers
      SET StatusId = 2
      WHERE SystemUserId = (SELECT Id FROM SystemUsers WHERE UserName = '${username}');
    `);

    // ========================================================
    // SCENARIO 2: Capture Lease - Verify Validation Rules
    // ========================================================
    console.log('Logging in to test invalid capture validation...');
    await page.locator('#UserName').fill(username);
    await page.locator('#Password').fill('Arsenal5@');
    await page.getByRole('button', { name: 'Sign In' }).click();
    await page.waitForURL(/.*\/Profile\/Index3.*/, { timeout: 15000 });

    // Navigate to Capture
    await page.locator('ul.nav-list a:has-text("Real Estate Applications")').click();
    await page.locator('ul.nav-list a:has-text("Capture")').click();
    await page.waitForURL(/.*\/RealEstate\/Capture/, { timeout: 15000 });

    // Verify error occurs if we try to submit without selecting facility details
    console.log('Testing submission with empty form fields...');
    await page.locator('#btnSubmitForm').click();
    await delay(1000);
    // Client-side validation prevents submission and focus remains on capture page
    await expect(page).toHaveURL(/.*\/RealEstate\/Capture/);

    // Complete the form details correctly for submission
    console.log('Completing capture details...');
    await page.locator('#ddlApplicantType').selectOption({ label: 'Company (PTY LTD) / Partnership' });
    await delay(1000);

    await page.locator('input[name="Application.EntityName"]').fill('Negative Ventures (Pty) Ltd');
    await page.locator('input[name="Application.CompanyRegistrationNumber"]').fill('2026/098765/07');
    await page.locator('input[name="Application.VatRegistrationNumber"]').fill('4510293847');
    await page.locator('input[name="Application.TaxReferenceNumber"]').fill('9019283746');
    await page.locator('input[name="Application.AuthorizedRepresentativeName"]').fill('Sasha Rep');
    await page.locator('input[name="Application.AuthorizedRepresentativeCapacity"]').fill('Director');
    await page.locator('input[name="Application.EntityRegisteredAddress"]').fill('100 Civic Centre Boulevard');
    await page.locator('input[name="Application.EntityRegisteredPostalCode"]').fill('1401');
    await page.locator('input[name="Application.EntityTelephone"]').fill('0115551111');
    await page.locator('input[name="Application.EntityMobile"]').fill('0721112222');
    await page.locator('input[name="Application.EntityEmail"]').fill('sasha@siyakhokhaventures.co.za');

    await page.locator('input[name="Application.BankName"]').fill('Standard Bank');
    await page.locator('select[name="Application.BankAccountType"]').selectOption({ label: 'Cheque / Current Account' });
    await page.locator('input[name="Application.BankAccountName"]').fill('Negative Ventures');
    await page.locator('input[name="Application.BankAccountNumber"]').fill('1029384756');
    await page.locator('input[name="Application.BankBranchCode"]').fill('051001');

    await page.locator('select[name="Application.PurposeOfLease"]').selectOption({ label: 'Offices/Professional Units' });
    await page.locator('select[name="Application.CCCId"]').selectOption({ value: '10' }); // Tokoza CCC
    await page.locator('input[name="Application.ErfFarmNumber"]').fill('Erf 123 Boksburg');
    await page.locator('input[name="Application.PropertyAddress"]').fill('12 Commissioner Street, Boksburg');
    await page.locator('input[name="Application.TownshipSuburbFarmName"]').fill('Boksburg Central');
    await page.locator('input[name="Application.PropertyPostalCode"]').fill('1459');
    await delay(1000);

    await page.waitForSelector('#ddlSelectedFacility:not([disabled])', { timeout: 10000 });
    await page.locator('#ddlSelectedFacility').selectOption({ index: 1 });
    await delay(1000);

    await page.waitForSelector('#ddlSelectedUnit:not([disabled])', { timeout: 10000 });
    await page.locator('#ddlSelectedUnit').selectOption({ index: 1 });
    await delay(1000);

    await page.waitForSelector('#txtUnitCount:not([disabled])', { timeout: 10000 });
    await page.locator('#txtUnitCount').fill('2');
    await delay(1000);

    // Upload 11 files
    const uploadHelper = async (labelId, inputName, fileName) => {
      await page.setInputFiles(`input[name="${inputName}"]`, filePaths[fileName]);
      await delay(500);
    };

    await uploadHelper('#lblFile_Id', 'file_Id', 'id.pdf');
    await uploadHelper('#lblFile_Address', 'file_Address', 'address.pdf');
    await uploadHelper('#lblFile_Cipc', 'file_Cipc', 'cipc.pdf');
    await uploadHelper('#lblFile_Sars', 'file_Sars', 'sars.pdf');
    await uploadHelper('#lblFile_Profile', 'file_Profile', 'profile.pdf');
    await uploadHelper('#lblFile_References', 'file_References', 'references.pdf');
    await uploadHelper('#lblFile_Letters', 'file_Letters', 'letter.pdf');
    await uploadHelper('#lblFile_Locality', 'file_Locality', 'locality.pdf');
    await uploadHelper('#lblFile_Zoning', 'file_Zoning', 'zoning.pdf');
    await uploadHelper('#lblFile_Income', 'file_Income', 'income.pdf');
    await uploadHelper('#lblFile_Fee', 'file_Fee', 'fee.pdf');
    await delay(1500);

    // Submit
    await page.locator('#btnSubmitForm').click();
    await page.locator('.swal-button--confirm').click();
    await page.waitForURL(/.*\/RealEstate\/MyApplications/, { timeout: 35000 });

    const refNumber = await page.locator('table.re-table tbody tr:last-child span.ref-badge').innerText();
    console.log(`Submitted Application: Ref = ${refNumber}`);

    // Log off customer
    await page.evaluate(() => {
      document.getElementById('logoutForm').submit();
    });
    await page.waitForURL(/.*\/Account\/Login/, { timeout: 15000 });

    // ========================================================
    // SCENARIO 3: Back-Office Payment Rejection Flow (Alternative Flow)
    // ========================================================
    console.log('Logging in as Admin to test POP Rejection workflow...');
    // Set BOSystemAdminstrator department to 2 (Real Estate Development) in DB
    runQuery(`
      UPDATE SystemUsers
      SET DepartmentId = 2
      WHERE UserName = 'BOSystemAdminstrator';
    `);

    await page.locator('#UserName').fill('BOSystemAdminstrator');
    await page.locator('#Password').fill('Arsenal5@');
    await page.getByRole('button', { name: 'Sign In' }).click();
    await page.waitForURL(/.*\/AreaManager\/AdminInbox/, { timeout: 15000 });

    // Navigate to Payments
    await page.locator('ul.nav-list span:has-text("Financials")').click();
    await page.locator('ul.nav-list a:has-text("Application Fee Payments")').click();
    await page.waitForURL(/.*\/RealEstateAdmin\/ApplicationFeePayments/, { timeout: 15000 });

    // Locate row and click verify
    const paymentRow = page.locator('table.admin-table tbody tr', { hasText: refNumber });
    await expect(paymentRow).toBeVisible();
    await paymentRow.locator('a.btn-re-gold').click();
    await page.waitForURL(/.*\/RealEstateAdmin\/VerifyPayment.*/, { timeout: 15000 });

    // SCENARIO 3.1: Verify validation error if no comment is supplied on Reject
    console.log('Testing POP rejection without comments...');
    let dialogMessage = '';
    page.once('dialog', async dialog => {
      dialogMessage = dialog.message();
      await dialog.accept();
    });
    await page.locator('#btn-reject-submit').click();
    await delay(2500);
    expect(dialogMessage).toContain('rejection comments/reason is mandatory');
    console.log('POP rejection dialog validation verified successfully!');
    await delay(1000);

    // SCENARIO 3.2: Verify POP rejection with comments
    console.log('Submitting POP rejection with comment...');
    await page.locator('#txtComment').fill('The uploaded Proof of Payment is blurred and the transaction date is cut off. Please upload a clear bank receipt.');
    await page.locator('#btn-reject-submit').click();
    await delay(1500);
    
    // Confirm rejection submission in custom modal
    const rejectConfirmModal = page.locator('#confirmModal');
    await expect(rejectConfirmModal).toBeVisible();
    await delay(1500);
    await rejectConfirmModal.locator('button:has-text("Yes, I am sure")').click();

    // Redirected back to payments list
    await page.waitForURL(/.*\/RealEstateAdmin\/ApplicationFeePayments/, { timeout: 15000 });
    await expect(page.locator('.alert-success')).toBeVisible();
    console.log('Payment successfully rejected. Applicant notified.');
    await delay(2000);

    // Verify history logs in DB contains the rejection
    const logComment = runQuery(`
      SELECT TOP 1 AuditAction 
      FROM PLMApplicationHistortyLogs 
      WHERE RealEstateApplicationId = (SELECT Id FROM RE_Applications WHERE ApplicationReferenceNumber = '${refNumber}')
      ORDER BY Id DESC;
    `);
    console.log(`Database History Log: ${logComment}`);
    expect(logComment).toContain('Application fee payment rejected');

    // ========================================================
    // SCENARIO 4: Client-side PTO Date constraints validation
    // ========================================================
    // Manually force application to Concluded Approved so customer can request PTO
    console.log('Manually updating application to Concluded Approved in DB to access PTO...');
    runQuery(`
      UPDATE RE_Applications
      SET StatusId = (SELECT Id FROM Status WHERE [Key] = 're_concluded_approved')
      WHERE ApplicationReferenceNumber = '${refNumber}';
    `);
    await delay(1000);

    // Log off administrator
    await page.locator('#burgerbutton').click();
    await page.locator('#logoutForm a:has-text("Log off")').click();
    await page.waitForURL(/.*\/Account\/Login/, { timeout: 15000 });

    // Log in as Customer
    await page.locator('#UserName').fill(username);
    await page.locator('#Password').fill('Arsenal5@');
    await page.getByRole('button', { name: 'Sign In' }).click();
    await page.waitForURL(/.*\/Profile\/Index3.*/, { timeout: 15000 });

    // Fetch application ID from DB to navigate directly to RequestPto page
    console.log('Fetching Application ID from database...');
    const appIdOutput = runQuery(`
      SET NOCOUNT ON;
      SELECT Id FROM RE_Applications WHERE ApplicationReferenceNumber = '${refNumber}';
    `);
    const lines = appIdOutput.split('\n').map(l => l.trim()).filter(l => l && !l.startsWith('-') && l !== 'Id');
    const appId = lines[0];
    console.log(`Navigating directly to RequestPto page with ID: ${appId}...`);
    await page.goto(`http://localhost:3450/RealEstate/RequestPto/${appId}`);
    await page.waitForURL(/.*\/RealEstate\/RequestPto.*/, { timeout: 15000 });

    // Setup dialog listener for the alert
    let alertMessage = '';
    page.on('dialog', async dialog => {
      alertMessage = dialog.message();
      await dialog.accept();
    });

    // Enter start date in the past
    console.log('Entering invalid PTO start date (in the past)...');
    await page.locator('#txtStartDate').fill('2020-01-01');
    await page.locator('#txtEndDate').fill('2020-06-01');
    await page.locator('#txtPurpose').fill('Early fit-out execution');
    await page.locator('#chkIndemnity').click();
    await delay(1000);

    // Try submitting
    await page.locator('#btn-submit-pto').click();
    await delay(2000);

    // Form should fail validation and display error message in alert dialog
    expect(alertMessage).toContain('The start date cannot fall in the past.');
    console.log('PTO start date validation constraints successfully verified!');
    await delay(2000);

  } finally {
    // Restore admin department in DB
    runQuery(`
      UPDATE SystemUsers
      SET DepartmentId = 3
      WHERE UserName = 'BOSystemAdminstrator';
    `);

    // Clean up temporary files
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
