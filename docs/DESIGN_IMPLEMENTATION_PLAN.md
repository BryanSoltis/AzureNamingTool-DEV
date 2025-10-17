# Design Implementation Plan - DesignLinear Modernization

## Overview
This document outlines the plan to implement the new DesignLinear design across the entire Azure Naming Tool application. The new design features a dark gray sidebar with white text, Azure-inspired color accents (blue, teal, cyan, purple), and modern minimal styling.

## Current State
- **Current Design**: Bootstrap-based with traditional components
- **New Design**: DesignLinear mockup (dark gray sidebar, Azure colors, modern minimal)
- **Mockup Location**: `src/Components/Pages/DesignMockups/DesignLinear.razor`
- **Status**: Design approved, ready for implementation

## Design System - Color Palette

### Sidebar Colors
- **Background**: `linear-gradient(180deg, #2d3748 0%, #1a202c 100%)`
- **Text**: `rgba(255, 255, 255, 0.85)` (nav items), `#ffffff` (active/hover)
- **Borders**: `rgba(255, 255, 255, 0.1)`
- **Hover**: `rgba(255, 255, 255, 0.1)` background
- **Active**: `rgba(255, 255, 255, 0.15)` background

### Main Content Colors
- **Background**: `#fafafa` (body), `#ffffff` (cards/panels)
- **Borders**: `#e5e5e5`
- **Text Primary**: `#171717`
- **Text Secondary**: `#737373`

### Accent Colors (Azure-inspired)
- **Primary Blue** (buttons, main actions): `#0078d4` → `#005a9e`
- **Teal** (stats, data values): `#00b7c3`
- **Bright Cyan** (section titles): `#50e6ff`
- **Purple** (activity icons, variety): `#8661c5`
- **Green** (success indicators): `#059669` (keep existing)

### Component Styling
- **Border Radius**: `6px` (inputs), `8px` (cards)
- **Shadows**: Subtle on hover, `0 2px 8px` for buttons
- **Transitions**: `0.15s ease` for all interactions
- **Typography**: `-apple-system, BlinkMacSystemFont, "Segoe UI", "Inter", "Roboto", "Helvetica Neue", Arial, sans-serif`

## Light/Dark Mode Theme System

### Overview
The application will support both Light and Dark modes with a user-controlled toggle. The theme preference will be persisted in browser localStorage and respected across sessions. The design system already includes a dark sidebar, so dark mode will primarily affect the main content area.

### Theme Colors

#### Light Mode (Default)
- **Background**: `#fafafa` (body), `#ffffff` (cards/panels)
- **Borders**: `#e5e5e5`
- **Text Primary**: `#171717`
- **Text Secondary**: `#737373`
- **Sidebar**: Dark gray gradient (remains same)
- **Stat Card Hover**: `#f0fafb` (light teal tint)
- **Activity Item Hover**: `#fafafa`

#### Dark Mode
- **Background**: `#0d1117` (body), `#161b22` (cards/panels)
- **Borders**: `#30363d`
- **Text Primary**: `#e6edf3`
- **Text Secondary**: `#8b949e`
- **Sidebar**: Dark gray gradient (remains same)
- **Stat Card Hover**: `#1c2128` (darker on hover)
- **Activity Item Hover**: `#1c2128`

### Implementation Strategy

#### 1. CSS Variables Approach
Use CSS custom properties (variables) for all theme-dependent colors. Toggle theme by changing `data-theme` attribute on `<html>` or `<body>` element.

**CSS Structure**:
```css
/* Default Light Theme */
:root {
    --color-bg-primary: #fafafa;
    --color-bg-secondary: #ffffff;
    --color-border: #e5e5e5;
    --color-text-primary: #171717;
    --color-text-secondary: #737373;
    --color-hover-bg: #fafafa;
    --color-card-hover-bg: #f0fafb;
    
    /* Accent colors remain consistent */
    --color-azure-blue: #0078d4;
    --color-azure-teal: #00b7c3;
    --color-cyan-bright: #50e6ff;
    --color-purple: #8661c5;
    --color-green: #059669;
}

/* Dark Theme */
[data-theme="dark"] {
    --color-bg-primary: #0d1117;
    --color-bg-secondary: #161b22;
    --color-border: #30363d;
    --color-text-primary: #e6edf3;
    --color-text-secondary: #8b949e;
    --color-hover-bg: #1c2128;
    --color-card-hover-bg: #1c2128;
}
```

#### 2. Theme Toggle Component
**File**: `src/Components/General/ThemeToggle.razor`

