# Requirements Quality Checklist: Lamps Management

**Purpose**: Unit-test the requirements for completeness, clarity, consistency, measurability, and coverage across all 32 FRs. Items flag gaps, ambiguities, and missing definitions — not implementation behavior.
**Created**: 2026-02-21
**Resolved**: 2026-02-21 (all 41 items)
**Feature**: [spec.md](../spec.md) | [plan.md](../plan.md)
**Scope**: Equal coverage across all FR groups + plan open-question gaps
**Depth**: Standard | **Actor**: Reviewer (pre-task-generation)

---

## Requirement Completeness

*Are all necessary requirements present?*

- [x] CHK001 — Is there a requirement defining system behavior when the Bluetooth hardware is unavailable at scan initiation? → **FR-033 added**: scan must not start; error directs user to system settings. [Completeness, Gap, FR-001]

- [x] CHK002 — Is there a functional requirement resolving what happens when the user attempts to add a lamp already in the **current** aquarium? → **FR-034 added**: informational message shown; no duplicate created. [Completeness, Gap, Edge Cases §5]

- [x] CHK003 — Is the display order of lamps in the Lamps page list defined? → **FR-035 added**: sorted by `CreatedAt` ascending (oldest first). [Completeness, Gap, FR-007]

- [x] CHK004 — Is a maximum number of lamps per aquarium defined? → **Explicitly unbounded**; added to Assumptions. [Completeness, Gap, Spec §Key Entities]

- [x] CHK005 — Is there a requirement specifying control behavior when the device becomes Disconnected mid-session? → **FR-036 added**: controls disabled; InfoBar shown; last-known values visible read-only. [Completeness, Gap, FR-011–FR-013]

- [x] CHK006 — Is the content of the removal confirmation dialog defined? → **FR-031 updated**: dialog must name the lamp and model and warn of permanent deletion. [Completeness, Gap, FR-031]

- [x] CHK007 — Is there a requirement to disconnect an active BLE session before removal? → **FR-030 updated**: system must disconnect active session before deleting configuration. [Completeness, Gap, FR-030]

- [x] CHK008 — Is there a requirement for the Dashboard when zero lamps are assigned? → **FR-037 added**: empty-state message required. [Completeness, Gap, FR-027]

- [x] CHK009 — Is there a requirement for the detail view header? → **FR-038 added**: header must show model name, device name, and connection state indicator. [Completeness, Gap, FR-010]

- [x] CHK010 — Is the visual distinction between Manual Brightness and Schedule Brightness sliders defined? → **FR-011 and FR-020 updated** with "Manual Brightness" and "Schedule Brightness" section headings; also added to Assumptions. [Completeness, Gap, FR-011, FR-020]

---

## Requirement Clarity

*Are requirements specific and unambiguous?*

- [x] CHK011 — Is "signal strength" in FR-002 sufficiently specified? → **FR-002 updated**: "relative visual indicator (e.g., signal-strength icon with bars), not a raw RSSI numeric value." Assumption also updated to note this is now also a requirement. [Clarity, FR-002, Assumptions]

- [x] CHK012 — Is "current on/off state" in FR-028 defined for all three modes? → **FR-028 updated**: Off → off; Manual → on; Automatic → follows current schedule time. [Clarity, Ambiguity, FR-028]

- [x] CHK013 — Is "the schedule is paused" in US5-A2 a defined concept? → **US5-A2 updated**: "device switches from autonomous schedule operation to manual control mode; stored schedule is preserved but not executed until Automatic mode is re-enabled." [Clarity, Ambiguity, US5-A2]

- [x] CHK014 — Is "visually marked as unavailable" in FR-005 specific enough? → **FR-005 updated**: greyed-out appearance, interaction disabled, "Already assigned" label visible on interaction. [Clarity, FR-005]

- [x] CHK015 — Is the content of the "informational message" in FR-006 specified? → **FR-006 updated**: message must state scanning is in progress (e.g., "Scanning for nearby Chihiros devices..."). [Clarity, FR-006]

- [x] CHK016 — Is the content of the unmanaged lamp notice in FR-012 specified? → **FR-012 updated**: "This lamp model is not yet supported. Controls are unavailable." plus device BLE name. [Clarity, FR-012]

- [x] CHK017 — Are validation error messages required by FR-024 specified? → **FR-024 updated**: each failure must surface a distinct message identifying the specific issue. [Clarity, FR-024, Edge Cases §6]

- [x] CHK018 — Is "gradual transition" in US4-A2 measurable? → **FR-018 updated**: ramp-up segment rendered as a gradient from off-period color to on-period color spanning ramp-up duration width. US4-A2 updated to match. [Clarity, Measurability, US4-A2, FR-018]

---

## Requirement Consistency

*Do requirements align without conflicts?*

- [x] CHK019 — Do FR-013's dual obligations specify precedence when BLE send fails? → **FR-013 updated**: local persistence is unconditional; BLE failure shows notification only; local value is unaffected. [Consistency, Conflict, FR-013]

- [x] CHK020 — Are FR-008 (Lamps page: mode + connection state) and FR-028 (Dashboard: on/off state) consistent? → Confirmed as two different surfaces with different representations; resolved by CHK012 (FR-028 now defines derived on/off). No conflict. [Consistency, FR-008, FR-028]

- [x] CHK021 — Does FR-032 clarify whether device firmware is cleared on removal? → **FR-032 updated**: "the app only removes its local configuration record; the physical device retains any schedule previously programmed to its firmware and is not reset." [Consistency, FR-032, FR-023]

