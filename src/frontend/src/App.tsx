import { AuthenticatedTemplate, UnauthenticatedTemplate, useMsal } from '@azure/msal-react';
import { Button, FluentProvider, webLightTheme } from '@fluentui/react-components';
import { loginRequest } from './authConfig';
import Dashboard from './pages/Dashboard';

function App() {
  const { instance } = useMsal();

  const handleLogin = () => {
    instance.loginPopup(loginRequest).catch((e) => {
      console.error(e);
    });
  };

  return (
    <FluentProvider theme={webLightTheme}>
      <AuthenticatedTemplate>
        <Dashboard />
      </AuthenticatedTemplate>
      <UnauthenticatedTemplate>
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: '100vh' }}>
          <h1>ClarityDQ</h1>
          <p>Data Quality Management for Microsoft Fabric</p>
          <Button appearance="primary" onClick={handleLogin}>
            Sign in with Microsoft
          </Button>
        </div>
      </UnauthenticatedTemplate>
    </FluentProvider>
  );
}

export default App;
