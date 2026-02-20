# Implementation Plan: Global Settings Page

**Branch**: `002-settings-page` | **Date**: 2026-02-20 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-settings-page/spec.md`

## Summary

Add a global Settings page with five sections: default unit preferences (volume/dimension), theme switching (system/light/dark with immediate effect), data folder location management, data export as ZIP, and an About section. Settings are accessible from both the ShellPage sidebar footer (existing) and the AquariumSelectorPage (new gear icon). An `ISettingsService` singleton loads settings at startup and provides them to all consumers.

## Technical Context

**Language/Version**: C# 14, .NET 10 (`net10.0-windows10.0.19041.0`)
**Primary Dependencies**: WinUI3 (Windows App SDK 1.7), CommunityToolkit.Mvvm 8.4, Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Hosting
**Storage**: JSON files via `IDataService` at `%LOCALAPPDATA%/AquaSync/settings/app-settings.json`
**Testing**: Manual UI testing (no test framework in project)
**Target Platform**: Windows 11 desktop
**Project Type**: WinUI3 desktop application (single project `AquaSync.App`)
**Performance Goals**: Theme switch < 1 second, settings load at startup < 500ms
**Constraints**: Local-first, no cloud, no third-party UI libs, `TreatWarningsAsErrors`, sealed classes, file-scoped namespaces, manual `SetProperty` pattern
**Scale/Scope**: Single-user desktop app, ~15 existing pages/viewmodels

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Windows-Native Design | PASS | Standard WinUI3 controls only: RadioButtons, TextBlock, Button, ContentDialog, InfoBar, ProgressRing |
| II. Local-First Data | PASS | Settings stored as JSON via `IDataService` under `%LOCALAPPDATA%/AquaSync/`. Export is local ZIP. No cloud dependency |
| III. MVVM Architecture | PASS | `SettingsViewModel` extends `ViewModelBase`, manual `SetProperty`, manual `RelayCommand`, registered as Transient in DI |
| IV. Device Integration | N/A | No device interaction in Settings |
| V. Minimal Dependencies | PASS | Uses only `System.IO.Compression.ZipFile` (built-in .NET), no new NuGet packages |
| VI. Single-Aquarium Context | PASS | Settings has dual scope per constitution v1.1.0: global settings section + aquarium-scoped profile editing. Settings also accessible from AquariumSelectorPage via gear icon (global-only mode) |
| VII. English Only | PASS | Hardcoded English strings, no localization infrastructure |

All gates pass. No violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/002-settings-page/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
AquaSync.App/
├── Models/
│   └── AppSettings.cs                      # NEW — settings data model
├── Contracts/Services/
│   └── ISettingsService.cs                 # NEW — settings service interface
├── Services/
│   └── SettingsService.cs                  # NEW — settings service implementation
├── ViewModels/
│   └── SettingsViewModel.cs                # MODIFY — add global settings properties/commands
├── Views/
│   ├── SettingsPage.xaml                   # MODIFY — add global settings UI sections
│   ├── SettingsPage.xaml.cs                # MODIFY — add event handlers for new UI
│   ├── AquariumSelectorPage.xaml           # MODIFY — add gear icon button
│   ├── AquariumSelectorPage.xaml.cs        # MODIFY — add gear icon click handler
│   ├── MainWindow.xaml.cs                  # MODIFY — expose theme setter, navigate to Settings
│   └── ShellPage.xaml.cs                   # MODIFY — handle Settings navigation from footer
├── App.xaml.cs                             # MODIFY — register ISettingsService, apply theme at startup
└── App.xaml                                # MODIFY — (no changes expected, theme set via code)
```

**Structure Decision**: All changes are within the existing `AquaSync.App` project structure. Three new files (model, interface, service) follow established patterns. No new projects or structural changes required.

## Complexity Tracking

No constitution violations. Table not applicable.
