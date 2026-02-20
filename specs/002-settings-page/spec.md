# Feature Specification: Global Settings Page

**Feature Branch**: `002-settings-page`
**Created**: 2026-02-20
**Status**: Draft
**Input**: User description: "Settings page accessible from the navigation sidebar footer with global preferences for default units, data folder, theme, data export, and about section."

## Clarifications

### Session 2026-02-20

- Q: The navigation sidebar only exists in ShellPage (after selecting an aquarium). How should Settings be accessed when no aquarium is selected on AquariumSelectorPage? → A: Settings accessible from both AquariumSelectorPage (e.g., gear icon) and ShellPage sidebar.
- Q: Should there be a dedicated "Reset to default" button for the data folder? → A: Yes. A "Reset to default" button appears when a custom path is set, moves data back to the default location.
- Q: What should happen if the user navigates away from Settings during a data folder move? → A: Block navigation — disable navigation and back button during the move so the user must wait for completion.
- Q: Should long-running operations (export, data folder move) support cancellation? → A: No. Operations run to completion once started. No cancel button is shown.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Set Default Unit Preferences (Priority: P1)

A user wants to configure their preferred units of measurement so that every new aquarium profile is pre-filled with those defaults. The user navigates to the Settings page from the sidebar footer, selects their preferred volume unit (liters or gallons) and dimension unit (centimeters or inches), and the preference is saved immediately. The next time they create an aquarium profile, the unit dropdowns default to the chosen values.

**Why this priority**: Default units directly affect the most common workflow — creating aquarium profiles. Users in metric or imperial regions want their preferred units pre-selected to reduce repetitive manual selection.

**Independent Test**: Can be fully tested by changing defaults in Settings, then creating a new aquarium profile and verifying the unit dropdowns match the chosen defaults.

**Acceptance Scenarios**:

1. **Given** the user has not changed any settings, **When** they open the Settings page, **Then** the default volume unit is "Liters" and the default dimension unit is "Centimeters".
2. **Given** the user sets default volume to "Gallons" and dimension to "Inches", **When** they create a new aquarium profile, **Then** the volume unit dropdown defaults to "Gallons" and the dimension unit dropdown defaults to "Inches".
3. **Given** the user changes the default units, **When** they close and reopen the application, **Then** the Settings page reflects the previously saved defaults.
4. **Given** an existing aquarium profile was created with "Liters", **When** the user changes the default to "Gallons", **Then** the existing profile remains unchanged at "Liters".
5. **Given** the user is on the AquariumSelectorPage with no aquarium selected, **When** they open Settings via the gear icon, **Then** the Settings page shows only global settings sections (no aquarium-scoped profile section) and the user can navigate back to AquariumSelectorPage.

---

### User Story 2 - Switch Application Theme (Priority: P2)

A user wants to switch the application between light and dark modes to suit their environment or preference. The user navigates to the Settings page and chooses between "Follow system theme", "Always light", or "Always dark". The theme change takes effect immediately without restarting the application.

**Why this priority**: Theme preference is a high-visibility setting that affects everyday comfort and usability. Immediate visual feedback makes this a satisfying and impactful feature.

**Independent Test**: Can be fully tested by selecting each theme option and visually confirming the application appearance changes immediately.

**Acceptance Scenarios**:

1. **Given** the user has not changed the theme setting, **When** they open the Settings page, **Then** "Follow system theme" is selected.
2. **Given** the user selects "Always dark", **When** the selection is made, **Then** the entire application switches to dark mode immediately without restart.
3. **Given** the user selects "Always light", **When** the selection is made, **Then** the entire application switches to light mode immediately without restart.
4. **Given** the user selects "Follow system theme", **When** the operating system theme changes, **Then** the application follows the system theme automatically.
5. **Given** the user previously selected "Always dark", **When** they close and reopen the application, **Then** the app launches in dark mode and the setting is preserved.

---

### User Story 3 - Export Application Data (Priority: P3)

A user wants to create a backup of all their aquarium data. The user navigates to the Settings page, clicks the export button, chooses a save location, and receives a ZIP archive containing all application data — aquarium profiles, water parameter logs, maintenance logs, equipment configurations, fertilizer plans, plant records, and photos.

**Why this priority**: Data export provides essential backup capability and peace of mind. Users with extensive aquarium records need to safeguard their data against loss.

