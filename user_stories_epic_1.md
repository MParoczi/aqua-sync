## Epic 1: Foundation & Project Setup

### US-001: Initialize Electron + React Project
**As a** developer  
**I want** to set up the Electron + React project structure  
**So that** I have a solid foundation for building the application

**Technical Details:**
- Initialize project with Electron Forge + React + TypeScript
- Configure Vite for React bundling
- Set up project structure: `/src/main` (Electron), `/src/renderer` (React), `/src/shared` (types/utils)
- Configure hot reload for development
- Set up Windows-specific configurations (no cross-platform needed)

**Acceptance Criteria:**
- [ ] Project runs with `yarn start`
- [ ] Hot reload works for React components
- [ ] Electron window opens with basic React app
- [ ] TypeScript compilation works without errors

---

### US-002: Implement Glassmorphism Design System
**As a** developer  
**I want** to create reusable glassmorphism UI components  
**So that** the application has a consistent visual style

**Technical Details:**
- Create Tailwind CSS configuration with glassmorphism utilities
- Implement theme system supporting light/dark modes
- Create base components: `GlassCard`, `GlassModal`, `GlassButton`
- Implement system theme detection using `matchMedia('(prefers-color-scheme: dark)')`
- Auto-update theme when system preferences change

**Glassmorphism CSS Properties:**
```css
background: rgba(255, 255, 255, 0.1);
backdrop-filter: blur(10px);
border: 1px solid rgba(255, 255, 255, 0.2);
```

**Acceptance Criteria:**
- [ ] Application follows system theme (light/dark)
- [ ] Theme switches automatically when system preference changes
- [ ] Glassmorphism components render correctly in both themes
- [ ] No flashing/flickering during theme transitions

---

### US-003: Implement Local Data Persistence Layer
**As a** developer  
**I want** to implement a local file-based data storage system  
**So that** user data persists between application sessions

**Technical Details:**
- Create data service in Electron main process
- Use `app.getPath('userData')` for AppData folder location
- Implement JSON file storage for:
    - `aquariums.json` - aquarium configurations
    - `devices.json` - connected devices (filters, lamps)
    - `water-tests.json` - water parameter measurements
    - `settings.json` - application settings
- Implement IPC (Inter-Process Communication) for renderer ↔ main communication
- Create TypeScript interfaces for all data models

**Data Models:**
```typescript
interface Aquarium {
  id: string; // GUID
  name: string;
  type: 'freshwater' | 'marine';
  dimensions: {
    width: number;
    length: number;
    height: number;
    unit: 'cm' | 'inch';
  };
  volume: {
    value: number;
    unit: 'liter' | 'gallon';
    isCustom: boolean;
  };
  startDate: string; // ISO date
  thumbnailPath?: string;
  createdAt: string;
}
```

**Acceptance Criteria:**
- [ ] Data files are created in AppData folder
- [ ] Data persists after application restart
- [ ] IPC handlers work for CRUD operations
- [ ] File read/write operations handle errors gracefully

---

### US-004: Implement Toast Notification System
**As a** user  
**I want** to receive visual feedback for my actions  
**So that** I know whether operations succeeded or failed

**Technical Details:**
- Create `ToastProvider` context for React
- Implement toast component with glassmorphism design
- Support toast types: success, error, info, warning
- Auto-dismiss after 5 seconds (configurable)
- Position: top-right corner
- Stack multiple toasts vertically

**Acceptance Criteria:**
- [ ] Toasts appear with glassmorphism styling
- [ ] Multiple toasts stack properly
- [ ] Toasts auto-dismiss after 5 seconds
- [ ] Toast colors adapt to light/dark theme
- [ ] Toasts are accessible (ARIA labels)