/**
 * PublicLayout.jsx - Layout for public pages
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * TODO Phase 3C-4B/4C Implementation:
 * - Add public navigation header
 * - Add footer if needed
 * - Style for public pages (home, events, login, register)
 * 
 * Usage:
 *   <Route element={<PublicLayout />}>
 *     <Route path="/" element={<HomePage />} />
 *     <Route path="/login" element={<LoginPage />} />
 *   </Route>
 */

import { Outlet } from 'react-router-dom';

function PublicLayout() {
  return (
    <div className="public-layout">
      {/* TODO Phase 3C-4B/4C: Add public nav header */}
      <header className="public-header">
        <div className="container">
          <h1>Student Organization Manager</h1>
          {/* TODO: Add navigation links */}
        </div>
      </header>

      <main className="public-main">
        <Outlet />
      </main>

      {/* TODO Phase 3C-4B/4C: Add footer if needed */}
      <footer className="public-footer">
        <div className="container">
          <p>&copy; 2026 Student Organization Manager</p>
        </div>
      </footer>
    </div>
  );
}

export default PublicLayout;