**Features**:
- Toggle button in header (sun/moon icons)
- Smooth transition between themes
- Persist preference in localStorage
- Initialize theme on app load

**Component Structure**:
```razor
<button class="theme-toggle-btn" @onclick="ToggleTheme">
    @if (currentTheme == "light")
    {
        <span class="theme-icon">🌙</span>
        <span class="theme-label">Dark Mode</span>
    }
    else
    {
        <span class="theme-icon">☀️</span>
        <span class="theme-label">Light Mode</span>
    }
</button>

@code {
    private string currentTheme = "light";
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            currentTheme = await JSRuntime.InvokeAsync<string>("getTheme");
            StateHasChanged();
        }
    }
    
    private async Task ToggleTheme()
    {
        currentTheme = currentTheme == "light" ? "dark" : "light";
        await JSRuntime.InvokeVoidAsync("setTheme", currentTheme);
    }
}
```

#### 3. JavaScript Theme Helper
**File**: `src/wwwroot/js/theme.js`

```javascript
// Initialize theme on page load
function initTheme() {
    const savedTheme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', savedTheme);
    return savedTheme;
}

// Get current theme
function getTheme() {
    return localStorage.getItem('theme') || 'light';
}

// Set theme
function setTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
    
    // Add transition class temporarily for smooth transition
    document.documentElement.classList.add('theme-transitioning');
    setTimeout(() => {
        document.documentElement.classList.remove('theme-transitioning');
    }, 300);
}

// Initialize on load
initTheme();
```

#### 4. Theme Transition CSS
**File**: `src/wwwroot/css/modern-design.css`

```css
/* Smooth theme transitions */
html.theme-transitioning,
html.theme-transitioning *,
html.theme-transitioning *:before,
html.theme-transitioning *:after {
    transition: background-color 0.3s ease, 
                color 0.3s ease, 
                border-color 0.3s ease !important;
    transition-delay: 0s !important;
}
```

### Phase Implementation

#### Phase 1.4: Theme System Foundation (New - Est. 3-4 hours)
**Goal**: Implement CSS variable system and theme infrastructure

**Tasks**:
- [ ] Create CSS variables for all theme-dependent colors
- [ ] Add `data-theme` attribute support in CSS
- [ ] Create `theme.js` file with localStorage management
- [ ] Add theme transition CSS classes
- [ ] Test theme switching works correctly
- [ ] Verify all colors properly use CSS variables

#### Phase 5.5: Theme Toggle Component (New - Est. 2-3 hours)
**Goal**: Create and integrate theme toggle UI component

**Tasks**:
- [ ] Create `ThemeToggle.razor` component
- [ ] Add toggle button styling (consistent with design system)
- [ ] Integrate component into main header
- [ ] Add keyboard accessibility (Space/Enter to toggle)
- [ ] Add screen reader labels (aria-label)
- [ ] Test theme toggle in all pages
- [ ] Test theme persistence across page navigation
- [ ] Test theme initialization on app load

### Considerations

#### Sidebar Theme Behavior
**Decision**: Keep sidebar dark in both modes
- The dark gray gradient sidebar remains constant in both themes
- This provides consistent navigation experience
- Dark sidebars are common in professional tools
- Easier to implement (less CSS to maintain)

**Alternative**: Make sidebar theme-aware
- Light mode: Light gray sidebar
- Dark mode: Dark gray sidebar (current)
- Requires additional CSS variables for sidebar
- Adds complexity but provides full theme consistency

#### Admin-Only Theme Settings
**Option**: Add admin preference for default theme
- Admin can set organization-wide default (light/dark)
- Users can still override with toggle
- Stored in configuration settings
- **Recommendation**: Implement in Phase 8 (post-launch enhancement)

#### Respect System Preference
**Option**: Auto-detect system dark/light mode
- Use `prefers-color-scheme` media query
- Check on first visit before localStorage exists
- **Implementation**:
```javascript
function getSystemTheme() {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

function initTheme() {
    const savedTheme = localStorage.getItem('theme');
    const theme = savedTheme || getSystemTheme();
    document.documentElement.setAttribute('data-theme', theme);
    return theme;
}
```

### Testing Requirements

#### Visual Testing
- [ ] Test all pages in light mode (current design)
- [ ] Test all pages in dark mode (new implementation)
- [ ] Test theme toggle button functionality
- [ ] Test theme transitions are smooth (no flash)
- [ ] Verify stat card values still use Azure teal in both modes
- [ ] Verify buttons use Azure blue gradient in both modes
- [ ] Verify activity icons use purple gradient in both modes

