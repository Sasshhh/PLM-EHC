const { test, expect } = require('@playwright/test');
const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

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

async function logoutUser(page) {
  console.log('Logging off user...');
  await page.evaluate(() => {
    const form = document.getElementById('logoutForm');
    if (form) form.submit();
    else window.location.href = '/Account/Login';
  });
  try {
    await page.waitForURL(url => url.pathname.includes('/Account/Login') || url.pathname === '/' || url.pathname === '', { timeout: 20000 });
  } catch (err) {
    console.warn('Logout navigation timed out:', err.message);
  }
  if (!page.url().includes('/Account/Login')) {
    await page.goto('/Account/Login');
    await applyZoom(page);
    await delay(800);
  }
}

async function loginUser(page, username, password) {
  console.log(`Logging in as ${username}...`);
  await page.locator('#UserName').fill(username);
  await delay(1000);
  await page.locator('#Password').fill(password);
  await delay(500);
  try {
    await page.locator('button[type="submit"]').click({ timeout: 5000, noWaitAfter: true });
  } catch (e) {
    console.log('Submit button click timed out, pressing Enter on password field instead...');
    await page.locator('#Password').press('Enter');
  }
  await page.waitForURL(url => !url.pathname.includes('/Account/Login'), { timeout: 60000, waitUntil: 'domcontentloaded' });
  await applyZoom(page);
  await delay(800);
}


// Helper to run database queries
function runQuery(query) {
  try {
    const tempFile = path.join(__dirname, 'temp_query_e2e.sql');
    fs.writeFileSync(tempFile, query, 'utf8');
    const cmd = `sqlcmd -S localhost -E -d PropertyLeaseManagementRealEstate -i "${tempFile}" -W`;
    const output = execSync(cmd, { encoding: 'utf8' });
    try { fs.unlinkSync(tempFile); } catch (e) {}
    return output.trim();
  } catch (err) {
    console.error('SQL Execution Error:', err);
    return '';
  }
}

test.use({
  viewport: { width: 3840, height: 2160 },
  video: {
    mode: 'on',
    size: { width: 3840, height: 2160 }
  }
});