- [x] CHK022 — Is the signal-strength assumption inconsistent with FR-002? → **Resolved by CHK011**: assumption updated to note the presentation rule is now also FR-002. [Consistency, FR-002, Assumptions]

---

## Acceptance Criteria Quality

*Are success criteria measurable and system-observable?*

- [x] CHK023 — Is SC-001 fully system-observable? → **SC-001 updated**: "under 90 seconds" retained as a UX target; a system-observable metric added — scan must begin within 2 seconds of initiation. [Acceptance Criteria, SC-001]

- [x] CHK024 — Is SC-002 "correctly reflected" measurable? → **SC-002 updated**: "reflecting the last-known LampMode and connection state as reported by the lamp service." [Acceptance Criteria, SC-002]

- [x] CHK025 — Is US4-A2 independently testable? → **Resolved by CHK018**: FR-018 now defines gradient rendering; the scenario is testable once that visual spec is implemented. [Acceptance Criteria, US4-A2]

---

## Scenario Coverage

*Are all primary, alternate, and exception flows addressed?*

- [x] CHK026 — Is there a scenario for the detail view when the device is Disconnected? → **FR-036 added** (same as CHK005); US3 A4 added to User Story 3. [Coverage, Gap, FR-010–FR-013]

- [x] CHK027 — Is there a requirement for brightness when transitioning from Automatic → Manual? → **FR-039 added**: device receives stored ManualBrightness values; defaults to 100% per channel if never set. [Coverage, Gap, FR-013]

- [x] CHK028 — Is there a scenario for OS-level Bluetooth permission denial? → **FR-033** (merged with CHK001) covers both hardware unavailability and permission denial; Edge Cases updated. [Coverage, Gap, Edge Cases §2]

---

## Edge Case Coverage

*Are boundary conditions defined?*

- [x] CHK029 — Is there a deduplication requirement for devices that reappear during scan? → **FR-040 added**: existing entry updated (signal strength refreshed); no duplicate created. [Edge Case, Gap, FR-001]

- [x] CHK030 — Are timeline boundary positions (00:00 / 23:59) permitted? → **Assumption added**: both are valid inputs subject to FR-024 gap constraint. [Edge Case, FR-024]

- [x] CHK031 — Is all-channels-at-0% in Manual mode distinct from LampMode.Off? → **Assumption added**: 0% on all channels is functionally dark but does not change mode to Off; LampMode.Off uses the device's power-off command. [Edge Case, Gap, FR-013, FR-014]

- [x] CHK032 — Is there a truncation requirement for long device names? → **FR-041 added**: names exceeding display width truncated with trailing ellipsis in lamp list and scan results. [Edge Case, Gap, FR-002, FR-008]

- [x] CHK033 — Is RampUpMinutes = 0 defined as "instant on"? → **Assumption added**: RampUpMinutes = 0 means instant-on; ramp-up timeline segment not rendered. FR-018 updated to note this. [Edge Case, FR-019]

- [x] CHK034 — Is a minimum gap between Sunrise and Sunset enforced? → **FR-024 updated**: interval from sunrise to sunset must be at least RampUpMinutes + 1 minute. [Edge Case, FR-024]

---

## Non-Functional Requirements

*Are performance, accessibility, and quality attributes specified?*

- [x] CHK035 — Are accessibility requirements defined for the Canvas timeline control? → **FR-042 added**: standard time-picker inputs required for sunrise and sunset, providing keyboard-accessible entry. [Non-Functional, Gap, FR-015–FR-017]

- [x] CHK036 — Is a keyboard alternative specified for drag handles? → **Resolved by FR-042**: time-picker inputs are the primary entry path; drag handles are a supplementary visual shortcut. [Non-Functional, Gap, FR-016, FR-017]

- [x] CHK037 — Is "stale state acceptable" quantified? → **Assumption updated**: status reflects last BLE event (not a timer); Disconnected shown when BLE disconnection event received. SC-002 updated accordingly. [Non-Functional, Ambiguity, Assumptions, SC-002]

---

## Spec–Plan Gaps

*Open questions raised during planning that require spec-level resolution.*

- [x] CHK038 — How does a user place the first handle in blank schedule state? → **FR-015 and FR-042 updated**: time-picker inputs set initial times first; drag handles appear on timeline only after both times are set. Also added to Assumptions. [Gap, FR-015, FR-016, FR-017, Plan §Open Questions]

- [x] CHK039 — What is the weekday display format and ordering? → **FR-043 added**: three-letter abbreviated names (Mon–Sun), Monday-to-Sunday order. Also added to Assumptions. [Gap, FR-021, Plan §Open Questions]

- [x] CHK040 — Is connection state shown in the detail view? → **Resolved by FR-038** (CHK009): detail view header includes connection state indicator. [Gap, FR-008, FR-011]

- [x] CHK041 — Is the Dashboard mode control widget type specified? → **FR-029 updated**: segmented control (or functionally equivalent three-option control) presenting Off | Manual | Automatic. Also added to Assumptions. [Gap, FR-029, Plan §Open Questions]

---

## Notes

- All 41 items resolved on 2026-02-21.
- 11 new FRs added: FR-033 through FR-043.
- 12 existing FRs updated: FR-002, FR-005, FR-006, FR-012, FR-013, FR-015–FR-018, FR-024, FR-028–FR-032.
- 2 Success Criteria updated: SC-001, SC-002.
- 8 Assumptions added or updated.
- Spec FR count: 43 total (FR-001–FR-043).