#### Functional Testing
- [ ] Test theme persists across page navigation
- [ ] Test theme persists across browser refresh
- [ ] Test theme persists across browser close/reopen
- [ ] Test theme toggle works in all major browsers (Chrome, Edge, Firefox, Safari)
- [ ] Test localStorage fallback if disabled
- [ ] Test theme initialization on first visit

#### Accessibility Testing
- [ ] Test theme toggle with keyboard (Tab, Enter, Space)
- [ ] Test theme toggle with screen reader
- [ ] Verify color contrast meets WCAG AA in both themes
  - Light mode: Dark text on light backgrounds
  - Dark mode: Light text on dark backgrounds
- [ ] Test focus indicators visible in both themes

### Updated Timeline

**New Time Estimates**:
- Phase 1.4: Theme System Foundation: **3-4 hours**
- Phase 5.5: Theme Toggle Component: **2-3 hours**
- **Additional Testing Time**: **1-2 hours**

**Total Additional Time**: **6-9 hours**

**Original Estimate**: 33-46 hours  
**New Estimate with Light/Dark Mode**: **39-55 hours**

---

## Implementation Phases

### Phase 1: Foundation Setup (Est. 7-10 hours - Updated)
**Goal**: Create shared CSS foundation, update layout structure, and implement theme system

#### 1.1: Create Global CSS File
- **File**: `src/wwwroot/css/modern-design.css`
- **Content**:
  - CSS variables for all colors
  - Typography system
  - Spacing utilities
  - Common component styles
  - Animation/transition definitions
- **Action Items**:
  - [ ] Create new CSS file with design system variables
  - [ ] Add CSS file reference to `App.razor` or `_Imports.razor`
  - [ ] Test CSS loading in development environment

#### 1.2: Update Main Layout Structure
- **Files**: 
  - `src/Components/Layout/MainLayout.razor`
  - `src/Components/Layout/NavMenu.razor`
- **Changes**:
  - Replace existing layout structure with DesignLinear layout
  - Implement dark gray sidebar navigation
  - Add collapsible sidebar functionality
  - Update logo/header section
- **Action Items**:
  - [ ] Backup existing `MainLayout.razor`
  - [ ] Implement new sidebar structure from DesignLinear
  - [ ] Add JavaScript for sidebar collapse toggle
  - [ ] Update navigation menu items with new styling
  - [ ] Test responsive behavior (mobile/tablet/desktop)

#### 1.3: Update Navigation Menu
- **File**: `src/Components/Layout/NavMenu.razor`
- **Changes**:
  - Apply dark gray background gradient
  - White text with transparency
  - Add hover/active states
  - Update section dividers
  - Add section labels (Main, Components, System)
- **Action Items**:
  - [ ] Update navigation item structure
  - [ ] Apply new color scheme
  - [ ] Add icons to navigation items (if not present)
  - [ ] Test navigation highlighting for active pages
  - [ ] Verify collapse/expand functionality

---

### Phase 2: Component Library Updates (Est. 6-8 hours)
**Goal**: Modernize all reusable UI components

#### 2.1: Update Card Components
- **Files**: All pages using card layouts
- **Changes**:
  - Replace Bootstrap cards with new card styling
  - Apply border: `1px solid #e5e5e5`
  - Border radius: `8px`
  - Hover effects with Azure blue borders
  - Remove Bootstrap classes (`card`, `card-body`, etc.)
- **Action Items**:
  - [ ] Identify all card usages across application
  - [ ] Create reusable card component (optional)
  - [ ] Update card styling site-wide
  - [ ] Test card hover effects

#### 2.2: Update Button Components
- **Changes**:
  - **Primary Buttons**: Azure blue gradient (`#0078d4` → `#005a9e`)
  - **Secondary Buttons**: White with gray border
  - **Danger Buttons**: Red (keep existing, adjust to match style)
  - Remove Bootstrap button classes
  - Add consistent padding: `10px 20px`
  - Add hover lift effect: `translateY(-1px)`
  - Add shadow: `0 2px 8px rgba(0, 120, 212, 0.2)`
- **Action Items**:
  - [ ] Audit all button usages
  - [ ] Create button CSS classes (`.btn-primary-modern`, `.btn-secondary-modern`)
  - [ ] Replace Bootstrap button classes
  - [ ] Test button states (normal, hover, active, disabled)

