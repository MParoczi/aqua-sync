# Full Coverage Checklist: Aquarium Profile Management

**Purpose**: Thorough requirements quality validation across all domains (UX, data, navigation, edge cases) for author self-review before task generation
**Created**: 2026-02-15
**Resolved**: 2026-02-16
**Feature**: [spec.md](../spec.md) | [plan.md](../plan.md) | [data-model.md](../data-model.md)

## Requirement Completeness

- [x] CHK001 - Is the sort/ordering of aquarium cards in the selector grid specified? → **Resolved**: FR-005 specifies active first then archived, newest first within each group.
- [x] CHK002 - Is the "Add Aquarium" button placement documented? → **Resolved**: FR-004 specifies "Add Aquarium" card as the first item in the grid.
- [x] CHK003 - Are default values for UOM toggles specified in the creation form? → **Resolved**: FR-008 specifies default: liters and default: centimeters.
- [x] CHK004 - Is the setup date default value specified? → **Resolved**: FR-008 specifies default: today, date-only.
- [x] CHK005 - Are loading/progress state requirements defined for async operations? → **Resolved**: FR-040 requires progress indicator during async operations.
- [x] CHK006 - Are requirements specified for where archive/restore/delete actions are triggered? → **Resolved**: FR-007 specifies context menu on cards; FR-028/031/032 detail each action trigger.
- [x] CHK007 - Are the confirmation dialog contents specified for archive and delete? → **Resolved**: US5-AS1 and US6-AS1 specify exact dialog text.
- [x] CHK008 - Is the entry point for profile editing after creation specified? → **Resolved**: FR-016 specifies Settings page within management shell.
- [x] CHK009 - Are requirements defined for how "back to selector" navigation appears? → **Resolved**: FR-026 specifies back button in shell with grid refresh on return.
- [x] CHK010 - Are thumbnail image size constraints specified? → **Resolved**: FR-008 specifies max 10MB; FR-012 specifies original resolution, no compression.
- [x] CHK011 - Is the empty state content described beyond "prompt and button"? → **Resolved**: FR-003 specifies illustration + "No aquariums yet" message + create button.
- [x] CHK012 - Are substrate entry reordering requirements defined? → **Resolved**: FR-020 specifies up/down controls for reordering.
- [x] CHK013 - Is the thumbnail preview behavior specified during photo upload? → **Resolved**: FR-012 requires preview shown in form before saving.
- [x] CHK014 - Are requirements defined for visual feedback after successful save operations? → **Resolved**: FR-039 requires brief success notification after create/edit/archive/restore.

## Requirement Clarity

- [x] CHK015 - Is "muted/greyed styling" for archived cards quantified? → **Resolved**: FR-029 specifies 50% opacity + "Archived" badge overlay.
- [x] CHK016 - Is "subtle warning" for duplicate names defined? → **Resolved**: US2-AS10 specifies inline warning when name field loses focus.
- [x] CHK017 - Are character limits specified as firm constraints? → **Resolved**: Edge Cases section confirms firm limits (100 name, 2000 notes) enforced by validation.
- [x] CHK018 - Is "description/notes" defined as single or two fields? → **Resolved**: Standardized as single "notes" field throughout spec.
- [x] CHK019 - Is layer depth unit tied to parent aquarium's dimension unit in FRs? → **Resolved**: FR-019 explicitly states "Layer depth MUST use the same dimension unit as the parent aquarium profile."
- [x] CHK020 - Is the card click target area defined? → **Resolved**: FR-002 specifies "The entire card surface MUST be clickable."
- [x] CHK021 - Is "user-friendly error message" for storage failures specified? → **Resolved**: Edge Cases specifies exact text: "Could not save profile. Please check disk space and permissions."
- [x] CHK022 - Are "visual indicators" for read-only mode defined? → **Resolved**: FR-030 specifies disabled controls + banner with "This aquarium is archived (read-only)" + restore option.
- [x] CHK023 - Is "at any time after profile creation" for substrate editing specified in terms of access points? → **Resolved**: FR-020 specifies "during profile creation and from the Settings page in the management shell."

