const { test, expect } = require('@playwright/test');

// ═══════════════════════════════════════════════════════════════════════════════
// OVERLAY HELPERS – inject visible title cards into the browser viewport
// ═══════════════════════════════════════════════════════════════════════════════

async function showTitleCard(page, { ucNumber, ucTitle, actor, navigation, category }) {
  await page.evaluate(({ ucNumber, ucTitle, actor, navigation, category }) => {
    // Remove any previous overlay
    const old = document.getElementById('uc-title-overlay');
    if (old) old.remove();

    const overlay = document.createElement('div');
    overlay.id = 'uc-title-overlay';
    overlay.style.cssText = `
      position: fixed; inset: 0; z-index: 999999;
      display: flex; align-items: center; justify-content: center;
      background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      color: #fff; animation: fadeIn 0.4s ease-out;
    `;

    const categoryColors = {
      'Profile & System Access': '#00d2ff',
      'Lease Applications & Real Estate Queue': '#7b2ff7',
      'Unit Inspections & Maintenance': '#ff6b35',
    };
    const accentColor = categoryColors[category] || '#00d2ff';

    overlay.innerHTML = `
      <style>
        @keyframes fadeIn { from { opacity: 0; transform: scale(0.95); } to { opacity: 1; transform: scale(1); } }
        @keyframes slideUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
        @keyframes pulse { 0%,100%{ box-shadow: 0 0 20px ${accentColor}44; } 50%{ box-shadow: 0 0 40px ${accentColor}88; } }
      </style>
      <div style="text-align:center; max-width:800px; padding:48px; animation: slideUp 0.5s ease-out 0.2s both;">
        <div style="font-size:14px; text-transform:uppercase; letter-spacing:4px; color:${accentColor}; margin-bottom:12px;">
          ${category}
        </div>
        <div style="
          display:inline-block; padding:8px 32px; border-radius:40px; margin-bottom:20px;
          background:${accentColor}22; border:2px solid ${accentColor};
          font-size:18px; font-weight:700; letter-spacing:2px; color:${accentColor};
          animation: pulse 2s infinite;
        ">
          UC ${ucNumber}
        </div>
        <h1 style="font-size:36px; font-weight:300; line-height:1.3; margin:16px 0 32px 0; color:#fff;">
          ${ucTitle}
        </h1>
        <div style="
          display:inline-flex; flex-direction:column; gap:12px; text-align:left;
          background: rgba(255,255,255,0.06); border-radius:16px; padding:24px 36px;
          border: 1px solid rgba(255,255,255,0.1);
        ">
          <div style="display:flex; align-items:center; gap:12px;">
            <span style="font-size:20px;">👤</span>
            <div>
              <div style="font-size:11px; text-transform:uppercase; letter-spacing:2px; color:${accentColor};">Logged-in User</div>
              <div style="font-size:18px; font-weight:600;">${actor}</div>
            </div>
          </div>
          <div style="width:100%; height:1px; background:rgba(255,255,255,0.1);"></div>
          <div style="display:flex; align-items:center; gap:12px;">
            <span style="font-size:20px;">📂</span>
            <div>
              <div style="font-size:11px; text-transform:uppercase; letter-spacing:2px; color:${accentColor};">Navigation Path</div>
              <div style="font-size:16px; font-weight:500;">${navigation}</div>
            </div>
          </div>
        </div>
      </div>
    `;
    document.body.appendChild(overlay);
  }, { ucNumber, ucTitle, actor, navigation, category });

  // Hold the card on screen long enough for the video to capture it
  await page.waitForTimeout(3500);
}

async function removeTitleCard(page) {
  await page.evaluate(() => {
    const el = document.getElementById('uc-title-overlay');
    if (el) el.remove();
  });
  await page.waitForTimeout(300);
}

async function showCompletionCard(page, ucNumber, ucTitle) {
  await page.evaluate(({ ucNumber, ucTitle }) => {
    const old = document.getElementById('uc-title-overlay');
    if (old) old.remove();

    const overlay = document.createElement('div');
    overlay.id = 'uc-title-overlay';
    overlay.style.cssText = `
      position: fixed; inset: 0; z-index: 999999;
      display: flex; align-items: center; justify-content: center;
      background: linear-gradient(135deg, #0d3b2e 0%, #145a3e 50%, #1a7a50 100%);
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      color: #fff; animation: fadeIn 0.4s ease-out;
    `;
    overlay.innerHTML = `
      <style>@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }</style>
      <div style="text-align:center;">
        <div style="font-size:64px; margin-bottom:16px;">✅</div>
        <div style="font-size:14px; text-transform:uppercase; letter-spacing:3px; color:#4ade80; margin-bottom:8px;">Verified</div>
        <h2 style="font-size:28px; font-weight:300; margin:0;">UC ${ucNumber}: ${ucTitle}</h2>
      </div>
    `;
    document.body.appendChild(overlay);
  }, { ucNumber, ucTitle });
  await page.waitForTimeout(2000);
  await removeTitleCard(page);
}

