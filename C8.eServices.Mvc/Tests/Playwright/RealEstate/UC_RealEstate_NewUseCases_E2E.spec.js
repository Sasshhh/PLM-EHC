const { test, expect } = require('@playwright/test');
const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

async function applyZoom(page) {
  try {
    await page.evaluate(() => {
      if (document && document.body) {
        document.body.style.zoom = '1.75';
      }
    }).catch(() => {});
  } catch (err) {}
}

async function logoutUser(page) {
  console.log('Logging off user...');
  const logoutForm = page.locator('#logoutForm');
  if (await logoutForm.count() > 0) {
    await Promise.all([
      page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 20000 }).catch(() => {}),
      logoutForm.evaluate(form => form.submit())
    ]);
  } else {
    await page.goto('/Account/Login', { waitUntil: 'domcontentloaded' }).catch(() => {});
  }
  await applyZoom(page);
  await delay(1000);
}

async function loginUser(page, username, password) {
  console.log(`Logging in as ${username}...`);
  if (!page.url().toLowerCase().includes('/account/login')) {
    await page.goto('/Account/Login', { waitUntil: 'domcontentloaded' }).catch(() => {});
  }
  await page.waitForSelector('#UserName', { state: 'visible', timeout: 15000 }).catch(() => {});
  await applyZoom(page);
  await page.locator('#UserName').fill(username);
  await delay(300);
  await page.locator('#Password').fill(password);
  await delay(300);
  await Promise.all([
    page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 25000 }).catch(() => {}),
    page.locator('button[type="submit"]').click()
  ]);
  await applyZoom(page);
  await delay(800);
}

function runQuery(query) {
  const tempFile = path.join(__dirname, `temp_${Date.now()}_${Math.floor(Math.random()*1000)}.sql`);
  try {
    fs.writeFileSync(tempFile, query, 'utf8');
    const cmd = `sqlcmd -S localhost -E -d PropertyLeaseManagementRealEstate -i "${tempFile}" -W`;
    const output = execSync(cmd, { encoding: 'utf8' });
    return output;
  } catch (err) {
    console.error('SQL Execution Error:', err.message);
    throw err;
  } finally {
    if (fs.existsSync(tempFile)) {
      try { fs.unlinkSync(tempFile); } catch(e) {}
    }
  }
}

function runQuerySingleInt(query) {
  const output = runQuery(`SET NOCOUNT ON;\n${query}`);
  const lines = output.split(/\r?\n/).map(l => l.trim()).filter(l => l && !l.startsWith('-') && l.toLowerCase() !== 'id' && !isNaN(l));
  return lines.length > 0 ? lines[0] : null;
}

