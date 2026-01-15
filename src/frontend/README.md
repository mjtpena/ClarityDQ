# ClarityDQ Frontend

React-based frontend application for ClarityDQ (Fabric Quality Guard).

## Technology Stack

- React 18+ with TypeScript
- Fluent UI React v9 (Microsoft Design System)
- Redux Toolkit for state management
- React Query for API data fetching
- D3.js/Cytoscape.js for lineage visualization
- Recharts for quality metrics visualization
- Vite for build tooling

## Key Features

### Quality Dashboard
- Real-time quality score cards
- Trend charts for quality metrics
- Failed rules summary
- Alert center
- Quick actions

### Rule Builder
- Visual rule designer with drag-drop
- Rule template library
- SQL/Python expression editor with IntelliSense
- Test rule functionality
- Schedule configuration

### Lineage Viewer
- Interactive graph visualization
- Zoom, pan, and filter controls
- Node details on hover/click
- Path highlighting
- Impact analysis mode

### Data Catalog
- Searchable table/column browser
- Metadata editor
- Tag management
- Business glossary integration

### Governance Console
- Policy management
- Compliance dashboards
- Audit logs
- User access controls

## Getting Started

### Prerequisites
- Node.js 18+
- npm 9+

### Installation

```bash
cd src/frontend
npm install
```

### Development

```bash
npm run dev
```

### Build

```bash
npm run build
```

### Test

```bash
npm run test
```

## Project Structure

```
frontend/
├── public/              # Static assets
├── src/
│   ├── components/      # React components
│   ├── services/        # API clients
│   ├── hooks/           # Custom hooks
│   ├── utils/           # Utilities
│   ├── store/           # Redux store
│   └── types/           # TypeScript types
├── package.json
└── vite.config.ts
```

## Component Development

Components follow Fluent UI design patterns and are organized by feature area.

### Example Component Structure

```typescript
// components/quality/QualityScoreCard.tsx
import { Card } from '@fluentui/react-components';
import { useQualityScore } from '../../hooks/useQualityScore';

export const QualityScoreCard = ({ tableId }: { tableId: string }) => {
  const { score, loading } = useQualityScore(tableId);
  
  return (
    <Card>
      {/* Component implementation */}
    </Card>
  );
};
```

## API Integration

API calls are centralized in the `services/` directory:

```typescript
// services/qualityApi.ts
export const qualityApi = {
  createRule: async (rule: QualityRule) => {
    // Implementation
  },
  executeRule: async (ruleId: string) => {
    // Implementation
  }
};
```

## State Management

Redux Toolkit is used for global state:

```typescript
// store/slices/qualitySlice.ts
export const qualitySlice = createSlice({
  name: 'quality',
  initialState,
  reducers: {
    // Reducers
  }
});
```

## Styling

Use Fluent UI tokens for consistent theming:

```typescript
import { tokens } from '@fluentui/react-components';

const styles = {
  container: {
    backgroundColor: tokens.colorNeutralBackground1,
    padding: tokens.spacingVerticalM
  }
};
```

## Testing

- Unit tests with Jest and React Testing Library
- Component tests for UI components
- Hook tests for custom hooks
- Integration tests for API services

## Contributing

See [CONTRIBUTING.md](../../CONTRIBUTING.md) for guidelines.
