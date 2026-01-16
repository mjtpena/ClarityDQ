import { test, expect } from '@playwright/test';

test.describe('Quality Rules E2E', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.getByRole('tab', { name: 'Quality Rules' }).click();
  });

  test('should open create rule dialog', async ({ page }) => {
    await page.getByRole('button', { name: 'Create Rule' }).click();
    await expect(page.getByRole('dialog')).toBeVisible();
  });

  test('should show empty state', async ({ page }) => {
    await expect(page.getByText('No rules found')).toBeVisible();
  });
});
