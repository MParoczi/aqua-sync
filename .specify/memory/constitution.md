<!--
  ============================================================================
  SYNC IMPACT REPORT
  ============================================================================
  Version change: N/A → 1.0.0 (initial ratification)
  Modified principles: N/A (initial creation)
  Added sections:
    - Core Principles (7 principles)
    - Technology Stack
    - Project Structure & Constraints
    - Governance
  Removed sections: N/A
  Templates requiring updates:
    - .specify/templates/plan-template.md ✅ no updates needed (generic)
    - .specify/templates/spec-template.md ✅ no updates needed (generic)
    - .specify/templates/tasks-template.md ✅ no updates needed (generic)
    - .specify/templates/checklist-template.md ✅ no updates needed (generic)
    - .specify/templates/agent-file-template.md ✅ no updates needed (generic)
  Follow-up TODOs: None
  ============================================================================
-->

# AquaSync Constitution

## Core Principles

### I. Windows-Native Design

Every UI element MUST use standard WinUI3 controls and follow the
Windows 11 design language. The application MUST look and feel as if
Microsoft developed it.

- MUST use Mica/acrylic backdrop for window chrome.
- MUST use NavigationView for sidebar navigation.
- MUST use ContentDialog for modal forms.
- MUST use InfoBar for alerts and notifications.
- MUST use Fluent Design (Segoe Fluent Icons) for all iconography.
- MUST NOT apply custom styling that deviates from the Windows 11
  aesthetic.
- MUST NOT use third-party UI control libraries unless absolutely
  necessary (e.g., a charting library for water parameter graphs).
  Any such exception MUST be documented and justified.

**Rationale**: A consistent, native appearance reduces user friction,
ensures accessibility compliance via platform controls, and eliminates
the maintenance burden of custom UI frameworks.

### II. Local-First Data

All application data MUST be stored locally on the user's machine as
JSON files under `%LOCALAPPDATA%/AquaSync/`.

- MUST use `System.Text.Json` for all serialization and
  deserialization.
- MUST NOT depend on cloud sync, remote databases, or any
  network-dependent data storage.
- The application MUST function fully offline except when
  communicating with physical aquarium devices on the local network
  or via Bluetooth.

**Rationale**: Local-first storage ensures the application works
without an internet connection, keeps user data private, and avoids
external service dependencies that could break or incur costs.

### III. MVVM Architecture

All UI logic MUST follow the Model-View-ViewModel pattern using
CommunityToolkit.Mvvm and Microsoft.Extensions.DependencyInjection.

- ViewModels MUST inherit from `ViewModelBase` (which extends
  `ObservableObject`).
- MUST use manual `SetProperty` pattern for observable properties.
- MUST NOT use `[ObservableProperty]` attribute on fields (not
  compatible with WinUI3 AOT requirements).
- Views MUST resolve ViewModels from DI via `App.GetService<T>()`.
- Services MUST be registered in `App.xaml.cs`.

**Rationale**: MVVM provides clear separation of concerns, enables
testability of UI logic without UI framework dependencies, and aligns
with the standard WinUI3/XAML development model.

### IV. Device Integration via Existing Libraries

Hardware control for Chihiros LED lamps (BLE) and Eheim Professionel
5E filters (WebSocket) MUST use the existing `AquaSync.Chihiros` and
`AquaSync.Eheim` libraries.

- These libraries are referenced as project dependencies — MUST NOT
  duplicate or rewrite their functionality.
- Chihiros uses event-based patterns — MUST marshal to UI thread via
  `DispatcherQueue`.
- Eheim uses `System.Reactive` `IObservable` patterns — MUST marshal
  via `ObserveOn`.
- Both libraries use `async/await` with `CancellationToken` support
  and all consuming code MUST propagate cancellation tokens.

**Rationale**: The device libraries encapsulate complex hardware
communication protocols. Reusing them prevents duplication, reduces
bugs, and ensures a single source of truth for device interaction.

### V. Minimal Dependencies

NuGet dependencies MUST be kept to a minimum. The core stack is:

