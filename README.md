# AquaSync

A Windows desktop application for aquarium enthusiasts to manage multiple aquariums, track equipment (Eheim filters, Chihiros lamps), monitor water quality parameters, and schedule maintenance.

**Target Platform:** Windows 10/11 only
**Status:** Epic 2 Complete (Aquarium Management)

## 🎯 Project Status

**Current Phase:** Epic 2 Complete ✅
**Latest:** US-009 - Navigate to Main Page from Aquarium Card

### Completed Features (US-001 to US-009)
- ✅ Project setup with Electron + React + TypeScript + Vite
- ✅ Glassmorphism design system (light/dark themes)
- ✅ JSON file persistence in AppData
- ✅ Toast notification system
- ✅ Aquarium CRUD operations
- ✅ Thumbnail upload with auto-generated placeholders
- ✅ Volume auto-calculation from dimensions
- ✅ Responsive grid layout (1→4 columns)
- ✅ Right-click context menus (Edit/Delete)
- ✅ Navigation between pages

### Next Up (Epic 3 - Dashboard)
- 🚧 US-010: Main page layout
- 🚧 US-011: Overview dashboard
- 🚧 US-012: Quick statistics

## ✨ Features

### Aquarium Management
- **Multiple aquariums** - Track freshwater and marine setups
- **Custom dimensions** - Width/Length/Height in cm or inches
- **Auto volume calculation** - Converts dimensions to liters/gallons
- **Thumbnail photos** - Upload images or use generated placeholders
- **Quick actions** - Right-click context menu for Edit/Delete

### User Interface
- **Glassmorphism design** - Frosted glass aesthetic with backdrop blur
- **Auto theme detection** - Follows Windows light/dark preference
- **Real-time theme switching** - Changes instantly when system theme updates
- **Toast notifications** - Success/Error/Info/Warning messages
- **Responsive layout** - Adapts from mobile to desktop sizes

### Data Management
- **Local JSON storage** - No database required
- **AppData persistence** - Data stored in `%APPDATA%\AquaSync\`
- **Cascading deletes** - Removing aquarium cleans up devices/tests
- **Type safety** - Full TypeScript coverage

## 📁 Project Structure

```
aquaSync/
├── src/
│   ├── main/                          # Electron Main Process
│   │   ├── main.ts                    # Entry point + IPC handlers
│   │   ├── preload.ts                 # IPC bridge via contextBridge
│   │   └── services/
│   │       ├── dataService.ts         # JSON file persistence (443 lines)
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
│   │   └── pages/
│   │       ├── LandingPage.tsx        # Aquarium list/grid
│   │       └── MainPage.tsx           # Aquarium detail page
│   └── shared/
│       └── types.ts                   # All TypeScript interfaces (206 lines)
├── forge.config.ts                    # Electron Forge configuration
├── vite.main.config.ts                # Main process bundling
├── vite.preload.config.ts             # Preload script bundling
├── vite.renderer.config.ts            # React renderer bundling
├── tailwind.config.js                 # Tailwind CSS configuration
├── tsconfig.json                      # TypeScript configuration
└── package.json                       # Dependencies
```

## 🛠️ Tech Stack

- **Electron 39**: Desktop application framework
- **React 18**: UI library with hooks and context
- **TypeScript 5**: Strict type-safe JavaScript
- **Vite**: Lightning-fast build tool with HMR
- **Tailwind CSS 3**: Utility-first CSS framework
- **Electron Forge**: Build and packaging tool
- **Context API**: State management (no Redux/Zustand)
- **Windows Squirrel**: Installer/updater for Windows

## 💾 Data Storage

All data is stored locally in JSON files (no database required).

**Location:** `%APPDATA%\AquaSync\`
*(Typically: `C:\Users\{username}\AppData\Roaming\AquaSync\`)*

**Files:**
- `aquariums.json` - All aquarium data
- `devices.json` - Filters and lamps (future)
- `water-tests.json` - Water test history (future)
- `settings.json` - App configuration
- `thumbnails/` - Aquarium thumbnail images

**Benefits:**
- ✅ No database setup required
- ✅ Easy backup (copy entire folder)
- ✅ Human-readable format for debugging
- ✅ Portable data

**Note:** Data is NOT synced across devices. Single-user, single-machine design.

## 🏗️ Architecture

**Electron Multi-Process Model:**

```
┌─────────────────────────────────────┐
│   Renderer Process (React)          │
│   - UI Components                   │
│   - Context API State               │
│   - Calls window.electron APIs      │
└──────────────┬──────────────────────┘
               │ IPC via contextBridge
┌──────────────▼──────────────────────┐
│   Preload Script                    │
│   - Exposes secure APIs             │
│   - Type-safe IPC bridge            │
└──────────────┬──────────────────────┘
               │ IPC invoke/handle
┌──────────────▼──────────────────────┐
│   Main Process                      │
│   - IPC Handlers                    │
│   - File I/O Operations             │
│   - Window Management               │
└──────────────┬──────────────────────┘
               │ fs.readFile/writeFile
┌──────────────▼──────────────────────┐
│   JSON Files (AppData)              │
│   - aquariums.json                  │
│   - devices.json                    │
│   - water-tests.json                │
└─────────────────────────────────────┘
```

**Data Flow Example:**
1. User clicks "Create Aquarium" → Renderer component
2. Renderer calls `window.electron.data.createAquarium(data)`
3. Preload forwards to Main via `ipcRenderer.invoke('createAquarium')`
4. Main process handler validates, generates UUID, timestamps
5. dataService reads `aquariums.json`, appends new item, writes back
6. Returns `IpcResult<Aquarium>` success/error
7. Preload returns to Renderer
8. Renderer updates UI + shows toast notification

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
- DevTools open automatically in development (see `src/main/main.ts:30`)
- IPC communication is type-safe via TypeScript interfaces

## ⚠️ Known Limitations

- **Windows only** - No macOS/Linux support planned
- **Single user** - No multi-user accounts or cloud sync
- **No concurrent access** - JSON files not safe for simultaneous writes
- **Bluetooth not implemented** - Eheim/Chihiros integration pending (Epic 5-6)
- **No database** - Large datasets (1000+ aquariums) may impact performance

## 🗺️ Roadmap

### Epic 3: Dashboard & Statistics (Next)
- Main page overview dashboard
- Quick statistics display
- Recent activity feed

### Epic 4: Water Testing
- Parameter tracking (pH, ammonia, nitrite, nitrate, etc.)
- Test history and trends
- Parameter status indicators

### Epic 5: Filter Management
- Eheim filter Bluetooth integration
- Status monitoring
- Maintenance tracking and reminders

### Epic 6: Lamp Control
- Chihiros lamp Bluetooth integration
- Light scheduling
- Brightness and color control

### Epic 7: Polish
- Settings page
- Data backup/restore
- Export to PDF/CSV
- Performance optimizations

## License

MIT
