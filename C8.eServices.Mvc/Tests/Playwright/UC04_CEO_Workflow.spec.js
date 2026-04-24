import { test, expect } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';

// Path to the shared state file
const stateFilePath = path.join(__dirname, 'test-state.json');

/**
 * Helper to get the reference number from the shared trail
 */
function getTargetRef() {
  if (fs.existsSync(stateFilePath)) {
    const data = JSON.parse(fs.readFileSync(stateFilePath, 'utf8'));
    return data.appRef;
  }
  return 'EHC2026042300001'; // Fallback if no trail exists
}

test('CEO Approval (Positive)', async ({ page }) => {
  const targetAppRef = getTargetRef();
  console.log(`Running test for application: ${targetAppRef}`);

  await page.goto('http://localhost:3450/');
  await page.getByRole('link', { name: ' Profile ' }).click();
  await page.getByRole('link', { name: ' Sign In' }).click();
  await page.getByRole('textbox', { name: 'username or e-mail' }).click();
  await page.getByRole('textbox', { name: 'username or e-mail' }).fill('CoeSolarDev08');
  await page.getByRole('textbox', { name: 'Please enter your password' }).click();
  await page.getByRole('textbox', { name: 'Please enter your password' }).fill('Arsenal5@');
  await page.getByRole('button', { name: 'Sign In' }).click();
  
  await page.getByRole('link', { name: ' PLM Applications ' }).click();
  await page.getByRole('link', { name: ' Inbox Applications' }).click();
  
  await page.getByRole('searchbox', { name: 'Search:' }).fill(targetAppRef);
  await page.getByRole('link', { name: ' Finalize' }).click();
  
  await page.getByRole('textbox', { name: 'CEO Official Number*' }).fill('48596412536526');
  
  // Signature pad interactions (Simulated)
  await page.locator('#signature-pad').click({ position: { x: 130, y: 71 } });
  await page.locator('#signature-pad').click({ position: { x: 178, y: 47 } });
  
  await page.locator('#ApprovalStatus').selectOption('Approved');
  await page.locator('#CEOComment').fill('CEO Approved - Final Step');
  
  await page.getByRole('button', { name: 'Submit' }).click();
  await page.getByRole('button', { name: 'Yes, I am sure!' }).click();
  
  // Verify completion
  await expect(page.getByText('Application successfully processed')).toBeVisible();
  
  await page.getByRole('link', { name: ' Profile ' }).click();
  await page.getByRole('link', { name: 'Log off' }).click();
});

/**
 * UC04: CEO Rejection Workflow (Negative Case - Return to CSO)
 * This test verifies the redirection logic where a CEO rejection
 * routes the task back to the original CSO.
 */
test('CEO Rejection (Negative - Return to CSO)', async ({ page }) => {
  const targetAppRef = getTargetRef();
  await page.goto('http://localhost:3450/');
  await page.getByRole('link', { name: ' Profile ' }).click();
  await page.getByRole('link', { name: ' Sign In' }).click();
  await page.getByRole('textbox', { name: 'username or e-mail' }).fill('CoeSolarDev08');
  await page.getByRole('textbox', { name: 'Please enter your password' }).fill('Arsenal5@');
  await page.getByRole('button', { name: 'Sign In' }).click();
  
  await page.getByRole('link', { name: ' PLM Applications ' }).click();
  await page.getByRole('link', { name: ' Inbox Applications' }).click();
  await page.getByRole('searchbox', { name: 'Search:' }).fill(targetAppRef);
  await page.getByRole('link', { name: ' Finalize' }).click();
  
  await page.getByRole('textbox', { name: 'CEO Official Number*' }).fill('48596412536526');
  await page.locator('#signature-pad').click({ position: { x: 130, y: 71 } });
  
  // SELECT REJECTED / NOT SUPPORTED
  await page.locator('#ApprovalStatus').selectOption('Rejected'); 
  await page.locator('#CEOComment').fill('CEO Rejection Test - Returning to CSO for remediation.');
  
  await page.getByRole('button', { name: 'Submit' }).click();
  await page.getByRole('button', { name: 'Yes, I am sure!' }).click();
  
  // Final verification: Ensure we are redirected back to the inbox or a success page
  await expect(page.getByText('Application Returned to CSO')).toBeVisible();
});