- `Microsoft.WindowsAppSDK`
- `CommunityToolkit.Mvvm`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Hosting`

Additional rules:

- MUST NOT add an ORM, database engine, or heavy framework.
- MUST prefer `System.Text.Json` over `Newtonsoft.Json`.
- MUST prefer built-in .NET APIs over wrapper libraries.
- New packages MUST only be added when the framework does not provide
  the functionality natively. Each addition MUST be justified.

**Rationale**: Fewer dependencies reduce supply-chain risk, minimize
binary size, simplify updates, and avoid version conflicts in the
Windows App SDK ecosystem.

### VI. Single-Aquarium Context

After launch, the user selects an aquarium profile from a grid of
cards. All subsequent pages are scoped to that selected aquarium.

- Pages within the shell: Dashboard, Lamps, Filters, Equipment,
  Water Parameters, Maintenance, Gallery, Fertilizers, Plants.
- There MUST NOT be a multi-aquarium view within the shell — the user
  MUST return to the selector to switch aquariums.
- Settings is the only page that is global (not aquarium-scoped).

**Rationale**: A single-aquarium context simplifies state management,
navigation, and data binding. It avoids confusion from multi-context
views and keeps each page focused on one aquarium's data.

### VII. English Only

The application is English-only.

- MUST NOT implement localization infrastructure, resource files, or
  `x:Uid` bindings.
- MUST use hardcoded English strings in XAML and code.
- Localization MAY be added in a future phase but MUST NOT influence
  current architecture decisions.

**Rationale**: Avoiding premature localization infrastructure reduces
code complexity and XAML verbosity. English-only allows direct string
usage without indirection layers.

## Technology Stack

- **Runtime**: .NET 10 (`net10.0-windows10.0.19041.0`)
- **Language**: C# 14 with nullable reference types enabled,
  file-scoped namespaces, `sealed` concrete classes
- **UI Framework**: WinUI3 via Windows App SDK 1.7
- **MVVM**: CommunityToolkit.Mvvm 8.4
- **DI**: Microsoft.Extensions.DependencyInjection + Hosting
- **Data**: JSON files via `System.Text.Json`
- **Device Libraries**: `AquaSync.Chihiros` (BLE,
  `net10.0-windows10.0.19041.0`), `AquaSync.Eheim`
  (WebSocket/System.Reactive, `net10.0`)
- **Build**: SDK-style projects, `Directory.Build.props` for shared
  settings, `TreatWarningsAsErrors` enabled

## Project Structure & Constraints

### Solution Layout

`AquaSync.sln` contains three projects:

| Project | Type | Target |
|---------|------|--------|
| `AquaSync.Chihiros` | Class library (BLE) | `net10.0-windows10.0.19041.0` |
| `AquaSync.Eheim` | Class library (WebSocket) | `net10.0` |
| `AquaSync.App` | WinUI3 desktop app | `net10.0-windows10.0.19041.0` |

### App Folder Structure

```
AquaSync.App/
├── Views/
├── ViewModels/
├── Models/
├── Services/
├── Contracts/Services/
├── Helpers/
├── Converters/
└── Assets/
```

### Navigation Architecture

- `MainWindow` hosts a root `Frame` that switches between
  `AquariumSelectorPage` and `ShellPage`.
- `ShellPage` contains a `NavigationView` with a content `Frame`
  managed by `INavigationService` / `IPageService`.

### Data Storage

- `IDataService` reads/writes JSON files.
- `DataService` implementation uses `SemaphoreSlim` for thread
  safety.

### Hard Constraints

- One equipment item (lamp, filter, or other) is assigned to exactly
  one aquarium — no sharing across aquariums.
- Water parameter ideal ranges are global constants per aquarium
  type — users MUST NOT override them.
- Photos are stored at original resolution with no compression.
- No test reminders, no dosing event logging, no water change
  tracking.
- No templates for aquarium profiles.
- No tags or categories for organizing aquariums.

## Governance

This constitution is the authoritative source of architectural and
design decisions for the AquaSync project. All implementation work
MUST comply with the principles defined herein.

### Amendment Procedure

1. Propose the change with a rationale explaining why the current
   principle is insufficient or incorrect.
2. Document the amendment in this file with updated version number.
3. Verify all dependent templates and artifacts remain consistent
   (see propagation checklist below).
4. Update `LAST_AMENDED_DATE` and increment `CONSTITUTION_VERSION`
   per semantic versioning rules.

### Versioning Policy

- **MAJOR**: Backward-incompatible governance or principle removals
  or redefinitions.
- **MINOR**: New principle or section added, or materially expanded
  guidance.
- **PATCH**: Clarifications, wording, typo fixes, non-semantic
  refinements.

### Compliance Review

- Every feature specification (`spec.md`) MUST reference and align
  with the principles in this constitution.
- Every implementation plan (`plan.md`) MUST include a Constitution
  Check section validating compliance.
- Code reviews MUST verify that implementation follows the
  architectural constraints defined here.

### Propagation Checklist

When this constitution is amended, verify consistency with:

- [ ] `.specify/templates/plan-template.md` — Constitution Check
  section aligns with updated principles.
- [ ] `.specify/templates/spec-template.md` — Scope and requirements
  sections reflect any new mandatory constraints.
- [ ] `.specify/templates/tasks-template.md` — Task categorization
  reflects principle-driven task types.
- [ ] `.specify/templates/checklist-template.md` — Checklist
  categories cover new compliance areas.
- [ ] `.specify/templates/agent-file-template.md` — Development
  guidelines reference current principles.

**Version**: 1.0.0 | **Ratified**: 2026-02-15 | **Last Amended**: 2026-02-15
