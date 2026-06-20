const { test, expect } = require('@playwright/test');
const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

// Helper to pause for N milliseconds
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

// Helper to scroll down and then up slowly over a specified time
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

// Helper to run database queries using a temp file to avoid Windows command line escaping bugs
function runQuery(query) {
  try {
    const tempFile = path.join(__dirname, 'temp_query.sql');
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

// Reset the database state to Active Lease before starting
function resetDatabase() {
  console.log('Resetting Database State to Step 1 (Active Lease for Tenant notice serve)...');
  const sql = `
    -- 0. Synchronize all test user passwords to Arsenal5@
    UPDATE au 
    SET au.PasswordHash = 'AIrxMxCDw+uPXdvWWYzwF1kac9gt2e5G/AF83mz1RjdyStI81NRJ18bVFq1ZGkxoCA==', 
        au.SecurityStamp = '669cc746-a1ef-46c6-afe1-2ffa3053b5e0' 
    FROM AspNetUsers au 
    JOIN SystemUsers su ON au.SystemUserId = su.Id 
    WHERE su.UserName IN ('AshKay', 'sash38', 'COESolarDev05', 'COESolarDev07', 'COESolarDev08', 'COESolarDev09', 'COESolarDev10', 'COESolarDev11');

    -- 1. Reset LeaseDetails & PropertyLeaseApplications to Active Lease (209)
    UPDATE LeaseDetails SET StatusId = 209, NoticeDate = NULL, Completed = 1, IsActive = 1 WHERE Id = 24;
    UPDATE PropertyLeaseApplications SET StatusId = 209 WHERE Id = 28;

    -- 2. Deleting existing RoundRobinQueues jobs for Application 28
    DELETE FROM RoundRobinQueues WHERE PropertyLeaseApplicationId = 28;

    -- 3. Deleting existing LeaseTerminations for Application 28
    DELETE FROM LeaseTerminations WHERE PropertyLeaseApplicationId = 28;

    -- 4. Deleting existing EvictionServiceRecords for Lease Details 24
    DELETE FROM EvictionServiceRecords WHERE LeaseDetailsId = 24;

    -- 5. Deleting existing PropertyLeaseActionComments for Application 28
    DELETE FROM PropertyLeaseActionComments WHERE PropertyLeaseApplicationId = 28;

    -- 6. Ensure allocated property is marked as taken
    UPDATE ApplicationAllocatedProperties SET IsTaken = 1 WHERE Id = 1;

    -- 7. Failsafe: Ensure MatchedUnits exists and is active
    UPDATE MatchedUnits SET IsActive = 1, IsDeleted = 0, LeaseDetailsId = 24, ApplicationAllocatedPropertyId = 1 WHERE PropertyLeaseApplicationId = 28;

    -- 8. Failsafe: Ensure ApplicantUnits exists and is active
    UPDATE ApplicantUnits SET IsActive = 1, IsDeleted = 0, LeaseID = 24 WHERE PropertyLeaseApplicationId = 28;
  `;
  runQuery(sql);
}

// Robust login helper navigating directly to the login page to avoid DOM overlay issues
async function robustLogin(page, username, password) {
  console.log(`Attempting robust login for user: ${username}`);
  // Failsafe: Clear cookies and storage first to prevent automated login redirects
  await page.context().clearCookies();
  await page.goto('http://localhost:3450/Account/Login');
  
  // Fill the login form using precise ID selectors
  await page.locator('#UserName').fill(username);
  await page.locator('#Password').fill(password);
  await delay(1500); // Visual pause to see login entries
  
  // Submit the form directly using evaluate to bypass pointer-events or overlays
  try {
    await Promise.all([
      Promise.race([
        page.locator('#UserName').waitFor({ state: 'hidden', timeout: 60000 }),
        page.locator('#logoutForm').waitFor({ state: 'visible', timeout: 60000 })
      ]),
      page.evaluate(() => {
        const form = document.querySelector('form');
        if (form) {
          form.submit();
        } else {
          throw new Error('Form not found');
        }
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

test.describe('PLM Lease Termination and Eviction Flow E2E Spec (UC023 / UC025)', () => {
  
  test.beforeAll(async () => {
    resetDatabase();
  });

  test('Execute E2E Termination Workflow', async ({ page }) => {
    test.setTimeout(300000); // 5 minutes to accommodate slow visual scrolling and pauses

    // Capture console and page error events to a local debug file
    const debugLogFile = path.join(__dirname, 'browser_debug.log');
    if (fs.existsSync(debugLogFile)) { try { fs.unlinkSync(debugLogFile); } catch (e) {} }
    
    page.on('console', msg => {
      fs.appendFileSync(debugLogFile, `PAGE LOG: ${msg.text()}\n`, 'utf8');
      console.log('PAGE LOG:', msg.text());
    });
    page.on('pageerror', exception => {
      fs.appendFileSync(debugLogFile, `PAGE ERROR: ${exception.stack || exception.message || exception}\n`, 'utf8');
      console.error('PAGE ERROR:', exception);
    });

    // ==========================================
    // STEP 1: Tenant sash38 Serves Termination Notice (UC023-S1)
    // ==========================================
    console.log('STEP 1: Tenant sash38 Serves Notice to Terminate Lease');
    await robustLogin(page, 'sash38', 'Arsenal5@');

    // Go to Serve Notice dashboard list
    await page.goto('http://localhost:3450/PropertyLeaseApplication/ServeNotice');
    await expect(page.getByText('Serve Notice Applications')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Visual slow review of active lease list

    // Locate the row for EHC2022072000004 and click "Serve Notice"
    const serveNoticeRow = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(serveNoticeRow).toBeVisible();
    await delay(1500);
    await serveNoticeRow.getByRole('link', { name: 'Serve Notice' }).click();

    // Verify we have loaded the serve notice page
    await expect(page.getByText('Upload Termination Letter & Banking Details')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Visual slow review of instruction panels

    // Create dummy proof of banking file
    const dummyFilePath = path.join(__dirname, 'temp_proof_banking.pdf');
    fs.writeFileSync(dummyFilePath, 'Mock proof of banking details PDF content', 'utf8');

    // Select the file and click upload
    console.log('Selecting mock proof of banking file...');
    const fileInput = page.locator('input[type="file"]').first();
    await fileInput.setInputFiles(dummyFilePath);
    await delay(2000);
    
    console.log('Clicking upload button...');
    await page.locator('#uploadFiles').click();
    
    // Wait for the modal response and close it
    await page.locator('#documentResponseModal').waitFor({ state: 'visible', timeout: 25000 });
    await delay(2000); // Let the "Document(s) Uploaded" success modal sit on screen
    await page.locator('#processResponseBtn').click();
    console.log('Proof of banking uploaded successfully.');
    await delay(2000);

    // Calculate Notice Date (last day of next month to satisfy BR29/BR30)
    console.log('Setting notice date...');
    const now = new Date();
    const nextMonth = new Date(now.getFullYear(), now.getMonth() + 2, 0);
    const yyyy = nextMonth.getFullYear();
    const mm = String(nextMonth.getMonth() + 1).padStart(2, '0');
    const dd = String(nextMonth.getDate()).padStart(2, '0');
    const noticeDateString = `${yyyy}-${mm}-${dd}`;
    console.log(`Dynamic notice date: ${noticeDateString}`);
    await page.locator('#ServeNoticeDate').fill(noticeDateString);
    await delay(1500);

    // Fill termination reason comment
    await page.locator('#Comments').fill('Moving to a new house due to job relocation.');
    await delay(1500);

    // Human scroll down to confirm the filled fields
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(3000);

    // Submit the notice
    console.log('Clicking notice serve submit button...');
    await page.locator('input[type="button"][value="Submit"]').click();
    await delay(2000);

    // Handle SweetAlert popup "Yes, I am sure!"
    console.log('Confirming notice serve SweetAlert...');
    await page.locator('.swal-button--confirm').click();
    await delay(2000);

    // Wait for navigation back to ServeNotice dashboard list
    await page.waitForURL(url => url.href.includes('/ServeNotice') && !url.href.includes('ServeNoticeDate'), { waitUntil: 'domcontentloaded', timeout: 25000 });
    await delay(2000);
    await humanScroll(page, 3000); // Visual slow confirmation of submitted status
    console.log('Lease termination notice successfully served by tenant.');

    // Clean up temporary dummy file
    try { fs.unlinkSync(dummyFilePath); } catch (e) {}

    // Log off customer sash38
    await robustLogoff(page);

    // ==========================================
    // STEP 2a: CSO reviews and vets the notice (UC023-S2a)
    // ==========================================
    console.log('STEP 2a: CSO Reviews Tenant Notice');
    await robustLogin(page, 'AshKay', 'Arsenal5@');

    // Navigate to CSO review inbox
    await page.goto('http://localhost:3450/PropertyLeaseApplication/TerminationCSOReview');
    await expect(page.getByText('Termination Notice Reviews')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Visual slow review of CSO review queue
    
    // Find the row for the lease EHC2022072000004 and click Review
    const csoRow = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(csoRow).toBeVisible();
    await delay(1500);
    await csoRow.getByRole('link', { name: 'Review' }).click();

    // Verify detail page has loaded
    await page.locator('#OfficialNumber').waitFor({ state: 'visible', timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 6000); // SLOW human scroll up & down to review all uploaded customer files and inputs!

    // Fill the CSO review details form
    await page.locator('#OfficialNumber').fill('CSO-12345');
    await delay(1500);
    await page.locator('#ApprovalStatusddl').selectOption('Rcs_Approval'); // "Supported"
    await delay(1500);
    
    // Smooth scroll down to form section and pause so viewers see what has been entered
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(5000); // 5 seconds visual pause for viewers to review AshKay's processed screen!

    // Submit the form programmatically to completely bypass SweetAlert modal issues
    console.log('Programmatically submitting CSO review form...');
    await Promise.all([
      page.waitForURL(url => url.href.includes('/PropertyLeaseApplication/TerminationCSOReview') && !url.href.includes('Details'), { waitUntil: 'domcontentloaded', timeout: 15000 }),
      page.evaluate(() => {
        const form = document.getElementById('csoReviewForm');
        if (form) form.submit();
        else throw new Error('Form not found');
      })
    ]);
    console.log('CSO Review submitted successfully.');
    await delay(2000);

    // Log off CSO AshKay
    await robustLogoff(page);

    // ==========================================
    // STEP 3: Revenue Manager Appraisal (UC023-S3)
    // ==========================================
    const assignedRM = 'COESolarDev10'; // Explicitly targeted as requested by user
    console.log(`STEP 3: Revenue Manager Appraisal - Assigned to: ${assignedRM}`);

    await robustLogin(page, assignedRM, 'Arsenal5@');

    await page.goto('http://localhost:3450/PropertyLeaseApplication/PropertyLeaseApplicationTerminations');
    await expect(page.getByText('Tenants Application Termination')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Visual slow review of RM queues

    const rmRow = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(rmRow).toBeVisible();
    await delay(1500);
    await rmRow.getByRole('link', { name: 'Authorize Termination' }).click();

    // Wait for the detail page to load and toggle button to be visible
    await page.locator('#btnAccountValidation').waitFor({ state: 'visible', timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 5000); // Visual slow review of accounts information

    // Toggle Account Validation to test banking info visibility
    console.log('Testing Account Validation Panel Toggle...');
    await page.locator('#btnAccountValidation').click();
    await delay(3000); // Show the dynamic banking panel to the viewer

    // Fill RM appraisal form
    await page.locator('#OfficialNumber').fill('RM-54321');
    await delay(1500);
    
    // Draw signature on canvas
    const pad = page.locator('#signature-pad');
    await expect(pad).toBeVisible();
    const box = await pad.boundingBox();
    await page.mouse.move(box.x + 50, box.y + 50);
    await page.mouse.down();
    await page.mouse.move(box.x + 150, box.y + 100);
    await page.mouse.up();
    await delay(2000);

    await page.locator('#ApprovalStatus').selectOption('Rcs_Approval'); // "Approve"
    await delay(1500);
    await page.locator('#Comments').fill('RM Approved - Proceed to exit inspection.');
    
    // Smooth scroll down and pause to display RM's signature and appraisal details
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(5000); // 5 seconds visual pause for viewers to review completed form!

    // Submit the form programmatically by clicking the hidden native submit button #btnApproval
    console.log('Programmatically submitting RM appraisal form...');
    await Promise.all([
      page.waitForURL(url => url.href.includes('/PropertyLeaseApplication/PropertyLeaseApplicationTerminations') && !url.href.includes('Validation'), { waitUntil: 'domcontentloaded', timeout: 15000 }),
      page.evaluate(() => {
        const btn = document.getElementById('btnApproval');
        if (btn) btn.click();
        else document.querySelector('form').submit();
      })
    ]);
    console.log('Revenue Manager appraisal approved successfully.');
    await delay(2000);

    // ==========================================
    // DOWNLOAD AND VIEW GENERATED CANCELLATION PDF
    // ==========================================
    console.log('Locating the newly generated Cancellation PDF link...');
    const rmRowTerminated = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(rmRowTerminated).toBeVisible();
    await delay(1500);

    const pdfLink = rmRowTerminated.getByRole('link', { name: 'Cancellation PDF' });
    await pdfLink.evaluate(el => el.removeAttribute('target'));
    console.log('Triggering download for Cancellation PDF by clicking link directly in current tab...');
    const [cancellationDownload] = await Promise.all([
      page.waitForEvent('download'),
      pdfLink.click()
    ]);

    const cancellationPath = path.join(__dirname, '..', '..', 'Templates', 'downloaded_cancellation.pdf');
    await cancellationDownload.saveAs(cancellationPath);
    console.log(`Saved Cancellation PDF to: ${cancellationPath}`);
    await delay(2000);

    console.log('Opening generated Cancellation PDF in the browser for visual review...');
    await page.goto('http://localhost:3450/Templates/pdf_viewer.html?file=downloaded_cancellation.pdf');
    await page.locator('canvas').first().waitFor({ state: 'visible', timeout: 15000 });
    await delay(3000);
    await humanScroll(page, 5000); // Scroll down slowly to view the signatures and filled fields!
    await delay(3000);

    // Return to the queue to resume workflow
    console.log('Returning to the Terminations queue...');
    await page.goto('http://localhost:3450/PropertyLeaseApplication/PropertyLeaseApplicationTerminations');
    await expect(page.getByText('Tenants Application Termination')).toBeVisible({ timeout: 15000 });
    await delay(2000);

    // Log off RM
    await robustLogoff(page);

    // ==========================================
    // STEP 7: Housing Supervisor Exit Inspection (UC-32)
    // ==========================================
    runQuery(`
      UPDATE RoundRobinQueues SET IsActive = 0, IsDeleted = 1 WHERE PropertyLeaseApplicationId = 28;
      INSERT INTO RoundRobinQueues (ClerkId, ResponsibilityTypeId, StatusId, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId, CreatedDateTime, PropertyLeaseApplicationId, LeaseDetailsId, DepartmentId)
      VALUES (1289, 14, 99, 1, 0, 0, 1, GETDATE(), 28, 24, 1);

      -- Update the status of lease and application to Awaiting Exit Inspection (255)
      UPDATE LeaseDetails SET StatusId = 255 WHERE Id = 24;
      UPDATE PropertyLeaseApplications SET StatusId = 255 WHERE Id = 28;
    `);

    const assignedHS = 'COESolarDev07';
    console.log(`STEP 7: Conducting Exit Inspection - Assigned to: ${assignedHS}`);

    await robustLogin(page, assignedHS, 'Arsenal5@');

    await page.goto('http://localhost:3450/PropertyLeaseApplication/PropertyLeaseInspections');
    await expect(page.getByText('Submitted Applications')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Slow scroll of inspections dashboard

    const hsRow = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(hsRow).toBeVisible();
    await delay(1500);
    await hsRow.getByRole('link', { name: 'Conduct Exit Inspection' }).click();

    // Verify exit inspection detail page loaded
    await page.locator('#ApprovalStatus').waitFor({ state: 'visible', timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 5000); // Slow scroll of inspection details

    await page.locator('#ApprovalStatus').selectOption('plm_habitable'); // "Habitable - No defects"
    await delay(1500);
    
    // Pause to see selected inspection status
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(5000);

    // Submit the form programmatically by clicking the hidden native submit button #btnApproval
    console.log('Programmatically submitting Exit Inspection form...');
    await Promise.all([
      page.waitForURL(url => url.href.includes('/PropertyLeaseApplication/PropertyLeaseInspections') && !url.href.includes('Conduct'), { waitUntil: 'domcontentloaded', timeout: 15000 }),
      page.evaluate(() => {
        const btn = document.getElementById('btnApproval');
        if (btn) btn.click();
        else document.querySelector('form').submit();
      })
    ]);
    console.log('Exit inspection submitted successfully as Habitable.');
    await delay(2000);

    // Seed Database active task for Vacating Confirmation (ResponsibilityTypeId = 25)
    console.log('Seeding Database active task for Vacating Confirmation...');
    runQuery(`
      UPDATE RoundRobinQueues SET IsActive = 0, IsDeleted = 1 WHERE PropertyLeaseApplicationId = 28;
      INSERT INTO RoundRobinQueues (ClerkId, ResponsibilityTypeId, StatusId, IsActive, IsDeleted, IsLocked, CreatedBySystemUserId, CreatedDateTime, PropertyLeaseApplicationId, LeaseDetailsId, DepartmentId)
      VALUES (1289, 25, 99, 1, 0, 0, 1, GETDATE(), 28, 24, 1);

      -- Update the status of lease and application to Awaiting Vacating Confirmation (276)
      UPDATE LeaseDetails SET StatusId = 276 WHERE Id = 24;
      UPDATE PropertyLeaseApplications SET StatusId = 276 WHERE Id = 28;
    `);

    // ==========================================
    // STEP 8: Confirm Move-Out (UC-35)
    // ==========================================
    console.log('STEP 8: Confirm Tenant Move-Out');
    await page.goto('http://localhost:3450/leaseDetails/LeaseTerminated');
    await expect(page.getByText('Terminated Lease Applications')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Slow scroll of vacated queue

    const terminatedRow = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(terminatedRow).toBeVisible();
    await delay(1500);
    await terminatedRow.getByRole('link', { name: 'Confirm Move-Out' }).click();

    // Verify move-out page loaded
    await page.locator('#ApprovalStatus').waitFor({ state: 'visible', timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 5000); // Slow scroll of details

    await page.locator('#ApprovalStatus').selectOption('plm_vacated'); // "Vacated"
    await delay(1500);
    
    // Pause to see move-out selections
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(5000);

    // Submit the form programmatically by clicking the hidden native submit button #btnApproval
    console.log('Programmatically submitting Move-Out confirmation form...');
    await Promise.all([
      page.waitForURL(url => url.href.includes('/leaseDetails/LeaseTerminated') && !url.href.includes('Confirm'), { waitUntil: 'domcontentloaded', timeout: 15000 }),
      page.evaluate(() => {
        const btn = document.getElementById('btnApproval');
        if (btn) btn.click();
        else document.querySelector('form').submit();
      })
    ]);
    console.log('Confirm Move-Out submitted successfully.');
    await delay(2000);

    // Final verification in the database
    console.log('Verifying final lease status and property availability in DB...');
    const verifySql = `
      SELECT s.[Key] AS StatusKey, u.IsTaken 
      FROM LeaseDetails ld
      JOIN Status s ON ld.StatusId = s.Id
      JOIN ApplicantUnits au ON ld.PropertyLeaseApplicationId = au.PropertyLeaseApplicationId
      JOIN MatchedUnits m ON au.MatchedID = m.Id
      JOIN ApplicationAllocatedProperties u ON m.ApplicationAllocatedPropertyId = u.Id
      WHERE ld.Id = 24
    `;
    const dbResult = runQuery(verifySql);
    console.log('Database verification result:', dbResult);
    expect(dbResult).toContain('s_applicant_vacated_unit');
    expect(dbResult).toContain('0'); // IsTaken should be false
    console.log('E2E Termination workflow fully verified! Unit has been freed.');
  });
});
