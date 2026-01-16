import { test, expect } from '@playwright/test';

test.describe('API Tests', () => {
  test('should require auth for profiling', async ({ request }) => {
    const response = await request.post('http://localhost:5000/api/profiling', {
      data: { workspaceId: 'test', datasetName: 'test', tableName: 'test' }
    });
    expect(response.status()).toBe(401);
  });
});
