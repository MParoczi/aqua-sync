# Feature Specification: Aquarium Profile Management

**Feature Branch**: `001-aquarium-profiles`
**Created**: 2026-02-15
**Status**: Draft
**Input**: User description: "Aquarium profile management system — grid-based selector with profile creation, substrate/additive tracking, archival/deletion, and unit-of-measurement locking per profile."

## Clarifications

### Session 2026-02-15

- Q: Can users edit core profile fields after creation? → A: Only name, notes, and thumbnail photo are editable. Aquarium type, volume, dimensions, units, and setup date are locked after creation.
- Q: What happens when a user clicks an archived profile in the grid? → A: The management shell opens in read-only mode — users can browse all data but cannot make changes. Restoration is not required to view.
- Q: Where does profile editing live given the constitution declares Settings as global? → A: The Settings page has dual scope — a global section for app-wide settings and an aquarium-scoped section for profile editing when inside the management shell. Constitution Principle VI amended to reflect this.
- Q: Should SetupDate be date-only or date+time? → A: Date-only. Users select a calendar date; time is not relevant for aquarium setup.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Aquarium Selector Grid (Priority: P1)

When the user launches AquaSync, they see a grid of aquarium profile cards. Each card displays the aquarium's thumbnail photo (or a default graphic), name, aquarium type, volume with unit, and setup date. The entire card surface is clickable to select the aquarium. Cards are ordered with active profiles first, then archived profiles; within each group, newest profiles (by creation date) appear first. If no profiles exist, the grid shows an empty state with an illustration, a message ("No aquariums yet"), and a "Create your first aquarium" button.

The grid uses an adaptive layout that auto-flows cards based on window width. A special "Add Aquarium" card appears as the first item in the grid, providing a persistent entry point for profile creation.

**Why this priority**: This is the application's landing screen and primary navigation hub. Without it, users cannot access any aquarium-specific functionality.

**Independent Test**: Can be fully tested by launching the application and verifying the grid renders with existing profile data (or empty state), and delivers the core navigation entry point.

**Acceptance Scenarios**:

1. **Given** the user has no aquarium profiles, **When** the application launches, **Then** the selector grid displays an empty state with an illustration, a "No aquariums yet" message, and a "Create your first aquarium" button.
2. **Given** the user has one or more aquarium profiles, **When** the application launches, **Then** the selector grid displays an "Add Aquarium" card first, followed by profile cards showing thumbnail, name, aquarium type, volume (with unit), and setup date.
3. **Given** the user has both active and archived profiles, **When** the selector grid loads, **Then** active profiles appear before archived profiles. Archived profiles are visually distinct with 50% opacity and an "Archived" badge overlay.
4. **Given** the selector grid is displayed, **When** the user clicks the "Add Aquarium" card (or the empty state button), **Then** the profile creation form opens.
5. **Given** the selector grid has many profiles, **When** the grid renders, **Then** it supports virtualized scrolling to maintain performance.
6. **Given** the user right-clicks (or long-presses) a profile card, **When** the context menu appears, **Then** it shows actions appropriate to the card's status: "Archive" (for active), "Restore" (for archived), and "Delete" (for both).

---

### User Story 2 - Create a New Aquarium Profile (Priority: P1)