**Independent Test**: Can be fully tested by clicking export, selecting a save location, and verifying the resulting ZIP contains all expected data files and photos.

**Acceptance Scenarios**:

1. **Given** the user has aquarium data, **When** they click the export button and choose a save location, **Then** a ZIP archive is created at the chosen location containing all application data. The default filename MUST be `AquaSync-Export-YYYY-MM-DD.zip` (with the current date), and the user may change the filename in the save dialog.
2. **Given** the export is in progress, **When** the user observes the interface, **Then** a progress indicator is visible showing the export is running.
3. **Given** the export completes successfully, **When** the user is notified, **Then** a success message confirms the export location.
4. **Given** the user has no aquarium data, **When** they attempt to export, **Then** the system informs them there is no data to export.
5. **Given** the user cancels the save location picker, **When** they return to the Settings page, **Then** no export is performed and no error is shown.

---

### User Story 4 - Change Data Folder Location (Priority: P4)

A user wants to move their application data to a different folder — for example, to a larger drive or a cloud-synced folder. The user navigates to the Settings page, sees the current data folder path, clicks a browse button, selects a new folder, and the application moves all existing data to the new location.

**Why this priority**: This is an advanced feature for users with specific storage needs. While valuable, most users will use the default location, making this a lower-priority enhancement.

**Independent Test**: Can be fully tested by browsing to a new folder, confirming the move, and verifying all data files and photos are present in the new location and the application continues to function correctly.

**Acceptance Scenarios**:

1. **Given** the user opens the Settings page, **When** they view the data folder section, **Then** the current data folder path is displayed.
2. **Given** the user clicks the browse button and selects a new folder, **When** the system prompts for confirmation, **Then** a confirmation dialog shows the source path, destination path, and a warning that the operation may take time depending on data size.
3. **Given** the user confirms the folder change, **When** the move is in progress, **Then** a progress indicator is shown and navigation away from the Settings page is blocked (back button and sidebar items are disabled) until the operation completes.
4. **Given** the data move completes successfully, **When** the user continues using the application, **Then** all data reads and writes use the new folder location.
5. **Given** the user opens the folder picker and cancels, **When** they return to the Settings page, **Then** no changes are made and no error is shown.
6. **Given** the user has previously moved the data folder to a custom location, **When** they view the data folder section, **Then** a "Reset to default" button is visible. Clicking it moves data back to the default location using the same confirmation and move flow.
7. **Given** the data folder is already at the default location, **When** the user views the data folder section, **Then** no "Reset to default" button is shown.

---

### User Story 5 - View Application Information (Priority: P5)

A user wants to check what version of the application they are running. The user navigates to the Settings page and scrolls to the About section, which displays the application name, version number, and a brief description.

**Why this priority**: The About section is purely informational with no interactive behavior. It is low effort and low risk but provides useful reference information.

**Independent Test**: Can be fully tested by navigating to Settings and verifying the About section displays the correct application name, current version, and description text.

**Acceptance Scenarios**:

1. **Given** the user navigates to the Settings page, **When** they view the About section, **Then** the application name "AquaSync" is displayed.
2. **Given** the user views the About section, **When** they read the version, **Then** it matches the currently installed application version.
3. **Given** the user views the About section, **When** they read the description, **Then** a one-sentence description of the application's purpose is shown (e.g., "A desktop application for managing home aquariums, controlling LED lights, and monitoring filters.").

---

### Edge Cases

