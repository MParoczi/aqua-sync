# Feature Specification: Lamps Management

**Feature Branch**: `003-lamps-management`
**Created**: 2026-02-21
**Status**: Draft
**Input**: User description: "The Lamps page allows the user to manage Chihiros aquarium LED lamps assigned to the currently selected aquarium profile."

## Clarifications

### Session 2026-02-21

- Q: Does the BLE scan run continuously (results update in real-time) or for a fixed duration? → A: Continuous streaming — scan runs indefinitely, results populate in real-time as devices are detected; user explicitly stops the scan.
- Q: What is the default operational mode when a lamp is first added? → A: Off — lamp starts inactive; user must explicitly switch to Manual or Automatic.
- Q: If a discovered device has an unrecognized model, should it be blocked or addable? → A: Allow unmanaged — device is added and stored by name only, with no controls shown until a profile update arrives.
- Q: When returning to Manual mode after switching away, are previous brightness values retained or reset? → A: Retain — last manually set per-channel brightness values are persisted and restored when returning to Manual mode.
- Q: When a lamp has no existing schedule, does the schedule editor open blank or pre-filled with defaults? → A: Blank — all fields are empty/zero; user must set every value before saving.

### Session 2026-02-21 (spec-quality checklist review)

- CHK001/028: BLE unavailable or permission denied → scan must not start; error message directs user to system settings.
- CHK002: Duplicate add to current aquarium → informational message; no action.
- CHK003: Lamp list sorted by CreatedAt ascending.
- CHK004: Max lamps per aquarium is unbounded.
- CHK005/026: Detail view when device Disconnected → all controls disabled; last-known values shown read-only; InfoBar warning.
- CHK006: Removal confirmation dialog must name the lamp and warn of permanent deletion.
- CHK007: Active BLE session must be disconnected before configuration is deleted.
- CHK008: Dashboard lamp section shows empty-state when no lamps are assigned.
- CHK009/040: Detail view header shows model name, device name, and connection state indicator.
- CHK010: Schedule and Manual brightness sliders are visually separated by section headings.
- CHK011/022: Signal strength promoted from assumption to requirement (FR-002).
- CHK012: "On/off state" on Dashboard is derived: Off → off, Manual → on, Automatic → follows current schedule time.
- CHK013: US5-A2 "schedule is paused" replaced with precise language.
- CHK014: Unavailable scan devices shown greyed-out with "Already assigned" label.
- CHK015: FR-006 informational message content specified.
- CHK016: FR-012 unmanaged notice content specified.
- CHK017: FR-024 now requires per-issue validation messages.
- CHK018: FR-018 defines gradient rendering for ramp-up segment.
- CHK019: FR-013 clarified — local persistence is unconditional; BLE failure shows notification only.
- CHK020: FR-008 and FR-028 serve different surfaces; resolved by CHK012.
- CHK021: FR-032 clarified — device firmware schedule is not cleared on removal.
- CHK023: SC-001 updated — 90s is UX target; scan-start latency ≤2s added.
- CHK024: SC-002 "correctly" replaced with specific definition.
- CHK027: Automatic→Manual brightness transition defined (FR-039).
- CHK029: Scan result deduplication defined (FR-040).
- CHK030: Timeline boundary positions (00:00, 23:59) are valid.
- CHK031: All channels at 0% ≠ LampMode.Off.
- CHK032: Device name truncation defined (FR-041).
- CHK033: RampUpMinutes=0 means instant-on; no ramp segment rendered.
- CHK034: FR-024 updated — minimum gap = RampUpMinutes + 1 minute.
- CHK035/036/038: Schedule editor requires time-picker inputs (FR-042); drag handles appear after both times are set.
- CHK037: Stale state reflects last BLE event, not a timer.
- CHK039: Weekday format defined (FR-043).
- CHK041: FR-029 specifies segmented control for Dashboard mode toggle.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Discover and Add a Lamp (Priority: P1)

An aquarium owner wants to add a Chihiros lamp they own to their aquarium profile. They navigate to the Lamps page and initiate a Bluetooth scan. Nearby Chihiros devices stream into a list in real time, showing model name and signal strength. The scan runs until the user explicitly stops it. They tap a device to assign it to the current aquarium. If the device is already assigned to a different aquarium, it is greyed out and cannot be selected. Once confirmed, the lamp appears in the aquarium's lamp list.

**Why this priority**: Without the ability to discover and add lamps, there is nothing to manage. This is the entry point for the entire feature and is a prerequisite for all other user stories.

