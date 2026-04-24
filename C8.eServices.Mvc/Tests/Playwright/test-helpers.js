const { expect } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

const stateFilePath = path.join(__dirname, 'test-state.json');

async function login(page, username = 'CoeSolarDev08', password = 'Arsenal5@') {
  await page.goto('/');
  await page.getByRole('link', { name: ' Profile ' }).click();
  await page.getByRole('link', { name: ' Sign In' }).click();
  await page.getByRole('textbox', { name: 'username or e-mail' }).fill(username);
  await page.getByRole('textbox', { name: 'Please enter your password' }).fill(password);
  await page.getByRole('button', { name: 'Sign In' }).click({ force: true });
  await expect(page.getByRole('link', { name: 'Log off' })).toBeVisible({ timeout: 15000 });
}

function saveState(data) {
  let state = {};
  if (fs.existsSync(stateFilePath)) {
    state = JSON.parse(fs.readFileSync(stateFilePath, 'utf8'));
  }
  state = { ...state, ...data };
  fs.writeFileSync(stateFilePath, JSON.stringify(state, null, 2));
}

async function updateTestState(newState) {
  let state = {};
  if (fs.existsSync(stateFilePath)) {
    state = JSON.parse(fs.readFileSync(stateFilePath, 'utf8'));
  }
  
  // Update state
  state = { ...state, ...newState, lastUpdate: new Date().toISOString() };
  
  // Add to history
  if (!state.history) state.history = [];
  if (newState.lastAction) {
    state.history.unshift({
      title: newState.lastAction,
      time: new Date().toLocaleTimeString(),
      details: newState.details || 'Step completed successfully'
    });
  }

  // Limit history to 10 items
  if (state.history.length > 10) state.history = state.history.slice(0, 10);

  fs.writeFileSync(stateFilePath, JSON.stringify(state, null, 2));
}

function getState() {
  if (fs.existsSync(stateFilePath)) {
    return JSON.parse(fs.readFileSync(stateFilePath, 'utf8'));
  }
  return {};
}

module.exports = { login, saveState, getState, updateTestState };