The user creates a new aquarium profile by filling in a creation form. Required fields: name, volume (with liters/gallons toggle, defaulting to liters), dimensions in length x width x height (with cm/inches toggle, defaulting to centimeters), aquarium type (freshwater, saltwater, or brackish), and setup date (defaulting to today's date, date-only — no time component). Optional fields: notes and thumbnail photo.

The chosen unit of measurement for volume and dimensions is locked to that profile permanently. If no photo is uploaded, a default aquarium graphic is assigned. If a photo is uploaded, a preview is shown before saving. Photos must be JPEG, PNG, BMP, GIF, or WebP format with a maximum file size of 10MB. Photos are stored at original resolution with no compression.

The form supports scrolling when content exceeds the visible area. Validation occurs when the user attempts to save, with inline error indicators on invalid fields. If the user cancels the form and has entered data, a confirmation prompt asks whether to discard changes.

**Why this priority**: Profile creation is the prerequisite for all other aquarium management features.

**Independent Test**: Can be fully tested by opening the creation form, filling in all fields, saving, and verifying the new profile appears in the selector grid with correct data.

**Acceptance Scenarios**:

1. **Given** the user opens the profile creation form, **When** the form loads, **Then** the volume unit defaults to liters, the dimension unit defaults to centimeters, and the setup date defaults to today's date.
2. **Given** the user fills in all required fields, **When** they save the profile, **Then** it appears in the selector grid and a brief success notification is shown.
3. **Given** the user is creating a profile, **When** they toggle volume unit to gallons, **Then** the volume input accepts gallons and all future volume displays for this profile use gallons.
4. **Given** the user is creating a profile, **When** they toggle dimension unit to inches, **Then** the dimension inputs accept inches and all future dimension displays for this profile use inches.
5. **Given** the user is creating a profile, **When** they do not upload a thumbnail photo, **Then** a default aquarium graphic is assigned to the profile.
6. **Given** the user is creating a profile, **When** they upload a thumbnail photo, **Then** a preview of the image is shown in the form, and upon saving, the image is stored and displayed as the profile's thumbnail.
7. **Given** the user is creating a profile, **When** they attempt to save without filling in a required field, **Then** the system shows inline validation errors on the missing fields and prevents saving.
8. **Given** the user is creating a profile, **When** they attempt to upload a file larger than 10MB or an unsupported format, **Then** the system shows an error and does not accept the file.
9. **Given** the user has entered data in the creation form, **When** they cancel the form, **Then** the system prompts for confirmation before discarding the unsaved data.
10. **Given** the user enters a name that matches an existing profile, **When** the name field loses focus, **Then** an inline warning appears noting a profile with that name already exists (saving is still allowed).

---

### User Story 3 - Register Substrates and Additives (Priority: P2)

As part of the aquarium profile, the user registers one or more substrate and additive entries. Each entry includes: brand, product name, type (substrate, additive, or soil cap), layer depth, date added, and optional notes. Layer depth uses the same dimension unit (cm/inches) as the parent aquarium profile. Multiple entries are supported to represent layered setups (e.g., base layer of Power Sand + top layer of Amazonia + root tabs). The number of substrate entries per profile is unbounded.

Substrates can be added during profile creation or managed later from the Settings page within the management shell. Entries can be reordered using up/down controls to reflect their physical layering. If the user cancels a substrate entry mid-edit, the partial entry is discarded with no side effects.

**Why this priority**: Substrate tracking is an important part of the aquarium profile data model, but the core profile can function without it.

**Independent Test**: Can be fully tested by creating a profile, adding multiple substrate/additive entries, and verifying they are saved and displayed correctly with all fields.

**Acceptance Scenarios**:

1. **Given** the user is creating or editing an aquarium profile, **When** they add a substrate entry with brand, product name, type "substrate", layer depth, and date added, **Then** the entry is saved and associated with the profile.
2. **Given** the user has added one substrate entry, **When** they add another entry (e.g., a soil cap), **Then** both entries are listed in their display order.
3. **Given** a substrate/additive entry exists, **When** the user edits its details, **Then** the updated values are saved.
4. **Given** a substrate/additive entry exists, **When** the user removes it, **Then** the entry is deleted from the profile.
5. **Given** the user is adding a substrate entry, **When** they select the type field, **Then** they can choose from "Substrate", "Additive", or "Soil Cap".
6. **Given** multiple substrate entries exist, **When** the user uses the reorder controls (up/down), **Then** the display order updates accordingly and persists.
7. **Given** the user starts adding a substrate entry but cancels, **When** the entry form is dismissed, **Then** no partial data is saved.
8. **Given** the parent aquarium uses centimeters, **When** the user enters a layer depth, **Then** the depth is labeled and stored in centimeters.

---

### User Story 4 - Select Aquarium and Enter Management Shell (Priority: P1)

When the user clicks on an active aquarium card in the selector grid, the application navigates to that aquarium's management shell. The shell header displays the aquarium name, aquarium type, and (for archived profiles) a read-only indicator. The sidebar navigation contains: Dashboard, Lamps, Filters, Other Equipment, Water Parameters, Maintenance, Gallery, Fertilizers, Plants, and Settings. All pages within the shell are scoped to the selected aquarium.

A back button in the shell navigates back to the aquarium selector grid. When returning to the selector, the profile list refreshes to reflect any changes made during the shell session.

**Why this priority**: This is the gateway from the selector grid to all aquarium-specific management. Without it, the grid leads nowhere.

**Independent Test**: Can be fully tested by selecting an aquarium from the grid and verifying the management shell loads with the correct sidebar navigation items and the selected aquarium's context.

**Acceptance Scenarios**:

1. **Given** the selector grid is displayed with aquarium profiles, **When** the user clicks on an active aquarium card, **Then** the application navigates to that aquarium's management shell.
2. **Given** the user is in the management shell, **When** they view the sidebar, **Then** it contains navigation items: Dashboard, Lamps, Filters, Other Equipment, Water Parameters, Maintenance, Gallery, Fertilizers, Plants, and Settings.
3. **Given** the user is in the management shell, **When** they view the header area, **Then** it displays the aquarium name and aquarium type.
4. **Given** the user is in the management shell, **When** they navigate between sidebar sections, **Then** each page displays data scoped to the selected aquarium.
5. **Given** the user is in the management shell, **When** they click the back button, **Then** the application returns to the selector grid with a refreshed profile list.
6. **Given** the user is in the management shell, **When** a progress-dependent operation is running (loading data, saving), **Then** a progress indicator is shown.

---

### User Story 5 - Archive an Aquarium Profile (Priority: P3)

The user can archive (decommission) an aquarium profile via the context menu on the profile card in the selector grid. Archiving preserves all historical data but marks the profile as inactive. Archived profiles appear visually distinct in the selector grid with 50% opacity and an "Archived" badge overlay. An archived profile can be restored to active status via the same context menu.

Clicking an archived card enters the management shell in read-only mode. In read-only mode, all controls are disabled and a banner at the top of the shell indicates the profile is archived and read-only, with an option to restore.

**Why this priority**: Archival supports long-term use of the application when aquariums are decommissioned.

**Independent Test**: Can be fully tested by archiving an active profile, verifying its visual distinction in the grid, entering the read-only shell, and restoring it back to active.

**Acceptance Scenarios**:

1. **Given** an active aquarium profile exists, **When** the user selects "Archive" from the card's context menu, **Then** the system asks for confirmation ("This aquarium will be archived. You can restore it later. Archive?").
2. **Given** the user confirms archival, **When** the archival completes, **Then** the profile is marked as archived, the card displays with 50% opacity and an "Archived" badge, and a success notification is shown.
3. **Given** an archived profile exists in the grid, **When** the user selects "Restore" from its context menu, **Then** the profile returns to active status and appears normally in the grid.
4. **Given** an archived profile exists in the grid, **When** the user clicks on it, **Then** the management shell opens in read-only mode with a banner stating "This aquarium is archived (read-only). Restore to make changes." All editing controls are disabled.
5. **Given** the user is in the read-only shell, **When** they click "Restore" on the banner, **Then** the profile is restored and the shell switches to normal (editable) mode.

---

### User Story 6 - Delete an Aquarium Profile (Priority: P3)

The user can permanently delete an aquarium profile (active or archived) via the context menu on the profile card. Deletion removes the profile data file, all associated substrate data, and the gallery folder including the thumbnail image. A confirmation dialog warns that this action is permanent and irreversible.

**Why this priority**: Permanent deletion is a necessary housekeeping feature but is secondary to creation, viewing, and archival.

**Independent Test**: Can be fully tested by deleting a profile, confirming the action, and verifying the profile no longer appears in the grid.

**Acceptance Scenarios**:

1. **Given** an aquarium profile exists (active or archived), **When** the user selects "Delete" from the card's context menu, **Then** the system displays a confirmation dialog: "Permanently delete [Aquarium Name]? This will remove all profile data, substrates, and photos. This action cannot be undone."
2. **Given** the confirmation dialog is displayed, **When** the user confirms deletion, **Then** the profile, all substrates, and the gallery folder are permanently removed. A success notification is shown.
3. **Given** the confirmation dialog is displayed, **When** the user cancels, **Then** the profile remains unchanged.
4. **Given** the user deletes the only remaining profile, **When** the deletion completes, **Then** the selector grid returns to the empty state.

---

### Edge Cases

- **Volume/dimension validation**: Volume and all dimensions (length, width, height) must be positive numbers greater than zero. The system accepts the user's system locale decimal separator (comma or period).
- **Unsupported image file**: The system validates the file is a supported format (JPEG, PNG, BMP, GIF, WebP) and under 10MB. Unsupported or oversized files are rejected with a specific error message.
- **Duplicate aquarium name**: The system allows duplicate names (different aquariums may share names) but shows an inline warning when a duplicate is detected. This is a deliberate design decision — users commonly have multiple similar setups.
- **Delete last profile**: Deletion proceeds normally; the user returns to the empty state in the selector grid.
- **Long text input**: The name field has a firm limit of 100 characters. The notes field has a firm limit of 2,000 characters. Input is truncated or prevented beyond these limits.
- **Storage inaccessible or full**: The system displays an error message: "Could not save profile. Please check disk space and permissions." The previous data state is preserved.
- **Corrupted aquarium JSON file**: If a profile's JSON file cannot be deserialized, the system skips that file and loads all other profiles. A warning notification informs the user that one or more profiles could not be loaded.
- **Missing gallery folder**: If the thumbnail path in the profile references a missing file or folder, the system silently falls back to the default aquarium graphic.
- **Photo upload failure**: If the file copy fails during thumbnail upload (permissions, interrupted), the previous thumbnail is preserved (or default graphic retained) and an error is shown.
- **Large number of profiles**: The selector grid supports virtualized scrolling so that performance remains acceptable even with 100+ profiles.
- **Cancel with unsaved data**: If the user cancels the creation form after entering data, a confirmation prompt asks "Discard changes?" before closing.
- **Substrate entry cancel**: If the user cancels a substrate entry mid-edit, the partial data is discarded with no side effects.
- **External data deletion**: If the aquarium JSON file is deleted externally while the management shell is open, the system shows an error notification and navigates back to the selector grid.
- **Numeric precision**: Volume and dimensions accept decimal values (e.g., 60.5 liters, 30.5 cm). Display uses up to 1 decimal place; stored values preserve input precision.

## Requirements *(mandatory)*

### Functional Requirements

**Selector Grid**

- **FR-001**: System MUST display a grid of aquarium profile cards as the application's landing screen.
- **FR-002**: Each aquarium card MUST show the profile's thumbnail image (or default graphic), name, aquarium type, volume with unit of measurement, and setup date. The entire card surface MUST be clickable.
- **FR-003**: System MUST display an empty state when no aquarium profiles exist, including an illustration, a "No aquariums yet" message, and a button to create the first profile.
- **FR-004**: The grid MUST include an "Add Aquarium" card as the first item, providing a persistent entry point for profile creation.
- **FR-005**: Cards MUST be ordered with active profiles first, then archived profiles. Within each group, cards MUST be ordered by creation date descending (newest first).
- **FR-006**: The grid MUST use an adaptive layout that adjusts the number of columns based on window width and MUST support virtualized scrolling for performance.
- **FR-007**: Each card MUST have a context menu (right-click or overflow button) with status-appropriate actions: "Archive" (for active cards), "Restore" (for archived cards), and "Delete" (for all cards).

**Profile Creation**

- **FR-008**: System MUST provide a profile creation form with fields for: name (required, max 100 characters), volume (required, positive number), volume unit toggle (liters/gallons, required, default: liters), dimensions — length, width, height (required, positive numbers), dimension unit toggle (cm/inches, required, default: centimeters), aquarium type (freshwater/saltwater/brackish, required), setup date (required, date-only, default: today), notes (optional, max 2,000 characters), and thumbnail photo (optional, max 10MB, JPEG/PNG/BMP/GIF/WebP).
- **FR-009**: System MUST lock the chosen volume unit of measurement to the profile permanently upon creation. All volume-related values for that profile MUST use the selected unit throughout the application.
- **FR-010**: System MUST lock the chosen dimension unit of measurement to the profile permanently upon creation. All dimension-related values for that profile MUST use the selected unit throughout the application.
- **FR-011**: System MUST assign a default aquarium graphic as the thumbnail when the user does not upload a photo. The default graphic MUST be a static asset bundled with the application.
- **FR-012**: When a thumbnail photo is uploaded, the system MUST show a preview of the image in the form before saving. Photos MUST be stored at original resolution with no compression.
- **FR-013**: The creation form MUST support scrolling when content exceeds the visible area.
- **FR-014**: Validation MUST occur on save attempt with inline error indicators on invalid fields. A confirmation prompt MUST appear when canceling a form with unsaved data.
- **FR-015**: The system MUST accept the user's system locale decimal separator for numeric inputs (volume, dimensions).

**Profile Editing**

- **FR-016**: System MUST allow users to edit the following profile fields after creation: name, notes, and thumbnail photo. Editing MUST be accessible from the Settings page within the management shell.
- **FR-017**: System MUST NOT allow editing of aquarium type, volume, volume unit, dimensions, dimension unit, or setup date after profile creation. These fields MUST be displayed as read-only with a visual lock indicator.

**Substrate / Additive Management**

- **FR-018**: System MUST support registering multiple substrate/additive entries per aquarium profile (unbounded count), each with: brand (required), product name (required), type — substrate, additive, or soil cap (required), layer depth (required, positive number), date added (required, date-only), and notes (optional).
- **FR-019**: Layer depth MUST use the same dimension unit (cm/inches) as the parent aquarium profile.
- **FR-020**: System MUST allow users to add, edit, remove, and reorder (via up/down controls) substrate/additive entries. Substrates can be managed during profile creation and from the Settings page in the management shell.
- **FR-021**: Canceling a substrate entry mid-edit MUST discard the partial entry with no side effects.

**Navigation & Management Shell**

- **FR-022**: System MUST navigate to the aquarium's management shell when the user clicks an active profile card in the grid.
- **FR-023**: The management shell MUST include a header displaying the aquarium name, aquarium type, and (for archived profiles) a read-only indicator.
- **FR-024**: The management shell MUST include a sidebar with navigation items: Dashboard, Lamps, Filters, Other Equipment, Water Parameters, Maintenance, Gallery, Fertilizers, Plants, and Settings.
- **FR-025**: All pages within the management shell MUST be scoped to the selected aquarium profile's data.
- **FR-026**: A back button in the shell MUST navigate to the aquarium selector grid. Upon return, the grid MUST refresh its profile list.
- **FR-027**: The Settings page in the management shell MUST have dual scope: a global section for app-wide settings and an aquarium-scoped section for profile editing when an aquarium is active.

**Archival & Deletion**

- **FR-028**: System MUST allow users to archive an aquarium profile (via card context menu), preserving all data while marking it as inactive. A confirmation dialog MUST be shown before archiving.
- **FR-029**: Archived profiles MUST appear visually distinct in the selector grid with 50% opacity and an "Archived" badge overlay.
- **FR-030**: System MUST allow users to open an archived profile's management shell in read-only mode. All editing controls MUST be disabled and a banner MUST indicate "This aquarium is archived (read-only)" with an option to restore.
- **FR-031**: System MUST allow users to restore an archived profile back to active status (via card context menu or read-only shell banner).
- **FR-032**: System MUST allow users to permanently delete an aquarium profile (active or archived) via card context menu, with a confirmation dialog warning that the action is permanent and irreversible.
- **FR-033**: Permanent deletion MUST remove the profile JSON file, all embedded substrate data, and the gallery folder including the thumbnail image.

**Data & Validation**

- **FR-034**: System MUST validate all required fields before allowing profile creation (name, volume, dimensions, aquarium type, setup date).
- **FR-035**: System MUST validate that volume and dimension values are positive numbers greater than zero.
- **FR-036**: System MUST persist all aquarium profile data locally as JSON files so it is available across application sessions.
- **FR-037**: When loading profiles, the system MUST skip corrupted or unreadable JSON files and still load all valid profiles. A warning MUST be shown for any files that failed to load.
- **FR-038**: If the thumbnail file referenced by a profile is missing, the system MUST silently fall back to the default aquarium graphic.
- **FR-039**: System MUST show a brief success notification after profile creation, editing, archival, and restoration operations.
- **FR-040**: System MUST show a progress indicator during asynchronous operations (loading profiles, saving, uploading photos).

### Key Entities

- **Aquarium Profile**: Represents a single aquarium. Key attributes: unique identifier, name, volume value, volume unit (liters/gallons), dimensions (length, width, height), dimension unit (cm/inches), aquarium type (freshwater/saltwater/brackish), setup date (date-only), notes, thumbnail image path, status (active/archived), creation date.
- **Substrate/Additive Entry**: Represents a single substrate or additive layer within an aquarium. Key attributes: unique identifier, parent aquarium identifier, brand, product name, type (substrate/additive/soil cap), layer depth value (inherits parent's dimension unit), date added (date-only), notes, display order.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can create a new aquarium profile with all required fields in under 2 minutes, measured from creation form opening to successful save confirmation.
- **SC-002**: The aquarium selector grid loads and displays all profile cards within 2 seconds of application launch (cold start).
- **SC-003**: Users can navigate from the selector grid to a specific aquarium's management shell in a single click.
- **SC-004**: 100% of profile data (including substrates) persists correctly across application restarts with no data loss.
- **SC-005**: Users can archive and restore a profile without any data loss — all historical information including substrates, notes, and thumbnail are fully preserved after restoration.
- **SC-006**: Users can complete the full profile lifecycle (create, view, edit, add substrates, archive, restore, delete) with zero errors during a scripted walkthrough.
- **SC-007**: The correct unit of measurement is consistently displayed in the selector grid card, management shell header, and Settings page for a given profile — no unit mixing or conversion errors.
- **SC-008**: Users can add, edit, reorder, and remove substrate entries with changes persisting correctly across application restarts.

## Assumptions

- The application is single-user (no multi-user access control or permissions needed).
- Thumbnail images are stored locally in the application's gallery folder under `%LOCALAPPDATA%/AquaSync/gallery/{aquarium-id}/`.
- There is no maximum limit on the number of aquarium profiles a user can create (bounded only by disk space).
- Duplicate aquarium names are permitted — this is a deliberate design decision since users commonly have multiple similar setups.
- Layer depth for substrates uses the same dimension unit (cm/inches) as the parent aquarium profile. This is a functional constraint, not a suggestion.
- The management shell sidebar navigation items (Dashboard, Lamps, Filters, etc.) are placeholders — their page content will be defined in separate features.
- The default aquarium graphic is a static asset bundled with the application.
- Archived profiles open in read-only mode in the management shell — all data is browsable but no modifications are permitted until the profile is restored to active status.
- The Settings page has dual scope: global app settings and aquarium-scoped profile editing when inside the management shell.
- Accessibility is provided by WinUI3's built-in platform support for standard controls (screen reader, keyboard navigation, high contrast). Custom accessibility requirements are deferred.
- Setup date is date-only (no time component). Stored with midnight UTC time internally.
- Photos are stored at original resolution with no compression, per project constitution.
