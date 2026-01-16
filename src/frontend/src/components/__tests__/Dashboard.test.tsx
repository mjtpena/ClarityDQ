import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { Provider } from 'react-redux';
import { BrowserRouter } from 'react-router-dom';
import { configureStore } from '@reduxjs/toolkit';
import Dashboard from '../../pages/Dashboard';

const mockStore = configureStore({
  reducer: {
    rules: (state = { rules: [], loading: false }) => state,
    schedules: (state = { schedules: [], loading: false }) => state,
  },
});

describe('Dashboard', () => {
  it('renders without crashing', () => {
    render(
      <Provider store={mockStore}>
        <BrowserRouter>
          <Dashboard />
        </BrowserRouter>
      </Provider>
    );
    expect(screen.getByText(/dashboard|quality/i)).toBeInTheDocument();
  });

  it('displays quality metrics section', () => {
    render(
      <Provider store={mockStore}>
        <BrowserRouter>
          <Dashboard />
        </BrowserRouter>
      </Provider>
    );
    const metricsSection = screen.queryByText(/quality.*score|metrics/i);
    expect(metricsSection).toBeTruthy();
  });
});
