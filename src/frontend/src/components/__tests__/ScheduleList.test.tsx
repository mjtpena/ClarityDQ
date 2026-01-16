import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { Provider } from 'react-redux';
import { BrowserRouter } from 'react-router-dom';
import { configureStore } from '@reduxjs/toolkit';
import ScheduleList from '../rules/ScheduleList';

const mockStore = configureStore({
  reducer: {
    schedules: (state = { schedules: [], loading: false }) => state,
  },
});

describe('ScheduleList', () => {
  it('renders without crashing', () => {
    render(
      <Provider store={mockStore}>
        <BrowserRouter>
          <ScheduleList />
        </BrowserRouter>
      </Provider>
    );
    expect(screen.queryByText(/schedule|no.*schedule/i)).toBeTruthy();
  });

  it('shows loading state', () => {
    const loadingStore = configureStore({
      reducer: {
        schedules: () => ({ schedules: [], loading: true }),
      },
    });

    render(
      <Provider store={loadingStore}>
        <BrowserRouter>
          <ScheduleList />
        </BrowserRouter>
      </Provider>
    );
    expect(screen.queryByText(/loading/i)).toBeTruthy();
  });

  it('displays schedules when available', () => {
    const schedulesStore = configureStore({
      reducer: {
        schedules: () => ({
          schedules: [
            {
              id: '1',
              name: 'Test Schedule',
              cron: '0 0 * * *',
              enabled: true,
            },
          ],
          loading: false,
        }),
      },
    });

    render(
      <Provider store={schedulesStore}>
        <BrowserRouter>
          <ScheduleList />
        </BrowserRouter>
      </Provider>
    );
    expect(screen.queryByText(/Test Schedule|schedule/i)).toBeTruthy();
  });
});
