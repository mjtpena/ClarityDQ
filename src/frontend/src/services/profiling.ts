import { DataProfile, ProfileRequest } from '../types/profile';
import { useApi } from './api';

export const useProfilingService = () => {
  const { fetchWithAuth } = useApi();

  const createProfile = async (request: ProfileRequest): Promise<string> => {
    return await fetchWithAuth('/api/profiling', {
      method: 'POST',
      body: JSON.stringify(request),
    });
  };

  const getProfile = async (id: string): Promise<DataProfile> => {
    return await fetchWithAuth(`/api/profiling/${id}`);
  };

  const getProfiles = async (workspaceId: string, skip = 0, take = 50): Promise<DataProfile[]> => {
    return await fetchWithAuth(`/api/profiling/workspace/${workspaceId}?skip=${skip}&take=${take}`);
  };

  return { createProfile, getProfile, getProfiles };
};
