import { test, expect } from '@playwright/test';

test.describe('Dashboard E2E', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should load dashboard page', async ({ page }) => {
    await expect(page).toHaveTitle(/ClarityDQ|Quality/i);
  });

  test('should show quality metrics', async ({ page }) => {
    await expect(page.getByText(/quality.*score/i)).toBeVisible();
  });

  test('should show recent executions', async ({ page }) => {
    const recentSection = page.locator('[data-testid="recent-executions"]');
    await expect(recentSection).toBeVisible();
  });

  test('should navigate between tabs', async ({ page }) => {
    await page.getByRole('tab', { name: 'Quality Rules' }).click();
    await expect(page).toHaveURL(/.*rules/);

    await page.getByRole('tab', { name: 'Data Profiling' }).click();
    await expect(page).toHaveURL(/.*profiling/);

    await page.getByRole('tab', { name: 'Lineage Viewer' }).click();
    await expect(page).toHaveURL(/.*lineage/);
  });

  test('should have navigation sidebar', async ({ page }) => {
    const sidebar = page.locator('nav[aria-label="Main navigation"]');
    await expect(sidebar).toBeVisible();
  });
});