#### 2.3: Update Form Components
- **Elements**: Inputs, Selects, Textareas, Checkboxes, Radio buttons
- **Changes**:
  - Remove Bootstrap form classes
  - Border: `1px solid #e5e5e5`
  - Border radius: `6px`
  - Focus state: Azure blue border (`#0078d4`)
  - Background: `#fafafa` (unfocused), `#ffffff` (focused)
  - Padding: `8px 12px`
- **Action Items**:
  - [ ] Create form input CSS classes
  - [ ] Update all input fields
  - [ ] Update select dropdowns
  - [ ] Update textareas
  - [ ] Update checkboxes/radio buttons styling
  - [ ] Test form validation styling
  - [ ] Test accessibility (focus indicators, labels)

#### 2.4: Update Tables
- **Changes**:
  - Remove Bootstrap table classes
  - Clean borders: `1px solid #e5e5e5`
  - Header background: `#f8f9fa` or light gray
  - Row hover: `#fafafa`
  - Padding: `12px 16px`
  - Remove striped backgrounds (optional)
- **Action Items**:
  - [ ] Identify all table usages
  - [ ] Create table CSS classes
  - [ ] Update table styling
  - [ ] Test responsive table behavior

---

### Phase 3: Page Updates (Est. 8-10 hours)
**Goal**: Update all application pages with new design

#### 3.1: Dashboard/Home Page
- **File**: `src/Components/Pages/Home.razor` (or main dashboard)
- **Changes**:
  - Add stats grid with teal values (`#00b7c3`)
  - Action cards with Azure blue hover
  - Recent activity section with purple icons
  - Update page title with Azure blue color
- **Action Items**:
  - [ ] Update page header/title styling
  - [ ] Implement stats card grid
  - [ ] Update action cards
  - [ ] Add/update activity list
  - [ ] Test responsive layout

#### 3.2: Configuration Page
- **File**: `src/Components/Pages/Configuration.razor`
- **Changes**:
  - Update section headers with bright cyan (`#50e6ff`)
  - Update import/export cards
  - Update backup/restore UI
  - Apply new button styling
- **Action Items**:
  - [ ] Update page layout
  - [ ] Update section styling
  - [ ] Update card components
  - [ ] Update buttons
  - [ ] Test backup/restore functionality with new design

#### 3.3: Admin Page
- **File**: `src/Components/Pages/Admin.razor`
- **Changes**:
  - Update migration section
  - Update settings sections
  - Apply new form styling
  - Update status indicators
- **Action Items**:
  - [ ] Update admin sections
  - [ ] Update form inputs
  - [ ] Update status displays
  - [ ] Test migration workflow with new design

#### 3.4: Resource Component Pages
- **Files**: 
  - `ResourceTypes.razor`
  - `ResourceLocations.razor`
  - `ResourceEnvironments.razor`
  - `ResourceOrgs.razor`
  - `ResourceProjAppSvcs.razor`
  - `ResourceUnitDepts.razor`
  - `ResourceFunctions.razor`
  - `CustomComponents.razor`
- **Changes**: Apply consistent styling to all component pages
- **Action Items**:
  - [ ] Update page headers
  - [ ] Update data tables
  - [ ] Update action buttons (Add, Edit, Delete)
  - [ ] Update search/filter inputs
  - [ ] Test CRUD operations with new design

#### 3.5: Generate Name Page
- **File**: `src/Components/Pages/ResourceNaming.razor` (or similar)
- **Changes**:
  - Update name generation form
  - Update preview/result display
  - Update component selection dropdowns
- **Action Items**:
  - [ ] Update form layout
  - [ ] Update result display
  - [ ] Update validation styling
  - [ ] Test name generation workflow

#### 3.6: Reference Page
- **File**: Reference/documentation pages
- **Changes**:
  - Update documentation display
  - Update code examples styling
  - Update navigation/breadcrumbs
- **Action Items**:
  - [ ] Update documentation layout
  - [ ] Update code snippet styling
  - [ ] Update navigation elements

---

### Phase 4: Modal & Notification Updates (Est. 4-6 hours)
**Goal**: Modernize all modals and notification components

#### 4.1: Update Modal Components
- **Files**: 
  - `src/Components/Modals/*.razor`
  - `TextConfirmationModal.razor`
  - Any other modal components
- **Changes**:
  - Update modal backdrop: `rgba(0, 0, 0, 0.5)`
  - Update modal container styling
  - Border radius: `8px`
  - Remove Bootstrap modal classes
  - Update modal header (close button, title)
  - Update modal footer buttons
  - Add subtle shadow: `0 10px 40px rgba(0, 0, 0, 0.2)`