## Requirement Consistency

- [x] CHK024 - Does constitution's Settings definition conflict with plan's use for profile editing? → **Resolved**: Constitution Principle VI amended (v1.1.0) to allow dual scope. FR-027 documents this.
- [x] CHK025 - Are card detail fields consistent between FR-002 and US1-AS2? → **Verified**: Already consistent — both list thumbnail, name, type, volume (with unit), setup date.
- [x] CHK026 - Is "description/notes" naming consistent across all artifacts? → **Resolved**: Standardized to "notes" throughout spec, plan, and clarifications.
- [x] CHK027 - Do editability rules in Clarifications, FR-016, and FR-017 align? → **Verified**: All three agree — editable: name, notes, thumbnail; locked: all others.
- [x] CHK028 - Are substrate editing scenarios consistent between US3 and FR-020? → **Resolved**: Both now specify "during profile creation and from the Settings page in the management shell."
- [x] CHK029 - Does data model VolumeUnit default align with spec requirements? → **Resolved**: Both specify Liters as default. FR-008 documents "default: liters."

## Acceptance Criteria Quality

- [x] CHK030 - Is SC-001 "under 2 minutes" measured from a defined starting point? → **Resolved**: SC-001 now reads "measured from creation form opening to successful save confirmation."
- [x] CHK031 - Is SC-002 "within 2 seconds" measured from cold or warm start? → **Resolved**: SC-002 now reads "application launch (cold start)."
- [x] CHK032 - Can SC-006 be objectively measured? → **Resolved**: SC-006 now reads "with zero errors during a scripted walkthrough."
- [x] CHK033 - Is SC-007 testable given most shell pages are stubs? → **Resolved**: SC-007 now explicitly lists "selector grid card, management shell header, and Settings page."
- [x] CHK034 - Are success criteria defined for substrate management? → **Resolved**: SC-008 added: "Users can add, edit, reorder, and remove substrate entries with changes persisting correctly across application restarts."
- [x] CHK035 - Are success criteria defined for archive/restore separately? → **Verified**: SC-005 already covers archive/restore with data loss verification.

## UX & Interaction Scenario Coverage

- [x] CHK036 - Are cancel-with-unsaved-data requirements defined for the creation form? → **Resolved**: FR-014 specifies confirmation prompt when canceling with unsaved data. US2-AS9 acceptance scenario added.
- [x] CHK037 - Are substrate entry cancel requirements defined? → **Resolved**: FR-021 specifies discarding partial entry with no side effects. US3-AS7 scenario added.
- [x] CHK038 - Are keyboard navigation requirements defined for the selector grid? → **Resolved**: Assumptions section notes reliance on WinUI3 built-in keyboard support for standard controls.
- [x] CHK039 - Are card layout requirements defined for responsive grid behavior? → **Resolved**: FR-006 specifies adaptive layout adjusting columns based on window width.
- [x] CHK040 - Are locked field visual requirements defined? → **Resolved**: FR-017 specifies "displayed as read-only with a visual lock indicator."
- [x] CHK041 - Are validation timing requirements defined? → **Resolved**: FR-014 specifies "Validation MUST occur on save attempt with inline error indicators."
- [x] CHK042 - Are creation dialog scroll requirements defined? → **Resolved**: FR-013 specifies "The creation form MUST support scrolling when content exceeds the visible area."

## Data Model & State Coverage

