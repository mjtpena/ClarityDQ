import { test, expect } from '@playwright/test';

test.describe('Schedules E2E', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.getByRole('tab', { name: 'Schedules' }).click();
  });

  test('should show schedules page', async ({ page }) => {
    await expect(page).toHaveURL(/.*schedules/);
  });

  test('should have create schedule button', async ({ page }) => {
    await expect(page.getByRole('button', { name: /create.*schedule/i })).toBeVisible();
  });

  test('should open create schedule dialog', async ({ page }) => {
    await page.getByRole('button', { name: /create.*schedule/i }).click();
    await expect(page.getByRole('dialog')).toBeVisible();
    await expect(page.getByLabel(/schedule.*name/i)).toBeVisible();
  });

  test('should show schedule list', async ({ page }) => {
    const scheduleList = page.locator('[data-testid="schedule-list"]');
    await expect(scheduleList).toBeVisible();
  });

  test('should filter schedules by status', async ({ page }) => {
    await page.getByLabel(/filter.*status/i).click();
    await page.getByRole('option', { name: 'Active' }).click();
  });
});