- **Action Items**:
  - [ ] Identify all modal components
  - [ ] Create modal CSS base classes
  - [ ] Update each modal component
  - [ ] Update modal animations (fade in/out)
  - [ ] Test modal open/close behavior
  - [ ] Test modal backdrop click handling
  - [ ] Test TextConfirmationModal specifically

#### 4.2: Update Notification/Toast Components
- **Files**: Notification/toast components
- **Changes**:
  - Position: top-right or top-center
  - Background colors:
    - Success: Light green with green border/icon
    - Error: Light red with red border/icon
    - Warning: Light yellow with yellow border/icon
    - Info: Light blue with blue border/icon
  - Border radius: `6px`
  - Shadow: `0 4px 12px rgba(0, 0, 0, 0.15)`
  - Slide-in animation from right
- **Action Items**:
  - [ ] Update notification styling
  - [ ] Update notification icons
  - [ ] Update notification animations
  - [ ] Test notification timing/auto-dismiss
  - [ ] Test multiple notifications stacking

#### 4.3: Update Confirmation Dialogs
- **Changes**:
  - Update styling to match modal design
  - Azure blue primary button
  - Gray secondary button
- **Action Items**:
  - [ ] Update confirmation dialog styling
  - [ ] Test confirmation workflows

---

### Phase 5: Header & Top Bar Updates (Est. 2-3 hours)
**Goal**: Modernize top navigation and header elements

#### 5.1: Update Top Bar
- **File**: `MainLayout.razor` or header component
- **Changes**:
  - Height: `64px`
  - Background: `#ffffff`
  - Border bottom: `1px solid #e5e5e5`
  - Update search box styling (if present)
  - Update user menu/settings icons
  - Update breadcrumbs (if present)
- **Action Items**:
  - [ ] Update header layout
  - [ ] Update search functionality styling
  - [ ] Update icon buttons
  - [ ] Update user menu dropdown
  - [ ] Test responsive behavior

#### 5.2: Update Page Headers
- **Changes**:
  - Page title: Azure blue color (`#0078d4`)
  - Subtitle: Gray (`#737373`)
  - Breadcrumbs: Azure blue links
  - Action buttons: Align right
- **Action Items**:
  - [ ] Create page header component/template
  - [ ] Apply to all pages
  - [ ] Test header consistency

---

### Phase 6: Responsive & Mobile Updates (Est. 4-6 hours)
**Goal**: Ensure design works across all screen sizes

#### 6.1: Mobile Sidebar
- **Changes**:
  - Sidebar: overlay on mobile (< 768px)
  - Add mobile menu toggle button
  - Backdrop when sidebar open
  - Slide-in animation
- **Action Items**:
  - [ ] Implement mobile sidebar overlay
  - [ ] Add hamburger menu button
  - [ ] Add backdrop click to close
  - [ ] Test on mobile devices/emulators

#### 6.2: Responsive Grid Adjustments
- **Changes**:
  - Stats grid: 1 column on mobile
  - Action cards: 1 column on mobile
  - Tables: horizontal scroll or stacked layout
  - Forms: full width on mobile
- **Action Items**:
  - [ ] Test all pages on mobile (< 768px)
  - [ ] Test on tablet (768px - 1024px)
  - [ ] Adjust breakpoints as needed
  - [ ] Test form usability on mobile

#### 6.3: Touch Interactions
- **Changes**:
  - Increase button/link tap targets (min 44px)
  - Test touch gestures (swipe, tap)
  - Test modal interactions on touch devices
- **Action Items**:
  - [ ] Test on actual touch devices
  - [ ] Adjust tap target sizes
  - [ ] Test swipe gestures if applicable

---

### Phase 7: Polish & Refinement (Est. 3-4 hours)
**Goal**: Final polish and consistency checks

#### 7.1: Consistency Audit
- **Action Items**:
  - [ ] Audit all pages for consistent spacing
  - [ ] Audit all pages for consistent colors
  - [ ] Audit all pages for consistent typography
  - [ ] Audit all buttons for consistent styling
  - [ ] Audit all forms for consistent styling
  - [ ] Audit all tables for consistent styling
  - [ ] Check for any remaining Bootstrap classes

#### 7.2: Animation & Transitions
- **Action Items**:
  - [ ] Verify all hover transitions (`0.15s ease`)
  - [ ] Verify button hover effects
  - [ ] Verify card hover effects
  - [ ] Verify modal animations
  - [ ] Verify notification animations
  - [ ] Ensure no janky animations

