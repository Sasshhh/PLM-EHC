const { test, expect } = require('@playwright/test');
const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

// Helper to pause for N milliseconds
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

// Helper to scroll down and then up slowly over a specified time for video demo
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
    const tempFile = path.join(__dirname, 'temp_query_uc025.sql');
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

// Reset database state to Awaiting Eviction Service for Lease details ID 24
function resetDatabase() {
  console.log('Resetting Database State to UC025: Awaiting Eviction Service for Lease EHC2022072000004...');
  const sql = `
    -- 0. Synchronize all test user passwords to Arsenal5@
    UPDATE au 
    SET au.PasswordHash = 'AIrxMxCDw+uPXdvWWYzwF1kac9gt2e5G/AF83mz1RjdyStI81NRJ18bVFq1ZGkxoCA==', 
        au.SecurityStamp = '669cc746-a1ef-46c6-afe1-2ffa3053b5e0' 
    FROM AspNetUsers au 
    JOIN SystemUsers su ON au.SystemUserId = su.Id 
    WHERE su.UserName IN ('AshKay', 'sash38', 'COESolarDev05', 'COESolarDev07', 'COESolarDev08', 'COESolarDev09', 'COESolarDev10', 'COESolarDev11');

    -- Get Awaiting Eviction Service Status ID
    DECLARE @AwaitingServiceId INT = (SELECT TOP 1 Id FROM Status WHERE [Key] = 's_awaiting_eviction_service');

    -- Reset LeaseDetails and PropertyLeaseApplications
    UPDATE LeaseDetails SET StatusId = @AwaitingServiceId, Completed = 1, IsActive = 1 WHERE Id = 24;
    UPDATE PropertyLeaseApplications SET StatusId = @AwaitingServiceId WHERE Id = 28;

    -- Deleting existing EvictionServiceRecords for Lease Details 24
    DELETE FROM EvictionServiceRecords WHERE LeaseDetailsId = 24;

    -- Seed LeaseTerminations record if not exists
    DELETE FROM LeaseTerminations WHERE PropertyLeaseApplicationId = 28;
    INSERT INTO LeaseTerminations (PropertyLeaseApplicationId, LeaseDetailsId, LeaseReferenceNumber, TerminationDate, ReasonForTermination, CreatedBySystemUserId, CreatedDateTime, IsActive, IsDeleted)
    VALUES (28, 24, 'EHC2022072000004', '2026-06-30', 'Transgression breach - 90 days outstanding balance.', 1, GETDATE(), 1, 0);

    -- Delete any existing Documents/Files of eviction notice proof to make the test idempotent
    DECLARE @DocTypeId INT = (SELECT TOP 1 Id FROM DocumentTypes WHERE [Key] = 'dt_property_eviction_document');
    DELETE FROM Documents WHERE PropertyLeaseApplicationId = 28 AND DocumentCheckListId IN (SELECT Id FROM DocumentCheckLists WHERE DocumentTypeId = @DocTypeId);

    -- Ensure plm_awaiting_debit_order is seeded in EmailContentTypes (failsafe)
    IF NOT EXISTS (SELECT 1 FROM EmailContentTypes WHERE [Key] = 'plm_awaiting_debit_order')
    BEGIN
        INSERT INTO EmailContentTypes ([Key], Name, Subject, Body, IsActive, IsDeleted, CreatedBySystemUserId, CreatedDateTime)
        VALUES ('plm_awaiting_debit_order', 'Awaiting Debit Order Instruction', 'Lease Approved - Billing & Debit Order Setup', 'Please visit CCC.', 1, 0, 1, GETDATE());
    END
  `;
  runQuery(sql);
  console.log('Database state reset successfully.');
}

// Robust login helper
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
        else throw new Error('Login form not found');
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

