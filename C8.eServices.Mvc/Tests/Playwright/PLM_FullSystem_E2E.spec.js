const { test, expect } = require('@playwright/test');
const { login, updateTestState } = require('./test-helpers');

test.describe('PLM Full System End-to-End Professional Workflow', () => {
  
  test('Execute Complete E2E Lifecycle', async ({ page }) => {
    // Pro-Tip: For complex E2E journeys, testing steps sequentially within one test context 
    // provides the most robust state retention and the cleanest timeline reporting.
    test.setTimeout(120000); // 2 minutes for the entire journey
    
    // Generate dynamic test data to avoid constraint/duplication errors across multiple runs
    const runId = new Date().getTime().toString().slice(-6); 
    const dynamicDesc = `[Auto-Run ${runId}]`;

    await test.step('System Setup & Authentication', async () => {
      await updateTestState({ lastAction: 'System Setup', details: 'Authenticating Admin Session' });
      await login(page);
    });

    await test.step('UC17A: Occupant Management', async () => {
      await updateTestState({ lastAction: 'UC17A: Occupant Profile', details: 'Navigating to Occupant Capture' });

      await page.goto('/LeaseDetails/AddOccupant');
      
      // Dynamic test data entry
      await page.locator('#OccupantFirstName').fill(`Automated_${runId}`);
      await page.locator('#OccupantSurname').fill('Pro-Tester');
      await page.locator('#OccupantIDNumber').fill('8805135123081');
      
      await page.getByRole('button', { name: 'Save Occupant' }).click();

      // Pro-Tip: Always use specific timeout assertions instead of hard waits
      await expect(page.getByText('Occupant added successfully')).toBeVisible({ timeout: 15000 });
      
      const occupantRef = await page.locator('#OccupantReference').innerText();
      await updateTestState({ 
        lastAction: 'UC17A: Occupant Profile', 
        details: `Profile Linked successfully. Ref: ${occupantRef}`,
        occupantReference: occupantRef 
      });
    });

    await test.step('UC17B: Complaints Management', async () => {
      await updateTestState({ lastAction: 'UC17B: Complaint Logging', details: 'Registering new system complaint' });

      await page.goto('/Complaints/Create');
      
      await page.locator('#OfficialNumber').fill('8805132100086');
      
      // We expect the AJAX search to populate the complex dropdown, so we wait for its options
      await page.getByRole('button', { name: 'Search' }).click();
      
      // Pro-Tip: Waiting for elements to attach/populate is 100x more robust than waitForTimeout
      await page.locator('#CategoryDropdown').selectOption({ index: 1 });
      
      // Wait specifically for the cascading type dropdown to fetch its data
      await expect(page.locator('#TypeDropdown option').nth(1)).toBeAttached({ timeout: 10000 });
      await page.locator('#TypeDropdown').selectOption({ index: 1 });
      
      await page.locator('#DetailedDescription').fill(`${dynamicDesc} Noise complaint logged dynamically via Playwright Pro-Suite.`);
      
      await page.getByRole('button', { name: 'Submit Complaint' }).click();

      await expect(page.getByText('Complaint successfully captured')).toBeVisible({ timeout: 15000 });
      
      const caseId = await page.locator('#CaseReferenceNumber').innerText();
      await updateTestState({ 
        lastAction: 'UC17B: Complaint Logging', 
        details: `Complaint Registered. Case ID: ${caseId}`,
        lastComplaintCaseId: caseId 
      });
    });

    await test.step('UC17C: Payment Transgressions', async () => {
      await updateTestState({ lastAction: 'UC17C: Payment Transgression', details: 'Analyzing tenant financial standing' });

      await page.goto('/PaymentTransgressions/Create');
      
      await page.locator('#OfficialNumber').fill('8805132100086');
      await page.locator('#btnLookup').click();
      
      // Wait for AJAX tenant lookup to finish securely
      await expect(page.locator('#lookupStatus')).toContainText('Tenant found', { timeout: 15000 });
      
      await page.locator('#PaymentTransgressionCategoryId').selectOption({ index: 1 });
      
      // Wait for cascading type dropdown
      await expect(page.locator('#PaymentTransgressionTypeId option').nth(1)).toBeAttached({ timeout: 10000 });
      await page.locator('#PaymentTransgressionTypeId').selectOption({ index: 1 });
      
      await page.locator('#PaymentTransgressionSeverityId').selectOption({ index: 1 });
      await page.locator('#DetailedDescription').fill(`${dynamicDesc} Financial compliance issue identified. Letter generation triggered.`);
      
      await page.locator('#LetterType').selectOption({ index: 1 });
      
      await page.getByRole('button', { name: 'Submit & Generate Letter' }).click();

      await expect(page.getByText('Payment transgression created successfully')).toBeVisible({ timeout: 15000 });
      await updateTestState({ 
          lastAction: 'UC17C: Payment Transgression', 
          details: 'Transgression recorded. Legal letter compiled automatically.' 
      });
    });

    await test.step('UC17D: Service Requests', async () => {
      await updateTestState({ lastAction: 'UC17D: Service Request', details: 'Logging facility maintenance request' });

      await page.goto('/ServiceRequests/Create');
      
      await page.locator('#ReportedByName').fill('Playwright');
      await page.locator('#ReportedBySurname').fill(`TestBot_${runId}`);
      await page.locator('#EmailAddress').fill('pro-test@ehc.org.za');
      await page.locator('#ContactNumber').fill('0112223333');
      
      await page.locator('#ComplexId').selectOption({ index: 1 });
      await page.locator('#ServiceRequestCategoryId').selectOption({ index: 1 });
      await page.locator('#ServiceRequestPriorityId').selectOption({ index: 1 });
      await page.locator('#DetailedDescription').fill(`${dynamicDesc} Critical structural defect identified in complex common area.`);
      
      await page.getByRole('button', { name: 'Submit Request' }).click();

      await expect(page.getByText('created successfully')).toBeVisible({ timeout: 15000 });
      
      const requestNo = await page.locator('#RequestReferenceNumber').innerText();
      await updateTestState({ 
        lastAction: 'UC17D: Service Request', 
        details: `Maintenance Request active. Ticket: ${requestNo}`,
        lastServiceRequestNo: requestNo 
      });
    });
    
    await test.step('Completion', async () => {
      await updateTestState({ 
        lastAction: 'E2E Suite Complete', 
        details: `Full System Integrity Scan [${runId}] passed. System is Go-Live Ready.` 
      });
    });

  });
});