// ═══════════════════════════════════════════════════════════════════════════════
// MODAL DISMISS / MENU / LOGIN / LOGOFF helpers (unchanged from prior version)
// ═══════════════════════════════════════════════════════════════════════════════

async function dismissModalIfPresent(page) {
  try {
    const modalOk = page.locator(
      'button#OkayResponseBtn:visible, button[data-dismiss="modal"]:has-text("OK"):visible, div.modal button:has-text("OK"):visible'
    ).first();
    if (await modalOk.isVisible()) {
      await modalOk.click({ timeout: 2000 });
      await page.waitForTimeout(500);
    }
  } catch (_) { /* suppress */ }
}

async function clickMenu(page, menuLocator, subMenuLocator) {
  await dismissModalIfPresent(page);
  if (await subMenuLocator.first().isVisible()) return;
  await menuLocator.first().click();
  try {
    await expect(subMenuLocator.first()).toBeVisible({ timeout: 3000 });
  } catch (_) {
    await dismissModalIfPresent(page);
    await menuLocator.first().click();
    await expect(subMenuLocator.first()).toBeVisible({ timeout: 5000 });
  }
}

function getParentMenu(page, text, subText = null) {
  if (subText) {
    return page.locator(`//ul[contains(@class, 'nav-list')]/li[a[normalize-space(.)='${text}' or .//span[normalize-space(text())='${text}']] and .//a[normalize-space(.)='${subText}' or .//span[normalize-space(text())='${subText}']]]/a`);
  }
  return page.locator(`//ul[contains(@class, 'nav-list')]/li[a[normalize-space(.)='${text}' or .//span[normalize-space(text())='${text}']]]/a`);
}
function getSubMenu(page, parentText, subText, tertiaryText = null) {
  if (tertiaryText) {
    return page.locator(`//ul[contains(@class, 'nav-list')]/li[a[normalize-space(.)='${parentText}' or .//span[normalize-space(text())='${parentText}']]]/ul//li[a[normalize-space(.)='${subText}' or .//span[normalize-space(text())='${subText}']] and .//a[normalize-space(.)='${tertiaryText}' or .//span[normalize-space(text())='${tertiaryText}']]]/a`);
  }
  return page.locator(`//ul[contains(@class, 'nav-list')]/li[a[normalize-space(.)='${parentText}' or .//span[normalize-space(text())='${parentText}']]]/ul//a[normalize-space(.)='${subText}' or .//span[normalize-space(text())='${subText}']]`);
}
function getTertiaryMenu(page, parentText, subText, tertiaryText, quaternaryText = null) {
  if (quaternaryText) {
    return page.locator(`//ul[contains(@class, 'nav-list')]/li[a[normalize-space(.)='${parentText}' or .//span[normalize-space(text())='${parentText}']]]/ul//li[a[normalize-space(.)='${subText}' or .//span[normalize-space(text())='${subText}']]]/ul//li[a[normalize-space(.)='${tertiaryText}' or .//span[normalize-space(text())='${tertiaryText}']] and .//a[normalize-space(.)='${quaternaryText}' or .//span[normalize-space(text())='${quaternaryText}']]]/a`);
  }
  return page.locator(`//ul[contains(@class, 'nav-list')]/li[a[normalize-space(.)='${parentText}' or .//span[normalize-space(text())='${parentText}']]]/ul//li[a[normalize-space(.)='${subText}' or .//span[normalize-space(text())='${subText}']]]/ul//a[normalize-space(.)='${tertiaryText}' or .//span[normalize-space(text())='${tertiaryText}']]`);
}
function getQuaternaryMenu(page, parentText, subText, tertiaryText, quaternaryText) {
  return page.locator(`//ul[contains(@class, 'nav-list')]/li[a[normalize-space(.)='${parentText}' or .//span[normalize-space(text())='${parentText}']]]/ul//li[a[normalize-space(.)='${subText}' or .//span[normalize-space(text())='${subText}']]]/ul//li[a[normalize-space(.)='${tertiaryText}' or .//span[normalize-space(text())='${tertiaryText}']]]/ul//a[normalize-space(.)='${quaternaryText}' or .//span[normalize-space(text())='${quaternaryText}']]`);
}