**Independent Test**: Can be fully tested by scanning for devices, selecting one, and confirming the lamp appears in the lamp list — delivering the ability to link a physical device to an aquarium profile.

**Acceptance Scenarios**:

1. **Given** the user is on the Lamps page, **When** they initiate a BLE scan, **Then** a continuously updating list of discovered Chihiros devices appears showing each device's model name and signal strength as they are detected.
2. **Given** a scan is in progress, **When** the user taps Stop, **Then** the scan ends and the discovered device list remains visible.
3. **Given** a device is not assigned to any aquarium, **When** the user selects it and confirms, **Then** the lamp is added to the current aquarium and appears in the lamp list.
4. **Given** a device is already assigned to a different aquarium, **When** it appears in the scan results, **Then** it is displayed greyed-out with an "Already assigned" label and cannot be selected.
5. **Given** the scan is running but no Chihiros devices have been detected yet, **When** the user views the scan screen, **Then** an in-progress indicator and a message stating that scanning is in progress are shown.

---

### User Story 2 - View Lamps List and Status (Priority: P2)

An aquarium owner wants to see at a glance all lamps connected to their aquarium. They open the Lamps page and see a list of assigned lamps, each showing the model name, device name, and current operational status. They can quickly assess whether any lamp is offline or in an unexpected state.

**Why this priority**: The lamp list is the central overview surface. It delivers immediate visibility into assigned devices and is required before users can navigate to individual lamp controls.

**Independent Test**: Can be fully tested by viewing the Lamps page with pre-assigned lamps and verifying that name, device name, and status are correctly displayed for each entry.

**Acceptance Scenarios**:

1. **Given** the current aquarium has lamps assigned, **When** the user opens the Lamps page, **Then** each lamp is listed with its model name, device name, and current operational status, ordered oldest-first by assignment date.
2. **Given** no lamps are assigned to the current aquarium, **When** the user opens the Lamps page, **Then** an empty-state message is shown with an option to add a lamp.
3. **Given** a lamp's status changes (e.g., goes offline), **When** the user views the lamp list, **Then** the updated status is reflected.

---

### User Story 3 - Manually Control Lamp Brightness (Priority: P3)

An aquarium owner wants to immediately adjust the light levels in their tank. They select a lamp from the list, switch to manual mode, and use the per-channel sliders to fine-tune brightness. Changes take effect on the device in real time. The channels displayed match the lamp's hardware profile — a WRGB II Pro shows Red, Green, Blue, and White sliders, while an A II shows only a White slider.

**Why this priority**: Manual brightness control is the most direct way for a user to interact with a lamp. It delivers immediate value and does not require schedule configuration.

**Independent Test**: Can be fully tested by selecting a lamp, adjusting brightness sliders, and verifying the physical device responds — without needing schedule or clock features.

**Acceptance Scenarios**:

1. **Given** the user opens a lamp's detail view, **When** the lamp model supports multiple color channels, **Then** individual brightness sliders for each channel are displayed under the "Manual Brightness" section.
2. **Given** the user drags a channel slider, **When** the value changes, **Then** the updated brightness is persisted locally and sent to the connected lamp. If BLE send fails, an error notification is shown but the local value is still saved.
3. **Given** a lamp model supports only one channel (e.g., A II), **When** the detail view opens, **Then** only a single brightness slider is shown.
4. **Given** the device becomes unreachable while the detail view is open, **When** the disconnection occurs, **Then** all interactive controls are disabled and an InfoBar informs the user the device is unreachable; last-known values remain visible.

---

### User Story 4 - Configure an Automated Daily Schedule (Priority: P4)

An aquarium owner wants their lights to simulate a natural sunrise and sunset each day. They open the schedule editor in the lamp detail view, enter a sunrise time of 7:00 AM and a sunset time of 9:00 PM using the time-picker controls, set a 30-minute ramp-up, configure peak brightness per channel, and select weekdays. The 24-hour timeline bar updates visually as values are entered. When they save, the schedule is written to the device, which then runs it autonomously.

**Why this priority**: Automated scheduling is the primary productivity feature of the lamp management system — it removes the need for manual daily intervention and is the core differentiator of the device.

**Independent Test**: Can be fully tested by configuring a complete schedule and verifying the device operates on that schedule without the app being open.

**Acceptance Scenarios**:

