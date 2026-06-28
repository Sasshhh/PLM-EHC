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
    const tempFile = path.join(__dirname, 'temp_query_full.sql');
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

// Luhn-valid South African ID generator to ensure correct syntax validation
function generateValidRSAID() {
  const year = Math.floor(Math.random() * 20) + 80; // 80 to 99
  const month = Math.floor(Math.random() * 12) + 1;
  const day = Math.floor(Math.random() * 28) + 1;
  
  const yy = String(year).padStart(2, '0');
  const mm = String(month).padStart(2, '0');
  const dd = String(day).padStart(2, '0');
  
  // Gender digit: 5-9 for male
  const gggg = String(Math.floor(Math.random() * 5000) + 5000).padStart(4, '0');
  
  // Citizenship (0) and check digit placeholder (8)
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

test('Real Estate Registration, Profile Complete, Admin Approve & Submit E2E', async ({ page }) => {
  test.setTimeout(300000); // 5 minutes timeout for the entire continuous presentation flow

  // Generate unique credentials for this run
  const randomSuffix = Math.floor(Math.random() * 90000) + 10000;
  const newUsername = `RERegUser${randomSuffix}`;
  const newEmail = `reuser_${randomSuffix}@siyakhokha-test.gov.za`;
  const newMobile = `072${Math.floor(Math.random() * 90000) + 10000}${Math.floor(Math.random() * 90) + 10}`;
  const saID = generateValidRSAID();

  console.log(`Generated test data: Username=${newUsername}, Email=${newEmail}, Mobile=${newMobile}, ID=${saID}`);

  // Create temporary mock upload directory and files
  const tempDir = path.join(__dirname, 'temp_mock_uploads_full');
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
    // PART 1: User Registration
    // ========================================================
    console.log('PART 1: Registering a new Real Estate user...');
    await page.context().clearCookies();
    await page.goto('http://localhost:3450/Account/Register');
    await applyZoom(page);
    await delay(3000); // Pause on registration screen

    // Select Department: Real Estate Development
    await page.locator('#ddlDepartment').click();
    await delay(800);
    await page.locator('#ddlDepartment').selectOption({ label: 'Real Estate Development' });
    await delay(1000);

    // First Name
    await page.locator('input[name="FirstName"]').click();
    await delay(800);
    await page.locator('input[name="FirstName"]').fill('Sasha');
    await delay(1000);

    // Last Name
    await page.locator('input[name="LastName"]').click();
    await delay(800);
    await page.locator('input[name="LastName"]').fill('Developer');
    await delay(1000);

    // ID Number
    await page.locator('#IdentificationNumber').click();
    await delay(800);
    await page.locator('#IdentificationNumber').fill(saID);
    await delay(1000);

    // Communication Method: Email
    await page.locator('#ddlNotificationType').click();
    await delay(800);
    await page.locator('#ddlNotificationType').selectOption({ label: 'Email' });
    await delay(1000);

    // Username
    await page.locator('#UserName').click();
    await delay(800);
    await page.locator('#UserName').fill(newUsername);
    await delay(1000);

    // Password
    await page.locator('input[name="Password"]').click();
    await delay(800);
    await page.locator('input[name="Password"]').fill('Arsenal5@');
    await delay(1000);

    // Confirm Password
    await page.locator('input[name="ConfirmPassword"]').click();
    await delay(800);
    await page.locator('input[name="ConfirmPassword"]').fill('Arsenal5@');
    await delay(1000);

    // Mobile Number
    await page.locator('#MobileNumber').click();
    await delay(800);
    await page.locator('#MobileNumber').fill(newMobile);
    await delay(1000);

    // Confirm Mobile Number
    await page.locator('#ConfirmMobileNumber').click();
    await delay(800);
    await page.locator('#ConfirmMobileNumber').fill(newMobile);
    await delay(1000);

    // Email
    await page.locator('#EmailAddress').click();
    await delay(800);
    await page.locator('#EmailAddress').fill(newEmail);
    await delay(1000);

    // Confirm Email
    await page.locator('#ConfirmEmailAddress').click();
    await delay(800);
    await page.locator('#ConfirmEmailAddress').fill(newEmail);
    await delay(1500);

    // Programmatically enable register button to bypass client reCAPTCHA check
    await page.evaluate(() => {
      document.getElementById('btnTandC').disabled = false;
    });
    await delay(1500);

    // Click register button to open T&C Modal
    await page.locator('#btnTandC').click();
    await delay(2000);

    // Scroll T&C Modal terms to the bottom to reveal the checkbox
    await page.evaluate(() => {
      const terms = document.getElementById('terms');
      if (terms) terms.scrollTop = 5000;
    });
    await delay(2000);

    // Click checkbox to submit registration
    await page.locator('#box').click();

    // Handle confirmation email modal and close it
    await page.locator('#confirmEmailModal').waitFor({ state: 'visible', timeout: 20000 });
    await delay(2000);
    await page.locator('#confirmEmailModal button:has-text("OK")').click();

    await expect(page).toHaveURL(/.*\/Account\/Login/, { timeout: 15000 });
    await applyZoom(page);
    console.log('User registered successfully! Redirected to login.');
    await delay(2000);

    // Bypass server confirmation locks by setting EmailConfirmed = 1 in database
    console.log('Bypassing email and phone confirmation locks in the database...');
    runQuery(`
      UPDATE AspNetUsers
      SET EmailConfirmed = 1, PhoneNumberConfirmed = 1
      WHERE UserName = '${newUsername}';
    `);
    await delay(1500);

    // ========================================================
    // PART 2: Profile Completion
    // ========================================================
    console.log('PART 2: Logging in as the new user to complete profile...');
    await page.locator('#UserName').click();
    await delay(800);
    await page.locator('#UserName').fill(newUsername);
    await delay(1000);

    await page.locator('#Password').click();
    await delay(800);
    await page.locator('#Password').fill('Arsenal5@');
    await delay(1500);

    await page.getByRole('button', { name: 'Sign In' }).click();

    // Verify redirected to Profile/Index (due to missing profile)
    await expect(page).toHaveURL(/.*\/Profile\/Index3.*/, { timeout: 25000 });
    await applyZoom(page);
    console.log('Successfully landed on profile prompt.');
    await delay(2000);

    // Click on the link to update profile
    await page.locator('a:has-text("here")').click();

    await expect(page).toHaveURL(/.*\/Profile\/ManageProfile.*/, { timeout: 15000 });
    await applyZoom(page);
    console.log('Navigated to ManageProfile page.');
    await delay(2000);

    // Fill out profile details
    // Select customer type "Individuals"
    await page.locator('#ddlCustomerTypes').click();
    await delay(800);
    await page.locator('#ddlCustomerTypes').selectOption({ label: 'Individuals' });
    await delay(1000);

    // Select Title
    await page.locator('#ddlTitleTypeId').click();
    await delay(800);
    await page.locator('#ddlTitleTypeId').selectOption({ label: 'Mr' });
    await delay(1000);

    // Home Phone
    await page.locator('#txtHomeNumber').click();
    await delay(800);
    await page.locator('#txtHomeNumber').fill('0115551234');
    await delay(1000);

    // Work Phone
    await page.locator('input[name="Customer.WorkPhoneNumber"]').click();
    await delay(800);
    await page.locator('input[name="Customer.WorkPhoneNumber"]').fill('0115555678');
    await delay(1000);

    // Physical Address 1 (Street Number)
    await page.locator('#PhysicalAddress1').click();
    await delay(800);
    await page.locator('#PhysicalAddress1').fill('45');
    await delay(1000);

    // Physical Address 2 (Street Name)
    await page.locator('#PhysicalAddress2').click();
    await delay(800);
    await page.locator('#PhysicalAddress2').fill('Albertina Sisulu Road');
    await delay(1000);

    // Physical Address 3 (Suburb)
    await page.locator('#PhysicalAddress3').click();
    await delay(800);
    await page.locator('#PhysicalAddress3').fill('3'); // Street No
    await delay(1000);

    // Physical Address 4 (City)
    await page.locator('#PhysicalAddress4').click();
    await delay(800);
    await page.locator('#PhysicalAddress4').fill('Germiston');
    await delay(1000);

    // Physical Address 5 (Province)
    await page.locator('#PhysicalAddress5').click();
    await delay(800);
    await page.locator('#PhysicalAddress5').fill('Gauteng');
    await delay(1000);

    // Physical Address Code
    await page.locator('#PhysicalAddressCode').click();
    await delay(800);
    await page.locator('#PhysicalAddressCode').fill('1401');
    await delay(1500);

    // Check same address box to copy to postal address
    await page.locator('#isSameAddressChk').click();
    await delay(2000);

    // Programmatically enable Save button to bypass client-side reCAPTCHA check
    await page.evaluate(() => {
      document.getElementById('btnSave').disabled = false;
    });
    await delay(1500);

    // Submit profile save
    await page.locator('#btnSave').click();
    await expect(page).toHaveURL(/.*\/Document\/Register.*/, { timeout: 30000 });
    await applyZoom(page);
    console.log('Profile saved successfully!');

    // Log off customer session
    await page.locator('#burgerbutton').click();
    await delay(1000);
    await page.locator('#logoutForm a:has-text("Log off")').click();
    await expect(page).toHaveURL(/.*\/Account\/Login/, { timeout: 15000 });
    await applyZoom(page);
    console.log('Logged off customer session.');
    await delay(2000);

    // Simulate "Pending Approval" state by manually setting Customer.StatusId = 1 (Pending)
    console.log('Manually updating customer profile status to Pending in DB...');
    runQuery(`
      UPDATE Customers
      SET StatusId = 1
      WHERE SystemUserId = (SELECT Id FROM SystemUsers WHERE UserName = '${newUsername}');
    `);
    await delay(1500);

    // ========================================================
    // PART 3: BOSystemAdminstrator Approval
    // ========================================================
    console.log('Setting BOSystemAdminstrator department to Real Estate Development (2) in DB...');
    runQuery(`
      UPDATE SystemUsers
      SET DepartmentId = 2
      WHERE UserName = 'BOSystemAdminstrator';
    `);
    await delay(1500);

    console.log('PART 3: Logging in as BOSystemAdminstrator to approve profile...');
    await page.locator('#UserName').click();
    await delay(800);
    await page.locator('#UserName').fill('BOSystemAdminstrator');
    await delay(1000);

    await page.locator('#Password').click();
    await delay(800);
    await page.locator('#Password').fill('Arsenal5@');
    await delay(1500);

    await page.getByRole('button', { name: 'Sign In' }).click();

    // Verify redirected to AreaManager dashboard
    await expect(page).toHaveURL(/.*\/AreaManager\/AdminInbox/, { timeout: 25000 });
    await applyZoom(page);
    console.log('BOSystemAdminstrator logged in successfully.');
    await delay(2000);

    // Navigate to Pending Customer Approval list via side nav menu
    await page.locator('span.text').filter({ hasText: /^Maintenance$/ }).click();
    await delay(1500);
    await page.locator('a:has-text("Pending Approval")').click();
    await expect(page.locator('h4').first()).toContainText('Pending Approval');
    await delay(2000);

    // Look for the newly registered user in the table and click Details
    console.log('Locating user in Pending Approval list...');
    const userRow = page.locator('tr', { hasText: newEmail });
    await expect(userRow).toBeVisible();
    await delay(1500);

    await userRow.getByRole('link', { name: 'Details' }).click();

    // Verify we landed on Details page
    await expect(page).toHaveURL(/.*\/Customer\/Details.*/, { timeout: 15000 });
    await applyZoom(page);
    console.log('Landed on customer details page.');
    await delay(3000); // Wait so viewer can inspect details

    // Scroll down slowly
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight / 2));
    await delay(2000);
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(3000);

    // Click "Activate Profile" button to approve
    console.log('Clicking Activate Profile button...');
    await page.locator('#btnActivate').click();

    // Wait for bootbox confirm alert and accept it
    await page.locator('.bootbox button:has-text("Ok")').waitFor({ state: 'visible', timeout: 15000 });
    await delay(2000);
    await page.locator('.bootbox button:has-text("Ok")').click();
    await delay(3000); // Visual pause showing approval confirmation

    console.log('Customer profile activated and approved!');

    // Log off administrator
    await page.locator('#burgerbutton').click();
    await delay(1000);
    await page.locator('#logoutForm a:has-text("Log off")').click();
    await expect(page).toHaveURL(/.*\/Account\/Login/, { timeout: 15000 });
    await applyZoom(page);
    console.log('Logged off BOSystemAdminstrator.');
    await delay(2000);

    // ========================================================
    // PART 4: Customer Lease Capture (UC 05)
    // ========================================================
    console.log('PART 4: Logging back in as approved customer to submit lease application...');
    await page.locator('#UserName').click();
    await delay(800);
    await page.locator('#UserName').fill(newUsername);
    await delay(1000);

    await page.locator('#Password').click();
    await delay(800);
    await page.locator('#Password').fill('Arsenal5@');
    await delay(1500);

    await page.getByRole('button', { name: 'Sign In' }).click();

    // Verify redirected to Customer Profile page
    await expect(page).toHaveURL(/.*\/Profile\/Index3.*/, { timeout: 25000 });
    await applyZoom(page);
    console.log('Logged back in as Customer successfully! Landed on Profile Details.');
    await delay(3000);

    // Navigate to Capture
    const reMenuCust = page.locator('ul.nav-list a:has-text("Real Estate Applications")');
    await reMenuCust.click();
    await delay(1500);

    const captureMenuCust = page.locator('ul.nav-list a:has-text("Capture")');
    await captureMenuCust.click();

    await expect(page).toHaveURL(/.*\/RealEstate\/Capture/, { timeout: 15000 });
    await applyZoom(page);
    console.log('Navigated to capture page.');
    await delay(3000);

    // Choose Company (PTY LTD) applicant type
    await page.locator('#ddlApplicantType').click();
    await delay(800);
    await page.locator('#ddlApplicantType').selectOption({ label: 'Company (PTY LTD) / Partnership' });
    await delay(2000);

    // Fill Entity details
    await page.locator('input[name="Application.EntityName"]').click();
    await page.locator('input[name="Application.EntityName"]').fill('Siyakhokha Ventures (Pty) Ltd');
    await delay(1000);

    await page.locator('input[name="Application.CompanyRegistrationNumber"]').click();
    await page.locator('input[name="Application.CompanyRegistrationNumber"]').fill('2025/112233/07');
    await delay(1000);

    await page.locator('input[name="Application.VatRegistrationNumber"]').click();
    await page.locator('input[name="Application.VatRegistrationNumber"]').fill('4510293847');
    await delay(1000);

    await page.locator('input[name="Application.TaxReferenceNumber"]').click();
    await page.locator('input[name="Application.TaxReferenceNumber"]').fill('9019283746');
    await delay(1000);

    await page.locator('input[name="Application.AuthorizedRepresentativeName"]').click();
    await page.locator('input[name="Application.AuthorizedRepresentativeName"]').fill('Sasha Rep');
    await delay(1000);

    await page.locator('input[name="Application.AuthorizedRepresentativeCapacity"]').click();
    await page.locator('input[name="Application.AuthorizedRepresentativeCapacity"]').fill('Director');
    await delay(1000);

    await page.locator('input[name="Application.EntityRegisteredAddress"]').click();
    await page.locator('input[name="Application.EntityRegisteredAddress"]').fill('100 Civic Centre Boulevard');
    await delay(1000);

    await page.locator('input[name="Application.EntityRegisteredPostalCode"]').click();
    await page.locator('input[name="Application.EntityRegisteredPostalCode"]').fill('1401');
    await delay(1000);

    await page.locator('input[name="Application.EntityTelephone"]').click();
    await page.locator('input[name="Application.EntityTelephone"]').fill('0115551111');
    await delay(1000);

    await page.locator('input[name="Application.EntityMobile"]').click();
    await page.locator('input[name="Application.EntityMobile"]').fill('0721112222');
    await delay(1000);

    await page.locator('input[name="Application.EntityEmail"]').click();
    await page.locator('input[name="Application.EntityEmail"]').fill('sasha@siyakhokhaventures.co.za');
    await delay(1500);

    // Fill Banking Details
    await page.locator('input[name="Application.BankName"]').click();
    await page.locator('input[name="Application.BankName"]').fill('Standard Bank');
    await delay(1000);

    await page.locator('select[name="Application.BankAccountType"]').click();
    await page.locator('select[name="Application.BankAccountType"]').selectOption({ label: 'Cheque / Current Account' });
    await delay(1000);

    await page.locator('input[name="Application.BankAccountName"]').click();
    await page.locator('input[name="Application.BankAccountName"]').fill('Siyakhokha Ventures');
    await delay(1000);

    await page.locator('input[name="Application.BankAccountNumber"]').click();
    await page.locator('input[name="Application.BankAccountNumber"]').fill('1029384756');
    await delay(1000);

    await page.locator('input[name="Application.BankBranchCode"]').click();
    await page.locator('input[name="Application.BankBranchCode"]').fill('051001');
    await delay(1500);

    // Fill Space / Lease Specifications
    await page.locator('select[name="Application.PurposeOfLease"]').click();
    await page.locator('select[name="Application.PurposeOfLease"]').selectOption({ label: 'Offices/Professional Units' });
    await delay(1000);

    await page.locator('select[name="Application.CCCId"]').click();
    await page.locator('select[name="Application.CCCId"]').selectOption({ value: '10' }); // Tokoza CCC (has seeded facilities)
    await delay(1500);

    await page.locator('input[name="Application.ErfFarmNumber"]').click();
    await page.locator('input[name="Application.ErfFarmNumber"]').fill('Erf 123 Boksburg');
    await delay(1000);

    await page.locator('input[name="Application.PropertyAddress"]').click();
    await page.locator('input[name="Application.PropertyAddress"]').fill('12 Commissioner Street, Boksburg');
    await delay(1000);

    await page.locator('input[name="Application.TownshipSuburbFarmName"]').click();
    await page.locator('input[name="Application.TownshipSuburbFarmName"]').fill('Boksburg Central');
    await delay(1000);

    await page.locator('input[name="Application.PropertyPostalCode"]').click();
    await page.locator('input[name="Application.PropertyPostalCode"]').fill('1459');
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

    // Document Uploads
    const uploadHelper = async (labelId, inputName, fileName) => {
      console.log(`Uploading ${fileName} via label ${labelId}...`);
      await page.locator(labelId).click();
      await delay(800);
      await page.setInputFiles(`input[name="${inputName}"]`, filePaths[fileName]);
      await delay(1000);
    };

    console.log('Uploading the 11 pre-qualification files...');
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

    await delay(3000); // Visual pause showing files successfully loaded in list

    // Submit form
    console.log('Submitting form...');
    await page.locator('#btnSubmitForm').click();
    await delay(2000);

    // Confirm SweetAlert
    await page.locator('.swal-button--confirm').click();

    // Verify redirected to MyApplications
    await expect(page).toHaveURL(/.*\/RealEstate\/MyApplications/, { timeout: 35000 });
    await applyZoom(page);
    console.log('Application submitted successfully! Redirected to MyApplications.');
    await delay(3000);

    // Verify success banner is present
    await expect(page.locator('.alert-success')).toContainText('Application submitted successfully!');
    await delay(2000);

    // Verify MyApplications table list & details modal
    console.log('Opening details modal...');
    const viewBtn = page.locator('button.btn-view-details').first();
    await expect(viewBtn).toBeVisible();
    await viewBtn.click();

    // Wait for modal content to load and verify correct details
    const modalContent = page.locator('#modalContent');
    await expect(modalContent).toBeVisible({ timeout: 15000 });
    await delay(2000);

    // Smooth scroll inside details modal
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

    console.log('Full registration to submission flow completed successfully!');

  } finally {
    // Restore BOSystemAdminstrator's department back to 3 (Ekurhuleni Housing Company)
    console.log('Restoring BOSystemAdminstrator department to 3 in DB...');
    try {
      runQuery(`
        UPDATE SystemUsers
        SET DepartmentId = 3
        WHERE UserName = 'BOSystemAdminstrator';
      `);
    } catch (err) {
      console.error('Failed to restore admin department:', err);
    }

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
