import { expect, afterEach, vi } from 'vitest';
import { cleanup } from '@testing-library/react';
import '@testing-library/jest-dom/vitest';

// Set React environment
if (!globalThis.IS_REACT_ACT_ENVIRONMENT) {
  globalThis.IS_REACT_ACT_ENVIRONMENT = true;
}

afterEach(() => {
  cleanup();
});
