# FE_OLD_REPO_UI_STYLE_REFERENCE

**Task:** FE-STYLE-ALIGNMENT-FROM-OLD-REPO
**Date:** 2026-05-08
**Status:** REFERENCE DOCUMENT

## Executive Summary

This document extracts UI/style patterns from the old PBL3 repo (Blazor frontend) to guide styling improvements for the new React frontend. The old repo uses a modern, clean design system with CSS variables, Inter font, and consistent component patterns.

## Old Repo Files Inspected

### Layout Files
- `D:\PBL\PBL3\src\Org.Frontend\Components\Layout\MainLayout.razor` - Main layout structure
- `D:\PBL\PBL3\src\Org.Frontend\Components\Layout\MainLayout.razor.css` - Layout CSS
- `D:\PBL\PBL3\src\Org.Frontend\Components\Layout\NavMenu.razor` - Sidebar navigation
- `D:\PBL\PBL3\src\Org.Frontend\Components\Layout\NavMenu.razor.css` - Sidebar CSS

### Global Styles
- `D:\PBL\PBL3\src\Org.Frontend\wwwroot\app.css` - Global CSS variables and component styles (788 lines)

### Auth Layout
- `D:\PBL\PBL3\src\Org.Frontend\Components\Layout\AuthLayout.razor` - Auth layout
- `D:\PBL\PBL3\src\Org.Frontend\Components\Layout\AuthLayout.razor.css` - Auth CSS

## Color Palette (CSS Variables)

```css
:root {
    --bg-page: #ffffff;
    --bg-surface: #EAEFEF;
    --bg-soft: #eaeff2;
    --bg-muted: #f5f8fb;
    --surface-card: #ffffff;
    --surface-subtle: #f8fbff;
    --ink-900: #25343F;
    --ink-700: #54626C;
    --ink-600: #687887;
    --ink-500: #89969F;
    --accent-500: #FF9B51;
    --accent-700: #954900;
    --brand-500: #1f4f7a;
    --brand-700: #102f49;
    --success-500: #168557;
    --warning-500: #b7791f;
    --danger-500: #b42318;
    --info-500: #2563eb;
    --border-soft: #d7e2ec;
    --shadow-soft: 0 14px 34px rgba(15, 31, 41, 0.08);
    --shadow-subtle: 0 8px 20px rgba(15, 31, 41, 0.06);
    --radius-card: 8px;
    --radius-control: 7px;
}
```

## Typography

**Font Family:** Inter, 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif

**Font Weights Used:**
- 400 (regular)
- 500 (medium)
- 700 (bold)
- 800 (extra bold)
- 850 (ultra bold)
- 900 (black)

**Font Sizes:**
- Page titles: clamp(1.45rem, 2vw, 1.9rem)
- Section titles: 1.03rem
- Body text: 0.86rem - 0.98rem
- Labels: 0.72rem - 0.82rem
- Small text: 0.7rem - 0.8rem

## Layout Patterns

### Two-Column App Layout
```
┌─────────────────────────────────────────┐
│  Sidebar (284px)  │  Main Content       │
│                   │                     │
│  - Dark Rail (64px)│  - TopBar (62px)   │
│  - Menu Pane (220px)│                    │
│                   │  - Page Content     │
│                   │                     │
└─────────────────────────────────────────┘
```