#### 7.3: Accessibility Check
- **Action Items**:
  - [ ] Test keyboard navigation (Tab, Enter, Esc)
  - [ ] Test focus indicators visibility
  - [ ] Test color contrast (WCAG AA minimum)
  - [ ] Test with screen reader (basic check)
  - [ ] Verify form labels and ARIA attributes
  - [ ] Verify button/link accessible names

#### 7.4: Cross-Browser Testing
- **Action Items**:
  - [ ] Test on Chrome/Edge (Chromium)
  - [ ] Test on Firefox
  - [ ] Test on Safari (Mac/iOS)
  - [ ] Test on mobile browsers (Chrome Mobile, Safari Mobile)
  - [ ] Fix any browser-specific issues

---

### Phase 8: Cleanup & Documentation (Est. 2-3 hours)
**Goal**: Remove old code and document changes

#### 8.1: Remove Bootstrap Dependencies
- **Action Items**:
  - [ ] Remove Bootstrap CSS references
  - [ ] Remove unused Bootstrap JavaScript
  - [ ] Remove Bootstrap class usage (search for `class="btn `, `class="card `, etc.)
  - [ ] Update package.json if Bootstrap was a dependency
  - [ ] Test application after Bootstrap removal

#### 8.2: Remove Design Mockups
- **Action Items**:
  - [ ] Delete `src/Components/Pages/DesignMockups/` folder
  - [ ] Remove mockup routes
  - [ ] Clean up any mockup-related code

#### 8.3: Update Documentation
- **Files**: 
  - `README.md`
  - Any style guide or contributing docs
- **Action Items**:
  - [ ] Document new color palette (light and dark modes)
  - [ ] Document component styling patterns
  - [ ] Document theme toggle usage
  - [ ] Update screenshots/demos if applicable (show both themes)
  - [ ] Add design system reference

#### 8.4: Theme System Documentation
- **Action Items**:
  - [ ] Document CSS variable naming conventions
  - [ ] Document how to add new theme-aware components
  - [ ] Document localStorage theme persistence
  - [ ] Document system preference detection (if implemented)
  - [ ] Create guide for future theme customization

#### 8.4: Performance Optimization
- **Action Items**:
  - [ ] Minify CSS in production
  - [ ] Check for unused CSS
  - [ ] Verify bundle size hasn't increased significantly
  - [ ] Test page load performance

---

## Testing Strategy

### Functional Testing
- [ ] Test all existing functionality works with new design
- [ ] Test all forms submit correctly
- [ ] Test all CRUD operations
- [ ] Test name generation workflow
- [ ] Test configuration import/export
- [ ] Test backup/restore functionality
- [ ] Test migration workflow
- [ ] Test admin features

### Visual Testing
- [ ] Compare pages to DesignLinear mockup
- [ ] Verify color consistency
- [ ] Verify spacing consistency
- [ ] Verify typography consistency
- [ ] Take screenshots for documentation

### Responsive Testing
- [ ] Test on mobile devices (< 768px)
- [ ] Test on tablets (768px - 1024px)
- [ ] Test on desktop (> 1024px)
- [ ] Test on large screens (> 1920px)

### Browser Testing
- [ ] Chrome/Edge (Windows)
- [ ] Firefox (Windows)
- [ ] Safari (Mac/iOS)
- [ ] Mobile browsers

### Accessibility Testing
- [ ] Keyboard navigation
- [ ] Screen reader compatibility (basic)
- [ ] Color contrast
- [ ] Focus indicators

---

## Risk Assessment

### High Risk Items
1. **Bootstrap Removal**: May break existing functionality if not carefully removed
   - **Mitigation**: Test thoroughly after removal, keep Bootstrap until end
2. **Layout Changes**: Major layout changes could affect existing functionality
   - **Mitigation**: Test all pages after layout updates
3. **Form Styling**: Custom form styling could break validation displays
   - **Mitigation**: Test all forms with validation errors

### Medium Risk Items
1. **Modal Updates**: Could affect modal open/close behavior
   - **Mitigation**: Test all modals thoroughly
2. **Responsive Behavior**: Mobile layout could be problematic
   - **Mitigation**: Test on actual devices early
3. **Color Contrast**: New colors might not meet accessibility standards
   - **Mitigation**: Check WCAG contrast ratios

