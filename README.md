# AquaSync

A Windows desktop application for aquarium enthusiasts to manage multiple aquariums, track equipment (Eheim filters, Chihiros lamps), monitor water quality parameters, and schedule maintenance.

**Target Platform:** Windows 10/11 only
**Status:** Epic 4 Complete (Water Testing & Dashboard Graphs)

## 🎯 Project Status

**Current Phase:** Epic 4 Complete ✅
**Latest:** US-018 - Link Water Test Data to Dashboard Graphs

### Completed Features (US-001 to US-018)

**Epic 1 - Foundation (US-001 to US-004):**
- ✅ Project setup with Electron + React + TypeScript + Vite
- ✅ Glassmorphism design system (light/dark themes)
- ✅ JSON file persistence in AppData
- ✅ Toast notification system with 4 types (success/error/info/warning)

**Epic 2 - Aquarium Management (US-005 to US-009):**
- ✅ Landing page with aquarium grid (responsive 1→4 columns)
- ✅ Aquarium CRUD operations (Create, Read, Update, Delete)
- ✅ Thumbnail upload with auto-generated placeholders
- ✅ Volume auto-calculation from dimensions
- ✅ Right-click context menus (Edit/Delete)
- ✅ Navigation to Main Page from aquarium card

**Epic 3 - Dashboard & Statistics (US-010 to US-015):**
- ✅ Main page with sidebar navigation (28% / 72% split layout)
- ✅ Section routing (Dashboard, Filters, Lamps, Tests)
- ✅ Aquarium information display with thumbnail
- ✅ Connected devices status cards
- ✅ Water parameter multi-select dropdown
- ✅ Recharts line graphs with glassmorphism tooltips
- ✅ Per-aquarium graph settings persistence
- ✅ Event-driven data refresh mechanism

**Epic 4 - Water Tests (US-016 to US-018):**
- ✅ Water tests page with 13 color-coded parameter cards
- ✅ Test measurement modal with date/time picker
- ✅ Parameter-specific validation ranges (min/max/step)
- ✅ Water test data linked to dashboard graphs
- ✅ Automatic dashboard refresh after test recording
- ✅ 13 parameters: pH, GH, KH, NO₂, NO₃, NH₄, Fe, Cu, SiO₂, PO₄, CO₂, O₂, Temperature

### Next Up (Epic 5 - Filter Management)
- 🚧 US-019: Filter devices page
- 🚧 US-020: Add filter device
- 🚧 US-021: Eheim Bluetooth integration

## ✨ Features

### Aquarium Management
- **Multiple aquariums** - Track freshwater and marine setups
- **Custom dimensions** - Width/Length/Height in cm or inches
- **Auto volume calculation** - Converts dimensions to liters/gallons
- **Thumbnail photos** - Upload images or use generated placeholders
- **Quick actions** - Right-click context menu for Edit/Delete

### Dashboard & Monitoring
- **Sidebar navigation** - Four main sections (Dashboard, Filters, Lamps, Tests)
- **Aquarium overview** - Display selected aquarium details and image
- **Connected devices** - Visual status cards for filters and lamps
- **Water parameter graphs** - Recharts line graphs with custom tooltips
- **Graph customization** - Multi-select dropdown to choose displayed parameters
- **Persistent settings** - Per-aquarium graph preferences saved automatically

### Water Testing
- **13 water parameters** - pH, GH, KH, NO₂, NO₃, NH₄, Fe, Cu, SiO₂, PO₄, CO₂, O₂, Temperature
- **Color-coded cards** - Each parameter has unique color for easy identification
- **Quick recording** - Click parameter card to open measurement modal
- **Smart validation** - Parameter-specific ranges prevent invalid entries
- **Historical tracking** - All measurements stored with timestamps
- **Visual trends** - Graphs automatically update after recording new tests

### User Interface
- **Glassmorphism design** - Frosted glass aesthetic with backdrop blur
- **Auto theme detection** - Follows Windows light/dark preference
- **Real-time theme switching** - Changes instantly when system theme updates
- **Toast notifications** - Success/Error/Info/Warning messages
- **Responsive layout** - Adapts from mobile to desktop sizes
- **Smooth transitions** - Section navigation with fade effects