**Sidebar Structure:**
- Fixed position: 284px width, 100vh height
- Dark rail (64px): #0f1f29 background, user icon, org icons, logout
- Menu pane (220px): #25343f background, navigation items
- Active state: Orange accent (#ff9b51) left border

**TopBar Structure:**
- Sticky position, 62px min-height
- Background: rgba(255, 255, 255, 0.86) with backdrop-filter blur
- Left: Workspace title or spacer
- Right: User profile with avatar, dropdown menu

**Main Content:**
- Margin-left: 284px (sidebar width)
- Padding: 0.9rem 1rem 1.5rem (user workspace)
- Padding: 0 (org workspace - handled per page)

## Component Patterns

### Page Header Pattern
```html
<div class="app-page-header">
  <div>
    <div class="app-kicker">Kicker text</div>
    <h1 class="app-page-title">Page Title</h1>
    <p class="app-page-subtitle">Subtitle text</p>
  </div>
  <div class="app-action-row">
    <!-- Action buttons -->
  </div>
</div>
```

**CSS:**
- `.app-page-header`: flex, space-between, align-items flex-end
- `.app-kicker`: uppercase, letter-spacing 0.06em, font-size 0.72rem, accent color
- `.app-page-title`: font-weight 850, clamp size, ink-900
- `.app-page-subtitle`: font-size 0.92rem, ink-600, margin-top 0.42rem

### Card Pattern
```css
.app-card {
    background: var(--surface-card);
    border: 1px solid var(--border-soft);
    border-radius: var(--radius-card);
    box-shadow: var(--shadow-subtle);
}

.app-card--interactive {
    cursor: pointer;
    transition: border-color 0.16s ease, box-shadow 0.16s ease, transform 0.16s ease;
}

.app-card--interactive:hover {
    border-color: rgba(255, 155, 81, 0.7);
    box-shadow: var(--shadow-soft);
    transform: translateY(-1px);
}
```

### Button Pattern
```css
.app-button {
    min-height: 2rem;
    border-radius: var(--radius-control);
    border: 1px solid var(--border-soft);
    padding: 0.42rem 0.7rem;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 0.38rem;
    background: var(--surface-card);
    color: var(--ink-900);
    font-size: 0.8rem;
    font-weight: 750;
    cursor: pointer;
    transition: background-color 0.16s ease, border-color 0.16s ease, color 0.16s ease;
}

.app-button--primary {
    border-color: var(--brand-700);
    background: var(--brand-700);
    color: #ffffff;
}

.app-button--secondary {
    border-color: rgba(255, 155, 81, 0.55);
    background: #fff7ed;
    color: #8a3f00;
}

.app-button--ghost {
    background: var(--surface-card);
    color: var(--ink-700);
}

.app-button--danger {
    border-color: #f1c4bd;
    background: #fff3f1;
    color: var(--danger-500);
}
```

### Badge/Chip Pattern
```css
.app-badge, .app-chip {
    display: inline-flex;
    align-items: center;
    width: fit-content;
    border-radius: 999px;
    border: 1px solid var(--border-soft);
    background: var(--surface-subtle);
    color: var(--ink-700);
    font-size: 0.72rem;
    font-weight: 750;
}

.app-badge--success {
    border-color: #bbf1d5;
    background: #ecfdf5;
    color: #087443;
}

.app-badge--info {
    border-color: #c6ddff;
    background: #eff6ff;
    color: #1d4ed8;
}

.app-badge--warning {
    border-color: #f8ddb0;
    background: #fff8eb;
    color: #9a5a00;
}
```

### Form Pattern (Auth)
```css
.auth-form-shell {
    width: min(31.5rem, 100%);
    background: var(--bg-page);
    border: 1px solid #e6ebf3;
    border-radius: 0.5rem;
    box-shadow: 0 18px 45px rgba(19, 36, 61, 0.09);
    padding: 1.5rem;
}

.auth-input {
    width: 100%;
    border-radius: 0.5rem;
    border: 2px solid transparent;
    background: var(--bg-surface);
    min-height: 2.95rem;
    padding: 0.68rem 0.88rem;
    color: var(--ink-900);
    font-size: 0.97rem;
    transition: all 0.2s ease;
}

.auth-input:focus {
    outline: none;
    border-color: var(--accent-500);
    background: #ffffff;
}

.auth-primary-btn {
    width: 100%;
    border: none;
    border-radius: 0.5rem;
    min-height: 2.95rem;
    font-weight: 700;
    font-size: 0.98rem;
    color: #ffffff;
    background: var(--accent-500);
    box-shadow: 0 4px 12px rgba(255, 155, 81, 0.3);
    transition: all 0.2s ease;
}
```

### Table Pattern
```css
.tool-table thead th {
    color: #5e7284;
    font-size: 0.8rem;
    text-transform: uppercase;
    letter-spacing: 0.06em;
}
```

### State Components
```css
.app-state, .app-empty, .app-loading, .app-error {
    border: 1px solid var(--border-soft);
    border-radius: var(--radius-card);
    background: var(--surface-card);
    padding: 1rem;
    color: var(--ink-600);
    box-shadow: var(--shadow-subtle);
}

.app-error {
    border-color: #f2c3bd;
    background: #fff7f5;
    color: var(--danger-500);
}
```

## Navigation Patterns

### Sidebar Navigation
- Dark theme: #0f1f29 (rail), #25343f (menu pane)
- Active state: Orange accent (#ff9b51) left border + background highlight
- Hover: Slightly lighter background
- Icons: Material Icons (white/gray)
- Text: #94a3b8 (inactive), #ffffff (active/hover)

### TopBar
- Sticky, translucent with backdrop-filter blur
- User profile with avatar (round, 40px)
- Dropdown menu on hover
- Workspace title for org workspace

## What Should Be Reused Conceptually

### 1. Color System
- CSS variables for consistent theming
- Brand blue (#1f4f7a) for primary actions
- Accent orange (#FF9B51) for highlights/active states
- Semantic colors (success, warning, danger, info)

### 2. Typography
- Inter font family
- Consistent font weights and sizes
- Uppercase kickers/labels with letter-spacing
- Clear hierarchy (title, subtitle, body)

### 3. Layout Structure
- Two-column layout with fixed sidebar
- Sticky topbar with backdrop blur
- Card-based content sections
- Consistent padding and spacing

### 4. Component Styles
- Button variants (primary, secondary, ghost, danger)
- Badge/chip status indicators
- Card hover effects
- Form input styling with focus states
- State components (loading, error, empty, forbidden)

### 5. Navigation
- Dark sidebar with active state highlighting
- User profile dropdown
- Workspace context indication

## What Should NOT Be Copied

### 1. Blazor-Specific Code
- Razor component syntax
- MudBlazor components
- C# logic and state management
- Blazor lifecycle methods

### 2. Business Logic
- Organization loading logic
- Permission checks
- API integration patterns
- User authentication flow

### 3. Mock Data
- Hardcoded user avatars
- Fake organization lists
- Mock notification counts

### 4. Complex Animations
- Advanced transitions (keep simple)
- Complex hover effects (keep minimal)

## Mapping from Old UI Pattern to New React Pages

### Layouts
- **Old:** MainLayout.razor + NavMenu.razor
- **New:** AppLayout.jsx + Sidebar.jsx + TopBar.jsx
- **Apply:** Same CSS variables, similar layout structure, dark sidebar

### Auth Pages
- **Old:** AuthLayout.razor with auth-form-shell
- **New:** LoginPage.jsx, RegisterPage.jsx
- **Apply:** Auth form styling, input styling, button styling

### User Pages
- **Old:** User workspace pages with page headers
- **New:** UserOrganizationsPage.jsx, UserProfilePage.jsx, UserSettingsPage.jsx
- **Apply:** Page header pattern, card layout, table styling

### Org Pages
- **Old:** Organization workspace with sidebar navigation
- **New:** OrgOverviewPage.jsx, OrgMembersPage.jsx, OrgDepartmentsPage.jsx, OrgEventsPage.jsx, OrgRolesPage.jsx
- **Apply:** Page header pattern, card layout, table styling, badge components

### Components
- **Old:** MudBlazor components
- **New:** Custom React components
- **Apply:** Recreate similar visual style with React/CSS

## Implementation Approach

### Phase 1: Global Styles
1. Add CSS variables to index.css
2. Add Inter font import
3. Add global utility classes (app-page, app-card, app-button, etc.)

### Phase 2: Layout Styling
1. Style AppLayout with two-column structure
2. Style Sidebar with dark theme and active states
3. Style TopBar with sticky positioning and user profile

### Phase 3: Component Styling
1. Style shared components (PageHeader, StatusBadge, EmptyState, ErrorState, LoadingSpinner)
2. Add button variants
3. Add form styling

### Phase 4: Page Styling
1. Apply page header pattern to all pages
2. Apply card layout to content sections
3. Style tables consistently
4. Style forms consistently

### Phase 5: Navigation Updates
1. Hide prototype links from main sidebar
2. Add prototype placeholder message
3. Ensure active state highlighting works

## CSS Variables to Add to New Frontend

```css
:root {
    --bg-page: #ffffff;
    --bg-surface: #EAEFEF;
    --bg-soft: #eaeff2;
    --bg-muted: #f5f8fb;
    --surface-card: #ffffff;
    --surface-subtle: #f8fbff;
    --ink-900: #25343F;
    --ink-700: #54626C;
    --ink-600: #687887;
    --ink-500: #89969F;
    --accent-500: #FF9B51;
    --accent-700: #954900;
    --brand-500: #1f4f7a;
    --brand-700: #102f49;
    --success-500: #168557;
    --warning-500: #b7791f;
    --danger-500: #b42318;
    --info-500: #2563eb;
    --border-soft: #d7e2ec;
    --shadow-soft: 0 14px 34px rgba(15, 31, 41, 0.08);
    --shadow-subtle: 0 8px 20px rgba(15, 31, 41, 0.06);
    --radius-card: 8px;
    --radius-control: 7px;
}
```

## Font Import to Add

```css
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;700;800;900&display=swap');
```

## Summary

The old repo has a well-designed, modern UI system with:
- Consistent color palette using CSS variables
- Inter font family with clear typography hierarchy
- Two-column layout with dark sidebar
- Card-based content sections
- Polished component styles (buttons, badges, forms)
- Clear navigation patterns

The new React frontend should adopt these visual patterns while maintaining its own React architecture and real backend API integration. No business logic or Blazor-specific code should be copied.

---

**End of FE_OLD_REPO_UI_STYLE_REFERENCE.md**
