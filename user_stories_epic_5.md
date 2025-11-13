## Epic 5: Water Tests Management

### US-016: Create Water Tests Page with Parameter Cards
**As a** user  
**I want** to see all available water tests as cards  
**So that** I can easily add measurements

**Technical Details:**
- Create `TestsContent` component
- Display 13 parameter cards in grid
- Each card colored per specification (same as graph colors)
- Card shows parameter name clearly
- Cards are clickable to open measurement modal
- Responsive grid layout (3-4 columns)

**Acceptance Criteria:**
- [ ] All 13 parameter cards display
- [ ] Cards use correct colors per specification
- [ ] Parameter names are clearly visible
- [ ] Cards have hover effects
- [ ] Clicking card opens measurement modal

---

### US-017: Implement Water Test Measurement Modal
**As a** user  
**I want** to record water test results  
**So that** I can track my aquarium's water quality

**Technical Details:**
- Create `TestMeasurementModal` component
- Display selected parameter name at top (with colored indicator)
- Form fields:
    - Value input (number, 2 decimal places, min/max based on parameter)
    - Unit label (read-only, shown next to input)
    - Date/time picker (defaults to current date/time)
- Show historical measurements table below:
    - Columns: Value (with unit), Date/Time
    - Sort: Newest first
    - Show last 10 measurements (scrollable if more)
- Empty state for table: "Historical measurement data will appear here"
- Buttons: "Cancel", "Save"
- Validate value ranges (e.g., pH: 1-14)

**Acceptance Criteria:**
- [ ] Modal opens with parameter name and color
- [ ] Value input accepts 2 decimal places
- [ ] Correct unit displays next to input
- [ ] Date/time picker defaults to now
- [ ] Historical table shows previous measurements
- [ ] Empty state shows if no history
- [ ] Save button adds measurement and closes modal
- [ ] Cancel button closes without saving
- [ ] Success toast shows after saving
- [ ] Value validation prevents invalid inputs

---

### US-018: Link Water Test Data to Dashboard Graphs
**As a** developer  
**I want** to ensure new measurements update dashboard graphs  
**So that** users see their data visualized immediately

**Technical Details:**
- Store measurements in `water-tests.json`:
```typescript
interface WaterTest {
  id: string;
  aquariumId: string;
  parameter: WaterParameter;
  value: number;
  unit: string;
  measuredAt: string; // ISO datetime
  createdAt: string;
}
```
- Query measurements filtered by aquarium ID and parameter
- Update dashboard graphs when returning from Tests page
- Implement data refresh mechanism (polling or event-based)

**Acceptance Criteria:**
- [ ] New measurements save to storage
- [ ] Dashboard graphs show new data
- [ ] Data points are sorted by date
- [ ] Multiple measurements for same parameter display correctly