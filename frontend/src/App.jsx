/**
 * App.jsx - Main application component
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * Wraps the application with context providers and router.
 */

import { AuthProvider } from './contexts/AuthContext';
import { OrgProvider } from './contexts/OrgContext';
import AppRouter from './router/AppRouter';

function App() {
  return (
    <AuthProvider>
      <OrgProvider>
        <AppRouter />
      </OrgProvider>
    </AuthProvider>
  );
}

export default App;

