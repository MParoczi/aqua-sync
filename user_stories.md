# Aqua Sync - User Stories for Claude Code Implementation

## Implementation Notes for Claude Code

### Recommended Tech Stack
- **Framework**: Electron + React + TypeScript + Vite
- **UI Library**: Tailwind CSS (for glassmorphism utilities)
- **State Management**: Zustand or React Context
- **Charting**: Recharts (React-friendly)
- **Date/Time**: date-fns or Day.js
- **File System**: Node.js fs/promises
- **IPC**: Electron's ipcMain/ipcRenderer
- **Bluetooth**: @abandonware/noble or noble
- **HTTP Client**: Axios or native fetch

### Project Structure
```
aqua-sync/
├── src/
│   ├── main/           # Electron main process
│   │   ├── index.ts
│   │   ├── services/
│   │   │   ├── data.service.ts
│   │   │   ├── eheim.service.ts
│   │   │   └── chihiros.service.ts
│   │   └── ipc/
│   ├── renderer/       # React app
│   │   ├── App.tsx
│   │   ├── pages/
│   │   │   ├── LandingPage.tsx
│   │   │   ├── MainPage.tsx
│   │   │   └── ...
│   │   ├── components/
│   │   │   ├── common/
│   │   │   ├── modals/
│   │   │   └── ...
│   │   ├── contexts/
│   │   ├── hooks/
│   │   └── utils/
│   ├── shared/         # Shared types
│   │   └── types.ts
│   └── preload/
└── assets/
```

### Testing Strategy
- Unit tests for utilities and calculations
- Integration tests for IPC communication
- Manual testing for device integration (Eheim, Chihiros)
- E2E tests for critical user flows

### Development Phases
1. **Phase 1**: Foundation (US-001 to US-004)
2. **Phase 2**: Aquarium Management (US-005 to US-009)
3. **Phase 3**: Main Page & Dashboard (US-010 to US-015)
4. **Phase 4**: Water Tests (US-016 to US-018)
5. **Phase 5**: Filters (US-019 to US-028)
6. **Phase 6**: Lamps (US-029 to US-037)
7. **Phase 7**: Polish (US-038 to US-047)