- What happens when the user selects the same folder as the current data folder for the data folder change? The system detects this and informs the user that no change is needed.
- What happens when the destination folder for a data move is not empty? The system warns the user and requires confirmation before proceeding to prevent accidental data mixing.
- What happens when a data move fails midway (e.g., disk full, permission denied)? The system rolls back to the original folder, leaves data intact at the source, deletes any partial copy at the destination, and displays an error message explaining the failure (e.g., "Could not move data: insufficient disk space" or "Could not move data: permission denied").
- What happens when the user tries to export data while a data folder move is in progress? The export button is disabled until the move completes.
- What happens when the export ZIP would be very large (e.g., many gallery photos)? The progress indicator reflects actual progress, and the UI remains responsive throughout the operation. Both export and data folder move operations run on a background thread to keep the UI non-blocking.
- What happens when the custom data folder path is invalid on application startup (e.g., external drive removed, folder deleted)? The system falls back to the default data folder location and displays a warning on the Settings page informing the user that the custom path was inaccessible.
- What happens when the export destination has insufficient disk space or the path is read-only? The system displays an error message explaining the failure (e.g., "Could not export data: insufficient disk space" or "Could not export data: permission denied").
- What happens when only some of the data types listed in FR-014 exist (e.g., no water parameter logs have been created yet)? The export includes whatever data currently exists in the data folder. Missing data types are simply absent from the archive — this is not an error.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Settings page MUST be accessible from the ShellPage navigation sidebar footer item when an aquarium is selected, and MUST also be accessible from the AquariumSelectorPage (e.g., via a gear icon) when no aquarium is selected. When accessed from AquariumSelectorPage, only global settings sections are shown (no aquarium-scoped profile section). The user MUST be able to navigate back to AquariumSelectorPage from the Settings page.
- **FR-002**: Settings MUST be global — they apply across all aquarium profiles, not scoped to any individual profile.
- **FR-003**: The user MUST be able to set a default volume unit of measurement (liters or gallons).
- **FR-004**: The user MUST be able to set a default dimension unit of measurement (centimeters or inches).
- **FR-005**: Default unit preferences MUST be pre-selected when creating a new aquarium profile but MUST be changeable per profile during creation.
- **FR-006**: The default volume unit MUST default to "Liters" and the default dimension unit MUST default to "Centimeters" when no preference has been set.
- **FR-007**: The Settings page MUST display the current data folder path.
- **FR-008**: The user MUST be able to browse and select a different data folder location.
- **FR-009**: When the data folder is changed, a confirmation dialog MUST be shown displaying the source and destination paths. After confirmation, all existing data MUST be moved to the new location without data loss.
- **FR-010**: The user MUST be able to select a theme from three options: "Follow system theme" (default), "Always light", or "Always dark".
- **FR-011**: Theme changes MUST take effect immediately (within 1 second) without requiring an application restart.
- **FR-012**: The selected theme MUST persist across application sessions.
- **FR-013**: The user MUST be able to export all application data as a ZIP archive to a user-chosen location. The default filename MUST be `AquaSync-Export-YYYY-MM-DD.zip` with the current date.
- **FR-014**: The data export MUST include all data currently present in the application data folder. This includes aquarium profiles, gallery photos, and any other data types that exist at the time of export (e.g., water parameter logs, maintenance logs, equipment configurations, fertilizer plans, plant records as they are implemented).
- **FR-015**: The About section MUST display the application name ("AquaSync"), version number, and a one-sentence description of the application's purpose.
- **FR-016**: All settings changes MUST persist across application restarts.
- **FR-017**: The data folder move operation MUST show progress feedback and keep the UI responsive by running on a background thread. Navigation MUST be blocked (back button and sidebar items disabled) during the move to prevent leaving the page mid-operation. On failure (disk full, permission denied, or partial copy), the system MUST roll back by deleting any partial copy at the destination, keeping original data intact, and displaying an error message identifying the cause.
- **FR-018**: The data export operation MUST show progress feedback, keep the UI responsive by running on a background thread, and allow the user to choose the save location before starting. On failure (disk full, permission denied), the system MUST display an error message identifying the cause.
- **FR-019**: When the data folder has been moved to a custom location, a "Reset to default" button MUST be shown. Clicking it MUST move data back to the default location using the same confirmation and move flow as FR-009.
- **FR-020**: Export and data folder move operations MUST NOT support user-initiated cancellation. Once started, they run to completion.

### Key Entities

- **AppSettings**: Represents the persisted global settings including default volume unit, default dimension unit, selected theme, and data folder path.
- **DataExport**: Represents a single export operation producing a ZIP archive containing all application data and photos.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can change default unit preferences and see them reflected in the next aquarium profile creation within the same session.
- **SC-002**: Users can switch between all three theme options and see the visual change take effect within 1 second, without restarting the application.
- **SC-003**: Users can export all application data to a ZIP archive in a single action from the Settings page.
- **SC-004**: Users can relocate the data folder and continue using the application without errors or restart, with all previously saved data accessible in the new location.
- **SC-005**: Users can identify the application version from the About section on the Settings page.
- **SC-006**: All settings persist correctly across application restarts with no data loss.