- [x] CHK043 - Are numeric precision requirements defined? → **Resolved**: Edge Cases section specifies "Display uses up to 1 decimal place; stored values preserve input precision." Data model updated.
- [x] CHK044 - Is gallery folder lifecycle in FR-019/FR-033? → **Resolved**: FR-033 now reads "the gallery folder including the thumbnail image."
- [x] CHK045 - Are substrate entry count limits specified? → **Resolved**: FR-018 specifies "(unbounded count)."
- [x] CHK046 - Is corrupted JSON file handling defined? → **Resolved**: FR-037 specifies skip + load other profiles + warning notification.
- [x] CHK047 - Are archived profiles' data integrity requirements complete? → **Verified**: FR-030 + SC-005 together confirm all data preserved and browsable in read-only mode.
- [x] CHK048 - Is SetupDate type consistent between UX and data model? → **Resolved**: FR-008 specifies "date-only." Data model updated with "Date-only input (time stored as midnight UTC)." Clarification added.

## Navigation & Architecture Coverage

- [x] CHK049 - Are requirements defined for external JSON deletion while shell is open? → **Resolved**: Edge Cases specifies "system shows an error notification and navigates back to the selector grid."
- [x] CHK050 - Is grid refresh on return from shell specified? → **Resolved**: FR-026 specifies "Upon return, the grid MUST refresh its profile list."
- [x] CHK051 - Are ShellPage header content requirements specified? → **Resolved**: FR-023 specifies "aquarium name, aquarium type, and (for archived profiles) a read-only indicator."

## Edge Case & Failure Coverage

- [x] CHK052 - Are large profile count requirements defined? → **Resolved**: FR-006 specifies virtualized scrolling. Edge Cases notes "performance remains acceptable even with 100+ profiles."
- [x] CHK053 - Is missing gallery folder fallback defined? → **Resolved**: FR-038 specifies "silently fall back to the default aquarium graphic."
- [x] CHK054 - Is archive-while-viewing behavior defined? → **Resolved**: Not applicable — archive is only triggered from the selector grid context menu (FR-028), not from within the shell.
- [x] CHK055 - Is photo upload failure behavior defined? → **Resolved**: Edge Cases specifies "previous thumbnail is preserved and an error is shown."
- [x] CHK056 - Are locale decimal separator requirements defined? → **Resolved**: FR-015 specifies "MUST accept the user's system locale decimal separator." Data model updated.

## Non-Functional Coverage

- [x] CHK057 - Are accessibility requirements specified for screen readers? → **Resolved**: Assumptions section documents reliance on WinUI3 built-in accessibility; custom requirements deferred.
- [x] CHK058 - Are high contrast mode requirements addressed? → **Resolved**: Same as CHK057 — platform-provided via WinUI3 standard controls.
- [x] CHK059 - Are error recovery requirements defined for partial save failures? → **Resolved**: Edge Cases specifies "previous data state is preserved" on storage failure.
- [x] CHK060 - Are requirements defined for app launch with previously archived aquarium? → **Verified**: App always starts at selector grid (no "last selected" memory). Not applicable.

## Dependencies & Assumptions Validation

- [x] CHK061 - Is layer depth DimensionUnit inheritance promoted to FR? → **Resolved**: FR-019 now explicitly states this as a functional requirement.
- [x] CHK062 - Is default thumbnail asset reflected in FR-007/FR-011? → **Resolved**: FR-011 now reads "The default graphic MUST be a static asset bundled with the application."
- [x] CHK063 - Is duplicate names permission documented as deliberate? → **Resolved**: Assumptions section now reads "Duplicate aquarium names are permitted — this is a deliberate design decision since users commonly have multiple similar setups."
- [x] CHK064 - Is original resolution/no compression reflected in spec? → **Resolved**: FR-012 specifies "Photos MUST be stored at original resolution with no compression." Assumptions section adds "per project constitution."

## Summary

**All 64 items resolved.** Documents updated:
- `spec.md` — Comprehensive rewrite: 40 FRs (up from 22), 8 SCs (up from 7), 14 edge cases (up from 7), 12 assumptions (up from 8), 4 clarifications (up from 2)
- `plan.md` — Constitution check, design decisions #3/#5/#6 updated
- `data-model.md` — Date-only annotations, numeric precision section added
- `constitution.md` — Principle VI amended (v1.0.0 → v1.1.0), Settings dual scope