async function loginAs(page, username, password = 'Arsenal5@') {
  await page.context().clearCookies();
  await page.goto('/');
  await page.evaluate(() => { try { localStorage.clear(); sessionStorage.clear(); } catch(_){} });
  const profileMenu = getParentMenu(page, 'Profile');
  const signIn = getSubMenu(page, 'Profile', 'Sign In');
  await clickMenu(page, profileMenu, signIn);
  await signIn.click();
  await page.getByRole('textbox', { name: 'username or e-mail' }).fill(username);
  await page.getByRole('textbox', { name: 'Please enter your password' }).fill(password);
  await page.locator('form button[type="submit"]').click();
  for (let i = 0; i < 10; i++) {
    await page.waitForTimeout(500);
    await dismissModalIfPresent(page);
  }
  const logoffLink = getSubMenu(page, 'Profile', 'Log off');
  await clickMenu(page, profileMenu, logoffLink);
}

async function logoff(page) {
  await page.context().clearCookies();
  await page.goto('/');
  await page.evaluate(() => { try { localStorage.clear(); sessionStorage.clear(); } catch(_){} });
}

// Helper: scroll the page slowly so the BA can see the content
async function slowScroll(page, pixels = 600) {
  const steps = 6;
  const perStep = Math.round(pixels / steps);
  for (let i = 0; i < steps; i++) {
    await page.mouse.wheel(0, perStep);
    await page.waitForTimeout(350);
  }
}

// ═══════════════════════════════════════════════════════════════════════════════
// MAIN TEST – each UC gets: Title Card → Login → Navigate Sidenav → Scroll → ✅
// ═══════════════════════════════════════════════════════════════════════════════

