import { useState, useEffect } from 'react';
import { useMsal } from '@azure/msal-react';
import { Button, Input, Card, Text, TabList, Tab } from '@fluentui/react-components';
import { useProfilingService } from '../services/profiling';
import { DataProfile } from '../types/profile';
import { RulesPage } from './rules/RulesPage';

function Dashboard() {
  const { accounts, instance } = useMsal();
  const [workspaceId, setWorkspaceId] = useState('workspace-001');
  const [datasetName, setDatasetName] = useState('');
  const [tableName, setTableName] = useState('');
  const [profiles, setProfiles] = useState<DataProfile[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedTab, setSelectedTab] = useState<string>('profiling');

  const { createProfile, getProfiles } = useProfilingService();

  const loadProfiles = async () => {
    try {
      const data = await getProfiles(workspaceId);
      setProfiles(data);
    } catch (error) {
      console.error('Failed to load profiles:', error);
    }
  };

  useEffect(() => {
    if (selectedTab === 'profiling') {
      loadProfiles();
    }
  }, [workspaceId, selectedTab]);

  const handleProfile = async () => {
    if (!datasetName || !tableName) return;

    setLoading(true);
    try {
      await createProfile({ workspaceId, datasetName, tableName });
      await loadProfiles();
      setDatasetName('');
      setTableName('');
    } catch (error) {
      console.error('Failed to create profile:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    instance.logoutPopup();
  };

  return (
    <div style={{ padding: '20px', maxWidth: '1200px', margin: '0 auto' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '20px' }}>
        <div>
          <h1>ClarityDQ Dashboard</h1>
          <Text>Welcome, {accounts[0]?.name}</Text>
        </div>
        <Button onClick={handleLogout}>Sign Out</Button>
      </div>

      <div style={{ marginBottom: '20px' }}>
        <Input 
          placeholder="Workspace ID" 
          value={workspaceId}
          onChange={(e) => setWorkspaceId(e.target.value)}
          style={{ maxWidth: '300px' }}
        />
      </div>

      <TabList selectedValue={selectedTab} onTabSelect={(_, data) => setSelectedTab(data.value as string)}>
        <Tab value="profiling">Data Profiling</Tab>
        <Tab value="rules">Quality Rules</Tab>
      </TabList>

      <div style={{ marginTop: '24px' }}>
        {selectedTab === 'profiling' && (
          <>
            <Card style={{ padding: '20px', marginBottom: '20px' }}>
              <h2>Create Data Profile</h2>
              <div style={{ display: 'flex', gap: '10px', marginTop: '10px' }}>
                <Input 
                  placeholder="Dataset Name" 
                  value={datasetName}
                  onChange={(e) => setDatasetName(e.target.value)}
                />
                <Input 
                  placeholder="Table Name" 
                  value={tableName}
                  onChange={(e) => setTableName(e.target.value)}
                />
                <Button 
                  appearance="primary" 
                  onClick={handleProfile}
                  disabled={loading || !datasetName || !tableName}
                >
                  {loading ? 'Profiling...' : 'Profile Table'}
                </Button>
              </div>
            </Card>

            <h2>Recent Profiles</h2>
            <div style={{ display: 'grid', gap: '10px' }}>
              {profiles.map((profile) => (
                <Card key={profile.id} style={{ padding: '15px' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <div>
                      <Text weight="bold">{profile.datasetName}.{profile.tableName}</Text>
                      <Text size={200}> - {profile.rowCount.toLocaleString()} rows</Text>
                    </div>
                    <Text size={200}>{new Date(profile.profiledAt).toLocaleString()}</Text>
                  </div>
                </Card>
              ))}
            </div>
          </>
        )}
        
        {selectedTab === 'rules' && <RulesPage workspaceId={workspaceId} />}
      </div>
    </div>
  );
}

export default Dashboard;
