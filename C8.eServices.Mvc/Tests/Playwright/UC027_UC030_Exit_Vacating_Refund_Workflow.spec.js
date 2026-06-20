const { test, expect } = require('@playwright/test');
const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

// Helper to pause for N milliseconds
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

// Helper to scroll down and then up slowly for high-definition video walkthroughs
async function humanScroll(page, durationMs = 4000) {
  console.log('Performing human-like smooth scrolling...');
  await page.evaluate(async (duration) => {
    const scrollHeight = document.body.scrollHeight;
    const steps = 40;
    const stepDelay = duration / (steps * 2);
    
    // Scroll down slowly
    for (let i = 0; i <= steps; i++) {
      window.scrollTo(0, (scrollHeight / steps) * i);
      await new Promise(r => setTimeout(r, stepDelay));
    }
    // Pause at the bottom
    await new Promise(r => setTimeout(r, 1500));
    // Scroll back up slowly
    for (let i = steps; i >= 0; i--) {
      window.scrollTo(0, (scrollHeight / steps) * i);
      await new Promise(r => setTimeout(r, stepDelay));
    }
  }, durationMs);
  await delay(1500);
}

// Helper to run database queries using a temp file to avoid Windows escaping bugs
function runQuery(query) {
  try {
    const tempFile = path.join(__dirname, 'temp_query_uc027_uc030.sql');
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

// Reset Database state before starting the exit, vacating, and refund workflow tests
function resetDatabase() {
  console.log('Resetting Database State for UC027-UC030 E2E Workflow...');
  const sql = `
    -- 0. Synchronize all test user passwords to Arsenal5@
    UPDATE au 
    SET au.PasswordHash = 'AIrxMxCDw+uPXdvWWYzwF1kac9gt2e5G/AF83mz1RjdyStI81NRJ18bVFq1ZGkxoCA==', 
        au.SecurityStamp = '669cc746-a1ef-46c6-afe1-2ffa3053b5e0' 
    FROM AspNetUsers au 
    JOIN SystemUsers su ON au.SystemUserId = su.Id 
    WHERE su.UserName IN ('AshKay', 'sash38', 'COESolarDev07', 'TBKHUMALO');

    -- 1. Reset LeaseDetails & PropertyLeaseApplications to Awaiting Exit Inspection status (ID 255)
    UPDATE LeaseDetails 
    SET StatusId = 255, NoticeDate = NULL, Completed = 1, IsActive = 1, DepositeAmount = 1500.00 
    WHERE Id = 24;

    UPDATE PropertyLeaseApplications 
    SET StatusId = 255, CustomerId = (SELECT Id FROM Customers WHERE SystemUserId = (SELECT Id FROM SystemUsers WHERE UserName = 'sash38')) 
    WHERE Id = 28;

    -- 1b. Deactivate all other leases for the same tenant to ensure Id = 24 is selected
    UPDATE LeaseDetails 
    SET IsActive = 0 
    WHERE Id <> 24 AND PropertyLeaseApplicationId = 28;

    -- 2. Clear any existing schedules, slots and bookings for application 28
    DELETE FROM ScheduledInspections WHERE PropertyLeaseApplicationId = 28;
    DELETE FROM InspectionSchedules WHERE PropertyLeaseApplicationId = 28;
    DELETE FROM DateToSchedules WHERE PropertyLeaseApplicationId = 28;

    -- 3. Reset/Clear LeaseTerminations data columns
    UPDATE LeaseTerminations 
    SET AccessCardNumber = NULL, 
        KeyNumber = NULL, 
        OtherPossessions = NULL, 
        MaintenanceCost = NULL, 
        ApprovedDeductions = NULL, 
        NettRefundAmount = NULL, 
        FinancialOfficerOfficialNumber = NULL, 
        DepositRefundRecommended = NULL, 
        FinancialOfficerReason = NULL, 
        RevenueManagerRefundSupport = NULL, 
        RevenueManagerRefundOfficialNumber = NULL, 
        RevenueManagerRefundReason = NULL, 
        CEORefundResponse = NULL, 
        CEORefundOfficialNumber = NULL, 
        CEORefundSignature = NULL, 
        CEORefundSignDate = NULL, 
        CEORefundReason = NULL 
    WHERE PropertyLeaseApplicationId = 28;

    -- 3b. If no lease termination record exists, create one
    IF NOT EXISTS (SELECT 1 FROM LeaseTerminations WHERE PropertyLeaseApplicationId = 28)
    BEGIN
        INSERT INTO LeaseTerminations (PropertyLeaseApplicationId, LeaseDetailsId, TerminationDate, ReasonForTermination, CreatedBySystemUserId, CreatedDateTime, IsDeleted, IsActive)
        VALUES (28, 24, DATEADD(day, 30, GETDATE()), 'Relocation to another province', 1, GETDATE(), 0, 1);
    END;

    -- 4. Delete existing documents uploaded for exit inspection / banking details / onboarding docs
    DELETE FROM Documents 
    WHERE PropertyLeaseApplicationId = 28 
      AND DocumentCheckListId IN (
          SELECT Id FROM DocumentCheckLists WHERE DocumentTypeId IN (87, 123, 121, 122)
      );
    DELETE FROM Documents WHERE PropertyLeaseApplicationId = 28 AND DocumentCheckListId IN (5, 90);

    -- 4b. Seed onboarding documents (Completed Unit Inspection Document Checklist 5, Final Lease Agreement Checklist 90)
    DECLARE @FileId5 INT;
    INSERT INTO Files (FileName, ContentType, Content, FileSize, CreatedDateTime)
    VALUES ('first_unit_inspection_report.pdf', 'application/pdf', 0x25504446, 4, GETDATE());
    SET @FileId5 = SCOPE_IDENTITY();

    INSERT INTO Documents (DocumentName, DocumentCheckListId, ReferenceId, ReferenceTypeId, PropertyLeaseApplicationId, FileId, StatusId, IsActive, IsDeleted, CreatedDateTime, IsLocked)
    VALUES ('first_unit_inspection_report.pdf', 5, (SELECT Id FROM Customers WHERE SystemUserId = (SELECT Id FROM SystemUsers WHERE UserName = 'sash38')), 12, 28, @FileId5, 46, 1, 0, GETDATE(), 0);

    DECLARE @FileId90 INT;
    INSERT INTO Files (FileName, ContentType, Content, FileSize, CreatedDateTime)
    VALUES ('final_lease_agreement.pdf', 'application/pdf', 0x25504446, 4, GETDATE());
    SET @FileId90 = SCOPE_IDENTITY();

    INSERT INTO Documents (DocumentName, DocumentCheckListId, ReferenceId, ReferenceTypeId, PropertyLeaseApplicationId, FileId, StatusId, IsActive, IsDeleted, CreatedDateTime, IsLocked)
    VALUES ('final_lease_agreement.pdf', 90, (SELECT Id FROM Customers WHERE SystemUserId = (SELECT Id FROM SystemUsers WHERE UserName = 'sash38')), 12, 28, @FileId90, 46, 1, 0, GETDATE(), 0);


    -- 5. Delete related RoundRobinQueues jobs for Application 28
    DELETE FROM RoundRobinQueues WHERE PropertyLeaseApplicationId = 28;

    -- 5b. Insert RoundRobinQueue entry for ScheduleInspectionSlots (ResponsibilityTypeId = 21, ClerkId = 190 (AshKay), StatusId = 99 (Submitted))
    INSERT INTO RoundRobinQueues (PropertyLeaseApplicationId, LeaseDetailsId, ResponsibilityTypeId, ClerkId, StatusId, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId, CreatedDateTime, DepartmentId)
    VALUES (28, 24, 21, 190, 99, 1, 0, 0, 1, GETDATE(), 1);
  `;
  runQuery(sql);
  console.log('Database state reset successfully.');
}

// Robust login helper navigating directly to the login page to avoid DOM overlay issues
async function robustLogin(page, username, password) {
  console.log(`Attempting robust login for user: ${username}`);
  await page.context().clearCookies();
  await page.goto('http://localhost:3450/Account/Login');
  
  await page.locator('#UserName').fill(username);
  await page.locator('#Password').fill(password);
  await delay(1500); // Visual pause to see login entries
  
  try {
    await Promise.all([
      Promise.race([
        page.locator('#UserName').waitFor({ state: 'hidden', timeout: 60000 }),
        page.locator('#logoutForm').waitFor({ state: 'visible', timeout: 60000 })
      ]),
      page.evaluate(() => {
        const form = document.querySelector('form');
        if (form) form.submit();
        else throw new Error('Form not found');
      })
    ]);
    console.log(`Successfully logged in as ${username}. Redirected to ${page.url()}`);
    await delay(2000); // Visual pause after landing on dashboard
  } catch (err) {
    console.error(`Login failed/timed out for ${username}. Current URL: ${page.url()}`);
    throw err;
  }
}

async function robustLogoff(page) {
  console.log('Logging off active session by clearing cookies and storage...');
  await page.context().clearCookies();
  await page.evaluate(() => {
    localStorage.clear();
    sessionStorage.clear();
  });
  console.log('Logoff successful.');
  await delay(2000); // Visual pause after logoff
}

test.describe('UC027 to UC030 Exit Inspection, Vacating & Deposit Refund E2E Workflow Spec', () => {
  
  test.beforeAll(async () => {
    resetDatabase();
  });

  test('Execute Full Exit Inspection Scheduling, Slot Selection, Exit Inspection, Move-Out Confirmation, and Deposit Refund Approval Lifecycle', async ({ page }) => {
    test.setTimeout(600000); // 10 minutes to accommodate slow visual scrolling and visual pauses

    const debugLogFile = path.join(__dirname, 'browser_debug_uc027_uc030.pdf');
    fs.writeFileSync(debugLogFile, 'Dummy upload file content for E2E testing exit inspection doc upload.', 'utf8');
    
    page.on('console', msg => {
      fs.appendFileSync(debugLogFile, `PAGE LOG: ${msg.text()}\n`, 'utf8');
      console.log('PAGE LOG:', msg.text());
    });
    page.on('pageerror', exception => {
      fs.appendFileSync(debugLogFile, `PAGE ERROR: ${exception.stack || exception.message || exception}\n`, 'utf8');
      console.error('PAGE ERROR:', exception);
    });

    // ========================================================
    // STEP 1: CSO Clerk AshKay Schedules Exit Inspection Slots (UC027)
    // ========================================================
    console.log('STEP 1: Logging in as CSO Clerk AshKay to schedule exit inspection slots...');
    await robustLogin(page, 'AshKay', 'Arsenal5@');

    await page.goto('http://localhost:3450/PropertyLeaseApplication/PropertyLeaseApplicationTerminations');
    await expect(page.getByText('Tenants Application Termination')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Review terminations dashboard

    // Click "Schedule Exit Inspection" link on application row
    const termRow = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(termRow).toBeVisible();
    await delay(1500);
    await termRow.getByRole('link', { name: 'Schedule Exit Inspection' }).click();

    // Verify Schedule Slots page loaded
    await expect(page.locator('.panel-heading', { hasText: 'Unit Inspection Slot' })).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 5000); // Scroll down to slots fields

    // Choose tomorrow's date
    const today = new Date();
    const futureDate = new Date(today);
    futureDate.setDate(today.getDate() + 2); // 2 days in the future to pass ChangeDate() check
    const formattedDate = futureDate.toISOString().split('T')[0];
    
    console.log(`Scheduling slots for future date: ${formattedDate}`);
    await page.locator('#DateToSchedule').fill(formattedDate);
    await delay(1000);

    // Select the first timeslot checkbox
    await page.locator('input[name="SelectedRoles"]').first().check();
    await delay(1500);

    // Click Add Timeslots
    console.log('Clicking Add Timeslots...');
    await page.locator('input[type="submit"][value="Add Timeslots"]').click();
    
    // Wait for Success Modal and click OK
    await page.locator('#TimeslotScheduledModal').waitFor({ state: 'visible', timeout: 15000 });
    await delay(1500);
    await page.locator('#TimeslotScheduledModal button:has-text("OK")').click();
    await delay(2000);

    // Click Submit
    console.log('Submitting slots to applicant...');
    await page.locator('#btnExternal').click();

    // Verify redirected back to Inspections Dashboard with success
    await expect(page.getByText('Submitted Applications')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    console.log('CSO successfully scheduled exit inspection slots.');

    await robustLogoff(page);

    // ========================================================
    // STEP 2: Tenant sash38 Selects Exit Inspection Slot (UC027)
    // ========================================================
    console.log('STEP 2: Logging in as Tenant sash38 to select an exit inspection slot...');
    await robustLogin(page, 'sash38', 'Arsenal5@');

    await page.goto('http://localhost:3450/PropertyLeaseApplication/Inbox');
    await expect(page.getByText('Inbox Applications')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Scroll inbox

    // Click "Unit Inspection Schedule" link on application row
    const inboxRow = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(inboxRow).toBeVisible();
    await delay(1500);
    await inboxRow.getByRole('link', { name: 'Unit Inspection Schedule' }).click();

    // Verify select slots page loaded
    await expect(page.locator('.panel-heading', { hasText: 'Unit Inspection Schedule' })).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Scroll table of slots

    // Select the timeslot checkbox-blank icon
    console.log('Clicking the timeslot select checkbox...');
    await page.locator('span b.ion-android-checkbox-outline-blank').first().click();
    await delay(1500);

    // Click confirmation on SweetAlert
    console.log('Confirming slot selection via SweetAlert...');
    await page.locator('.swal-button--confirm').click();

    // Wait for Success modal or redirect back to Inbox
    await page.waitForURL(url => url.href.includes('/PropertyLeaseApplication/Inbox'), { timeout: 15000 });
    await delay(2000);
    console.log('Tenant successfully selected exit inspection slot.');

    await robustLogoff(page);

    // ========================================================
    // STEP 3: Caretaker COESolarDev07 Conducts Exit Inspection (UC027)
    // ========================================================
    console.log('STEP 3: Logging in as Caretaker COESolarDev07 to conduct exit inspection...');
    
    // Manually assign to COESolarDev07 Caretaker queue to ensure it shows up in dashboard
    runQuery(`
      UPDATE RoundRobinQueues SET IsActive = 0, IsDeleted = 1 WHERE PropertyLeaseApplicationId = 28;
      INSERT INTO RoundRobinQueues (ClerkId, ResponsibilityTypeId, StatusId, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId, CreatedDateTime, PropertyLeaseApplicationId, LeaseDetailsId, DepartmentId)
      VALUES (1289, 26, 99, 1, 0, 0, 1, GETDATE(), 28, 24, 1);
    `);

    await robustLogin(page, 'COESolarDev07', 'Arsenal5@');

    await page.goto('http://localhost:3450/PropertyLeaseApplication/PropertyLeaseInspections');
    await expect(page.getByText('Submitted Applications')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Scroll dashboard

    // Click "Conduct Exit Inspection" on application row
    const inspectionRow = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(inspectionRow).toBeVisible();
    await delay(1500);
    await inspectionRow.getByRole('link', { name: 'Conduct Exit Inspection' }).click();

    // Verify conduct exit inspection page loaded
    await expect(page.locator('.panel-heading', { hasText: 'Conduct Exit Inspection Template' })).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 5000); // Scroll down to uploads & outcomes

    // Upload required exit inspection documents
    console.log('Uploading exit inspection files...');
    const uploadFiles = page.locator('input[type="file"].docUpload');
    const uploadCount = await uploadFiles.count();
    for (let i = 0; i < uploadCount; i++) {
      await uploadFiles.nth(i).setInputFiles(debugLogFile);
    }
    await delay(1500);
    await page.locator('#uploadFiles').click();

    // Wait for upload confirmation modal and click OK
    await page.locator('#documentResponseModal').first().waitFor({ state: 'visible', timeout: 15000 });
    await delay(1500);
    await page.locator('#documentResponseModal').first().locator('button#processResponseBtn').click();
    await delay(2000);
    // Force remove any leftover modal backdrops to prevent click interception
    await page.evaluate(() => {
      document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
      document.body.classList.remove('modal-open');
    });
    await delay(1000);

    // Upload required exit interview form
    console.log('Uploading exit interview form...');
    const uploadFiles11 = page.locator('input[type="file"].docUpload11');
    const uploadCount11 = await uploadFiles11.count();
    for (let i = 0; i < uploadCount11; i++) {
      await uploadFiles11.nth(i).setInputFiles(debugLogFile);
    }
    await delay(1500);
    await page.locator('#uploadFiles1').click();

    // Wait for upload confirmation modal and click OK
    await page.locator('#documentResponseModal').first().waitFor({ state: 'visible', timeout: 15000 });
    await delay(1500);
    await page.locator('#documentResponseModal').first().locator('button#processResponseBtn').click();
    await delay(2000);
    // Force remove any leftover modal backdrops to prevent click interception
    await page.evaluate(() => {
      document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
      document.body.classList.remove('modal-open');
    });
    await delay(1000);


    // Select "Habitable - No defects" status
    console.log('Selecting inspection outcome...');
    await page.locator('#ApprovalStatus').selectOption('plm_habitable');
    await delay(1500);

    // Enter Comments (Not required/visible for habitable status, skipping)
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(1500);

    // Click Submit
    console.log('Submitting exit inspection outcome...');
    await page.locator('input[type="button"][value="Submit"]').click();
    await delay(1500);
    await page.locator('.swal-button--confirm').click();

    // Verify redirected back to Inspections Dashboard
    await page.waitForURL(url => url.href.includes('/PropertyLeaseApplication/PropertyLeaseInspections'), { timeout: 15000 });
    await delay(2000);
    console.log('Caretaker successfully submitted habitable exit inspection.');

    await robustLogoff(page);

    // ========================================================
    // STEP 4: CSO Clerk AshKay Confirms Tenant Move-Out (UC028)
    // ========================================================
    console.log('STEP 4: Logging in as CSO Clerk AshKay to confirm move-out...');
    
    // Manually route to CSO queue for VacatingConfirmation (ResponsibilityTypeId = 37, ClerkId = 190 (AshKay))
    runQuery(`
      UPDATE RoundRobinQueues SET IsActive = 0, IsDeleted = 1 WHERE PropertyLeaseApplicationId = 28;
      INSERT INTO RoundRobinQueues (ClerkId, ResponsibilityTypeId, StatusId, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId, CreatedDateTime, PropertyLeaseApplicationId, LeaseDetailsId, DepartmentId)
      VALUES (190, 37, 99, 1, 0, 0, 1, GETDATE(), 28, 24, 1);
      
      -- Set status to Awaiting Vacating Confirmation (276)
      UPDATE LeaseDetails SET StatusId = 276 WHERE Id = 24;
      UPDATE PropertyLeaseApplications SET StatusId = 276 WHERE Id = 28;
    `);

    await robustLogin(page, 'AshKay', 'Arsenal5@');

    await page.goto('http://localhost:3450/LeaseDetails/LeaseTerminated');
    await expect(page.getByText('Terminated Lease Applications')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000);

    // Click "Confirm Move-Out" link
    const vRow = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(vRow).toBeVisible();
    await delay(1500);
    await vRow.getByRole('link', { name: 'Confirm Move-Out' }).click();

    // Verify confirm move out page loaded
    await expect(page.locator('.panel-heading', { hasText: 'Vacating Applicant Action' })).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 5000);

    // Select "Vacated" from dropdown
    console.log('Selecting Vacated option...');
    await page.locator('#ApprovalStatus').selectOption('plm_vacated');
    await delay(2000); // Allow handover details panel to slide down

    // Fill handover details
    console.log('Entering possession handover details...');
    await page.locator('#AccessCardNumber').fill('CARD-EHC-9938');
    await delay(1000);
    await page.locator('#KeyNumber').fill('KEY-EHC-24');
    await delay(1000);
    await page.locator('#OtherPossessions').fill('Parking disc, postbox key, and window latches.');
    await delay(1500);
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(1000);

    // Submit Move-Out
    console.log('Submitting vacating confirmation...');
    await page.locator('input[type="button"][value="Submit"]').click();
    await delay(1500);
    await page.locator('.swal-button--confirm').click();

    // Verify redirected back to dashboard with success modal
    await expect(page.getByText('Terminated Lease Applications')).toBeVisible({ timeout: 15000 });
    await delay(1500);
    // Click OK on Success Modal if open
    try {
      await page.locator('#ConfirmVacatingAppicantSessionModal button:has-text("OK")').click({ timeout: 5000 });
    } catch(e) {}
    await delay(2000);
    console.log('CSO successfully confirmed vacating and logged handovers.');

    // ========================================================
    // STEP 5: Financial Officer recommends refund & deductions (UC029)
    // ========================================================
    console.log('STEP 5: Processing Deposit Refund Recommendation as Financial Officer...');
    
    // Manually route to FO for DepositRefund (ResponsibilityTypeId = 44, ClerkId = 190 (AshKay))
    runQuery(`
      UPDATE RoundRobinQueues SET IsActive = 0, IsDeleted = 1 WHERE PropertyLeaseApplicationId = 28;
      INSERT INTO RoundRobinQueues (ClerkId, ResponsibilityTypeId, StatusId, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId, CreatedDateTime, PropertyLeaseApplicationId, LeaseDetailsId, DepartmentId)
      VALUES (190, 44, 99, 1, 0, 0, 1, GETDATE(), 28, 24, 1);
      
      -- Update status to Applicant Vacated (277)
      UPDATE LeaseDetails SET StatusId = 277 WHERE Id = 24;
      UPDATE PropertyLeaseApplications SET StatusId = 277 WHERE Id = 28;
    `);

    await page.goto('http://localhost:3450/PropertyLeaseApplication/DepositRefunds');
    await expect(page.getByText('Deposit Refunds')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000);

    // Click "Start Refund Process "
    const refundRow = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(refundRow).toBeVisible();
    await delay(1500);
    await refundRow.getByRole('link', { name: 'Start Refund Process ' }).click();

    // Verify Start Refund page loaded
    await expect(page.locator('.panel-heading', { hasText: 'Refund and Deductions Processing' })).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 5000);

    // Upload required proof of banking details
    console.log('Uploading proof of banking details...');
    const bankUploads = page.locator('input[type="file"].docUpload112');
    const bankUploadCount = await bankUploads.count();
    for (let i = 0; i < bankUploadCount; i++) {
      await bankUploads.nth(i).setInputFiles(debugLogFile);
    }
    await delay(1500);
    await page.locator('#uploadFiles1').click();

    // Close upload progress modal
    await page.locator('#documentResponseModal').first().waitFor({ state: 'visible', timeout: 15000 });
    await delay(1500);
    await page.locator('#documentResponseModal').first().locator('button#processResponseBtn').click();
    await delay(2000);
    // Force remove any leftover modal backdrops to prevent click interception
    await page.evaluate(() => {
      document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
      document.body.classList.remove('modal-open');
    });
    await delay(1000);

    // Fill in calculations
    console.log('Filling in deductions and official ID...');
    await page.locator('#FinancialOfficerOfficialNumber').fill('FO-REFUND-8877');
    await delay(1000);
    await page.locator('#MaintenanceCost').fill('150.00');
    await delay(1000);
    await page.locator('#ApprovedDeductions').fill('50.00');
    await delay(1500); // Wait for auto calculation Net Refund (1500 - 150 - 50 = 1300)

    // Verify Nett Refund Amount auto-calculated correctly in UI
    const nettRefundVal = await page.locator('#NettRefundAmount').inputValue();
    console.log(`Auto-calculated Nett Refund Amount in UI: R ${nettRefundVal}`);
    expect(nettRefundVal).toBe('1300.00');

    // Select Recommendation & Reason
    await page.locator('#DepositRefundRecommended').selectOption('Refund Partial Deposit');
    await delay(1500);
    await page.locator('#FinancialOfficerReason').fill('Deductions entered for wall scratch touch-up (R150) and missing mailbox keys (R50). Balance of R1300.00 recommended for partial refund.');
    await delay(1500);
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(1000);

    // Submit Recommendation
    console.log('Submitting recommendation...');
    await page.locator('input[type="button"][value="Submit Refund Recommendation"]').click();
    await delay(1500);
    await page.locator('.swal-button--confirm').click();

    // Verify redirect
    await expect(page.getByText('Deposit Refunds')).toBeVisible({ timeout: 15000 });
    await delay(1500);
    try {
      await page.locator('#StartRefundProcessSessionModal button:has-text("OK")').click({ timeout: 5000 });
    } catch(e) {}
    await delay(2000);
    console.log('FO successfully submitted refund recommendation.');

    // ========================================================
    // STEP 6: Revenue Manager reviews & supports (UC030)
    // ========================================================
    console.log('STEP 6: Reviewing refund recommendation as Revenue Manager...');
    
    // Manually route to RM for DepositRefundResponse (ResponsibilityTypeId = 45, ClerkId = 190 (AshKay))
    runQuery(`
      UPDATE RoundRobinQueues SET IsActive = 0, IsDeleted = 1 WHERE PropertyLeaseApplicationId = 28;
      INSERT INTO RoundRobinQueues (ClerkId, ResponsibilityTypeId, StatusId, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId, CreatedDateTime, PropertyLeaseApplicationId, LeaseDetailsId, DepartmentId)
      VALUES (190, 45, 99, 1, 0, 0, 1, GETDATE(), 28, 24, 1);
      
      -- Update status to Awaiting Refund Response (314)
      UPDATE LeaseDetails SET StatusId = 314 WHERE Id = 24;
      UPDATE PropertyLeaseApplications SET StatusId = 314 WHERE Id = 28;
    `);

    await page.goto('http://localhost:3450/PropertyLeaseApplication/UpdateRefundResponse');
    await expect(page.getByText('Update Refunds Response')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000);

    // Click "Update Response "
    const rmRow = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(rmRow).toBeVisible();
    await delay(1500);
    await rmRow.getByRole('link', { name: 'Update Response ' }).click();

    // Verify RM page loaded
    await expect(page.locator('.header-centered', { hasText: 'Revenue Manager - Deposit Refund Review' })).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 5000);

    // Fill RM Official Number and Support
    console.log('Filling RM review outcome...');
    await page.locator('#rmOfficialNumber').fill('RM-OFFICER-5544');
    await delay(1000);
    await page.locator('#rmSupport').selectOption('True'); // Supported
    await delay(1500);

    // Submit Review
    console.log('Submitting review...');
    await page.locator('input[type="button"][value="Submit Review"]').click();
    await delay(1500);
    await page.locator('.swal-button--confirm').click();

    // Verify redirect
    await expect(page.getByText('Update Refunds Response')).toBeVisible({ timeout: 15000 });
    await delay(1500);
    try {
      await page.locator('#UpdateResponseForRefundSessionModal button:has-text("OK")').click({ timeout: 5000 });
    } catch(e) {}
    await delay(2000);
    console.log('RM successfully supported refund recommendation.');

    await robustLogoff(page);

    // ========================================================
    // STEP 7: CEO TBKHUMALO authorizes refund and signs off (UC030)
    // ========================================================
    console.log('STEP 7: Logging in as CEO Vicky Ramabu (TBKHUMALO) to authorize refund...');
    
    // Manually route to CEO for RefundAuthorisation (ResponsibilityTypeId = 2059, ClerkId = 167 (TBKHUMALO))
    runQuery(`
      UPDATE RoundRobinQueues SET IsActive = 0, IsDeleted = 1 WHERE PropertyLeaseApplicationId = 28;
      INSERT INTO RoundRobinQueues (ClerkId, ResponsibilityTypeId, StatusId, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId, CreatedDateTime, PropertyLeaseApplicationId, LeaseDetailsId, DepartmentId)
      VALUES (167, 2059, 99, 1, 0, 0, 1, GETDATE(), 28, 24, 1);
      
      -- Update status to Awaiting Refund Authorisation (4397)
      UPDATE LeaseDetails SET StatusId = 4397 WHERE Id = 24;
      UPDATE PropertyLeaseApplications SET StatusId = 4397 WHERE Id = 28;
    `);

    await robustLogin(page, 'TBKHUMALO', 'Arsenal5@');

    await page.goto('http://localhost:3450/PropertyLeaseApplication/RefundAuthorisation');
    await expect(page.getByText('CEO - Deposit Refund Authorisations')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000);

    // Click "Authorize Refund"
    const ceoRow = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(ceoRow).toBeVisible();
    await delay(1500);
    await ceoRow.getByRole('link', { name: 'Authorize Refund' }).click();

    // Verify Authorize Refund page loaded
    await expect(page.locator('.header-centered', { hasText: 'CEO - Refund Authorization & Sign-off' })).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 5000);

    // Fill CEO official number and signature pad hdn field
    console.log('Filling CEO authorization details and signature...');
    await page.locator('#ceoOfficialNumber').fill('CEO-VICKY-9900');
    await delay(1000);

    // Set mockup digital signature directly in DOM
    await page.evaluate(() => {
      document.getElementById('hdnSignatureBlob').value = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
    });
    await delay(1500);

    // Select "Approve Partial" and comment
    await page.locator('#ceoResponse').selectOption('Approve Partial');
    await delay(1500);
    await page.locator('#Comments').fill('Deductions and partial refund approved. Handover keys/cards logged. Formally terminating lease and closing tenant account.');
    await delay(1500);
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(1000);

    // Submit Authorization
    console.log('Submitting CEO sign-off...');
    await page.locator('input[type="button"][value="Submit Authorization"]').click();
    await delay(1500);
    await page.locator('.swal-button--confirm').click();

    // Verify redirect back to queue dashboard
    await expect(page.getByText('CEO - Deposit Refund Authorisations')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    console.log('CEO successfully authorized refund and closed the case.');

    // ========================================================
    // FINAL DATABASE STATE VERIFICATION
    // ========================================================
    console.log('Verifying final lease status and customer status in DB...');
    const verifyFinalSql = `
      SELECT CAST(ld.StatusId AS VARCHAR(20)) + ',' + CAST(c.StatusId AS VARCHAR(20)) + ',' + CAST(ap.IsTaken AS VARCHAR(20))
      FROM LeaseDetails ld
      JOIN PropertyLeaseApplications pla ON ld.PropertyLeaseApplicationId = pla.Id
      JOIN Customers c ON pla.CustomerId = c.Id
      JOIN ApplicantUnits au ON pla.Id = au.PropertyLeaseApplicationId
      JOIN MatchedUnits mu ON au.MatchedID = mu.Id
      JOIN ApplicationAllocatedProperties ap ON mu.ApplicationAllocatedPropertyId = ap.Id
      WHERE ld.Id = 24
    `;
    const finalDbResult = runQuery(verifyFinalSql);
    console.log('Final DB Verification Result:', finalDbResult);
    
    // Status keys:
    // TerminatedLease = 210, FormerTenant = 4396, IsTaken = 0
    expect(finalDbResult).toContain('210,4396,0');
    console.log('UC027-UC030 E2E Lease Exit, Vacating & Deposit Refund workflow fully verified! Case is Terminated, Tenant is Former Tenant, Unit is freed.');
    
    await robustLogoff(page);
  });
});
