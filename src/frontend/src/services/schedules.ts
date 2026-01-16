import { Schedule, ScheduleExecution, CreateScheduleRequest } from '../types/schedule';
import { useApi } from './api';

export const useScheduleService = () => {
  const { fetchWithAuth } = useApi();

  const createSchedule = async (request: CreateScheduleRequest): Promise<Schedule> => {
    return await fetchWithAuth('/api/schedules', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  };

  const getSchedules = async (enabledOnly?: boolean): Promise<Schedule[]> => {
    const params = enabledOnly !== undefined ? `?enabledOnly=${enabledOnly}` : '';
    return await fetchWithAuth(`/api/schedules${params}`);
  };

  const deleteSchedule = async (id: string): Promise<void> => {
    await fetchWithAuth(`/api/schedules/${id}`, { method: 'DELETE' });
  };

  const executeSchedule = async (id: string): Promise<ScheduleExecution> => {
    return await fetchWithAuth(`/api/schedules/${id}/execute`, { method: 'POST' });
  };

  return { createSchedule, getSchedules, deleteSchedule, executeSchedule };
};