### Data Management
- **Local JSON storage** - No database required
- **AppData persistence** - Data stored in `%APPDATA%\AquaSync\`
- **Cascading deletes** - Removing aquarium cleans up devices/tests
- **Type safety** - Full TypeScript coverage across all processes
- **Event-driven updates** - Real-time UI refresh after data changes

## 📁 Project Structure

```
aquaSync/
├── src/
│   ├── main/                          # Electron Main Process
│   │   ├── main.ts                    # Entry point + 17 IPC handlers
│   │   ├── preload.ts                 # IPC bridge via contextBridge
│   │   └── services/
│   │       ├── dataService.ts         # JSON file persistence (505 lines)
│   │       └── fileService.ts         # Thumbnail operations (135 lines)
│   ├── renderer/                      # React Application
│   │   ├── index.tsx                  # React root with providers
│   │   ├── App.tsx                    # Main routing component
│   │   ├── index.css                  # Global styles + Tailwind
│   │   ├── App.css                    # App-specific styles
│   │   ├── electron.d.ts              # TypeScript declarations
│   │   ├── components/
│   │   │   ├── AquariumCard.tsx       # Card with context menu
│   │   │   ├── AquariumGrid.tsx       # Responsive grid layout
│   │   │   ├── AquariumModal.tsx      # Create/edit form (618 lines)
│   │   │   ├── DashboardContent.tsx   # Main dashboard (300 lines)
│   │   │   ├── TestsContent.tsx       # Water tests page (114 lines)
│   │   │   ├── TestMeasurementModal.tsx  # Test recording form
│   │   │   ├── WaterParameterGraph.tsx   # Recharts graphs (170 lines)
│   │   │   ├── WaterParameterSelector.tsx # Multi-select dropdown
│   │   │   ├── ConnectedDevices.tsx   # Device status grid
│   │   │   ├── DeviceCard.tsx         # Individual device card
│   │   │   └── common/
│   │   │       ├── GlassCard.tsx      # Glassmorphism container
│   │   │       ├── GlassButton.tsx    # Primary/secondary buttons
│   │   │       ├── GlassModal.tsx     # Modal overlay
│   │   │       ├── ContextMenu.tsx    # Right-click menu
│   │   │       ├── DeleteConfirmationModal.tsx
│   │   │       └── Toast.tsx          # Notification component
│   │   ├── contexts/
│   │   │   ├── AquariumContext.tsx    # Selected aquarium state
│   │   │   ├── ThemeContext.tsx       # Light/dark theme manager
│   │   │   └── ToastContext.tsx       # Global notifications
│   │   ├── pages/
│   │   │   ├── LandingPage.tsx        # Aquarium list/grid (168 lines)
│   │   │   └── MainPage.tsx           # Aquarium detail page (198 lines)
│   │   └── styles/
│   │       └── index.css              # Global styles
│   └── shared/
│       ├── types.ts                   # All TypeScript interfaces (236 lines)
│       └── waterParameters.ts         # Parameter constants/config (106 lines)
├── forge.config.ts                    # Electron Forge configuration
├── vite.main.config.ts                # Main process bundling
├── vite.preload.config.ts             # Preload script bundling
├── vite.renderer.config.ts            # React renderer bundling
├── tailwind.config.js                 # Tailwind CSS + glassmorphism config
├── tsconfig.json                      # TypeScript configuration
└── package.json                       # Dependencies
```

## 🧩 Component Architecture

### Common Components (Reusable)
- **GlassCard**, **GlassButton**, **GlassModal** - Glassmorphism UI primitives
- **Toast**, **ContextMenu**, **DeleteConfirmationModal** - Interaction patterns

### Page Components
- **LandingPage** - Aquarium selection grid with CRUD operations
- **MainPage** - Sidebar layout (28%/72%) with section routing

### Dashboard Components
- **DashboardContent** - Main orchestrator, loads aquarium/device/test data
- **ConnectedDevices** - Device status grid (filters and lamps)
- **WaterParameterSelector** - Multi-select dropdown for graph configuration
- **WaterParameterGraph** - Recharts line graph with custom glassmorphism tooltip

### Water Test Components
- **TestsContent** - 13 parameter cards grid with color coding
- **TestMeasurementModal** - Measurement recording form with validation

### Contexts
- **AquariumContext** - Selected aquarium state (shared across app)
- **ThemeContext** - Light/dark theme with system preference detection
- **ToastContext** - Global notification system (success/error/info/warning)

## 🛠️ Tech Stack

- **Electron 39**: Desktop application framework
- **React 18**: UI library with hooks and context
- **TypeScript 5**: Strict type-safe JavaScript
- **Vite**: Lightning-fast build tool with HMR
- **Tailwind CSS 3**: Utility-first CSS framework
- **Recharts 3.4**: Charting library for water parameter graphs
- **Electron Forge**: Build and packaging tool
- **Context API**: State management (no Redux/Zustand)
- **Windows Squirrel**: Installer/updater for Windows

## 💾 Data Storage

All data is stored locally in JSON files (no database required).

**Location:** `%APPDATA%\AquaSync\`
*(Typically: `C:\Users\{username}\AppData\Roaming\AquaSync\`)*

**Files:**
- `aquariums.json` - All aquarium data (name, type, dimensions, volume, thumbnails)
- `devices.json` - Filters and lamps with full CRUD operations
- `water-tests.json` - Water test measurements (13 parameters with timestamps)
- `settings.json` - App configuration and per-aquarium graph preferences
- `thumbnails/` - Aquarium thumbnail images (UUID-based filenames)

**Benefits:**
- ✅ No database setup required
- ✅ Easy backup (copy entire folder)
- ✅ Human-readable format for debugging
- ✅ Portable data

**Note:** Data is NOT synced across devices. Single-user, single-machine design.

## 🔌 IPC Communication

The app uses type-safe IPC with **17 endpoints**:

**Aquariums:** `getAquariums`, `getAquarium`, `createAquarium`, `updateAquarium`, `deleteAquarium`

**Devices:** `getDevices`, `getDevice`, `createDevice`, `updateDevice`, `deleteDevice`

**Water Tests:** `getWaterTests`, `getWaterTest`, `createWaterTest`, `updateWaterTest`, `deleteWaterTest`

**Settings:** `getSettings`, `updateSettings`, `getAquariumSettings`, `updateAquariumSettings`

**Files:** `copyThumbnail`, `getThumbnailPath`

All handlers return `IpcResult<T>` with success/error structure for type-safe error handling.

## 🏗️ Architecture

**Electron Multi-Process Model:**

```
┌─────────────────────────────────────┐
│   Renderer Process (React)          │
│   - UI Components (20+ components)  │
│   - Context API State               │
│   - Calls window.electron APIs      │
└──────────────┬──────────────────────┘
               │ IPC via contextBridge
