const { test, expect } = require('@playwright/test');
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

// Configure high resolution (1080p) specifically for this presentation video
test.use({
  viewport: { width: 1920, height: 1080 },
  video: {
    mode: 'on',
    size: { width: 1920, height: 1080 }
  }
});

test('Real Estate and EHC/HSD Regression Customer Login Flows', async ({ page }) => {
  test.setTimeout(90000); // Extend timeout for slower presentation delays
  console.log('Testing Real Estate Customer login and redirection...');

  
  // 1. Navigate to the login page
  await page.goto('http://localhost:3450/Account/Login');
  await delay(2000); // Allow CEO to see the login screen clearly
  
  // 2. Log in with the newly created Real Estate user
  await page.locator('#UserName').fill('RealEstateCustomer');
  await delay(1000); // Wait between fields for visual clarity
  await page.locator('#Password').fill('Arsenal5@');
  await delay(2000); // Wait before clicking Sign In
  await page.getByRole('button', { name: 'Sign In' }).click();
  
  // 3. Assert redirection to the Real Estate Inbox
  await expect(page).toHaveURL(/.*\/RealEstate\/Inbox/, { timeout: 15000 });
  console.log('Redirected to /RealEstate/Inbox successfully!');
  await delay(3000); // Stay on dashboard so the empty inbox message can be read
  
  // 4. Assert dashboard content
  await expect(page.locator('text=Real Estate Development Dashboard')).toBeVisible();
  await expect(page.locator('text=No Applications Found')).toBeVisible();
  
  // 5. Assert that ONLY the Real Estate side navigation is shown in the sidebar menu
  const realEstateSidebarLink = page.locator('ul.nav-list a:has-text("Real Estate Applications")');
  await expect(realEstateSidebarLink).toBeVisible();
  await realEstateSidebarLink.click();
  await delay(1500); // Wait for dropdown animation to complete
  await expect(page.locator('ul.nav-list >> text=Inbox')).toBeVisible();
  await delay(4000); // Pause to show the sidebar layout configuration to the CEO
  
  // Assert EHC / HSD customer menus are NOT visible
  await expect(page.locator('text=PLM Applications')).not.toBeVisible();
  await expect(page.locator('text=My Training')).not.toBeVisible();
  await expect(page.locator('text=My Complaints')).not.toBeVisible();
  
  // 6. Log off robustly
  await page.locator('#burgerbutton').click();
  await delay(1000); // Wait for menu layout to slide out
  await page.locator('#logoutForm a:has-text("Log off")').click();
  await expect(page).toHaveURL(/.*\/Account\/Login/, { timeout: 15000 });
  console.log('Real Estate Customer verification passed successfully!');
  await delay(2000); // Pause on login screen before next login

  console.log('Testing EHC normal login regression...');
  
  // 7. Log in with sash38 (EHC customer)
  await page.locator('#UserName').fill('sash38');
  await delay(1000);
  await page.locator('#Password').fill('Arsenal5@');
  await delay(2000);
  await page.getByRole('button', { name: 'Sign In' }).click();
  
  // 8. Assert redirection to normal customer lease application inbox
  await expect(page).toHaveURL(/.*\/PropertyLeaseApplication\/Inbox/, { timeout: 15000 });
  console.log('Redirected sash38 to EHC Inbox successfully!');
  await delay(3000); // Stay on EHC dashboard
  
  // 9. Assert side menu navigation for EHC is visible and Real Estate is NOT visible
  await expect(page.locator('text=PLM Applications')).toBeVisible();
  await expect(page.locator('text=Real Estate Applications')).not.toBeVisible();
  await delay(3000); // Stay to display normal sidebar layout config
  
  // 10. Log off robustly
  await page.locator('#burgerbutton').click();
  await delay(1000);
  await page.locator('#logoutForm a:has-text("Log off")').click();
  await expect(page).toHaveURL(/.*\/Account\/Login/, { timeout: 15000 });
  await delay(2000);

  console.log('Testing HSD normal login regression...');
  
  // 11. Log in with sashtest12 (HSD customer)
  await page.locator('#UserName').fill('sashtest12');
  await delay(1000);
  await page.locator('#Password').fill('Arsenal5@');
  await delay(2000);
  await page.getByRole('button', { name: 'Sign In' }).click();
  
  // 12. Assert redirection to HSD customer profile / Inbox
  await expect(page).toHaveURL(/.*\/Profile\/Index.*/, { timeout: 15000 });
  console.log('sashtest12 loaded Profile Index page successfully!');
  await delay(4000); // Pause on HSD profile page
  
  // 13. Log off robustly
  await page.locator('#burgerbutton').click();
  await delay(1000);
  await page.locator('#logoutForm a:has-text("Log off")').click();
  await expect(page).toHaveURL(/.*\/Account\/Login/, { timeout: 15000 });
  await delay(2000);
  console.log('All regression checks passed in one continuous video!');
});