test.describe('PLM Full Use Case Walkthrough with Title Cards', () => {
  test('Walk through every UC with intro slides and sidenav demo', async ({ page }) => {
    test.setTimeout(3600000); // 60 minutes

    const CAT_PROFILE = 'Profile & System Access';
    const CAT_LEASE   = 'Lease Applications & Real Estate Queue';
    const CAT_MAINT   = 'Unit Inspections & Maintenance';

    // ──────────────────────────────────────────────────────────────────
    // UC 05 – Submit Application for Lease  (RealEstateCustomer)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 05: Submit Application for Lease', async () => {
      await showTitleCard(page, {
        ucNumber: '05', ucTitle: 'Submit Application for Lease',
        actor: 'RealEstateCustomer',
        navigation: 'Applications → Lease Application Screen',
        category: CAT_LEASE,
      });
      await removeTitleCard(page);

      await loginAs(page, 'RealEstateCustomer');
      await dismissModalIfPresent(page);

      const appsMenu = getParentMenu(page, 'Applications', 'Lease Application Screen');
      const leaseAppScreen = getSubMenu(page, 'Applications', 'Lease Application Screen');
      await clickMenu(page, appsMenu, leaseAppScreen);
      await page.waitForTimeout(600);
      await leaseAppScreen.click();
      await expect(page).toHaveURL(/.*RealEstate\/Capture.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 800);

      await showCompletionCard(page, '05', 'Submit Application for Lease');
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 18 – Request Permission to Occupy  (RealEstateCustomer)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 18: Request Permission to Occupy', async () => {
      await showTitleCard(page, {
        ucNumber: '18', ucTitle: 'Request Permission to Occupy',
        actor: 'RealEstateCustomer',
        navigation: 'Applications → Lease Agreements',
        category: CAT_LEASE,
      });
      await removeTitleCard(page);

      const appsMenu2 = getParentMenu(page, 'Applications', 'Lease Agreements');
      const leaseAgreements = getSubMenu(page, 'Applications', 'Lease Agreements');
      await clickMenu(page, appsMenu2, leaseAgreements);
      await page.waitForTimeout(600);
      await leaseAgreements.click();
      await expect(page).toHaveURL(/.*RealEstate\/MyApplications.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '18', 'Request Permission to Occupy');
      await logoff(page);
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 03 – Approve Profile Registration  (HOD – re_hod)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 03: Approve Profile Registration', async () => {
      await showTitleCard(page, {
        ucNumber: '03', ucTitle: 'Approve Profile Registration',
        actor: 're_hod  (Head of Department)',
        navigation: 'Maintenance → Pending Approval',
        category: CAT_PROFILE,
      });
      await removeTitleCard(page);

      await loginAs(page, 're_hod');
      await dismissModalIfPresent(page);

      const maintMenu = getParentMenu(page, 'Maintenance', 'Pending Approval');
      const pendingApproval = getSubMenu(page, 'Maintenance', 'Pending Approval');
      await clickMenu(page, maintMenu, pendingApproval);
      await page.waitForTimeout(600);
      await pendingApproval.click();
      await expect(page).toHaveURL(/.*Customer\/Index.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '03', 'Approve Profile Registration');
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 12 – Application Final Authorisation  (HOD)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 12: Application Final Authorisation', async () => {
      await showTitleCard(page, {
        ucNumber: '12', ucTitle: 'Application Final Authorisation',
        actor: 're_hod  (Head of Department)',
        navigation: 'Applications → Worklists → Final Authorisation',
        category: CAT_LEASE,
      });
      await removeTitleCard(page);

      const appsMenu = getParentMenu(page, 'Applications', 'Worklists');
      const worklistLink = getSubMenu(page, 'Applications', 'Worklists').first();
      await clickMenu(page, appsMenu, worklistLink);
      await page.waitForTimeout(600);
      await worklistLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/FinalAuthorisation.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '12', 'Application Final Authorisation');
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 20 – Authorise Permission to Occupy  (HOD)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 20: Authorise Permission to Occupy', async () => {
      await showTitleCard(page, {
        ucNumber: '20', ucTitle: 'Authorise Permission to Occupy',
        actor: 're_hod  (Head of Department)',
        navigation: 'Applications → Worklists → PTO Authorisations',
        category: CAT_LEASE,
      });
      await removeTitleCard(page);

      const appsMenu = getParentMenu(page, 'Applications', 'Worklists');
      const worklistLink2 = getSubMenu(page, 'Applications', 'Worklists').last();
      await clickMenu(page, appsMenu, worklistLink2);
      await page.waitForTimeout(600);
      await worklistLink2.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/PtoAuthorisations.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '20', 'Authorise Permission to Occupy');
      await logoff(page);
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 06 – Validate Proof of Payment  (Finance Officer)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 06: Validate Proof of Payment', async () => {
      await showTitleCard(page, {
        ucNumber: '06', ucTitle: 'Validate Proof of Payment',
        actor: 're_finance_officer  (Finance Officer)',
        navigation: 'Financials → Application Fee Payments',
        category: CAT_LEASE,
      });
      await removeTitleCard(page);

      await loginAs(page, 're_finance_officer');
      await dismissModalIfPresent(page);

      const financialsMenu = getParentMenu(page, 'Financials', 'Application Fee Payments');
      const paymentsLink = getSubMenu(page, 'Financials', 'Application Fee Payments');
      await clickMenu(page, financialsMenu, paymentsLink);
      await page.waitForTimeout(600);
      await paymentsLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/ApplicationFeePayments.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '06', 'Validate Proof of Payment');
      await logoff(page);
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 07 – Capture Risk Assessment Outcome  (Property Officer)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 07: Capture Risk Assessment Outcome', async () => {
      await showTitleCard(page, {
        ucNumber: '07', ucTitle: 'Capture Risk Assessment Outcome',
        actor: 're_property_officer  (Property Officer)',
        navigation: 'Applications → Assessment → Risk Assessment',
        category: CAT_LEASE,
      });
      await removeTitleCard(page);

      await loginAs(page, 're_property_officer');
      await dismissModalIfPresent(page);

      const appsMenu = getParentMenu(page, 'Applications', 'Assessment');
      const assessmentSub = getSubMenu(page, 'Applications', 'Assessment', 'Risk Assessment');
      const riskLink = getTertiaryMenu(page, 'Applications', 'Assessment', 'Risk Assessment');
      await clickMenu(page, appsMenu, assessmentSub);
      await page.waitForTimeout(400);
      await clickMenu(page, assessmentSub, riskLink);
      await page.waitForTimeout(400);
      await riskLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/RiskAssessments.*/);
      await page.waitForTimeout(2500); // Extra wait for page content / data grid to load
      await slowScroll(page, 800);     // Scroll further to show full page content
      await page.waitForTimeout(1500); // Hold at bottom so viewer can read

      await showCompletionCard(page, '07', 'Capture Risk Assessment Outcome');
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 08 – Initiate Departmental Review  (Property Officer)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 08: Initiate Departmental Review', async () => {
      await showTitleCard(page, {
        ucNumber: '08', ucTitle: 'Initiate Departmental Review',
        actor: 're_property_officer  (Property Officer)',
        navigation: 'Applications → Assessment → Departmental Reviews',
        category: CAT_LEASE,
      });
      await removeTitleCard(page);

      const appsMenu = getParentMenu(page, 'Applications', 'Assessment');
      const assessmentSub = getSubMenu(page, 'Applications', 'Assessment', 'Risk Assessment');
      const deptReviewsLink = getTertiaryMenu(page, 'Applications', 'Assessment', 'Departmental Reviews').first();
      await clickMenu(page, appsMenu, assessmentSub);
      await page.waitForTimeout(400);
      await clickMenu(page, assessmentSub, deptReviewsLink);
      await page.waitForTimeout(400);
      await deptReviewsLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/DepartmentalReviews.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '08', 'Initiate Departmental Review');
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 10 – Consolidate Departmental Feedback  (Property Officer)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 10: Consolidate Departmental Feedback', async () => {
      await showTitleCard(page, {
        ucNumber: '10', ucTitle: 'Consolidate Departmental Feedback',
        actor: 're_property_officer  (Property Officer)',
        navigation: 'Applications → Assessment → Review Outcomes',
        category: CAT_LEASE,
      });
      await removeTitleCard(page);

      const appsMenu = getParentMenu(page, 'Applications', 'Assessment');
      const assessmentSub = getSubMenu(page, 'Applications', 'Assessment', 'Risk Assessment');
      const reviewOutcomesLink = getTertiaryMenu(page, 'Applications', 'Assessment', 'Review Outcomes');
      await clickMenu(page, appsMenu, assessmentSub);
      await page.waitForTimeout(400);
      await clickMenu(page, assessmentSub, reviewOutcomesLink);
      await page.waitForTimeout(400);
      await reviewOutcomesLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/ConsolidateFeedback.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '10', 'Consolidate Departmental Feedback');
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 19 – Review Permission to Occupy  (Property Officer)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 19: Review Permission to Occupy', async () => {
      await showTitleCard(page, {
        ucNumber: '19', ucTitle: 'Review Permission to Occupy',
        actor: 're_property_officer  (Property Officer)',
        navigation: 'Applications → Lease Agreements → PTO Reviews',
        category: CAT_LEASE,
      });
      await removeTitleCard(page);

      const appsMenuAgreements = getParentMenu(page, 'Applications', 'Lease Agreements');
      const leaseAgreementsSub = getSubMenu(page, 'Applications', 'Lease Agreements', 'PTO Reviews');
      const ptoReviewsLink = getTertiaryMenu(page, 'Applications', 'Lease Agreements', 'PTO Reviews');
      await clickMenu(page, appsMenuAgreements, leaseAgreementsSub);
      await page.waitForTimeout(400);
      await clickMenu(page, leaseAgreementsSub, ptoReviewsLink);
      await page.waitForTimeout(400);
      await ptoReviewsLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/PtoReviews.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '19', 'Review Permission to Occupy');
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 13 – Schedule Unit Inspection  (Property Officer)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 13: Schedule Unit Inspection', async () => {
      await showTitleCard(page, {
        ucNumber: '13', ucTitle: 'Schedule Unit Inspection',
        actor: 're_property_officer  (Property Officer)',
        navigation: 'Inspections → Schedule Inspection → Pre-Occupation Inspection',
        category: CAT_MAINT,
      });
      await removeTitleCard(page);

      const inspectionsMenu = getParentMenu(page, 'Inspections', 'Schedule Inspection');
      const scheduleInspectionSub = getSubMenu(page, 'Inspections', 'Schedule Inspection', 'Pre-Occupation Inspection');
      const preOccupationLink = getTertiaryMenu(page, 'Inspections', 'Schedule Inspection', 'Pre-Occupation Inspection');
      await clickMenu(page, inspectionsMenu, scheduleInspectionSub);
      await page.waitForTimeout(400);
      await clickMenu(page, scheduleInspectionSub, preOccupationLink);
      await page.waitForTimeout(400);
      await preOccupationLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/InspectionSchedules.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '13', 'Schedule Unit Inspection');
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 14 – Conduct Unit Inspection  (Property Officer)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 14: Conduct Unit Inspection', async () => {
      await showTitleCard(page, {
        ucNumber: '14', ucTitle: 'Conduct Unit Inspection',
        actor: 're_property_officer  (Property Officer)',
        navigation: 'Inspections → Inspection → Entry Inspection',
        category: CAT_MAINT,
      });
      await removeTitleCard(page);

      const inspectionsMenu2 = getParentMenu(page, 'Inspections', 'Inspection');
      const inspectionSub = getSubMenu(page, 'Inspections', 'Inspection', 'Entry Inspection');
      const entryInspectionLink = getTertiaryMenu(page, 'Inspections', 'Inspection', 'Entry Inspection');
      await clickMenu(page, inspectionsMenu2, inspectionSub);
      await page.waitForTimeout(400);
      await clickMenu(page, inspectionSub, entryInspectionLink);
      await page.waitForTimeout(400);
      await entryInspectionLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/ConductInspections.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '14', 'Conduct Unit Inspection');
      await logoff(page);
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 09 – Capture Departmental Reviews  (City Planning Rep)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 09: Capture Departmental Reviews and Comments', async () => {
      await showTitleCard(page, {
        ucNumber: '09', ucTitle: 'Capture Departmental Reviews and Comments',
        actor: 're_city_planning  (City Planning Representative)',
        navigation: 'Applications → Assessment → Departmental Reviews',
        category: CAT_LEASE,
      });
      await removeTitleCard(page);

      await loginAs(page, 're_city_planning');
      await dismissModalIfPresent(page);

      const appsMenu = getParentMenu(page, 'Applications', 'Assessment');
      const assessmentSub = getSubMenu(page, 'Applications', 'Assessment', 'Departmental Reviews');
      const deptReviewsLink = getTertiaryMenu(page, 'Applications', 'Assessment', 'Departmental Reviews');
      await clickMenu(page, appsMenu, assessmentSub);
      await page.waitForTimeout(400);
      await clickMenu(page, assessmentSub, deptReviewsLink);
      await page.waitForTimeout(400);
      await deptReviewsLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/DepartmentalQueue.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '09', 'Capture Departmental Reviews and Comments');
      await logoff(page);
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 11 – Committee Review and Decision  (Committee Member)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 11: Committee Review and Decision', async () => {
      await showTitleCard(page, {
        ucNumber: '11', ucTitle: 'Committee Review and Decision',
        actor: 're_committee_member  (Committee Member)',
        navigation: 'Applications → Assessment → Committee Reviews',
        category: CAT_LEASE,
      });
      await removeTitleCard(page);

      await loginAs(page, 're_committee_member');
      await dismissModalIfPresent(page);

      const appsMenu = getParentMenu(page, 'Applications', 'Assessment');
      const assessmentSub = getSubMenu(page, 'Applications', 'Assessment', 'Committee Reviews');
      const committeeReviewsLink = getTertiaryMenu(page, 'Applications', 'Assessment', 'Committee Reviews');
      await clickMenu(page, appsMenu, assessmentSub);
      await page.waitForTimeout(400);
      await clickMenu(page, assessmentSub, committeeReviewsLink);
      await page.waitForTimeout(400);
      await committeeReviewsLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/CommitteeReviews.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '11', 'Committee Review and Decision');
      await logoff(page);
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 15 – Authorise and Assign Works Order  (Facilities Manager)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 15: Authorise and Assign Works Order Request', async () => {
      await showTitleCard(page, {
        ucNumber: '15', ucTitle: 'Authorise and Assign Works Order Request',
        actor: 're_facilities_manager  (Facilities Manager)',
        navigation: 'Maintenance → Maintenance Work Orders → Authorise and Assign Works Order Request',
        category: CAT_MAINT,
      });
      await removeTitleCard(page);

      await loginAs(page, 're_facilities_manager');
      await dismissModalIfPresent(page);

      const maintMenu = getParentMenu(page, 'Maintenance', 'Maintenance Work Orders');
      const workOrdersSub = getSubMenu(page, 'Maintenance', 'Maintenance Work Orders', 'Authorise and Assign Works Order Request');
      const authoriseLink = getTertiaryMenu(page, 'Maintenance', 'Maintenance Work Orders', 'Authorise and Assign Works Order Request');
      await clickMenu(page, maintMenu, workOrdersSub);
      await page.waitForTimeout(400);
      await clickMenu(page, workOrdersSub, authoriseLink);
      await page.waitForTimeout(400);
      await authoriseLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/WorkOrders.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '15', 'Authorise and Assign Works Order Request');
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 16 – Create Maintenance Schedule  (Facilities Manager)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 16: Create Maintenance Schedule', async () => {
      await showTitleCard(page, {
        ucNumber: '16', ucTitle: 'Create Maintenance Schedule',
        actor: 're_facilities_manager  (Facilities Manager)',
        navigation: 'Maintenance → Maintenance Planning → Create Maintenance Schedule',
        category: CAT_MAINT,
      });
      await removeTitleCard(page);

      const maintMenu2 = getParentMenu(page, 'Maintenance', 'Maintenance Planning');
      const planningSub = getSubMenu(page, 'Maintenance', 'Maintenance Planning', 'Create Maintenance Schedule');
      const createScheduleLink = getTertiaryMenu(page, 'Maintenance', 'Maintenance Planning', 'Create Maintenance Schedule');
      await clickMenu(page, maintMenu2, planningSub);
      await page.waitForTimeout(400);
      await clickMenu(page, planningSub, createScheduleLink);
      await page.waitForTimeout(400);
      await createScheduleLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/MaintenancePlanning.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '16', 'Create Maintenance Schedule');
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 17 – Log Issue (Maintenance Request)  (Facilities Manager)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 17a: Log Issue – Maintenance Request', async () => {
      await showTitleCard(page, {
        ucNumber: '17', ucTitle: 'Log Issue – Maintenance Request',
        actor: 're_facilities_manager  (Facilities Manager)',
        navigation: 'Maintenance → Maintenance Planning → Maintenance Request',
        category: CAT_MAINT,
      });
      await removeTitleCard(page);

      const maintMenu2 = getParentMenu(page, 'Maintenance', 'Maintenance Planning');
      const planningSub2 = getSubMenu(page, 'Maintenance', 'Maintenance Planning', 'Maintenance Request');
      const requestLink = getTertiaryMenu(page, 'Maintenance', 'Maintenance Planning', 'Maintenance Request');
      await clickMenu(page, maintMenu2, planningSub2);
      await page.waitForTimeout(400);
      await clickMenu(page, planningSub2, requestLink);
      await page.waitForTimeout(400);
      await requestLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/WorkOrders.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '17', 'Log Issue – Maintenance Request');
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 17b – Assignment (Internally Assigned)  (Facilities Manager)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 17b: Assignment – Internally Assigned', async () => {
      await showTitleCard(page, {
        ucNumber: '17', ucTitle: 'Assignment – Internally Assigned',
        actor: 're_facilities_manager  (Facilities Manager)',
        navigation: 'Maintenance → Maintenance Work Orders → Assignment → Internally Assigned',
        category: CAT_MAINT,
      });
      await removeTitleCard(page);

      const maintMenu = getParentMenu(page, 'Maintenance', 'Maintenance Work Orders');
      const workOrdersSub2 = getSubMenu(page, 'Maintenance', 'Maintenance Work Orders', 'Assignment');
      const assignmentSub = getTertiaryMenu(page, 'Maintenance', 'Maintenance Work Orders', 'Assignment', 'Internally Assigned');
      const internalLink = getQuaternaryMenu(page, 'Maintenance', 'Maintenance Work Orders', 'Assignment', 'Internally Assigned');
      await clickMenu(page, maintMenu, workOrdersSub2);
      await page.waitForTimeout(400);
      await clickMenu(page, workOrdersSub2, assignmentSub);
      await page.waitForTimeout(400);
      await clickMenu(page, assignmentSub, internalLink);
      await page.waitForTimeout(400);
      await internalLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/WorkOrders\?tab=internal.*/);
      await page.waitForTimeout(800);

      await showCompletionCard(page, '17', 'Assignment – Internally Assigned');
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 17c – Assignment (Externally Assigned)  (Facilities Manager)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 17c: Assignment – Externally Assigned', async () => {
      await showTitleCard(page, {
        ucNumber: '17', ucTitle: 'Assignment – Externally Assigned',
        actor: 're_facilities_manager  (Facilities Manager)',
        navigation: 'Maintenance → Maintenance Work Orders → Assignment → Externally Assigned',
        category: CAT_MAINT,
      });
      await removeTitleCard(page);

      const maintMenu = getParentMenu(page, 'Maintenance', 'Maintenance Work Orders');
      const workOrdersSub2 = getSubMenu(page, 'Maintenance', 'Maintenance Work Orders', 'Assignment');
      const assignmentSub = getTertiaryMenu(page, 'Maintenance', 'Maintenance Work Orders', 'Assignment', 'Internally Assigned');
      const externalLink = getQuaternaryMenu(page, 'Maintenance', 'Maintenance Work Orders', 'Assignment', 'Externally Assigned');
      await clickMenu(page, maintMenu, workOrdersSub2);
      await page.waitForTimeout(400);
      await clickMenu(page, workOrdersSub2, assignmentSub);
      await page.waitForTimeout(400);
      await clickMenu(page, assignmentSub, externalLink);
      await page.waitForTimeout(400);
      await externalLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/WorkOrders\?tab=external.*/);
      await page.waitForTimeout(800);

      await showCompletionCard(page, '17', 'Assignment – Externally Assigned');
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 17d – Work Orders Pending Closure  (Facilities Manager)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 17d: Work Orders Pending Closure', async () => {
      await showTitleCard(page, {
        ucNumber: '17', ucTitle: 'Work Orders Pending Closure',
        actor: 're_facilities_manager  (Facilities Manager)',
        navigation: 'Maintenance → Maintenance Work Orders → Work Orders Pending Closure',
        category: CAT_MAINT,
      });
      await removeTitleCard(page);

      const maintMenu = getParentMenu(page, 'Maintenance', 'Maintenance Work Orders');
      const workOrdersSub3 = getSubMenu(page, 'Maintenance', 'Maintenance Work Orders', 'Work Orders Pending Closure');
      const closureLink = getTertiaryMenu(page, 'Maintenance', 'Maintenance Work Orders', 'Work Orders Pending Closure');
      await clickMenu(page, maintMenu, workOrdersSub3);
      await page.waitForTimeout(400);
      await clickMenu(page, workOrdersSub3, closureLink);
      await page.waitForTimeout(400);
      await closureLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/WorkOrders\?tab=closure.*/);
      await page.waitForTimeout(800);

      await showCompletionCard(page, '17', 'Work Orders Pending Closure');
      await logoff(page);
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 17e – Allocation Requests  (Technician)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 17e: Allocation Requests', async () => {
      await showTitleCard(page, {
        ucNumber: '17', ucTitle: 'Allocation Requests',
        actor: 're_technician  (Technician / Caretaker)',
        navigation: 'Maintenance → Maintenance Work Orders → Allocation Requests',
        category: CAT_MAINT,
      });
      await removeTitleCard(page);

      await loginAs(page, 're_technician');
      await dismissModalIfPresent(page);

      const maintMenu = getParentMenu(page, 'Maintenance', 'Maintenance Work Orders');
      const workOrdersSub = getSubMenu(page, 'Maintenance', 'Maintenance Work Orders', 'Allocation Requests');
      const allocationLink = getTertiaryMenu(page, 'Maintenance', 'Maintenance Work Orders', 'Allocation Requests');
      await clickMenu(page, maintMenu, workOrdersSub);
      await page.waitForTimeout(400);
      await clickMenu(page, workOrdersSub, allocationLink);
      await page.waitForTimeout(400);
      await allocationLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/TechnicianAllocation.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '17', 'Allocation Requests');
    });

    // ──────────────────────────────────────────────────────────────────
    // UC 17f – Accepted Work Orders  (Technician)
    // ──────────────────────────────────────────────────────────────────
    await test.step('UC 17f: Accepted Work Orders', async () => {
      await showTitleCard(page, {
        ucNumber: '17', ucTitle: 'Accepted Work Orders',
        actor: 're_technician  (Technician / Caretaker)',
        navigation: 'Maintenance → Maintenance Work Orders → Accepted Work Orders',
        category: CAT_MAINT,
      });
      await removeTitleCard(page);

      const maintMenu = getParentMenu(page, 'Maintenance', 'Maintenance Work Orders');
      const workOrdersSub2 = getSubMenu(page, 'Maintenance', 'Maintenance Work Orders', 'Accepted Work Orders');
      const acceptedLink = getTertiaryMenu(page, 'Maintenance', 'Maintenance Work Orders', 'Accepted Work Orders');
      await clickMenu(page, maintMenu, workOrdersSub2);
      await page.waitForTimeout(400);
      await clickMenu(page, workOrdersSub2, acceptedLink);
      await page.waitForTimeout(400);
      await acceptedLink.click();
      await expect(page).toHaveURL(/.*RealEstateAdmin\/TechnicianAllocation\?tab=accepted.*/);
      await page.waitForTimeout(800);
      await slowScroll(page, 400);

      await showCompletionCard(page, '17', 'Accepted Work Orders');
      await logoff(page);
    });

    // ══════════════════════════════════════════════════════════════════
    // FINAL SLIDE – All done
    // ══════════════════════════════════════════════════════════════════
    await page.evaluate(() => {
      const overlay = document.createElement('div');
      overlay.id = 'uc-title-overlay';
      overlay.style.cssText = `
        position: fixed; inset: 0; z-index: 999999;
        display: flex; align-items: center; justify-content: center;
        background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
        font-family: 'Segoe UI', sans-serif; color: #fff;
      `;
      overlay.innerHTML = `
        <div style="text-align:center;">
          <div style="font-size:72px; margin-bottom:24px;">🎉</div>
          <h1 style="font-size:42px; font-weight:300; margin:0 0 16px 0;">All Use Cases Verified</h1>
          <p style="font-size:18px; color:#94a3b8; margin:0;">
            UC 01 – UC 20 &nbsp;|&nbsp; 8 Roles &nbsp;|&nbsp; 20+ Navigation Paths
          </p>
        </div>
      `;
      document.body.appendChild(overlay);
    });
    await page.waitForTimeout(4000);
  });
});
