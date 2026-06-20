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
    const tempFile = path.join(__dirname, 'temp_query_uc026.sql');
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

// Reset Database state before starting the disputes test
function resetDatabase() {
  console.log('Resetting Database State to Step 1 (Active Lease for Tenant sash38)...');
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

    -- 1b. Deactivate all other leases for the same tenant to ensure Id = 24 is selected
    UPDATE LeaseDetails 
    SET IsActive = 0 
    WHERE Id <> 24 AND PropertyLeaseApplicationId IN (
        SELECT Id FROM PropertyLeaseApplications WHERE CustomerId = (
            SELECT CustomerId FROM PropertyLeaseApplications WHERE Id = 28
        )
    );

    -- 2. Delete all existing Disputes to ensure absolute clean state for strict locator assertion
    DELETE FROM LeaseDisputes;

    -- 3. Delete related RoundRobinQueues jobs for Application 28
    DELETE FROM RoundRobinQueues WHERE PropertyLeaseApplicationId = 28;
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

test.describe('UC026 Manage Disputes E2E Workflow Spec', () => {
  
  test.beforeAll(async () => {
    resetDatabase();
  });

  test('Execute Full E2E Dispute Capture, Review, Resolution and Closure Lifecycle', async ({ page }) => {
    test.setTimeout(600000); // 10 minutes to accommodate slow visual scrolling

    const debugLogFile = path.join(__dirname, 'browser_debug_uc026.log');
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
    // STEP 1: Tenant sash38 Captures and Registers Dispute (UC26A)
    // ========================================================
    console.log('STEP 1: Logging in as Tenant sash38 to register a lease dispute...');
    await robustLogin(page, 'sash38', 'Arsenal5@');

    // Go to Disputes Dashboard
    await page.goto('http://localhost:3450/PropertyLeaseApplication/LeaseDisputes');
    await expect(page.getByText('Lease Dispute Management')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Review empty disputes dashboard

    // Select "Register Lease Dispute" from the sidebar dropdown
    console.log('Navigating to Register Lease Dispute screen...');
    await page.goto('http://localhost:3450/PropertyLeaseApplication/RegisterLeaseDispute');
    
    // Verify registration form loaded
    await expect(page.locator('.panel-heading', { hasText: 'Register Dispute Details' })).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 5000); // Visual review of the pre-populated lease info and capture fields

    // Fill in the capture fields
    console.log('Filling in dispute capture details...');
    await page.locator('input[name="ReportedByName"]').fill('Sashen Moodley');
    await delay(1000);
    await page.locator('input[name="ContactNumber"]').fill('082 123 4567');
    await delay(1000);
    await page.locator('input[name="EmailAddress"]').fill('sashen@moodley.co.za');
    await delay(1000);
    
    console.log('Selecting dispute category and subcategory...');
    await page.locator('#Category').selectOption('Financial and Billing');
    await delay(1500);
    await page.locator('input[name="SubCategory"]').fill('Incorrect rental amount');
    await delay(1000);
    
    console.log('Entering detailed dispute description...');
    await page.locator('textarea[name="Description"]').fill('The monthly rental charged is R5,500 but the signed lease agreement specifies R4,800. Please correct the billing ledger and refund/credit the difference.');
    await delay(1500);

    // Scroll to the submit button
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(3000);

    // Submit the dispute registration
    console.log('Submitting dispute registration...');
    await page.locator('input[type="button"][value="Register Dispute"]').click();
    await delay(2000); // Wait for SweetAlert dialog to show
    await page.locator('.swal-button--confirm').click();

    // Verify redirected back to dashboard showing success notification
    await expect(page.getByText('Lease Dispute Management')).toBeVisible({ timeout: 15000 });
    await delay(2000); // Allow SweetAlert success alert to render
    console.log('Dispute registered successfully! Reference number created.');
    
    // Assert that the dispute reference is in the dashboard list
    const disputeRow = page.locator('tr', { hasText: 'Financial and Billing' });
    await expect(disputeRow).toBeVisible();
    await delay(2000);
    await humanScroll(page, 4000); // Visual review of the dashboard containing the new dispute
    
    // Check status in DB
    console.log('Checking dispute status in database (should be Dispute Open — Awaiting Review)...');
    const verifyOpenSql = `
      SELECT CAST(s.[Key] AS VARCHAR(100)) AS StatusKey, s.Name AS StatusName
      FROM LeaseDisputes ld
      JOIN Status s ON ld.StatusId = s.Id
      WHERE ld.LeaseDetailsId = 24;
    `;
    let dbOpenResult = runQuery(verifyOpenSql);
    console.log('Open DB Verification Result:', dbOpenResult);
    expect(dbOpenResult).toContain('s_dispute_open_awaiting_review');
    expect(dbOpenResult).toContain('Dispute Open');

    await robustLogoff(page);

    // ========================================================
    // STEP 2: CSO Clerk AshKay Reviews Dispute (UC26B)
    // ========================================================
    console.log('STEP 2: Logging in as CSO Clerk AshKay to review the dispute...');
    await robustLogin(page, 'AshKay', 'Arsenal5@');

    await page.goto('http://localhost:3450/PropertyLeaseApplication/LeaseDisputes');
    await expect(page.getByText('Lease Dispute Management')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Review disputes list

    // Click "Review" on the dispute row
    const rowReview = page.locator('tr', { hasText: 'Financial and Billing' });
    await expect(rowReview).toBeVisible();
    await delay(1500);
    await rowReview.getByRole('link', { name: 'Review' }).click();

    // Verify review page loaded
    await expect(page.locator('.panel-heading', { hasText: 'CSO Review' })).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 5000); // Slow scroll to review case information

    // Fill in CSO review details
    console.log('Filling in review details...');
    await page.locator('#RiskClassification').selectOption('Medium');
    await delay(1500);
    await page.locator('input[name="OfficialNumber"]').fill('CSO-DISP-REV-1002');
    await delay(1000);
    await page.locator('textarea[name="ReviewComment"]').fill('Vetted dispute details against the signed agreement. Verified billing discrepancy on the ledger. Referring to Revenue Manager for ledger correction.');
    await delay(1500);

    // Scroll to action and submit
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(3000);

    console.log('Submitting review...');
    await page.locator('input[type="button"][value="Submit Review"]').click();
    await delay(2000); // Wait for SweetAlert dialog
    await page.locator('.swal-button--confirm').click();

    // Verify redirected back to dashboard with status Dispute Referred
    await expect(page.getByText('Lease Dispute Management')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Visual review of the updated referred status

    // Check status in DB
    console.log('Checking dispute status in database (should be Dispute Referred)...');
    let dbReferredResult = runQuery(verifyOpenSql);
    console.log('Referred DB Verification Result:', dbReferredResult);
    expect(dbReferredResult).toContain('s_dispute_referred');
    expect(dbReferredResult).toContain('Dispute Referred');

    await robustLogoff(page);

    // ========================================================
    // STEP 3: Revenue Manager COESolarDev10 Resolves Dispute (UC26C-S1)
    // ========================================================
    console.log('STEP 3: Logging in as Revenue Manager COESolarDev10 to resolve the dispute...');
    await robustLogin(page, 'COESolarDev10', 'Arsenal5@');

    await page.goto('http://localhost:3450/PropertyLeaseApplication/LeaseDisputes');
    await expect(page.getByText('Lease Dispute Management')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000);

    // Click "Resolve" on the dispute row
    const rowResolve = page.locator('tr', { hasText: 'Financial and Billing' });
    await expect(rowResolve).toBeVisible();
    await delay(1500);
    await rowResolve.getByRole('link', { name: 'Resolve' }).click();

    // Verify resolve page loaded
    await expect(page.locator('.panel-heading', { hasText: 'Revenue Manager Resolution' })).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 5000); // Slow scroll to review details

    // Fill in RM resolution details
    console.log('Filling in resolution details...');
    await page.locator('#ApprovalStatusddl').selectOption('Rcs_Approval'); // "Resolved"
    await delay(2000); // Wait for Resolved fields panel to toggle show

    await page.locator('select[name="ResolutionOutcomeType"]').selectOption('Billing correction');
    await delay(1500);
    await page.locator('textarea[name="ResolutionSummary"]').fill('Reviewed billing ledger and signed contract. Corrected the monthly recurring charge from R5,500 to R4,800 and issued a credit of R700 for the overcharge. Ledger is now balanced.');
    await delay(1500);

    // Scroll to action and submit
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(3000);

    console.log('Submitting resolution...');
    await page.locator('input[type="button"][value="Submit"]').click();
    await delay(2000); // Wait for SweetAlert dialog
    await page.locator('.swal-button--confirm').click();

    // Verify redirected back to dashboard with status Dispute Resolved
    await expect(page.getByText('Lease Dispute Management')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Visual review of the updated resolved status

    // Check status in DB
    console.log('Checking dispute status in database (should be Dispute Resolved)...');
    let dbResolvedResult = runQuery(verifyOpenSql);
    console.log('Resolved DB Verification Result:', dbResolvedResult);
    expect(dbResolvedResult).toContain('s_dispute_resolved');
    expect(dbResolvedResult).toContain('Dispute Resolved');

    await robustLogoff(page);

    // ========================================================
    // STEP 4: CEO COESolarDev08 Closes Dispute (UC26C-S2)
    // ========================================================
    console.log('STEP 4: Logging in as CEO COESolarDev08 to close the dispute...');
    await robustLogin(page, 'COESolarDev08', 'Arsenal5@');

    await page.goto('http://localhost:3450/PropertyLeaseApplication/LeaseDisputes');
    await expect(page.getByText('Lease Dispute Management')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000);

    // Click "Close" on the dispute row
    const rowClose = page.locator('tr', { hasText: 'Financial and Billing' });
    await expect(rowClose).toBeVisible();
    await delay(1500);
    await rowClose.getByRole('link', { name: 'Close' }).click();

    // Verify close page loaded
    await expect(page.locator('.panel-heading', { hasText: 'CEO Closure Decision' })).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 5000); // Slow scroll to review details

    // Fill in CEO closure details
    console.log('Filling in closure details...');
    await page.locator('#ApprovalStatusddl').selectOption('Rcs_Approval'); // "Close Dispute"
    await delay(2000); // Wait for Close fields panel to toggle show

    await page.locator('select[name="ClosureOutcome"]').selectOption('Dispute resolved by mutual agreement');
    await delay(1500);
    await page.locator('textarea[name="ClosureSummary"]').fill('Formally signed off on billing correction and credit. Dispute resolved successfully in favor of the tenant. Case is now closed.');
    await delay(1500);

    // Scroll to action and submit
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await delay(3000);

    console.log('Submitting closure decision...');
    await page.locator('input[type="button"][value="Submit Decision"]').click();
    await delay(2000); // Wait for SweetAlert dialog
    await page.locator('.swal-button--confirm').click();

    // Verify redirected back to dashboard with status Dispute Closed
    await expect(page.getByText('Lease Dispute Management')).toBeVisible({ timeout: 15000 });
    await delay(2000);
    await humanScroll(page, 4000); // Visual review of the closed dispute status

    // Final Database Verification
    console.log('Performing final database status assertions...');
    
    // Check Dispute Status
    let dbClosedResult = runQuery(verifyOpenSql);
    console.log('Closed Dispute DB Verification Result:', dbClosedResult);
    expect(dbClosedResult).toContain('s_dispute_closed');
    expect(dbClosedResult).toContain('Dispute Closed');

    // Check Lease Details Status (should transition to Awaiting Exit Inspection)
    const verifyLeaseSql = `
      SELECT CAST(s.[Key] AS VARCHAR(100)) AS StatusKey, s.Name AS StatusName
      FROM LeaseDetails ld
      JOIN Status s ON ld.StatusId = s.Id
      WHERE ld.Id = 24;
    `;
    let dbLeaseResult = runQuery(verifyLeaseSql);
    console.log('Lease Details DB Verification Result:', dbLeaseResult);
    expect(dbLeaseResult).toContain('s_awaiting_exit_inspec');
    expect(dbLeaseResult).toContain('Awaiting Exit Inspection');

    console.log('UC026 Disputes Lifecycle E2E Spec fully executed and verified! Dispute status is Closed, Lease details status is Awaiting Exit Inspection.');
    await delay(2000);
    await robustLogoff(page);
  });
});