1. **Given** the user opens the schedule editor, **When** they set a sunrise time using the time picker, **Then** the sunrise time updates in the timeline bar and the time display.
2. **Given** the user sets a ramp-up duration, **When** they view the timeline, **Then** the ramp-up period is rendered as a gradient segment from the off-period color to the on-period color across the ramp-up duration width.
3. **Given** the user configures all schedule fields and saves, **When** the schedule is saved, **Then** it is programmed onto the device and the device runs it autonomously.
4. **Given** a schedule already exists on the device, **When** the user saves a new schedule, **Then** the previous schedule is replaced.
5. **Given** the user selects specific weekdays for the schedule, **When** those days are not selected, **Then** the lights do not activate on those days.

---

### User Story 5 - Control Lamp Mode from Dashboard (Priority: P5)

An aquarium owner wants to quickly switch a lamp between Off, Manual, and Automatic modes without navigating into the lamp detail view. On the Dashboard, each lamp has a status card showing its current state and a segmented mode control. The owner can switch to Manual to temporarily put the device into manual control mode (schedule data preserved but not executed), switch to Automatic to resume schedule-driven operation, or turn the lamp Off entirely.

**Why this priority**: The Dashboard provides a quick-access surface that saves the user from navigating into each lamp's detail view for common mode switches.

**Independent Test**: Can be fully tested by viewing the Dashboard, switching a lamp mode, and verifying the lamp responds — independent of the Lamps page.

**Acceptance Scenarios**:

1. **Given** lamps are assigned to the aquarium, **When** the user opens the Dashboard, **Then** each lamp is represented by a status card showing its derived on/off state and a segmented control reflecting the current operational mode.
2. **Given** a lamp is in Automatic mode, **When** the user switches it to Manual via the Dashboard card, **Then** the device switches from autonomous schedule operation to manual control mode; the stored schedule data is preserved but the device no longer executes it until Automatic mode is re-enabled.
3. **Given** a lamp is in Manual mode, **When** the user switches it to Off, **Then** the lamp turns off immediately.
4. **Given** a lamp is Off, **When** the user switches it to Automatic, **Then** the lamp resumes schedule-driven operation.

---

### User Story 6 - Remove a Lamp from the Aquarium (Priority: P6)

An aquarium owner wants to remove a lamp that is no longer part of their setup. They select the lamp and choose to remove it. A confirmation dialog names the lamp and warns that the configuration will be permanently deleted. After confirming, any active BLE session is disconnected, the lamp is permanently deleted from the aquarium's configuration, and the physical device retains whatever schedule was last programmed to it. If they later want to reassign it to another aquarium, they must re-add it through a new BLE scan.

**Why this priority**: Lamp removal is essential lifecycle management. Without it, the lamp list would accumulate stale entries with no way to clean them up.

**Independent Test**: Can be fully tested by removing a lamp and verifying it no longer appears in the lamp list.

**Acceptance Scenarios**:

1. **Given** a lamp is in the list, **When** the user selects remove, **Then** a confirmation dialog appears naming the lamp and warning that its configuration will be permanently deleted.
2. **Given** the confirmation dialog is shown, **When** the user confirms, **Then** any active BLE session for the lamp is disconnected before the configuration is deleted, and the lamp no longer appears in the list.
3. **Given** the user initiates removal, **When** a confirmation prompt is shown, **Then** cancelling the prompt leaves the lamp unchanged.
4. **Given** a lamp has been removed, **When** the user initiates a BLE scan, **Then** that device can be discovered and added again.

---

### User Story 7 - Synchronize the Device Clock (Priority: P7)

An aquarium owner notices the lamp's internal clock has drifted and the schedule is triggering at the wrong time. In the lamp detail view, they use the clock sync control to immediately write the current system time to the device.

**Why this priority**: Clock synchronization is a maintenance utility. It ensures scheduled operations occur at the intended times but is only needed occasionally after initial setup.

**Independent Test**: Can be fully tested by triggering a clock sync and verifying the device's internal clock is updated to match the current time.

**Acceptance Scenarios**:

1. **Given** the user opens a lamp's detail view, **When** they activate the clock sync control, **Then** the device's internal clock is updated to match the current system time.
2. **Given** the sync fails (e.g., device is unreachable), **When** the user attempts the action, **Then** an error message is shown and the action can be retried.

---

### Edge Cases

