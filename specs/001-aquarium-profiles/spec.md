# Feature Specification: Aquarium Profile Management

**Feature Branch**: `001-aquarium-profiles`
**Created**: 2026-02-15
**Status**: Draft
**Input**: User description: "Aquarium profile management system — grid-based selector with profile creation, substrate/additive tracking, archival/deletion, and unit-of-measurement locking per profile."

## Clarifications

### Session 2026-02-15

- Q: Can users edit core profile fields after creation? → A: Only name, description/notes, and thumbnail photo are editable. Aquarium type, volume, dimensions, units, and setup date are locked after creation.
- Q: What happens when a user clicks an archived profile in the grid? → A: The management shell opens in read-only mode — users can browse all data but cannot make changes. Restoration is not required to view.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Aquarium Selector Grid (Priority: P1)

When the user launches AquaSync, they see a grid of aquarium profile cards. Each card displays the aquarium's thumbnail photo (or a default graphic), name, aquarium type, volume with unit, and setup date. The grid serves as the main entry point to the application. If no profiles exist yet, the grid area shows an empty state prompting the user to create their first aquarium.

**Why this priority**: This is the application's landing screen and primary navigation hub. Without it, users cannot access any aquarium-specific functionality. It must exist before any other feature can be reached.

**Independent Test**: Can be fully tested by launching the application and verifying the grid renders with existing profile data (or empty state), and delivers the core navigation entry point.

**Acceptance Scenarios**:

1. **Given** the user has no aquarium profiles, **When** the application launches, **Then** the selector grid displays an empty state with a prompt and button to create the first aquarium profile.
2. **Given** the user has one or more aquarium profiles, **When** the application launches, **Then** the selector grid displays a card for each profile showing thumbnail, name, aquarium type, volume (with unit), and setup date.
3. **Given** the user has both active and archived profiles, **When** the selector grid loads, **Then** archived profiles appear visually distinct from active profiles (e.g., muted/greyed appearance with an "Archived" badge).
4. **Given** the selector grid is displayed, **When** the user clicks the "Add Aquarium" button, **Then** the profile creation form opens.

---

### User Story 2 - Create a New Aquarium Profile (Priority: P1)

The user creates a new aquarium profile by filling in the required details: name, volume (with liters/gallons toggle), dimensions in length x width x height (with cm/inches toggle), aquarium type (freshwater, saltwater, or brackish), setup date, and optionally a description/notes field and thumbnail photo. The chosen unit of measurement for volume and dimensions is locked to that profile permanently. If no photo is uploaded, a default aquarium graphic is assigned.

**Why this priority**: Profile creation is the prerequisite for all other aquarium management features. Users cannot manage lamps, filters, water parameters, or anything else without first creating a profile.

**Independent Test**: Can be fully tested by opening the creation form, filling in all fields, saving, and verifying the new profile appears in the selector grid with correct data.

**Acceptance Scenarios**:

1. **Given** the user opens the profile creation form, **When** they fill in name, volume, dimensions, aquarium type, and setup date, **Then** they can save the profile successfully and it appears in the selector grid.
2. **Given** the user is creating a profile, **When** they toggle volume unit to gallons, **Then** the volume input accepts gallons and all future volume displays for this profile use gallons.
3. **Given** the user is creating a profile, **When** they toggle dimension unit to inches, **Then** the dimension inputs accept inches and all future dimension displays for this profile use inches.
4. **Given** the user is creating a profile, **When** they do not upload a thumbnail photo, **Then** a default aquarium graphic is assigned to the profile.
5. **Given** the user is creating a profile, **When** they upload a thumbnail photo, **Then** the uploaded image is stored and displayed as the profile's thumbnail in the selector grid.
6. **Given** the user is creating a profile, **When** they attempt to save without filling in the name, **Then** the system shows a validation error and prevents saving.
7. **Given** the user is creating a profile, **When** they attempt to save without selecting an aquarium type, **Then** the system shows a validation error and prevents saving.

---

### User Story 3 - Register Substrates and Additives (Priority: P2)

As part of the aquarium profile, the user registers one or more substrate and additive entries. Each entry includes: brand, product name, type (substrate, additive, or soil cap), layer depth, date added, and optional notes. Multiple entries are supported to represent layered setups (e.g., base layer of Power Sand + top layer of Amazonia + root tabs). Substrates/additives can be added during profile creation or edited later from the profile settings.

**Why this priority**: Substrate tracking is an important part of the aquarium profile data model, but the core profile can function without it. It enriches the profile with essential aquascaping data.

**Independent Test**: Can be fully tested by creating a profile, adding multiple substrate/additive entries, and verifying they are saved and displayed correctly with all fields.

**Acceptance Scenarios**:

1. **Given** the user is creating or editing an aquarium profile, **When** they add a substrate entry with brand, product name, type "substrate", layer depth, and date added, **Then** the entry is saved and associated with the profile.
2. **Given** the user has added one substrate entry, **When** they add another entry (e.g., a soil cap), **Then** both entries are listed and ordered as entered.
3. **Given** a substrate/additive entry exists, **When** the user edits its details, **Then** the updated values are saved.
4. **Given** a substrate/additive entry exists, **When** the user removes it, **Then** the entry is deleted from the profile.
5. **Given** the user is adding a substrate entry, **When** they select the type field, **Then** they can choose from "Substrate", "Additive", or "Soil Cap".

