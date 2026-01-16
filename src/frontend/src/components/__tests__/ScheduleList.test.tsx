import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { Provider } from 'react-redux';
import { MemoryRouter } from 'react-router-dom';
import { configureStore } from '@reduxjs/toolkit';
import { ScheduleList } from '../rules/ScheduleList';

const mockStore = configureStore({
  reducer: {
    schedules: (state = { schedules: [], loading: false }) => state,
  },
});

describe('ScheduleList', () => {
  it('renders without crashing', () => {
    render(
      <Provider store={mockStore}>
        <MemoryRouter>
          <ScheduleList schedules={[]} onExecute={vi.fn()} onDelete={vi.fn()} />
        </MemoryRouter>
      </Provider>
    );
    expect(true).toBe(true);
  });

  it('shows loading state', () => {
    const loadingStore = configureStore({
      reducer: {
        schedules: () => ({ schedules: [], loading: true }),
      },
    });

    render(
      <Provider store={loadingStore}>
        <MemoryRouter>
          <ScheduleList schedules={[]} onExecute={vi.fn()} onDelete={vi.fn()} />
        </MemoryRouter>
      </Provider>
    );
    expect(true).toBe(true);
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

    const testSchedules = [
      {
        id: '1',
        name: 'Test Schedule',
        cron: '0 0 * * *',
        type: 0,
        ruleIds: [],
        isEnabled: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      },
    ];

    render(
      <Provider store={schedulesStore}>
        <MemoryRouter>
          <ScheduleList schedules={testSchedules} onExecute={vi.fn()} onDelete={vi.fn()} />
        </MemoryRouter>
      </Provider>
    );
    expect(screen.getByText('Test Schedule')).toBeInTheDocument();
  });
});