test('Real Estate Complete End-to-End Workflow (UC 06 - UC 20) - 4K Video Presentation', async ({ page }) => {
  test.setTimeout(990000); // 11 minutes timeout for role-switching human-paced flow

  // Listen to browser console logs and errors
  page.on('console', msg => console.log('BROWSER CONSOLE:', msg.type(), msg.text()));
  page.on('pageerror', err => console.log('BROWSER ERROR:', err.message));

  // Create temporary mock upload directory and files
  const tempDir = path.join(__dirname, 'temp_e2e_mock_uploads');
  if (!fs.existsSync(tempDir)) {
    fs.mkdirSync(tempDir);
  }

  const fileNames = [
    'id.pdf', 'address.pdf', 'cipc.pdf', 'sars.pdf', 'profile.pdf',
    'references.pdf', 'letter.pdf', 'locality.pdf', 'zoning.pdf',
    'income.pdf', 'fee.pdf', 'evidence.pdf', 'resolution.pdf',
    'inspection.pdf', 'jobsheet.pdf', 'insurance.pdf', 'fitout.pdf', 'safety.pdf',
    'experience.pdf', 'financials.pdf', 'bizplan.pdf', 'mbd4.pdf'
  ];
  const filePaths = {};
  for (const name of fileNames) {
    const filePath = path.join(tempDir, name);
    fs.writeFileSync(filePath, `Dummy mock data for ${name}`);
    filePaths[name] = filePath;
  }

  try {
    // ========================================================
    // PART 1: CUSTOMER SUBMITS LEASE APPLICATION (UC 05)
    // ========================================================
    console.log('STEP 1: Logging in as Real Estate Customer...');
    await page.context().clearCookies();
    await page.goto('/Account/Login');
    await applyZoom(page);
    await delay(1000);

    await loginUser(page, 'RealEstateCustomer', 'Arsenal5@');
    await expect(page).toHaveURL(/.*\/RealEstate\/Inbox/, { timeout: 60000 });
    await applyZoom(page);
    console.log('Logged in successfully as Customer!');
    await delay(1000);

    console.log('STEP 2: Navigating to Capture page...');
    const reMenu = page.locator('ul.nav-list a:has-text("Real Estate Applications")');
    await reMenu.click();
    await delay(500);

    const captureMenu = page.locator('ul.nav-list a:has-text("Capture")');
    await captureMenu.click();

    await expect(page).toHaveURL(/.*\/RealEstate\/Capture/, { timeout: 15000 });
    await applyZoom(page);
    console.log('Navigated to capture page.');
    await delay(1000);

    console.log('STEP 3: Filling application form details...');
    await page.locator('#ddlApplicantType').selectOption({ label: 'Company (PTY LTD) / Partnership' });
    await delay(500);

    await page.locator('input[name="Application.EntityName"]').fill('Siyakhokha Tech Services (Pty) Ltd');
    await page.locator('input[name="Application.CompanyRegistrationNumber"]').fill('2026/000999/07');
    await page.locator('input[name="Application.VatRegistrationNumber"]').fill('4551122334');
    await page.locator('input[name="Application.TaxReferenceNumber"]').fill('9551122332');
    await page.locator('input[name="Application.AuthorizedRepresentativeName"]').fill('Sasha Rep');
    await page.locator('input[name="Application.AuthorizedRepresentativeCapacity"]').fill('Managing Director');
    await page.locator('input[name="Application.EntityRegisteredAddress"]').fill('12 Administration Avenue, Germiston');
    await page.locator('input[name="Application.EntityRegisteredPostalCode"]').fill('1401');
    await page.locator('input[name="Application.EntityTelephone"]').fill('0115550000');
    await page.locator('input[name="Application.EntityMobile"]').fill('0825550000');
    await page.locator('input[name="Application.EntityEmail"]').fill('sasha@siyakhokhatech.gov.za');
    await delay(500);



    console.log('STEP 5: Specifying premises details...');
    await page.locator('select[name="Application.PurposeOfLease"]').selectOption({ label: 'Offices/Professional Units' });
    await page.locator('select[name="Application.CCCId"]').selectOption({ value: '10' }); // Tokoza CCC
    await page.locator('input[name="Application.ErfFarmNumber"]').fill('Erf 90 Boksburg South');
    await page.locator('input[name="Application.PropertyAddress"]').fill('90 Commissioner Street, Boksburg');
    await page.locator('input[name="Application.TownshipSuburbFarmName"]').fill('Boksburg South');
    await page.locator('input[name="Application.PropertyPostalCode"]').fill('1459');
    await delay(800);

    console.log('Selecting Facility, Unit and Count...');
    await page.waitForSelector('#ddlSelectedFacility:not([disabled])', { timeout: 10000 });
    await page.locator('#ddlSelectedFacility').selectOption({ index: 1 });
    await delay(500);

    await page.waitForSelector('#ddlSelectedUnit:not([disabled])', { timeout: 10000 });
    await page.locator('#ddlSelectedUnit').selectOption({ index: 1 });
    await delay(500);

    await page.waitForSelector('#txtUnitCount:not([disabled])', { timeout: 10000 });
    await page.locator('#txtUnitCount').click();
    await page.locator('#txtUnitCount').fill('1');
    await delay(800);

    console.log('Adding unit to selection list...');
    await page.locator('#btnAddUnit').click();
    await delay(1000);

    console.log('STEP 6: Uploading pre-qualification files...');
    await page.setInputFiles('input[name="file_Id"]', filePaths['id.pdf']);
    await page.setInputFiles('input[name="file_Address"]', filePaths['address.pdf']);
    await page.setInputFiles('input[name="file_Cipc"]', filePaths['cipc.pdf']);
    await page.setInputFiles('input[name="file_Sars"]', filePaths['sars.pdf']);
    await page.setInputFiles('input[name="file_Profile"]', filePaths['profile.pdf']);
    await page.setInputFiles('input[name="file_Experience"]', filePaths['experience.pdf']);
    await page.setInputFiles('input[name="file_References"]', filePaths['references.pdf']);
    await page.setInputFiles('input[name="file_Letters"]', filePaths['letter.pdf']);
    await page.setInputFiles('input[name="file_Locality"]', filePaths['locality.pdf']);
    await page.setInputFiles('input[name="file_Zoning"]', filePaths['zoning.pdf']);
    await page.setInputFiles('input[name="file_Financials"]', filePaths['financials.pdf']);
    await page.setInputFiles('input[name="file_BusinessPlan"]', filePaths['bizplan.pdf']);
    await page.setInputFiles('input[name="file_Mbd4"]', filePaths['mbd4.pdf']);
    await page.setInputFiles('input[name="file_Fee"]', filePaths['fee.pdf']);
    await delay(1000);

    console.log('STEP 7: Submitting form...');
    await page.locator('#btnSubmitForm').click();
    await delay(800);

    const confirmBtn = page.locator('.swal-button--confirm');
    await expect(confirmBtn).toBeVisible();
    await confirmBtn.click();

    await expect(page).toHaveURL(/.*\/RealEstate\/MyApplications/, { timeout: 35000 });
    await applyZoom(page);
    console.log('Application submitted successfully!');
    await delay(1000);

    const refNumber = await page.locator('table.re-table tbody tr:last-child span.ref-badge').innerText();
    console.log(`Captured Reference Number: ${refNumber}`);
    await delay(800);

    // Fetch the Application ID from the database using sqlcmd
    const dbAppIdOutput = runQuery(`SELECT Id FROM RE_Applications WHERE ApplicationReferenceNumber = '${refNumber}';`);
    const appId = dbAppIdOutput.split('\n').map(l => l.trim()).find(l => l && !l.startsWith('-') && !l.startsWith('Id'));
    console.log(`Resolved Application ID from DB: ${appId}`);

    console.log('Logging out Customer...');
    await logoutUser(page);
    await delay(1000);

    // ========================================================
    // PART 2: ADMIN PAYMENT VALIDATION (UC 06) & RISK (UC 07) & CIRCULATE (UC 08)
    // ========================================================
    console.log('STEP 8: Logging in as Finance Officer (re_finance_officer)...');
    await loginUser(page, 're_finance_officer', 'Arsenal5@');

    // Verify payment
    console.log('STEP 9: Verifying payment (UC 06)...');
    await page.goto('/RealEstateAdmin/VerifyPayment/' + appId);
    await applyZoom(page);
    await delay(1000);
    await page.locator('#txtComment').fill('Lease application fee verified.');
    await delay(1000);
    await page.locator('#btn-approve-submit').click();
    await delay(500);
    await page.locator('#confirmModal button:has-text("Yes, I am sure")').click();
    await delay(1000);

    // Log off Finance Officer and log in as Property Officer
    console.log('Logging off Finance Officer...');
    await logoutUser(page);
    await delay(800);

    console.log('Logging in as Property Officer (re_property_officer)...');
    await loginUser(page, 're_property_officer', 'Arsenal5@');


    // Risk Assessment
    console.log('STEP 10: Conducting Risk Assessment (UC 07)...');
    await page.goto('/RealEstateAdmin/ConductAssessment/' + appId);
    await applyZoom(page);
    await delay(1000);
    await page.locator('#txtCredit').fill('Cleared - Low Risk');
    await page.locator('#txtHomeAffairs').fill('Verified');
    await page.locator('#txtDeeds').fill('Cleared');
    await page.locator('#txtSassa').fill('Not Registered');
    await page.locator('#txtCipc').fill('Active');
    await page.locator('#ddlRecommendation').selectOption({ value: 'Recommended' });
    await page.setInputFiles('input[name="evidenceFile"]', filePaths['evidence.pdf']);
    await page.locator('#txtReason').fill('All background checks cleared.');
    await delay(800);
    await page.locator('#btn-submit-assessment').click();
    await delay(500);
    await page.locator('#confirmModal button:has-text("Yes, I am sure")').click();
    await delay(1000);

    // Initiate Review
    console.log('STEP 11: Initiating Departmental Review (UC 08)...');
    await page.goto('/RealEstateAdmin/InitiateReview/' + appId);
    await applyZoom(page);
    await delay(1000);
    await page.locator('#btn-send-review').click();
    await delay(500);
    await page.locator('#confirmModal button:has-text("Yes, I am sure")').click();
    await delay(1000);

    // ========================================================
    // PART 3: DEPARTMENTAL COMMENT CAPTURE (UC 09) & CONSOLIDATION (UC 10)
    // ========================================================
    console.log('Logging off Property Officer to capture comment as Departmental Representative...');
    await logoutUser(page);
    await delay(800);

    console.log('Logging in as City Planning Department Representative (re_city_planning)...');
    await loginUser(page, 're_city_planning', 'Arsenal5@');

    console.log('STEP 12: Capturing Departmental comments (UC 09) as re_city_planning...');
    await page.goto('/RealEstateAdmin/CaptureDepartmentalComment/' + appId);
    await applyZoom(page);
    await delay(1000);
    
    // departmentName and representativeName are pre-selected and disabled, so we only select the outcome and enter comments
    await page.locator('#ddlOutcome').selectOption({ value: 'Supported' });
    await page.locator('#txtComments').fill('Property conforms to municipal spatial planning guidelines. Supported.');
    await page.setInputFiles('input[name="supportingFile"]', filePaths['evidence.pdf']);
    await delay(800);
    await page.locator('#btn-submit-comment').click();
    await delay(500);
    await page.locator('#confirmModal button:has-text("Yes, I am sure")').click();
    await delay(1000);

    console.log('Logging off City Planning Representative...');
    await logoutUser(page);
    await delay(800);

    console.log('Logging back in as Property Officer (re_property_officer) to consolidate feedback...');
    await loginUser(page, 're_property_officer', 'Arsenal5@');

    console.log('STEP 13: Consolidating departmental feedback (UC 10)...');
    await page.goto('/RealEstateAdmin/ConsolidateApplication/' + appId);
    await applyZoom(page);
    await delay(1000);
    await page.locator('button:has-text("Generate Consolidated Report")').click();
    await delay(800);
    await page.locator('#btn-submit-committee').click();
    await delay(500);
    await page.locator('#confirmModal button:has-text("Yes, I am sure")').click();
    await delay(1000);

    // Log off Property Officer and log in as Committee Member
    console.log('Logging off Property Officer...');
    await logoutUser(page);
    await delay(800);

    console.log('Logging in as Committee Member (re_committee_member)...');
    await loginUser(page, 're_committee_member', 'Arsenal5@');

    // ========================================================
    // PART 4: COMMITTEE REVIEW (UC 11) & HOD FINAL AUTHORISATION (UC 12)
    // ========================================================
    console.log('STEP 14: Reviewing application at Evaluation Committee (UC 11)...');
    await page.goto('/RealEstateAdmin/ReviewCommitteeItem/' + appId);
    await applyZoom(page);
    await delay(1000);
    await page.locator('#ddlDecision').selectOption({ value: 'Recommended' });
    await page.locator('#txtComments').fill('DPRE Evaluation Committee recommends leasing this space based on economic viability.');
    await page.setInputFiles('input[name="resolutionFile"]', filePaths['resolution.pdf']);
    await delay(800);
    await page.locator('#btn-submit-committee').click();
    await delay(500);
    await page.locator('#confirmModal button:has-text("Yes, Confirm")').click();
    await delay(1000);

    // Log off Committee Member and log in as HOD
    console.log('Logging off Committee Member...');
    await logoutUser(page);
    await delay(800);

    console.log('Logging in as HOD (re_hod)...');
    await loginUser(page, 're_hod', 'Arsenal5@');

    console.log('STEP 15: Performing HOD final authorisation with signature (UC 12)...');
    await page.goto('/RealEstateAdmin/AuthoriseApplication/' + appId);
    await applyZoom(page);
    await delay(1000);
    await page.locator('#ddlOutcome').selectOption({ value: 'Approved' });
    await page.locator('#txtComments').fill('Approved final lease contract award.');
    await delay(1000);

    // Draw signature programmatically
    await page.evaluate(() => {
        const canvas = document.getElementById('signatureCanvas');
        if (canvas) {
            const ctx = canvas.getContext('2d');
            ctx.beginPath();
            ctx.moveTo(10, 10);
            ctx.lineTo(150, 50);
            ctx.stroke();
            window.hasDrawn = true;
        }
    });
    await delay(800);

    await page.locator('#btn-submit-auth').click();
    await delay(500);
    await page.locator('#confirmModal button:has-text("Yes, Finalize")').click();
    await delay(1000);

    // Log off HOD and log in as Property Officer
    console.log('Logging off HOD...');
    await logoutUser(page);
    await delay(800);

    console.log('Logging in as Property Officer (re_property_officer)...');
    await loginUser(page, 're_property_officer', 'Arsenal5@');


    // ========================================================
    // PART 5: INSPECTION SCHEDULING (UC 13)
    // ========================================================
    console.log('STEP 16: Scheduling unit pre-occupation inspection (UC 13)...');
    await page.goto('/RealEstateAdmin/ScheduleInspection/' + appId);
    await applyZoom(page);
    await delay(1000);
    await page.locator('#ddlType').selectOption({ value: 'Pre-Occupation' });
    await page.locator('#txtDate').fill(new Date().toISOString().split('T')[0]);
    await page.locator('#txtTime').fill('11:00 - 12:30');
    await delay(800);
    await page.locator('#btn-submit-schedule').click();
    await delay(500);
    await page.locator('#confirmModal button:has-text("Yes, Schedule")').click();
    await delay(1000);

    console.log('Logging off Administrator session to switch to customer confirmation...');
    await logoutUser(page);
    await delay(1000);

    // Customer confirms slot
    console.log('STEP 17: Customer logs in to confirm inspection slot (UC 13)...');
    await loginUser(page, 'RealEstateCustomer', 'Arsenal5@');
    await delay(800);

    await page.goto('/RealEstate/SelectInspectionSlot/' + appId);
    await applyZoom(page);
    await delay(1500);
    await page.locator('#btn-confirm-slot').click();
    await delay(1000);

    console.log('Logging off Customer...');
    await logoutUser(page);
    await delay(1000);

    // ========================================================
    // PART 6: CONDUCT INSPECTION & LOG WORKS ORDER (UC 14)
    // ========================================================
    console.log('STEP 18: Property Officer logs in to conduct unit inspection (UC 14)...');
    await loginUser(page, 're_property_officer', 'Arsenal5@');

    await page.goto('/RealEstateAdmin/ConductInspection/' + appId);
    await applyZoom(page);
    await delay(1000);
    await page.locator('#ddlPlumbing').selectOption({ value: 'Good' });
    await page.locator('#ddlElectrical').selectOption({ value: 'Poor' }); // Defect to auto-check works order
    await page.locator('#ddlFixtures').selectOption({ value: 'Good' });
    await page.locator('#ddlSanitation').selectOption({ value: 'Good' });
    await page.locator('#ddlHazards').selectOption({ value: 'Good' });
    await page.locator('#ddlWearTear').selectOption({ value: 'Fair' });
    await page.locator('textarea[name="comments"]').fill('Electrical wiring checks failed in distribution board.');
    await page.setInputFiles('input[name="inspectionFormFile"]', filePaths['inspection.pdf']);

    // Works Order details (triggered by Poor selection)
    await delay(500);
    await page.locator('#txtWoIssue').fill('Distribution Board breaker needs replacement.');
    await page.locator('textarea[name="woTasks"]').fill('Replace faulty 60A circuit breaker, verify wiring insulation.');
    await page.locator('#ddlWoPriority').selectOption({ value: 'High' });
    await page.locator('#txtWoDueDate').fill(new Date(Date.now() + 86400000 * 3).toISOString().split('T')[0]); // 3 days
    await page.locator('textarea[name="woMaterials"]').fill('60A circuit breaker switch');
    await page.locator('textarea[name="woSafety"]').fill('Isolate main municipal line breaker before working.');
    await delay(1000);

    await page.locator('#btn-submit-inspection').click();
    await delay(500);
    await page.locator('#confirmModal button:has-text("Yes, Finalize")').click();
    await delay(1000);

    // ========================================================
    // PART 7: WORKS ORDER AUTHORISE, ASSIGN & CLOSE (UC 15, 17)
    // ========================================================
    // Log off Property Officer and log in as Facilities Manager
    console.log('Logging off Property Officer...');
    await logoutUser(page);
    await delay(800);

    console.log('Logging in as Facilities Manager (re_facilities_manager)...');
    await loginUser(page, 're_facilities_manager', 'Arsenal5@');


    console.log('STEP 19: Authorising and routing Works Order request (UC 15)...');
    await page.goto('/RealEstateAdmin/AuthoriseWorkOrder/' + appId);
    await applyZoom(page);
    await delay(1000);
    await page.locator('#ddlDecision').selectOption({ value: 'Approve' });
    await page.locator('#ddlAssignmentType').selectOption({ value: 'Internal' });
    await page.locator('#txtReason').fill('Authorized for urgent internal electrician task.');

    // Draw signature programmatically
    await page.evaluate(() => {
        const canvas = document.getElementById('signatureCanvas');
        if (canvas) {
            const ctx = canvas.getContext('2d');
            ctx.beginPath();
            ctx.moveTo(10, 10);
            ctx.lineTo(150, 50);
            ctx.stroke();
            window.hasDrawn = true;
        }
    });
    await delay(800);

    await page.locator('#btn-submit-wo-auth').click();
    await delay(500);
    await page.locator('#confirmModal button:has-text("Yes, Finalise")').click();
    await delay(1000);

    // Create Maintenance Schedule (UC 16)
    console.log('STEP 20: Planning Preventive Maintenance Schedule (UC 16)...');
    await page.goto('/RealEstateAdmin/MaintenancePlanning');
    await applyZoom(page);
    await delay(1000);
    await page.locator('button[id^="btn-schedule-main-"]').first().click();
    await delay(500);
    await page.locator('#txtActivity').fill('Preventive Electrical Distribution Board Check');
    await page.locator('#ddlFrequency').selectOption({ value: 'Bi-Annually' });
    await page.locator('#txtDueDate').fill(new Date(Date.now() + 86400000 * 90).toISOString().split('T')[0]); // 90 days
    await page.locator('#txtDuration').fill('3');
    await delay(800);
    await page.locator('#planningForm button:has-text("Save Schedule")').click();
    await delay(1000);

    // Assign Technician (UC 17)
    console.log('STEP 21: Assigning technician to works order (UC 17)...');
    await page.goto('/RealEstateAdmin/AssignTechnician/' + appId);
    await applyZoom(page);
    await delay(1000);
    await page.locator('#ddlTechnician').selectOption({ label: 'T. Ndlovu (Electrician)' });
    await delay(500);
    await page.locator('#btn-submit-assign').click();
    await delay(500);
    await page.locator('#confirmModal button:has-text("Yes, Assign")').click();
    await delay(1000);

    // Log off Facilities Manager and log in as Technician
    console.log('Logging off Facilities Manager...');
    await logoutUser(page);
    await delay(800);

    console.log('Logging in as Technician (re_technician)...');
    await loginUser(page, 're_technician', 'Arsenal5@');

    // Accept Job
    console.log('STEP 22: Technician accepts job allocation...');
    await page.goto('/RealEstateAdmin/TechnicianAllocation');
    await applyZoom(page);
    await delay(1000);
    await page.locator('button[id^="btn-accept-wo-"]').first().click();
    await delay(1000);

    // Update Job & upload Job Sheet
    console.log('STEP 23: Technician updates job and uploads Job Sheet...');
    await page.goto('/RealEstateAdmin/TechnicianUpdate/' + appId);
    await applyZoom(page);
    await delay(1000);
    await page.locator('#txtComments').fill('Wiring breaker replaced, tested voltage levels, all compliant.');
    await page.setInputFiles('input[name="jobSheetFile"]', filePaths['jobsheet.pdf']);
    await delay(800);
    await page.locator('#btn-complete-job').click();
    await delay(1000);

    // Log off Technician and log in as Facilities Manager to close
    console.log('Logging off Technician...');
    await logoutUser(page);
    await delay(800);

    console.log('Logging in as Facilities Manager (re_facilities_manager)...');
    await loginUser(page, 're_facilities_manager', 'Arsenal5@');

    // Close Job
    console.log('STEP 24: Manager verifies repairs and closes Work Order...');
    await page.goto('/RealEstateAdmin/CloseWorkOrder/' + appId);
    await applyZoom(page);
    await delay(1000);
    await page.locator('#ddlDecision').selectOption({ value: 'Approve' });
    await page.locator('#txtComments').fill('Verified on-site. Work order closed.');

    // Draw signature programmatically
    await page.evaluate(() => {
        const canvas = document.getElementById('signatureCanvas');
        if (canvas) {
            const ctx = canvas.getContext('2d');
            ctx.beginPath();
            ctx.moveTo(10, 10);
            ctx.lineTo(150, 50);
            ctx.stroke();
            window.hasDrawn = true;
        }
    });
    await delay(800);

    await page.locator('#btn-submit-close').click();
    await delay(500);
    await page.locator('#confirmModal button:has-text("Yes, Submit")').click();
    await delay(1000);

    console.log('Logging off Facilities Manager session...');
    await logoutUser(page);
    await delay(1000);

    // ========================================================
    // PART 8: CUSTOMER REQUESTS PERMISSION TO OCCUPY (UC 18)
    // ========================================================
    console.log('STEP 25: Customer logs in to request early Permission to Occupy (PTO) (UC 18)...');
    await loginUser(page, 'RealEstateCustomer', 'Arsenal5@');

    await page.goto('/RealEstate/RequestPto/' + appId);
    await applyZoom(page);
    await delay(1000);

    await page.locator('#txtPurpose').fill('Early access for network cabling setup and office furniture installation.');
    await page.locator('#txtStartDate').fill(new Date().toISOString().split('T')[0]);
    await page.locator('#txtEndDate').fill(new Date(Date.now() + 86400000 * 30).toISOString().split('T')[0]); // 30 days
    await page.setInputFiles('input[name="fileInsurance"]', filePaths['insurance.pdf']);
    await page.setInputFiles('input[name="fileFitout"]', filePaths['fitout.pdf']);
    await page.setInputFiles('input[name="fileHealthSafety"]', filePaths['safety.pdf']);
    await page.locator('#chkIndemnity').click();
    await delay(1000);

    await page.locator('#btn-submit-pto').click();
    await delay(1000);

    console.log('Logging off Customer session...');
    await logoutUser(page);
    await delay(1000);

    // ========================================================
    // PART 9: ADMIN REVIEW (UC 19) & HOD AUTHORISE PTO (UC 20)
    // ========================================================
    console.log('STEP 26: Property Officer logs in to review PTO request (UC 19)...');
    await loginUser(page, 're_property_officer', 'Arsenal5@');

    await page.goto('/RealEstateAdmin/ReviewPto/' + appId);
    await applyZoom(page);
    await delay(1000);
    await page.locator('#ddlRec').selectOption({ value: 'Recommended' });
    await page.locator('#txtReason').fill('All safety and fitout documents validated. Recommended for approval.');
    await delay(800);
    await page.locator('#btn-submit-rec').click();
    await delay(500);
    await page.locator('#confirmModal button:has-text("Yes, Submit")').click();
    await delay(1000);

    // Log off Property Officer and log in as HOD
    console.log('Logging off Property Officer...');
    await logoutUser(page);
    await delay(800);

    console.log('STEP 27: HOD final approval for PTO (UC 20)...');
    console.log('Logging in as HOD (re_hod)...');
    await loginUser(page, 're_hod', 'Arsenal5@');

    await page.goto('/RealEstateAdmin/AuthorisePto/' + appId);
    await applyZoom(page);
    await delay(1000);
    await page.locator('#ddlDecision').selectOption({ value: 'Approve' });
    await page.locator('textarea[name="comments"]').fill('Approved early access for fit-out tasks.');

    // Draw signature programmatically
    await page.evaluate(() => {
        const canvas = document.getElementById('signatureCanvas');
        if (canvas) {
            const ctx = canvas.getContext('2d');
            ctx.beginPath();
            ctx.moveTo(10, 10);
            ctx.lineTo(150, 50);
            ctx.stroke();
            window.hasDrawn = true;
        }
    });
    await delay(1500); // Visual pause showing signature before final click

    try {
      await page.locator('#btn-submit-pto-auth').click({ timeout: 5000, noWaitAfter: true });
    } catch (e) {
      console.log('PTO auth submit click timed out, forcing submit via modal...');
    }
    await delay(500);
    await page.locator('#confirmModal button:has-text("Yes, Finalise")').click();
    await delay(2000); // Final presentation pause showing landing dashboard

    console.log('Real Estate lease workflow E2E test completed successfully!');

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