- When the BLE scan is running but no Chihiros devices are detected: an in-progress indicator and message stating scanning is in progress are displayed (FR-006).
- When Bluetooth is unavailable (adapter disabled or OS permission denied): the scan must not start and must display an error directing the user to enable Bluetooth or grant permission in system settings (FR-033).
- When the device becomes unreachable while saving a schedule: an error notification is shown; the schedule is not marked as saved; the user may retry (FR-036).
- When an unrecognized lamp model is discovered: the device may be added and stored by name and BLE address, but its detail view shows only the model notice with no channel controls or schedule editor (FR-012).
- When the user tries to add a lamp already in the current aquarium: an informational message states the device is already assigned here; no duplicate is created (FR-034).
- If sunrise time is set later than sunset time, ramp-up duration extends past sunset time, no weekdays are selected, or sunrise/sunset times are not set: the schedule save action is blocked and a specific validation message identifies the issue (FR-024).
- When a lamp has no prior schedule: the schedule editor opens in a blank state; time-picker inputs allow the user to set sunrise and sunset before the timeline drag handles appear (FR-015, FR-042).
- When a device appears, disappears, and reappears during a scan: the scan results entry is updated (signal strength refreshed) rather than duplicated (FR-040).

## Requirements *(mandatory)*

### Functional Requirements

**Lamp Discovery & Assignment**

- **FR-001**: The system MUST allow users to initiate a continuous Bluetooth Low Energy scan for nearby Chihiros lamp devices from the Lamps page; discovered devices MUST appear in real-time as they are detected without waiting for the scan to end.
- **FR-002**: The system MUST display each discovered device's model name and signal strength during a scan. Signal strength MUST be presented as a relative visual indicator (e.g., signal-strength icon with bars), not as a raw RSSI numeric value.
- **FR-003**: The system MUST allow users to explicitly stop an in-progress scan via a stop control.
- **FR-004**: The system MUST allow users to select a discovered device and assign it to the current aquarium; the lamp MUST be assigned an initial operational mode of Off.
- **FR-005**: The system MUST prevent a lamp from being assigned to more than one aquarium simultaneously; devices already assigned elsewhere MUST be displayed with a greyed-out appearance, interaction disabled, and an "Already assigned" label visible on interaction.
- **FR-006**: The system MUST display an in-progress indicator and a message stating that scanning is in progress (e.g., "Scanning for nearby Chihiros devices...") when a scan is running but no devices have been discovered yet.
- **FR-033**: If Bluetooth hardware is unavailable (adapter disabled) or if the operating system has denied Bluetooth permission, the scan MUST NOT start; the system MUST display an error message directing the user to enable Bluetooth or grant permission in system settings.
- **FR-034**: If the user attempts to add a device that is already assigned to the current aquarium, the system MUST display an informational message stating the device is already in this aquarium and MUST NOT create a duplicate entry.
- **FR-040**: If a device already in the scan results list is detected again (same BLE address), the system MUST update the existing entry (refreshing signal strength) rather than adding a duplicate.

**Lamp List**

- **FR-007**: The Lamps page MUST display a list of all lamps assigned to the current aquarium.
- **FR-008**: Each lamp entry in the list MUST display the lamp's model name, device name, and current operational status (Off, Manual, or Automatic) along with connection state (Connected or Disconnected).
- **FR-009**: The Lamps page MUST display an empty-state message when no lamps are assigned to the current aquarium.
- **FR-010**: Users MUST be able to select a lamp from the list to open its detail view.
- **FR-035**: Lamps in the list MUST be ordered by assignment date, oldest first (ascending CreatedAt).
- **FR-041**: Device names that exceed the available display width in the lamp list and in scan results MUST be truncated with a trailing ellipsis ("…").

**Lamp Detail View — General**

- **FR-038**: The lamp detail view header MUST display the lamp's model name and device name, and MUST include a connection state indicator showing whether the device is currently Connected or Disconnected.
- **FR-036**: When the lamp's device is in a Disconnected state, the detail view MUST display a full-width InfoBar warning indicating the device is unreachable. All interactive controls (brightness sliders, schedule save, clock sync) MUST be disabled. Last-known values MUST remain visible in a read-only state.

**Manual Brightness Control**

- **FR-011**: The lamp detail view MUST display individual brightness sliders for each color channel supported by the specific lamp model's device profile, grouped under a "Manual Brightness" section heading.
- **FR-012**: The set of color channels displayed MUST adapt to the lamp's device profile (e.g., WRGB II Pro: Red, Green, Blue, White; A II: White only). If no device profile is available for the lamp's model, the detail view MUST show a notice stating "This lamp model is not yet supported. Controls are unavailable." alongside the device's BLE name, and no channel sliders are displayed.
- **FR-013**: Adjusting a channel slider MUST persist the value to `ManualBrightness` unconditionally (regardless of BLE outcome), and MUST attempt to send the updated brightness value to the connected lamp in real time. If the BLE send fails, an error notification MUST be shown; the locally persisted value is unaffected. The persisted value MUST be restored when the user returns to Manual mode after switching away.
- **FR-014**: Brightness values MUST be expressed as a percentage from 0% to 100%.
- **FR-039**: When the operational mode transitions from Automatic to Manual, the device MUST be sent the brightness values stored in `ManualBrightness` for each channel. If `ManualBrightness` is empty (never previously set), each channel MUST be sent a brightness value of 100%.

