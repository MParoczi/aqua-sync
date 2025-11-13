## Epic 3: Main Page Layout & Navigation

### US-010: Create Main Page Layout with Sidebar
**As a** user  
**I want** to see a navigation menu when viewing an aquarium  
**So that** I can access different sections

**Technical Details:**
- Create `MainPage` component with two-column layout
- Left sidebar: 25-30% width, fixed
- Right content area: 70-75% width, scrollable
- Sidebar background: glassmorphism panel
- Navigation menu items:
    - Dashboard (default/active)
    - Filters
    - Lamps
    - Tests
- Active menu item styling: different background/border
- Implement routing within MainPage (React Router or state-based)

**Acceptance Criteria:**
- [ ] Sidebar takes up 25-30% width
- [ ] Content area takes up 70-75% width
- [ ] Navigation items are clearly visible
- [ ] Active menu item is highlighted
- [ ] Dashboard is active by default

---

### US-011: Implement Navigation Between Sections
**As a** user  
**I want** to click navigation items to switch between sections  
**So that** I can view different parts of my aquarium management

**Technical Details:**
- Implement click handlers for navigation items
- Update active state when item clicked
- Render appropriate content component in right area:
    - Dashboard → `DashboardContent`
    - Filters → `FiltersContent`
    - Lamps → `LampsContent`
    - Tests → `TestsContent`
- Smooth transitions between sections

**Acceptance Criteria:**
- [ ] Clicking navigation item updates active state
- [ ] Content area updates to show selected section
- [ ] Transitions are smooth
- [ ] Active item remains highlighted