### Low Risk Items
1. **Button Styling**: Isolated changes, easy to test
2. **Typography**: Low impact on functionality
3. **Animations**: Can be disabled if problematic

---

## Rollback Plan

### If Critical Issues Found
1. Keep a branch with current Bootstrap design: `backup/bootstrap-design`
2. Git commits should be incremental and well-documented
3. Each phase should be committable separately
4. If rollback needed, can revert specific commits

### Rollback Triggers
- Broken functionality that can't be quickly fixed
- Severe accessibility issues
- Critical browser incompatibilities
- Performance degradation

---

## Timeline Estimate

### Total Estimated Time: 33-46 hours

| Phase | Estimated Time | Priority |
|-------|---------------|----------|
| Phase 1: Foundation Setup (includes Theme System) | 7-10 hours | CRITICAL |
| Phase 2: Component Library Updates | 6-8 hours | HIGH |
| Phase 3: Page Updates | 8-10 hours | HIGH |
| Phase 4: Modal & Notification Updates | 4-6 hours | MEDIUM |
| Phase 5: Header & Top Bar Updates (includes Theme Toggle) | 4-6 hours | MEDIUM |
| Phase 6: Responsive & Mobile Updates | 4-6 hours | HIGH |
| Phase 7: Polish & Refinement | 3-4 hours | MEDIUM |
| Phase 8: Cleanup & Documentation (includes Theme Docs) | 2-3 hours | LOW |
| **TOTAL** | **39-55 hours** | |

### Suggested Schedule
- **Week 1**: Phases 1-2 (Foundation + Theme System + Components)
- **Week 2**: Phase 3 (Page Updates)
- **Week 3**: Phases 4-5 (Modals + Header + Theme Toggle)
- **Week 4**: Phases 6-8 (Responsive + Polish + Cleanup + Theme Testing)

---

## Success Criteria

### Must Have
- [ ] All pages render correctly with new design
- [ ] All functionality works as before
- [ ] Mobile-responsive design works
- [ ] No console errors
- [ ] Meets WCAG AA color contrast standards
- [ ] Works in Chrome, Firefox, Safari

### Nice to Have
- [ ] Smooth animations throughout
- [ ] Improved performance over Bootstrap version
- [ ] Enhanced mobile experience
- [ ] Better accessibility than before

---

## Notes

### Design System Files to Create
1. `src/wwwroot/css/modern-design.css` - Main design system CSS
2. `src/wwwroot/css/modern-components.css` - Component-specific styles
3. `src/wwwroot/css/modern-responsive.css` - Responsive/mobile styles

### CSS Variables to Define
```css
:root {
    /* Sidebar */
    --sidebar-bg-start: #2d3748;
    --sidebar-bg-end: #1a202c;
    --sidebar-text: rgba(255, 255, 255, 0.85);
    --sidebar-text-active: #ffffff;
    --sidebar-hover: rgba(255, 255, 255, 0.1);
    
    /* Main Content */
    --bg-body: #fafafa;
    --bg-card: #ffffff;
    --border-color: #e5e5e5;
    --text-primary: #171717;
    --text-secondary: #737373;
    
    /* Azure Accents */
    --azure-blue: #0078d4;
    --azure-blue-dark: #005a9e;
    --azure-teal: #00b7c3;
    --azure-cyan: #50e6ff;
    --azure-purple: #8661c5;
    --success-green: #059669;
    
    /* Spacing */
    --spacing-xs: 4px;
    --spacing-sm: 8px;
    --spacing-md: 16px;
    --spacing-lg: 24px;
    --spacing-xl: 32px;
    
    /* Border Radius */
    --radius-sm: 6px;
    --radius-md: 8px;
    
    /* Transitions */
    --transition-fast: 0.15s ease;
}
```

### Key Decisions Made
1. **Dark Sidebar**: Provides professional look and contrasts with content
2. **Azure Colors**: Aligns with Azure branding, modern and recognizable
3. **Multiple Accent Colors**: Prevents monotony, creates visual hierarchy
4. **Minimal Shadows**: Clean, flat design with subtle depth
5. **System Fonts**: Fast loading, native feel

---

## Appendix: File Inventory

### Files to Create (NEW)
- `src/wwwroot/css/modern-design.css` - Main design system CSS with theme variables
- `src/wwwroot/css/modern-components.css` - Component-specific styles
- `src/wwwroot/css/modern-responsive.css` - Responsive/mobile styles
- `src/wwwroot/js/theme.js` - Theme management JavaScript
- `src/Components/General/ThemeToggle.razor` - Theme toggle component

