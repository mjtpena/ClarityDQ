import { useMsal } from '@azure/msal-react';
import { loginRequest, apiConfig } from '../authConfig';
import { getFabricContext } from '../fabricContext';

export const useApi = () => {
  const { instance, accounts } = useMsal();

  const getToken = async () => {
    // Check if running in Fabric context
    const fabricContext = getFabricContext();
    if (fabricContext) {
      return fabricContext.accessToken;
    }

    // Fallback to MSAL for standalone mode
    const request = {
      ...loginRequest,
      account: accounts[0],
    };
    const response = await instance.acquireTokenSilent(request);
    return response.accessToken;
  };

  const fetchWithAuth = async (url: string, options: RequestInit = {}) => {
    const token = await getToken();
    const response = await fetch(`${apiConfig.baseUrl}${url}`, {
      ...options,
      headers: {
        ...options.headers,
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`API error: ${response.statusText}`);
    }

    return response.json();
  };

  return { fetchWithAuth, getToken };
};