---

### User Story 4 - Select Aquarium and Enter Management Shell (Priority: P1)

When the user clicks on an aquarium card in the selector grid, the application navigates to that aquarium's management shell. The shell features a sidebar navigation with the following sections: Dashboard, Lamps, Filters, Other Equipment, Water Parameters, Maintenance, Gallery, Fertilizers, Plants, and Settings. All pages within the shell are scoped to the selected aquarium.

**Why this priority**: This is the gateway from the selector grid to all aquarium-specific management. Without it, the grid leads nowhere.

**Independent Test**: Can be fully tested by selecting an aquarium from the grid and verifying the management shell loads with the correct sidebar navigation items and the selected aquarium's context.

**Acceptance Scenarios**:

1. **Given** the selector grid is displayed with aquarium profiles, **When** the user clicks on an active aquarium card, **Then** the application navigates to that aquarium's management shell.
2. **Given** the user is in the management shell, **When** they view the sidebar, **Then** it contains navigation items: Dashboard, Lamps, Filters, Other Equipment, Water Parameters, Maintenance, Gallery, Fertilizers, Plants, and Settings.
3. **Given** the user is in the management shell, **When** they navigate between sidebar sections, **Then** each page displays data scoped to the selected aquarium.
4. **Given** the user is in the management shell, **When** they want to return to the selector grid, **Then** a clear navigation option (e.g., back button or aquarium name breadcrumb) takes them back.

---

### User Story 5 - Archive an Aquarium Profile (Priority: P3)

The user can archive (decommission) an aquarium profile. Archiving preserves all historical data but marks the profile as inactive. Archived profiles appear visually distinct in the selector grid (e.g., muted colors, "Archived" label). An archived profile can be restored to active status.

**Why this priority**: Archival supports long-term use of the application when aquariums are decommissioned. It's important but not needed for initial use.

**Independent Test**: Can be fully tested by archiving an active profile, verifying its visual distinction in the grid, and restoring it back to active.

**Acceptance Scenarios**:

1. **Given** an active aquarium profile exists, **When** the user chooses to archive it, **Then** the system asks for confirmation before archiving.
2. **Given** the user confirms archival, **When** the archival completes, **Then** the profile is marked as archived and appears visually distinct in the selector grid.
3. **Given** an archived profile exists in the grid, **When** the user views it, **Then** they can see it is archived through visual indicators (muted appearance, "Archived" badge).
4. **Given** an archived profile exists, **When** the user chooses to restore it, **Then** the profile returns to active status and appears normally in the grid.
5. **Given** an archived profile exists in the grid, **When** the user clicks on it, **Then** the management shell opens in read-only mode — the user can browse all sections and data but cannot make any changes.

---

### User Story 6 - Delete an Aquarium Profile (Priority: P3)

The user can permanently delete an aquarium profile. Deletion removes all data associated with that profile. A confirmation step prevents accidental deletion.

**Why this priority**: Permanent deletion is a necessary housekeeping feature but is secondary to creation, viewing, and archival.

**Independent Test**: Can be fully tested by deleting a profile, confirming the action, and verifying the profile no longer appears in the grid.

**Acceptance Scenarios**:

1. **Given** an aquarium profile exists (active or archived), **When** the user chooses to delete it, **Then** the system displays a confirmation dialog warning that this action is permanent.
2. **Given** the confirmation dialog is displayed, **When** the user confirms deletion, **Then** the profile and all associated data (substrates, thumbnail, etc.) are permanently removed.
3. **Given** the confirmation dialog is displayed, **When** the user cancels, **Then** the profile remains unchanged.

---

### Edge Cases