### Files to Modify (Estimated)
- **Layout Components**: 2-3 files
- **Page Components**: 15-20 files
- **Modal Components**: 5-8 files
- **Shared Components**: 5-10 files
- **CSS Files**: Create 3 new files
- **Configuration Files**: 1-2 files

### Files to Create
- `src/wwwroot/css/modern-design.css`
- `src/wwwroot/css/modern-components.css`
- `src/wwwroot/css/modern-responsive.css`

### Files to Delete
- `src/Components/Pages/DesignMockups/` (entire folder after completion)

---

## Status Tracking

### Phase 1: Foundation Setup (includes Theme System)
- [ ] 1.1: Create Global CSS File
- [ ] 1.2: Update Main Layout Structure
- [ ] 1.3: Update Navigation Menu
- [ ] 1.4: Theme System Foundation (NEW)
  - [ ] Create CSS variables for light/dark themes
  - [ ] Add data-theme attribute support
  - [ ] Create theme.js for localStorage management
  - [ ] Add theme transition CSS
  - [ ] Test theme switching

### Phase 2: Component Library Updates
- [ ] 2.1: Update Card Components
- [ ] 2.2: Update Button Components
- [ ] 2.3: Update Form Components
- [ ] 2.4: Update Tables

### Phase 3: Page Updates
- [ ] 3.1: Dashboard/Home Page
- [ ] 3.2: Configuration Page
- [ ] 3.3: Admin Page
- [ ] 3.4: Resource Component Pages
- [ ] 3.5: Generate Name Page
- [ ] 3.6: Reference Page

### Phase 4: Modal & Notification Updates
- [ ] 4.1: Update Modal Components
- [ ] 4.2: Update Notification/Toast Components
- [ ] 4.3: Update Confirmation Dialogs

### Phase 5: Header & Top Bar Updates (includes Theme Toggle)
- [ ] 5.1: Update Top Bar
- [ ] 5.2: Update Page Headers
- [ ] 5.5: Theme Toggle Component (NEW)
  - [ ] Create ThemeToggle.razor component
  - [ ] Add toggle button styling
  - [ ] Integrate into header
  - [ ] Add keyboard accessibility
  - [ ] Test theme persistence

### Phase 6: Responsive & Mobile Updates
- [ ] 6.1: Mobile Sidebar
- [ ] 6.2: Responsive Grid Adjustments
- [ ] 6.3: Touch Interactions

### Phase 7: Polish & Refinement
- [ ] 7.1: Consistency Audit
- [ ] 7.2: Animation & Transitions
- [ ] 7.3: Accessibility Check
- [ ] 7.4: Cross-Browser Testing
- [ ] 7.5: Theme Testing (NEW)
  - [ ] Test all pages in light mode
  - [ ] Test all pages in dark mode
  - [ ] Test theme persistence
  - [ ] Test color contrast (WCAG AA)
  - [ ] Test theme toggle accessibility

### Phase 8: Cleanup & Documentation
- [ ] 8.1: Remove Bootstrap Dependencies
- [ ] 8.2: Remove Design Mockups
- [ ] 8.3: Update Documentation
- [ ] 8.4: Theme System Documentation (NEW)
  - [ ] Document CSS variables
  - [ ] Document theme customization
  - [ ] Document localStorage persistence
  - [ ] Update README with theme screenshots
- [ ] 8.5: Performance Optimization

---

**Last Updated**: October 17, 2025
**Document Version**: 1.1
**Status**: Planning Phase - Theme System Added

---

## Design Implementation Commit Tracking

To ensure easy rollback and traceability, record the commit hashes for the following milestones:

- **PRE-DESIGN COMMIT**: The last commit before any DesignLinear modernization changes were applied.
  - Commit hash: `ae7ef626f7e91efbb50c5918abdc18c5ace522f7`
  - Date: October 17, 2025
  - Description: Last stable commit before design system implementation began.

- **POST-DESIGN COMMIT**: The first commit after the initial DesignLinear modernization changes were applied (foundation CSS, layout, theme system, sidebar styling).
  - Commit hash: `fb7b27b0fba08cefb00309380644084afd3167bb`
  - Date: 2025-10-17 08:19:01 -0500
  - Message: feat: Begin DesignLinear implementation - rollback point
  - Description: Initial commit for DesignLinear modernization (foundation CSS, theme system, MainLayout, NavMenu sidebar styling).

> Update these hashes after each commit to keep a clear record for rollback and auditing.
