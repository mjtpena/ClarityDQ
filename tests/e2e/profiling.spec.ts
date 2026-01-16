import { test, expect } from '@playwright/test';

test.describe('Data Profiling E2E', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should display dashboard title', async ({ page }) => {
    await expect(page.locator('h1')).toContainText('ClarityDQ Dashboard');
  });

  test('should show profiling tab by default', async ({ page }) => {
    await expect(page.getByRole('tab', { name: 'Data Profiling' })).toBeVisible();
    await expect(page.getByText('Create Data Profile')).toBeVisible();
  });

  test('should validate required fields', async ({ page }) => {
    const profileButton = page.getByRole('button', { name: 'Profile Table' });
    await expect(profileButton).toBeDisabled();
  });
});
