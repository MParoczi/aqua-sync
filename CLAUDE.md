# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AquaSync is an Electron desktop application for managing aquatic equipment (aquariums, filters, lamps, water testing). Built with Electron + React + TypeScript + Vite, targeting Windows only.

## Development Commands

### Start Development Server
```bash
yarn start
```
Launches the Electron app with hot reload enabled. DevTools open automatically in development mode.

### Lint Code
```bash
yarn lint
```
Runs ESLint on all `.ts` and `.tsx` files.

### Build & Package
```bash
yarn package    # Package the application
yarn make       # Create distributable installers (Windows Squirrel)
```

### Package Manager
This project uses **Yarn** (not npm). Always use `yarn` commands.

## Architecture

### Process Model (Electron)

**Main Process** (`src/main/`):
- Entry point: `src/main/main.ts`
- Manages BrowserWindow lifecycle
- Handles IPC communication with renderer
- Future: Will contain services for data persistence, device integration (Eheim/Chihiros Bluetooth), and file system operations

**Renderer Process** (`src/renderer/`):
- Entry point: `src/renderer/index.tsx`
- React 18 application with hot reload
- Main component: `src/renderer/App.tsx`
- Future: Will contain pages, components, contexts, hooks

**Preload Script** (`src/main/preload.ts`):
- Currently empty
- Future: Will expose IPC APIs to renderer via `contextBridge`

**Shared** (`src/shared/`):
- TypeScript types/interfaces shared between main and renderer
- Current: Basic `AppInfo` interface
- Future: Data models for aquariums, devices, water tests

### Build Configuration

**Electron Forge** (`forge.config.ts`):
- Vite plugin for bundling
- Windows Squirrel installer (primary maker)
- x64 architecture target
- Security fuses enabled (ASAR integrity validation, cookie encryption)

**Vite Configuration**:
- `vite.main.config.ts` - Main process bundling
- `vite.preload.config.ts` - Preload script bundling
- `vite.renderer.config.ts` - React renderer bundling

**TypeScript** (`tsconfig.json`):
- Target: ESNext
- Module resolution: bundler
- Strict mode enabled
- React JSX transform

### Data Storage Strategy

Per user stories (`user_stories_epic_1.md`), the app will use **local JSON file storage** in the AppData folder:
- Location: `app.getPath('userData')` (Electron API)
- Files: `aquariums.json`, `devices.json`, `water-tests.json`, `settings.json`
- Communication: IPC handlers in main process, called from renderer
- All data models defined in `src/shared/types.ts`

### Design System

The application uses a **glassmorphism** design with light/dark theme support:
- Auto-detects system theme via `matchMedia('(prefers-color-scheme: dark)')`
- Theme switches automatically when system preference changes
- Glassmorphism CSS properties:
  ```css
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  ```
- Planned components: `GlassCard`, `GlassModal`, `GlassButton`
- Tailwind CSS will be used for utilities

### Device Integration

Future features include Bluetooth integration for:
- **Eheim filters**: Status monitoring, maintenance tracking
- **Chihiros lamps**: Light control, scheduling

Libraries to be used: `@abandonware/noble` or `noble` for Bluetooth communication.

## Project Scope & Implementation Phases

User stories are organized into epics (`user_stories.md`, `user_stories_epic_*.md`):

1. **Epic 1**: Foundation (US-001 to US-004) - Project setup, design system, data persistence, notifications
2. **Epic 2**: Aquarium Management (US-005 to US-009) - Landing page, aquarium CRUD
3. **Epic 3**: Main Page & Dashboard (US-010 to US-015) - Overview, statistics
4. **Epic 4**: Water Tests (US-016 to US-018) - Parameter tracking, history
5. **Epic 5**: Filters (US-019 to US-028) - Eheim integration, maintenance tracking
6. **Epic 6**: Lamps (US-029 to US-037) - Chihiros integration, scheduling
7. **Epic 7**: Polish (US-038 to US-047) - Settings, backup/restore, refinements

Currently implemented: **US-001** (basic project structure).

## Key Technical Constraints

- **Windows-only**: No cross-platform considerations needed
- **No database**: All data stored in JSON files
- **Bluetooth required**: For Eheim/Chihiros integration
- **Local-first**: No cloud sync or backend services
- **Glassmorphism UI**: All components follow this design pattern

## Important Context

- The main window currently opens DevTools automatically (see `src/main/main.ts:30`)
- Vite dev server URLs are injected via Forge environment variables (`MAIN_WINDOW_VITE_DEV_SERVER_URL`, `MAIN_WINDOW_VITE_NAME`)
- Windows Squirrel handles install/uninstall shortcuts via `electron-squirrel-startup`
