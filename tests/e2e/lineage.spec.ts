import { test, expect } from '@playwright/test';

test.describe('Lineage Viewer E2E', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should navigate to lineage viewer', async ({ page }) => {
    await page.getByRole('tab', { name: 'Lineage Viewer' }).click();
    await expect(page).toHaveURL(/.*lineage/);
  });

  test('should show lineage graph container', async ({ page }) => {
    await page.getByRole('tab', { name: 'Lineage Viewer' }).click();
    const graphContainer = page.locator('[data-testid="lineage-graph"]');
    await expect(graphContainer).toBeVisible();
  });

  test('should have zoom controls', async ({ page }) => {
    await page.getByRole('tab', { name: 'Lineage Viewer' }).click();
    await expect(page.getByRole('button', { name: /zoom.*in/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /zoom.*out/i })).toBeVisible();
  });

  test('should have filter options', async ({ page }) => {
    await page.getByRole('tab', { name: 'Lineage Viewer' }).click();
    await expect(page.getByLabel(/filter/i)).toBeVisible();
  });
});
