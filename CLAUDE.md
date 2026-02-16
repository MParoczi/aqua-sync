# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Restore & build entire solution
dotnet build AquaSync.sln

# Build specific project
dotnet build AquaSync.App/AquaSync.App.csproj

# Run the app (must specify a runtime identifier)
dotnet run --project AquaSync.App -r win-x64

# Clean build
dotnet clean AquaSync.sln && dotnet build AquaSync.sln
```

Requires .NET SDK 10.0.101+ (pinned in `global.json`).

## Architecture

**AquaSync** is a WinUI3 desktop app for managing home aquariums, controlling Chihiros LED lights (BLE) and EHEIM filters (WebSocket).

### Projects

- **AquaSync.App** — WinUI3 desktop app (`net10.0-windows10.0.19041.0`). MVVM with DI via `Microsoft.Extensions.Hosting`. Entry point: `App.xaml.cs` configures the DI container.
- **AquaSync.Chihiros** — Class library for Chihiros LED control over BLE using WinRT APIs. No external NuGet dependencies. Clean-room port of chihiros-led-control (Python).
- **AquaSync.Eheim** — Class library for EHEIM filter control over local WebSocket. Uses `System.Reactive` for observable-based API and `Zeroconf` for mDNS discovery.

App references both libraries as project dependencies.

### App Structure

- **Views/** — WinUI3 XAML pages
- **ViewModels/** — Inherit `ViewModelBase` (wraps `CommunityToolkit.Mvvm.ObservableObject`). Use manual `SetProperty` calls, NOT `[ObservableProperty]` attributes (incompatible with WinUI3 AOT).
- **Services/** — `INavigationService`, `IPageService`, `IDataService` with implementations
- **Contracts/Services/** — Service interfaces

### Navigation

Two-frame architecture:
1. **MainWindow Root Frame** — switches between `AquariumSelectorPage` and `ShellPage`
2. **ShellPage Content Frame** — all aquarium management pages (Dashboard, Lamps, Filters, etc.)

After selecting an aquarium, all pages are scoped to that aquarium context.

### Data Persistence

JSON files stored under `%LOCALAPPDATA%/AquaSync/`. Uses `System.Text.Json` with camelCase policy. Thread-safe via `SemaphoreSlim`.

### Device Integration Threading

- **Chihiros (BLE)**: Event-based. Background thread events → marshal to UI via `DispatcherQueue.TryEnqueue()`
- **Eheim (WebSocket)**: Observable-based. Background emissions → marshal via `ObserveOn(SynchronizationContext)`

## Code Conventions

Enforced by `Directory.Build.props` and `.editorconfig`:

- C# 14, nullable reference types enabled, `TreatWarningsAsErrors: true`
- File-scoped namespaces: `namespace AquaSync.App;`
- Private fields: `_camelCase` prefix
- Prefer `var`, expression-bodied members, pattern matching
- Sealed classes for concrete implementations
- All public async methods take `CancellationToken` parameter; libraries use `ConfigureAwait(false)`
- 4-space indentation (2-space for XML/JSON)

## Key Constraints

- Local-first: no cloud sync, no remote database
- Windows 11 native aesthetic using standard WinUI3 controls only
- English-only, no localization
- Device libraries (Chihiros/Eheim) have complete README.md docs with full API reference