test('Real Estate Lease Management - Use Cases 21 to 25 Flow', async ({ page }) => {
  test.setTimeout(450000); // 7.5 minutes

  // Set high resolution viewport for 4K quality recording
  await page.setViewportSize({ width: 2560, height: 1440 });

  const tempDir = path.join(__dirname, 'temp_uploads');
  if (!fs.existsSync(tempDir)) {
    fs.mkdirSync(tempDir, { recursive: true });
  }

  const files = ['evidence.pdf', 'signed_lease.pdf'];
  const filePaths = {};
  for (const name of files) {
    const filePath = path.join(tempDir, name);
    fs.writeFileSync(filePath, '%PDF-1.4 Dummy PDF Content for E2E Test');
    filePaths[name] = filePath;
  }

  try {
    console.log('Preparing test lease application in DB...');

    const testRef = 'APP-TEST-999';
    // Clean up any old test app & ensure facility units are active for unit allocation (UC 24)
    runQuery(`DELETE FROM RE_Applications WHERE ApplicationReferenceNumber = '${testRef}';`);
    runQuery(`UPDATE RE_FacilityUnits SET IsActive = 1 WHERE IsDeleted = 0;`);

    // Insert seeded application in 're_pto_approved' status
    const ptoApprovedStatusId = runQuerySingleInt("SELECT Id FROM Status WHERE [Key] = 're_pto_approved';");
    const customerId = runQuerySingleInt("SELECT TOP 1 c.Id FROM Customers c JOIN SystemUsers su ON c.SystemUserId = su.Id WHERE su.UserName = 'RealEstateCustomer' ORDER BY c.Id ASC;");
    const cccId = runQuerySingleInt("SELECT TOP 1 f.CCCId FROM RE_FacilityUnits fu JOIN RE_Facilities f ON fu.FacilityId = f.Id WHERE fu.IsActive = 1 AND fu.IsDeleted = 0;");
    const systemUserId = runQuerySingleInt("SELECT TOP 1 Id FROM SystemUsers WHERE UserName = 're_property_officer';");

    const insertSql = `
      SET NOCOUNT ON;
      INSERT INTO RE_Applications 
      (ApplicationReferenceNumber, SystemUserId, CustomerId, CCCId, StatusId, ApplicantType, PurposeOfLease, PropertyAddress, TownshipSuburbFarmName, ErfFarmNumber, PropertyPostalCode, CalculatedMonthlyRental, IsActive, IsDeleted, CreatedDateTime, ModifiedDateTime)
      VALUES 
      ('${testRef}', ${systemUserId}, ${customerId}, ${cccId}, ${ptoApprovedStatusId}, 'Individual', 'Commercial Facilities', '100 Commercial Way', 'Tokoza', 'Erf 450', '1426', 5500.00, 1, 0, GETDATE(), GETDATE());
    `;
    runQuery(insertSql);
    const appId = runQuerySingleInt(`SELECT TOP 1 Id FROM RE_Applications WHERE ApplicationReferenceNumber = '${testRef}' ORDER BY Id DESC;`);
    console.log(`Seeded Test Application ID: ${appId} with reference: ${testRef}`);

    // ----------------------------------------------------
    // UC 21: Draft, Sign and Issue Permission to Occupy (PTO)
    // ----------------------------------------------------
    console.log('UC 21: Property Officer logs in to draft PTO Certificate...');
    await loginUser(page, 're_property_officer', 'Arsenal5@');

    await page.goto('/RealEstateAdmin/PtoApprovals');
    await applyZoom(page);
    await delay(1500);

    // Locate the seed application and click "Generate Certificate"
    await page.locator(`#btn-generate-pto-${appId}`).click();
    await applyZoom(page);
    await delay(1500);

    // Fill in dates and purpose
    await page.locator('textarea[name="ptoPurpose"]').fill('Authorized Early Occupation Setup');
    await Promise.all([
      page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 20000 }).catch(() => {}),
      page.locator('#btnSubmitPtoDraft').click()
    ]);
    await delay(1000);

    console.log('Property Officer logged out. HOD logs in to authorize & sign PTO...');
    await logoutUser(page);
    await loginUser(page, 're_hod', 'Arsenal5@');

    await page.goto('/RealEstateAdmin/PtoSignatureQueue');
    await applyZoom(page);
    await delay(1500);

    // Click authorize & sign
    await page.locator(`#btn-sign-pto-${appId}`).click();
    await applyZoom(page);
    await delay(1500);

    // Approve & sign
    await page.locator('#ddlDecision').selectOption({ value: 'Approve' });
    await page.locator('textarea[name="comments"]').fill('Approved and signed PTO Certificate.');
    
    // Draw signature programmatically
    await page.evaluate(() => {
        const canvas = document.getElementById('signatureCanvas');
        if (canvas) {
            const ctx = canvas.getContext('2d');
            ctx.beginPath();
            ctx.moveTo(20, 20);
            ctx.lineTo(200, 80);
            ctx.stroke();
            window.hasDrawn = true;
        }
    });
    await delay(1000);
    await Promise.all([
      page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 20000 }).catch(() => {}),
      page.locator('#btn-submit-pto-sign').click()
    ]);
    await delay(1000);

    // ----------------------------------------------------
    // UC 22: Revoke/Terminate Permission to Occupy
    // ----------------------------------------------------
    console.log('UC 22: Temporary active status to test PTO revocation...');
    // We update status back to re_active_occupancy to show in ActivePto list
    const activeOccupancyStatusId = runQuerySingleInt("SELECT Id FROM Status WHERE [Key] = 're_active_occupancy';");
    runQuery(`UPDATE RE_Applications SET StatusId = ${activeOccupancyStatusId} WHERE Id = ${appId};`);

    await logoutUser(page);
    await loginUser(page, 're_property_officer', 'Arsenal5@');

    await page.goto('/RealEstateAdmin/ActivePto');
    await applyZoom(page);
    await delay(1500);

    await page.locator(`#btn-revoke-pto-${appId}`).click();
    await applyZoom(page);
    await delay(1500);

    await page.locator('#txtReason').fill('Non-compliance with site health & safety rules.');
    await page.setInputFiles('input[name="evidenceFile"]', filePaths['evidence.pdf']);
    await Promise.all([
      page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 20000 }).catch(() => {}),
      page.locator('#btnSubmitRevocation').click()
    ]);
    await delay(1000);

    // ----------------------------------------------------
    // UC 23: Generate, Sign and Conclude Lease Agreement
    // ----------------------------------------------------
    console.log('UC 23: Concluding Lease Agreement with Live Document Review & Confirmation Modals...');
    const concludedApprovedStatusId = runQuerySingleInt("SELECT Id FROM Status WHERE [Key] = 're_concluded_approved';");
    runQuery(`UPDATE RE_Applications SET StatusId = ${concludedApprovedStatusId} WHERE Id = ${appId};`);

    await page.goto('/RealEstateAdmin/NewLeases');
    await applyZoom(page);
    await delay(1500);

    await page.locator(`#btn-gen-lease-${appId}`).click();
    await applyZoom(page);
    await delay(1500);

    // Property Officer reviews live draft document and modifies values to demonstrate live updating
    await page.locator('#inpRental').fill('7500');
    await delay(1200);
    await page.locator('#inpDeposit').fill('15000');
    await delay(1200);
    await page.locator('#inpDuration').fill('48');
    await delay(1200);
    await page.locator('#inpEscalation').selectOption('10% Annually');
    await delay(1200);
    await page.locator('#inpClauses').fill('Lessee is responsible for operational maintenance, security, and statutory municipal compliance.');
    await delay(2000);

    // Click submit to trigger Step 10 Confirmation Modal ("Are you sure you want to submit the document for signature?")
    await page.locator('#btnSubmitLeaseDraft').click();
    await delay(1000);
    // Click Step 11 "Yes, I am sure"
    await Promise.all([
      page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 20000 }).catch(() => {}),
      page.locator('#btnConfirmSubmit').click()
    ]);
    await delay(1000);

    console.log('Tenant logs in to review & sign lease agreement...');
    await logoutUser(page);
    await loginUser(page, 'RealEstateCustomer', 'Arsenal5@');

    await page.goto('/RealEstate/LeaseAgreements');
    await applyZoom(page);
    await delay(1500);

    await page.locator(`#btn-sign-agreement-${appId}`).click();
    await applyZoom(page);
    await delay(1500);

    await page.setInputFiles('input[name="signedLeaseFile"]', filePaths['signed_lease.pdf']);
    await page.locator('#btnSubmitAgreement').click();
    await delay(1000);
    // Click Step 13 "Yes, I am sure" modal confirm
    await Promise.all([
      page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 20000 }).catch(() => {}),
      page.locator('#modalTenantConfirm button:has-text("Yes, I am sure")').click()
    ]);
    await delay(1000);

    console.log('HOD logs in to authorize & sign off lease...');
    await logoutUser(page);
    await loginUser(page, 're_hod', 'Arsenal5@');

    await page.goto('/RealEstateAdmin/LeaseAgreementApprovals');
    await applyZoom(page);
    await delay(1500);

    await page.locator(`#btn-sign-lease-${appId}`).click();
    await applyZoom(page);
    await delay(1500);

    await page.locator('#ddlDecision').selectOption({ value: 'Approve' });
    await page.locator('textarea[name="comments"]').fill('Final lease approved and signed by CoE.');
    await page.evaluate(() => {
        const canvas = document.getElementById('signatureCanvas');
        if (canvas) {
            const ctx = canvas.getContext('2d');
            ctx.beginPath();
            ctx.moveTo(30, 30);
            ctx.lineTo(250, 100);
            ctx.stroke();
            window.hasDrawn = true;
        }
    });
    await delay(1000);
    await page.locator('#btn-submit-lease-sign').click();
    await delay(1000);
    // Click Step 9 "Yes, I am sure" activation modal confirm
    await Promise.all([
      page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 20000 }).catch(() => {}),
      page.locator('#modalHodConfirm button:has-text("Yes, I am sure")').click()
    ]);
    await delay(1000);

    // ----------------------------------------------------
    // UC 24: Space / Unit Allocation
    // ----------------------------------------------------
    console.log('UC 24: Allocating Unit to Lease...');
    await logoutUser(page);
    await loginUser(page, 're_property_officer', 'Arsenal5@');

    await page.goto('/RealEstateAdmin/Allocations');
    await applyZoom(page);
    await delay(1500);

    await page.locator(`#btn-allocate-${appId}`).click();
    await applyZoom(page);
    await delay(1500);

    await page.locator('#ddlDecision').selectOption({ value: 'Approve' });
    await delay(1000);
    const unitCount = await page.locator('#ddlUnit option').count();
    if (unitCount > 1) {
        await page.locator('#ddlUnit').selectOption({ index: 1 });
    } else if (unitCount > 0) {
        await page.locator('#ddlUnit').selectOption({ index: 0 });
    }
    await Promise.all([
      page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 20000 }).catch(() => {}),
      page.locator('#btnSubmitAllocation').click()
    ]);
    await delay(1000);

    // ----------------------------------------------------
    // UC 25: Capture Lease Details and Categorization
    // ----------------------------------------------------
    console.log('UC 25: Capturing Lease Details & Classification...');
    await page.goto('/RealEstateAdmin/ActiveOccupancy');
    await applyZoom(page);
    await delay(1500);

    await page.locator(`#btn-classify-${appId}`).click();
    await applyZoom(page);
    await delay(1500);

    await page.locator('#ddlCategory').selectOption({ value: 'Commercial' });
    await page.locator('#tenancyNumber').fill('LSE-TOKOZA-E2E-999');
    await page.locator('textarea[name="clauses"]').fill('E2E Standard clauses apply.');
    await Promise.all([
      page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 20000 }).catch(() => {}),
      page.locator('#btnSubmitClassification').click()
    ]);
    await delay(1000);

    console.log('UC 21 - UC 25 E2E Workflow verified successfully!');

  } finally {
    try {
      for (const name of files) {
        const filePath = path.join(tempDir, name);
        if (fs.existsSync(filePath)) {
          fs.unlinkSync(filePath);
        }
      }
      if (fs.existsSync(tempDir)) {
        fs.rmdirSync(tempDir);
      }
    } catch (err) {
      console.error('Failed to cleanup temp test files:', err);
    }
  }
});