- What happens when the user enters a volume of zero or a negative number? The system must validate that volume is a positive number greater than zero.
- What happens when the user enters dimensions of zero or negative values? The system must validate that all dimensions (length, width, height) are positive numbers greater than zero.
- What happens when the user uploads a corrupted or unsupported image file? The system must validate the file is a supported image format (JPEG, PNG, BMP, GIF, WebP) and show an error for unsupported or corrupted files.
- What happens when the user tries to create a profile with a name that already exists? The system must allow duplicate names (different aquariums may have similar names) but encourage uniqueness through a subtle warning.
- What happens when the user tries to delete the only remaining aquarium profile? The deletion proceeds normally; the user returns to the empty state in the selector grid.
- What happens when the user enters very long text in the name or notes fields? The name field should have a reasonable character limit (e.g., 100 characters). Notes fields may allow longer text (e.g., 2000 characters).
- What happens when the user's local storage directory is inaccessible or full? The system should display a user-friendly error message indicating the save failed and suggest checking disk space or permissions.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display a grid of aquarium profile cards as the application's landing screen.
- **FR-002**: Each aquarium card MUST show the profile's thumbnail image, name, aquarium type, volume with unit of measurement, and setup date.
- **FR-003**: System MUST display an empty state with a call-to-action when no aquarium profiles exist.
- **FR-004**: System MUST provide a profile creation form with fields for: name (required), volume (required), volume unit toggle (liters/gallons, required), dimensions — length, width, height (required), dimension unit toggle (cm/inches, required), aquarium type (freshwater/saltwater/brackish, required), setup date (required), description/notes (optional), and thumbnail photo (optional).
- **FR-005**: System MUST lock the chosen volume unit of measurement to the profile permanently upon creation. All volume-related values for that profile MUST use the selected unit throughout the application.
- **FR-006**: System MUST lock the chosen dimension unit of measurement to the profile permanently upon creation. All dimension-related values for that profile MUST use the selected unit throughout the application.
- **FR-007**: System MUST assign a default aquarium graphic as the thumbnail when the user does not upload a photo.
- **FR-008**: System MUST allow users to upload an image file (JPEG, PNG, BMP, GIF, or WebP) as the profile thumbnail.
- **FR-009**: System MUST support registering multiple substrate/additive entries per aquarium profile, each with: brand (required), product name (required), type — substrate, additive, or soil cap (required), layer depth (required), date added (required), and notes (optional).
- **FR-010**: System MUST allow users to add, edit, and remove substrate/additive entries at any time after profile creation.
- **FR-010a**: System MUST allow users to edit the following profile fields after creation: name, description/notes, and thumbnail photo.
- **FR-010b**: System MUST NOT allow editing of aquarium type, volume, volume unit, dimensions, dimension unit, or setup date after profile creation. These fields are permanently locked at creation time.
- **FR-011**: System MUST navigate to the aquarium's management shell when the user selects an active profile from the grid.
- **FR-012**: The management shell MUST include a sidebar with navigation items: Dashboard, Lamps, Filters, Other Equipment, Water Parameters, Maintenance, Gallery, Fertilizers, Plants, and Settings.
- **FR-013**: All pages within the management shell MUST be scoped to the selected aquarium profile's data.
- **FR-014**: System MUST provide a way to navigate back from the management shell to the aquarium selector grid.
- **FR-015**: System MUST allow users to archive an aquarium profile, preserving all data while marking it as inactive.
- **FR-016**: Archived profiles MUST appear visually distinct from active profiles in the selector grid (muted/greyed styling with an "Archived" indicator).
- **FR-016a**: System MUST allow users to open an archived profile's management shell in read-only mode, enabling browsing of all data without requiring restoration.
- **FR-017**: System MUST allow users to restore an archived profile back to active status.
- **FR-018**: System MUST allow users to permanently delete an aquarium profile (active or archived) with a confirmation step.
- **FR-019**: Permanent deletion MUST remove the profile and all associated data (substrates, additives, thumbnail image).
- **FR-020**: System MUST validate all required fields before allowing profile creation (name, volume, dimensions, aquarium type, setup date).
- **FR-021**: System MUST validate that volume and dimension values are positive numbers greater than zero.
- **FR-022**: System MUST persist all aquarium profile data locally so it is available across application sessions.

### Key Entities

- **Aquarium Profile**: Represents a single aquarium. Key attributes: unique identifier, name, volume value, volume unit (liters/gallons), dimensions (length, width, height), dimension unit (cm/inches), aquarium type (freshwater/saltwater/brackish), setup date, description/notes, thumbnail image path, status (active/archived), creation date.
- **Substrate/Additive Entry**: Represents a single substrate or additive layer within an aquarium. Key attributes: unique identifier, parent aquarium identifier, brand, product name, type (substrate/additive/soil cap), layer depth value, date added, notes, display order.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can create a new aquarium profile with all required fields in under 2 minutes.
- **SC-002**: The aquarium selector grid loads and displays all profile cards within 2 seconds of application launch.
- **SC-003**: Users can navigate from the selector grid to a specific aquarium's management shell in a single click/tap.
- **SC-004**: 100% of profile data (including substrates/additives) persists correctly across application restarts with no data loss.
- **SC-005**: Users can archive and restore a profile without any data loss — all historical information is fully preserved after restoration.
- **SC-006**: Users can successfully complete the full lifecycle (create, view, edit substrates, archive, restore, delete) without encountering errors or requiring guidance.
- **SC-007**: The correct unit of measurement is consistently displayed across all views for a given profile — no unit mixing or conversion errors.

## Assumptions

- The application is single-user (no multi-user access control or permissions needed).
- Thumbnail images are stored locally alongside profile data in the application's local storage directory.
- There is no maximum limit on the number of aquarium profiles a user can create (bounded only by disk space).
- Duplicate aquarium names are permitted (the user may have multiple tanks with similar names).
- Layer depth for substrates uses the same dimension unit (cm/inches) as the parent aquarium profile.
- The management shell sidebar navigation items (Dashboard, Lamps, Filters, etc.) are placeholders — their page content will be defined in separate features.
- The default aquarium graphic is a static asset bundled with the application.
- Archived profiles open in read-only mode in the management shell — all data is browsable but no modifications are permitted until the profile is restored to active status.