**Schedule Editor**

- **FR-015**: The lamp detail view MUST include a visual 24-hour timeline bar for configuring the lamp's daily schedule. When no schedule has been previously saved, the editor MUST open in a blank state with no times, ramp-up, brightness values, or weekdays pre-selected, and no drag handles visible on the timeline.
- **FR-016**: Users MUST be able to set a sunrise time by dragging a handle on the timeline; the sunrise handle represents the start of the ramp-up period. The drag handle MUST appear on the timeline only after a sunrise time has been set via the time-picker input (FR-042).
- **FR-017**: Users MUST be able to set a sunset time by dragging a handle on the timeline; the sunset handle represents the time at which the lamp turns off. The drag handle MUST appear on the timeline only after a sunset time has been set via the time-picker input (FR-042).
- **FR-018**: The timeline MUST visually distinguish three zones: (1) off-period (before sunrise and after sunset) rendered in a muted/dark fill; (2) ramp-up period rendered as a gradient from the off-period color to the on-period color spanning the full ramp-up duration width; (3) peak on-period rendered in a distinct accent fill. When RampUpMinutes is 0, the ramp-up zone is not rendered.
- **FR-019**: Users MUST be able to set a ramp-up duration between 0 and 150 minutes inclusive.
- **FR-020**: Users MUST be able to set per-channel peak brightness levels for the schedule's on-period under a "Schedule Brightness" section heading; the channels available match the lamp's device profile.
- **FR-021**: Users MUST be able to select which days of the week the schedule is active using weekday checkboxes.
- **FR-022**: Each device supports exactly one schedule; saving a schedule MUST replace any existing schedule programmed on the device.
- **FR-023**: When the user saves the schedule, the system MUST program it directly onto the device so it runs autonomously without requiring the app to remain connected.
- **FR-024**: The system MUST validate all of the following before allowing a schedule to be saved: (1) sunrise and sunset times are both set; (2) sunrise time is earlier than sunset time; (3) the interval from sunrise to sunset is at least RampUpMinutes + 1 minute; (4) ramp-up duration is in [0, 150]; (5) at least one weekday is selected. Each validation failure MUST surface a distinct message identifying the specific issue (e.g., "Sunrise must be earlier than sunset", "At least one day must be selected").
- **FR-042**: The schedule editor MUST provide standard time-picker input controls for both sunrise time and sunset time. These inputs are the primary mechanism for setting schedule times and ensure keyboard-accessible entry. Drag handles on the timeline serve as a supplementary visual shortcut.
- **FR-043**: Weekday selectors MUST be displayed as three-letter abbreviated names (Mon, Tue, Wed, Thu, Fri, Sat, Sun) in Monday-to-Sunday order.

**Device Clock Synchronization**

- **FR-025**: The lamp detail view MUST provide a control to synchronize the device's internal clock with the current system time.
- **FR-026**: The system MUST notify the user of success or failure after a clock synchronization attempt.

**Dashboard Integration**

- **FR-027**: The Dashboard MUST display a status card for each lamp assigned to the current aquarium.
- **FR-028**: Each Dashboard lamp card MUST show the lamp's derived on/off state: Off mode → lamp is off; Manual mode → lamp is on (at stored brightness); Automatic mode → lamp state follows the current schedule time (on during the scheduled on-period, off otherwise).
- **FR-029**: Each Dashboard lamp card MUST provide a segmented control (or functionally equivalent three-option control) presenting Off | Manual | Automatic, with the currently stored mode pre-selected. Selecting a segment switches the lamp to that mode.
- **FR-037**: The Dashboard lamp section MUST display an empty-state message (e.g., "No lamps assigned — go to Lamps to add one") when no lamps are assigned to the current aquarium.

**Lamp Removal**