┌──────────────▼──────────────────────┐
│   Preload Script                    │
│   - Exposes 17 secure APIs          │
│   - Type-safe IPC bridge            │
└──────────────┬──────────────────────┘
               │ IPC invoke/handle
┌──────────────▼──────────────────────┐
│   Main Process                      │
│   - IPC Handlers (17 endpoints)     │
│   - File I/O Operations             │
│   - Window Management               │
└──────────────┬──────────────────────┘
               │ fs.readFile/writeFile
┌──────────────▼──────────────────────┐
│   JSON Files (AppData)              │
│   - aquariums.json                  │
│   - devices.json                    │
│   - water-tests.json                │
│   - settings.json                   │
└─────────────────────────────────────┘
```

**Data Flow Examples:**

**Aquarium Creation Flow:**
1. User clicks "Create Aquarium" → Renderer component
2. Renderer calls `window.electron.data.createAquarium(data)`
3. Preload forwards to Main via `ipcRenderer.invoke('createAquarium')`
4. Main process handler validates, generates UUID, timestamps
5. dataService reads `aquariums.json`, appends new item, writes back
6. Returns `IpcResult<Aquarium>` success/error
7. Preload returns to Renderer
8. Renderer updates UI + shows toast notification

**Water Test Recording Flow:**
1. User clicks parameter card (e.g., pH) → Opens TestMeasurementModal
2. User enters value, date/time → Validates against parameter range (pH: 0-14)
3. Calls `window.electron.data.createWaterTest(testData)`
4. Main process validates, generates UUID, saves to water-tests.json
5. Returns success → Renderer emits `water-test-saved` custom event
6. DashboardContent listens to event → Reloads water test data via IPC
7. WaterParameterGraph re-renders with new data point
8. Graph animates transition with Recharts

## 🎨 Design System

AquaSync uses a **glassmorphism** design language inspired by modern OS interfaces.

**Visual Characteristics:**
- Frosted glass effect with `backdrop-filter: blur(10px)`
- Semi-transparent overlays (`rgba` colors)
- Subtle borders and shadows
- Smooth animations and transitions

**Theme Support:**
- **Auto-detection** of Windows theme preference
- **Light mode:** Purple-violet gradient background
- **Dark mode:** Navy-blue gradient background
- **Real-time switching** when system preference changes

**Component Library:**
- `GlassCard` - Container with glassmorphism effect
- `GlassButton` - Primary/secondary button variants
- `GlassModal` - Dialog overlays with backdrop
- `ContextMenu` - Right-click menus with auto-repositioning
- `Toast` - Notification system (4 types)

**Color Palette:**
- Light theme: Purple (#667eea) → Violet (#764ba2)
- Dark theme: Navy (#1a1a2e) → Dark Blue (#16213e)
- Water parameters: 13 unique colors (defined in `waterParameters.ts`)

## Getting Started

### Prerequisites

- **Node.js 18+** (LTS recommended)
- **Yarn** package manager
- **Windows 10/11** (this is a Windows-only application)

### Installation

```bash
yarn install
```

### Development

Start the application in development mode with hot reload:

```bash
yarn start
```

DevTools will open automatically in development mode.

### Linting

Run ESLint on all TypeScript files:

```bash
yarn lint
```

### Building

Package the application:

```bash
yarn package
```

Create distributable installers:

```bash
yarn make
```

This creates a Windows Squirrel installer in the `out/make/` directory.

## Development Notes

- Hot reload is enabled for React components in development mode
- TypeScript strict mode is enabled for both main and renderer processes
- Windows Squirrel installer is configured as the default maker
- DevTools open automatically in development (see `src/main/main.ts:32`)
- IPC communication is type-safe via TypeScript interfaces
- Event-driven architecture for cross-component data refresh

## ⚠️ Known Limitations

- **Windows only** - No macOS/Linux support planned
- **Single user** - No multi-user accounts or cloud sync
- **No concurrent access** - JSON files not safe for simultaneous writes
- **Bluetooth not implemented** - Device data structures exist, but Bluetooth communication (Eheim/Chihiros) pending Epic 5-6
- **No pagination** - All water tests loaded at once (could impact performance with 1000+ measurements per aquarium)
- **No database** - Large datasets (1000+ aquariums) may impact performance
- **Thumbnail preview missing** - Editing aquarium doesn't show existing thumbnail (minor TODO)

## 🗺️ Roadmap

### ✅ Completed Epics

**Epic 1: Foundation (US-001 to US-004)**
- Project structure with Electron + React + TypeScript + Vite
- Glassmorphism design system with theme support
- JSON file persistence in AppData
- Toast notification system

**Epic 2: Aquarium Management (US-005 to US-009)**
- Landing page with aquarium grid
- Full CRUD operations for aquariums
- Thumbnail upload and management
- Context menus and navigation

**Epic 3: Dashboard & Statistics (US-010 to US-015)**
- Main page with sidebar layout
- Section routing (Dashboard/Filters/Lamps/Tests)
- Aquarium information display
- Connected devices status cards
- Water parameter graph selector
- Recharts line graphs with custom tooltips

**Epic 4: Water Testing (US-016 to US-018)**
- 13 parameter tracking (pH, GH, KH, NO₂, NO₃, NH₄, Fe, Cu, SiO₂, PO₄, CO₂, O₂, Temp)
- Test measurement modal with validation
- Parameter history graphs
- Event-driven dashboard refresh

### 🚧 Upcoming Epics

**Epic 5: Filter Management (Next)**
- Filter devices page with device grid
- Add/edit/delete filter devices
- Eheim filter Bluetooth integration
- Status monitoring and diagnostics
- Maintenance tracking and reminders
- Filter performance graphs

**Epic 6: Lamp Control**
- Chihiros lamp Bluetooth integration
- Light scheduling system
- Brightness and color control
- Photoperiod tracking
- Lamp status monitoring

**Epic 7: Polish & Refinements**
- Settings page (app preferences, units, theme override)
- Data backup/restore functionality
- Export to PDF/CSV
- Performance optimizations (pagination, caching)
- Error boundaries and retry logic
- Testing infrastructure (Vitest/Jest)

## License

MIT
