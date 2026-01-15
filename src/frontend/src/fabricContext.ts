interface FabricContext {
  accessToken: string;
  workspaceId: string;
  userId: string;
  tenantId: string;
}

// This will be provided by Fabric when embedded
declare global {
  interface Window {
    fabricContext?: FabricContext;
  }
}

export const getFabricContext = (): FabricContext | null => {
  // In Fabric environment, context is provided via postMessage or global
  if (window.fabricContext) {
    return window.fabricContext;
  }

  // For local dev, fall back to env vars
  if (import.meta.env.DEV) {
    return {
      accessToken: 'dev-token',
      workspaceId: import.meta.env.VITE_WORKSPACE_ID || 'local-workspace',
      userId: 'dev-user',
      tenantId: 'dev-tenant',
    };
  }

  return null;
};

// Listen for Fabric context from parent frame
export const initializeFabricContext = (callback: (ctx: FabricContext) => void) => {
  window.addEventListener('message', (event) => {
    if (event.data.type === 'FABRIC_CONTEXT') {
      window.fabricContext = event.data.context;
      callback(event.data.context);
    }
  });

  // Request context from parent
  window.parent.postMessage({ type: 'REQUEST_FABRIC_CONTEXT' }, '*');
};
