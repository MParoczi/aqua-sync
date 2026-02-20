# Research: Aquarium Profile Management

**Branch**: `001-aquarium-profiles` | **Date**: 2026-02-15

## Overview

All technical context was already well-defined by the existing codebase, constitution, and user input. No NEEDS CLARIFICATION items existed in the technical context. Research focused on confirming best practices for the chosen patterns.

---

## R1: GridView Card Layout in WinUI3

**Decision**: Use `GridView` with `ItemTemplate` containing a custom `DataTemplate` for aquarium cards.

**Rationale**: GridView is the standard WinUI3 control for displaying collections of items in a grid layout. It supports virtualization out of the box, handles dynamic resizing, and follows Windows 11 design patterns. The `ItemTemplate` allows full customization of card appearance (thumbnail, name, type, volume).

**Alternatives considered**:
- `ItemsRepeater` with `UniformGridLayout`: More flexible but requires manual scroll handling. GridView provides built-in scrolling and selection. Overkill for this use case.
- Custom panel with manual layout: Violates constitution principle I (must use standard WinUI3 controls).

**Key implementation notes**:
- Use `AdaptiveGridView` behavior via `ItemWidth`/`DesiredWidth` properties to auto-flow cards based on window width.
- GridView's `IsItemClickEnabled="True"` with `ItemClick` event for navigation.
- Use `Visibility` binding to toggle between GridView and empty state `StackPanel`.

---

## R2: ContentDialog for Forms in WinUI3

**Decision**: Use `ContentDialog` for profile creation and substrate entry forms. Constitution mandates this ("MUST use ContentDialog for modal forms").

**Rationale**: ContentDialog is the native WinUI3 modal dialog with built-in primary/secondary/close button patterns, backdrop dimming, and accessibility support. It aligns with Windows 11 design language.

**Alternatives considered**:
- Full-page navigation for profile creation: Would work but constitution explicitly requires ContentDialog for modal forms. Also, a dialog keeps the context of the selector page visible.
- TeachingTip or Flyout: Too small for a multi-field form. Not designed for data entry.

**Key implementation notes**:
- ContentDialog `XamlRoot` must be set to the page's `XamlRoot` (WinUI3 requirement, unlike UWP).
- Use `ScrollViewer` inside the dialog for long forms.
- `IsPrimaryButtonEnabled` bound to a validation property on the ViewModel.
- For substrate entry within the creation dialog: use a nested ContentDialog or an `Expander` control with inline fields.
- ContentDialog maximum width/height can be overridden via `Style` to accommodate the form.

---

## R3: File Picker for Image Upload in WinUI3

**Decision**: Use `Windows.Storage.Pickers.FileOpenPicker` with `FileTypeFilter` set to supported image formats.

**Rationale**: FileOpenPicker is the built-in WinUI3/WinRT file picker that integrates with the Windows shell, providing a native experience including recent files, quick access, and folder navigation.

**Alternatives considered**:
- Drag-and-drop only: Less discoverable. FileOpenPicker is the standard approach.
- Third-party file dialog: Violates constitution principle V (minimal dependencies).

**Key implementation notes**:
- In WinUI3 (non-packaged), the picker requires `WinRT.Interop.InitializeWithWindow` to set the window handle.
- After picking, copy the file to `gallery/{aquariumId}/thumbnail.{ext}` using `File.Copy` or `StorageFile.CopyAsync`.
- Store the relative path (`gallery/{id}/thumbnail.jpg`) in the aquarium JSON.
- Supported filters: `.jpg`, `.jpeg`, `.png`, `.bmp`, `.gif`, `.webp`.

---

## R4: Per-Item JSON Storage Pattern

**Decision**: Store each aquarium as an individual JSON file named `{guid}.json` in the `aquariums/` folder. Substrate entries are embedded within the aquarium JSON as a list.

**Rationale**: Individual files per entity provide natural isolation (no locking contention between different aquariums), simple CRUD (create = write file, delete = remove file), and human-readable storage for debugging. The existing `IDataService` already supports this pattern with `ReadAsync`/`SaveAsync`/`DeleteAsync` taking folder and file name parameters.

**Alternatives considered**:
- Single `aquariums.json` file with all profiles: Simpler reads but creates write contention and grows unboundedly. A single corrupt write could lose all profiles.
- SQLite database: Violates constitution principle V (MUST NOT add ORM or database engine) and principle II (JSON files under LOCALAPPDATA).
- Separate files for substrates: Unnecessary complexity. Substrates are always loaded with their parent aquarium and have no independent identity.

**Key implementation notes**:
- GUID as filename ensures uniqueness without coordination.
- `ReadAllAsync<T>` enumerates the folder via `Directory.GetFiles(path, "*.json")` and deserializes each.
- Thread safety maintained by `DataService`'s existing `SemaphoreSlim` lock.
- File operations are atomic at the OS level for single-file writes (no partial corruption risk for small JSON files).

---

## R5: Aquarium Context Service Pattern

**Decision**: Use a singleton `IAquariumContext` service to hold the currently selected aquarium throughout the management shell session.

**Rationale**: Avoids threading the aquarium ID through every navigation parameter. Child ViewModels simply inject `IAquariumContext` and access the current aquarium. Clean separation between "which aquarium" (context) and "what to do with it" (view-specific logic).

**Alternatives considered**:
- Pass aquarium ID as navigation parameter to every child page: Works but creates boilerplate in every ViewModel's `OnNavigatedTo`. Error-prone if a page forgets to read the parameter.
- Static/global variable: Violates DI pattern established by the constitution.
- ShellViewModel as shared context: ShellViewModel is registered as transient, not singleton. Would require changing its lifetime, which conflicts with the existing pattern.

**Key implementation notes**:
- `IAquariumContext` exposes: `Aquarium? CurrentAquarium`, `bool IsReadOnly`, `void SetCurrentAquarium(Aquarium, bool isReadOnly)`, `void Clear()`.
- Set when entering ShellPage, cleared when returning to selector.
- `IsReadOnly` is true when viewing an archived aquarium — ViewModels check this to disable editing.