test.describe('UC025 Serve Eviction Notice & Capture Proof of Service E2E Spec', () => {
  
  test.beforeAll(async () => {
    resetDatabase();
    
    // Create a mock PDF file for upload testing
    const mockPdfPath = path.join(__dirname, 'SheriffReturn.pdf');
    fs.writeFileSync(mockPdfPath, '%PDF-1.4 mock sheriff return of service data');
    console.log(`Mock upload PDF created at: ${mockPdfPath}`);
  });

  test.afterAll(async () => {
    // Clean up mock PDF
    const mockPdfPath = path.join(__dirname, 'SheriffReturn.pdf');
    if (fs.existsSync(mockPdfPath)) {
      try { fs.unlinkSync(mockPdfPath); } catch (e) {}
    }
  });

  test('Execute E2E Notice Service and Proof of Service Capture Workflow', async ({ page }) => {
    test.setTimeout(300000); // 5 minutes

    const debugLogFile = path.join(__dirname, 'browser_debug_uc025.log');
    if (fs.existsSync(debugLogFile)) { try { fs.unlinkSync(debugLogFile); } catch (e) {} }
    
    page.on('console', msg => {
      fs.appendFileSync(debugLogFile, `PAGE LOG: ${msg.text()}\n`, 'utf8');
      console.log('PAGE LOG:', msg.text());
    });
    page.on('pageerror', exception => {
      fs.appendFileSync(debugLogFile, `PAGE ERROR: ${exception.stack || exception.message || exception}\n`, 'utf8');
      console.error('PAGE ERROR:', exception);
    });

    // ========================================================
    // STEP 1: CSO Clerk AshKay Serves Eviction Notice
    // ========================================================
    console.log('STEP 1: Logging in as CSO Clerk AshKay to serve the eviction notice...');
    await robustLogin(page, 'AshKay', 'Arsenal5@');

    // Go to Terminations queue dashboard
    await page.goto('http://localhost:3450/PropertyLeaseApplication/PropertyLeaseApplicationTerminations');
    await expect(page.getByText('Tenants Application Termination')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Review the terminations queue

    // Find the row for EHC2022072000004 and verify its action link
    const rowServe = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(rowServe).toBeVisible();
    await delay(1500);
    await rowServe.getByRole('link', { name: 'Serve Eviction Notice' }).click();

    // Verify redirected to ServeEvictionNotice page
    await expect(page.locator('.panel-heading', { hasText: 'Serve Eviction Notice' })).toBeVisible({ timeout: 15000 });
    console.log('Successfully redirected to ServeEvictionNotice page.');
    await delay(2000);
    await humanScroll(page, 5000); // Visual slow review of application details

    // Fill the Serve notice form
    console.log('Selecting service method from the dropdown...');
    await page.locator('#ServiceMethod').selectOption('Hand Delivery');
    await delay(1500);

    console.log('Entering service date...');
    await page.locator('input[name="ServiceDate"]').fill('2026-06-01');
    await delay(1500);

    console.log('Entering official/case number...');
    await page.locator('input[name="OfficialNumber"]').fill('OFF-SERV-1002');
    await delay(1500);

    // Scroll down to the action buttons and review
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(3000); // Visual pause for the audience

    // ==========================================
    // DOWNLOAD AND VIEW GENERATED EVICTION NOTICE PDF
    // ==========================================
    const serveNoticeUrl = page.url();
    const evictionLink = page.getByRole('link', { name: 'Download / View Notice' });
    await evictionLink.evaluate(el => el.removeAttribute('target'));
    console.log('Triggering download for Eviction Notice PDF by clicking link directly in current tab...');
    const [evictionDownload] = await Promise.all([
      page.waitForEvent('download'),
      evictionLink.click()
    ]);

    const evictionPath = path.join(__dirname, '..', '..', 'Templates', 'downloaded_eviction.pdf');
    await evictionDownload.saveAs(evictionPath);
    console.log(`Saved Eviction Notice PDF to: ${evictionPath}`);
    await delay(2000);

    console.log('Opening generated Eviction Notice PDF in the browser for visual review...');
    await page.goto('http://localhost:3450/Templates/pdf_viewer.html?file=downloaded_eviction.pdf');
    await page.locator('canvas').first().waitFor({ state: 'visible', timeout: 15000 });
    await delay(3000);
    await humanScroll(page, 5000); // Scroll down slowly to view the signatures and filled fields!
    await delay(3000);

    // Navigate back to the Serve notice page to submit it
    console.log('Returning to the Serve Notice form...');
    await page.goto(serveNoticeUrl);
    await page.locator('#ServiceMethod').waitFor({ state: 'visible', timeout: 15000 });
    // Re-fill form values since navigation back resets them
    await page.locator('#ServiceMethod').selectOption('Hand Delivery');
    await page.locator('input[name="ServiceDate"]').fill('2026-06-01');
    await page.locator('input[name="OfficialNumber"]').fill('OFF-SERV-1002');
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(2000);

    // Click "Serve Notice" and handle SweetAlert confirmation popup
    console.log('Submitting the Serve Notice form...');
    await page.locator('input[type="button"][value="Serve Notice"]').click();
    await delay(2000); // Wait for SweetAlert dialog to show
    await page.locator('.swal-button--confirm').click();

    // Verify redirect to dashboard with sweetalert success notification
    await expect(page.getByText('Tenants Application Termination')).toBeVisible({ timeout: 15000 });
    await delay(2000); // Allow SweetAlert success alert to render
    console.log('Eviction notice served successfully! Handling success alert...');
    await page.locator('.swal-button--confirm').click();
    await delay(2000);

    // Assert that the lease status has advanced to "Eviction Notice Served" in the database
    console.log('Running database check for Lease status (Eviction Notice Served)...');
    const verifyServedSql = `
      SELECT CAST(s.[Key] AS VARCHAR(100)) AS StatusKey, s.Name AS StatusName
      FROM LeaseDetails ld
      JOIN Status s ON ld.StatusId = s.Id
      WHERE ld.Id = 24;
    `;
    let dbServedResult = runQuery(verifyServedSql);
    console.log('Served DB Verification Result:', dbServedResult);
    expect(dbServedResult).toContain('s_eviction_notice_served');
    expect(dbServedResult).toContain('Eviction Notice Served');

    // ========================================================
    // STEP 2: CSO Clerk AshKay Captures Proof of Service
    // ========================================================
    console.log('STEP 2: Capturing Proof of Service for EHC2022072000004...');
    await humanScroll(page, 3000); // Visual queue review with served status

    const rowProof = page.locator('tr', { hasText: 'EHC2022072000004' });
    await expect(rowProof).toBeVisible();
    await delay(1500);
    await rowProof.getByRole('link', { name: 'Capture Proof of Service' }).click();

    // Verify redirected to CaptureProofOfService page
    await expect(page.locator('.panel-heading', { hasText: 'Capture Proof of Service' })).toBeVisible({ timeout: 15000 });
    console.log('Successfully redirected to CaptureProofOfService page.');
    await delay(2000);
    await humanScroll(page, 5000); // Visual slow review of details and proof forms

    // Fill the Capture Proof form
    console.log('Selecting Proof of Service Type...');
    await page.locator('#ProofOfServiceType').selectOption('Sheriff return of service');
    await delay(1500);

    console.log('Entering Proof Date...');
    await page.locator('input[name="ProofServiceDate"]').fill('2026-06-01');
    await delay(1500);

    console.log('Entering Comments...');
    await page.locator('textarea[name="ProofComments"]').fill('Formal sheriff return of service received and uploaded successfully.');
    await delay(1500);

    // Upload supporting evidence/document
    console.log('Uploading mandatory supporting document...');
    const fileChooserPromise = page.waitForEvent('filechooser');
    await page.locator('#ProofFile').click();
    const fileChooser = await fileChooserPromise;
    await fileChooser.setFiles(path.join(__dirname, 'SheriffReturn.pdf'));
    await delay(2000); // Wait for visual upload confirmation

    // Scroll to the submit button and review
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(3000);

    // Click "Capture Proof" and confirm SweetAlert
    console.log('Submitting Proof of Service...');
    await page.locator('input[type="button"][value="Capture Proof"]').click();
    await delay(2000); // Wait for SweetAlert dialog to show
    await page.locator('.swal-button--confirm').click();

    // Verify redirect to dashboard with sweetalert success notification
    await expect(page.getByText('Tenants Application Termination')).toBeVisible({ timeout: 15000 });
    await delay(2000); // Allow SweetAlert success alert to render
    console.log('Proof of service captured successfully! Handling success alert...');
    await page.locator('.swal-button--confirm').click();
    await delay(2000);

    // Final database verification: Status should be "Awaiting Exit Inspection" (s_awaiting_exit_inspection)
    console.log('Running final database check for Lease status (Awaiting Exit Inspection)...');
    const verifyInspectionSql = `
      SELECT CAST(s.[Key] AS VARCHAR(100)) AS StatusKey, s.Name AS StatusName
      FROM LeaseDetails ld
      JOIN Status s ON ld.StatusId = s.Id
      WHERE ld.Id = 24;
    `;
    let dbInspectionResult = runQuery(verifyInspectionSql);
    console.log('Final DB Verification Result:', dbInspectionResult);
    expect(dbInspectionResult).toContain('s_awaiting_exit_inspec');
    expect(dbInspectionResult).toContain('Awaiting Exit Inspection');

    // Assert that the file binary and document reference metadata are recorded in the SQL tables
    console.log('Asserting that the proof file and document are stored in the SQL database...');
    const verifyDocSql = `
      SELECT COUNT(*) AS DocumentCount 
      FROM Documents d
      JOIN Files f ON d.FileId = f.Id
      WHERE d.PropertyLeaseApplicationId = 28 
        AND d.DocumentName = 'EHC2022072000004_ProofOfService.pdf';
    `;
    let dbDocResult = runQuery(verifyDocSql);
    console.log('Uploaded Document Count in Database:', dbDocResult);
    expect(dbDocResult).toContain('1');

    console.log('UC025 Notice Service and Proof of Service Capture E2E Spec fully executed and verified! Status = Awaiting Exit Inspection.');
    await delay(2000);
    await robustLogoff(page);
  });
});