- **FR-030**: Users MUST be able to permanently remove a lamp from the current aquarium. Before deleting the configuration, the system MUST disconnect any active BLE session for that lamp.
- **FR-031**: The system MUST display a confirmation dialog before permanently removing a lamp. The dialog MUST state the lamp's display name and model, and MUST warn that the configuration will be permanently deleted and cannot be undone.
- **FR-032**: After removal, the lamp MUST be discoverable via BLE scan and assignable to any aquarium as if it were new. The app only removes its local configuration record; the physical device retains any schedule previously programmed to its firmware and is not reset.

### Key Entities

- **Lamp**: A Chihiros LED device assigned to an aquarium. Attributes: model name, device name, BLE device identifier, assigned aquarium reference, operational mode (Off / Manual / Automatic; default: Off on first assignment), last manual brightness per channel (persisted; restored when re-entering Manual mode; defaults to 100% per channel if never set), connection state (Connected / Disconnected).
- **Device Profile**: Defines the set of color channels available for a lamp model. Examples: WRGB II Pro → Red, Green, Blue, White; A II → White. Determines which controls are rendered. A lamp may exist without a matching device profile (unmanaged state); in this state no channel controls or schedule editor are shown, only the model-unavailable notice.
- **Lamp Schedule**: The autonomous daily program stored on the device. Attributes: sunrise time (start of ramp-up), sunset time (lights off), ramp-up duration (0–150 min; 0 = instant-on), per-channel peak brightness (0–100%), active days of week.
- **BLE Discovery Result**: A transient record of a discovered nearby device. Attributes: model name, device name, BLE device identifier, signal strength (displayed as relative visual indicator), availability status (available / already assigned).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can complete the full end-to-end flow of discovering, selecting, and adding a new lamp in under 90 seconds (UX target including user interaction time). Additionally, the system MUST begin the BLE scan within 2 seconds of the user initiating it.
- **SC-002**: All lamps assigned to the current aquarium are visible on the Lamps page within 2 seconds of opening the page, reflecting the last-known `LampMode` and connection state as reported by the lamp service.
- **SC-003**: Brightness slider changes are reflected on the physical device within 1 second of the user releasing the slider.
- **SC-004**: A fully configured schedule is saved to the device within 5 seconds of the user confirming the save action.
- **SC-005**: The device clock is synchronized with system time within 3 seconds of the user triggering the sync.
- **SC-006**: Mode changes made from the Dashboard lamp card are reflected on the device within 2 seconds.
- **SC-007**: 100% of lamp removal actions that pass the confirmation step result in the lamp being fully removed from the aquarium configuration.

## Assumptions

- Brightness values for both manual controls and schedule peak brightness are expressed as integer percentages (0–100%).
- Signal strength during BLE discovery is displayed as a relative visual indicator (e.g., signal-strength icon with bars), not a raw RSSI numeric value. This is now also a requirement (FR-002).
- Sunset transition is immediate with no fade-out; only the sunrise period has a configurable ramp-up duration.
- The schedule's "peak on-period" starts at the end of the sunrise ramp-up (sunrise_time + ramp_up_duration) and ends at the sunset time.
- RampUpMinutes = 0 means instant-on at sunrise time; the ramp-up timeline segment is not rendered.
- Days of the week not selected in the schedule are treated as fully off for that day.
- Only one BLE scan session can be active at a time; starting a new scan cancels any in-progress scan.
- The lamp list status reflects the last BLE event (connection, disconnection, or command result), not a timed polling window. Status is considered current until a BLE disconnection event is received.
- Moving a lamp to a different aquarium requires the user to remove it first and then re-add it via a new BLE scan on the target aquarium.
- The maximum number of lamps per aquarium is unbounded.
- Manual Brightness and Schedule Brightness sliders are visually distinguished by their respective section headings ("Manual Brightness" and "Schedule Brightness"), not by style differences within the sliders themselves.
- Setting all manual brightness channels to 0% is functionally dark but does NOT change the operational mode to Off. LampMode.Off is set only by explicit mode selection and uses the device's power-off command.
- Timeline boundary positions — sunrise at 00:00 and sunset at 23:59 — are valid inputs subject to the gap constraint in FR-024.
- Timeline drag handles for sunrise and sunset appear only after both times have been initially set via the time-picker inputs (FR-042). Until then, the timeline renders in the blank (all-off) state.
- Weekday selectors use three-letter abbreviated names (Mon, Tue, Wed, Thu, Fri, Sat, Sun) in Monday-to-Sunday order.
- The Dashboard mode segmented control (Off | Manual | Automatic) reflects the stored `LampMode` value from the last saved configuration, not a real-time device query.
