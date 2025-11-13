# AquaSync

Electron + React application for managing aquatic equipment.

## Project Structure

```
aqua-sync/
├── src/
│   ├── main/           # Electron main process
│   │   ├── main.ts     # Main entry point
│   │   └── preload.ts  # Preload script
│   ├── renderer/       # React application
│   │   ├── index.tsx   # React entry point
│   │   ├── App.tsx     # Main App component
│   │   ├── App.css     # App styles
│   │   └── index.css   # Global styles
│   └── shared/         # Shared types and utilities
│       └── types.ts    # TypeScript type definitions
├── forge.config.ts     # Electron Forge configuration
├── vite.*.config.ts    # Vite configuration files
├── tsconfig.json       # TypeScript configuration
└── package.json        # Project dependencies
```

## Tech Stack

- **Electron**: Desktop application framework
- **React 18**: UI library
- **TypeScript 5**: Type-safe JavaScript
- **Vite**: Fast build tool with hot reload
- **Electron Forge**: Build and packaging tool

## Features (US-001 Implementation)

- ✅ Electron + React + TypeScript project structure
- ✅ Vite bundling with hot reload support
- ✅ Organized directory structure (main/renderer/shared)
- ✅ Windows-specific build configurations
- ✅ Development environment with auto-reload

## Getting Started

### Prerequisites

- Node.js 16+
- Yarn package manager

### Installation

```bash
yarn install
```

### Development

Start the application in development mode with hot reload:

```bash
yarn start
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

## Development Notes

- Hot reload is enabled for React components in development mode
- TypeScript compilation is configured for both main and renderer processes
- Windows Squirrel installer is configured as the default maker
- DevTools are open by default in development mode

## License

MIT
