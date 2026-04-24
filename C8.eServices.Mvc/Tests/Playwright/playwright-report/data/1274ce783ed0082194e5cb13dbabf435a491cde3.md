# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: PLM_FullSystem_E2E.spec.js >> PLM Full System End-to-End Professional Workflow >> Execute Complete E2E Lifecycle
- Location: PLM_FullSystem_E2E.spec.js:6:3

# Error details

```
TimeoutError: locator.click: Timeout 15000ms exceeded.
Call log:
  - waiting for getByRole('button', { name: 'Sign In' })
    - locator resolved to <button type="submit">Sign In</button>
  - attempting click action
    2 × waiting for element to be visible, enabled and stable
      - element is visible, enabled and stable
      - scrolling into view if needed
      - done scrolling
      - <div id="holdon-overlay">…</div> intercepts pointer events
    - retrying click action
    - waiting 20ms
    2 × waiting for element to be visible, enabled and stable
      - element is visible, enabled and stable
      - scrolling into view if needed
      - done scrolling
      - <div id="holdon-overlay">…</div> intercepts pointer events
    - retrying click action
      - waiting 100ms
    - waiting for element to be visible, enabled and stable
    - element is visible, enabled and stable
    - scrolling into view if needed
    - done scrolling
    - <div id="holdon-overlay">…</div> intercepts pointer events
  - retrying click action
    - waiting 500ms
    - waiting for element to be visible, enabled and stable
    - element is visible, enabled and stable
    - scrolling into view if needed
    - done scrolling
    - performing click action
    - click action done
    - waiting for scheduled navigations to finish

```

# Test source

```ts
  1  | const { expect } = require('@playwright/test');
  2  | const fs = require('fs');
  3  | const path = require('path');
  4  | 
  5  | const stateFilePath = path.join(__dirname, 'test-state.json');
  6  | 
  7  | async function login(page, username = 'CoeSolarDev08', password = 'Arsenal5@') {
  8  |   await page.goto('/');
  9  |   await page.getByRole('link', { name: ' Profile ' }).click();
  10 |   await page.getByRole('link', { name: ' Sign In' }).click();
  11 |   await page.getByRole('textbox', { name: 'username or e-mail' }).fill(username);
  12 |   await page.getByRole('textbox', { name: 'Please enter your password' }).fill(password);
> 13 |   await page.getByRole('button', { name: 'Sign In' }).click();
     |                                                       ^ TimeoutError: locator.click: Timeout 15000ms exceeded.
  14 |   await expect(page.getByRole('link', { name: 'Log off' })).toBeVisible();
  15 | }
  16 | 
  17 | function saveState(data) {
  18 |   let state = {};
  19 |   if (fs.existsSync(stateFilePath)) {
  20 |     state = JSON.parse(fs.readFileSync(stateFilePath, 'utf8'));
  21 |   }
  22 |   state = { ...state, ...data };
  23 |   fs.writeFileSync(stateFilePath, JSON.stringify(state, null, 2));
  24 | }
  25 | 
  26 | async function updateTestState(newState) {
  27 |   let state = {};
  28 |   if (fs.existsSync(stateFilePath)) {
  29 |     state = JSON.parse(fs.readFileSync(stateFilePath, 'utf8'));
  30 |   }
  31 |   
  32 |   // Update state
  33 |   state = { ...state, ...newState, lastUpdate: new Date().toISOString() };
  34 |   
  35 |   // Add to history
  36 |   if (!state.history) state.history = [];
  37 |   if (newState.lastAction) {
  38 |     state.history.unshift({
  39 |       title: newState.lastAction,
  40 |       time: new Date().toLocaleTimeString(),
  41 |       details: newState.details || 'Step completed successfully'
  42 |     });
  43 |   }
  44 | 
  45 |   // Limit history to 10 items
  46 |   if (state.history.length > 10) state.history = state.history.slice(0, 10);
  47 | 
  48 |   fs.writeFileSync(stateFilePath, JSON.stringify(state, null, 2));
  49 | }
  50 | 
  51 | function getState() {
  52 |   if (fs.existsSync(stateFilePath)) {
  53 |     return JSON.parse(fs.readFileSync(stateFilePath, 'utf8'));
  54 |   }
  55 |   return {};
  56 | }
  57 | 
  58 | module.exports = { login, saveState, getState, updateTestState };
  59 | 
```