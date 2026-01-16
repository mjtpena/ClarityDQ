export const msalConfig = {
  auth: {
    clientId: process.env.VITE_AZURE_CLIENT_ID || "test-client-id",
    authority: `https://login.microsoftonline.com/${process.env.VITE_AZURE_TENANT_ID || "common"}`,
    redirectUri: "/",
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false,
  },
};

export const loginRequest = {
  scopes: ["User.Read"],
};

export const apiConfig = {
  uri: process.env.VITE_API_URL || "http://localhost:5000/api",
};
