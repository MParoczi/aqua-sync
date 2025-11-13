## Epic 4: Dashboard Implementation

### US-012: Display Aquarium Information on Dashboard
**As a** user  
**I want** to see my aquarium's details on the dashboard  
**So that** I can quickly reference its specifications

**Technical Details:**
- Create `DashboardContent` component
- Display aquarium info at top:
    - Aquarium name
    - Type (Freshwater/Marine)
    - Dimensions (formatted with units)
    - Volume (formatted with units)
    - Start date (formatted: YYYY.MM.DD)
    - Days since start (calculated: `today - startDate`)
- Display aquarium thumbnail on the right side
- Info in glassmorphism card

**Acceptance Criteria:**
- [ ] All aquarium info displays correctly
- [ ] Dimensions show with correct units (cm/inch)
- [ ] Volume shows with correct units (L/gal)
- [ ] Days since start calculates correctly
- [ ] Thumbnail displays (or default image)

---

### US-013: Display Connected Devices Status Cards
**As a** user  
**I want** to see all connected devices and their status  
**So that** I can monitor my equipment

**Technical Details:**
- Query devices associated with current aquarium ID
- Display device cards in grid below aquarium info
- Each device card shows:
    - Device name
    - Connection status: "Connecting", "Connected", "Offline", "Connection Failed"
    - Device type: "Filter" or "Lamp"
    - Active mode (if connected): e.g., "Bio mode - ON"
- Use color coding for status (green=connected, red=offline, yellow=connecting)
- Empty state: "Your connected devices will appear here"

**Acceptance Criteria:**
- [ ] Device cards display in grid
- [ ] Status shows with appropriate colors
- [ ] Device type is visible
- [ ] Active mode displays for connected devices
- [ ] Empty state shows when no devices

---

### US-014: Implement Water Test Graph Selector
**As a** user  
**I want** to select which water parameter graphs to display  
**So that** I can customize my dashboard view

**Technical Details:**
- Create multi-select dropdown component
- Options: pH, GH, KH, NO₂, NO₃, NH₄, Fe, Cu, SiO₂, PO₄, CO₂, O₂, Temperature
- Store selected parameters in aquarium-specific settings
- Load saved selection when dashboard opens
- Update graph display immediately when selection changes
- Empty state: "Select at least one test to display graphs"

**Acceptance Criteria:**
- [ ] Dropdown shows all 13 water parameters
- [ ] Multiple parameters can be selected
- [ ] Selection persists per aquarium
- [ ] Graphs update immediately on selection change
- [ ] Empty state shows when no parameters selected

---

### US-015: Implement Water Parameter Line Graphs
**As a** user  
**I want** to see line graphs of my water test results  
**So that** I can track parameters over time

**Technical Details:**
- Use charting library (Recharts or Chart.js)
- Create `WaterParameterGraph` component
- Display graphs in responsive grid (2 columns)
- Each graph shows:
    - Parameter name
    - Colored line matching specification:
        - pH: #0099CC, GH: #8D7B65, KH: #B89B5E, NO₂: #A347BA
        - NO₃: #E05C2B, NH₄: #7AC943, Fe: #A63E14, Cu: #0097A7
        - SiO₂: #C4B998, PO₄: #1FA75D, CO₂: #546E7A, O₂: #42A5F5
        - Temperature: #F39C12
    - Y-axis: Values with unit
    - X-axis: Dates
    - Hover tooltip showing exact value and date
- Sort data by date (oldest to newest)
- Glassmorphism styling for graph containers

**Unit Labels:**
- pH: no unit (1-14 scale)
- GH: °dGH
- KH: °dKH
- NO₂, NO₃, NH₄, Fe, Cu, SiO₂, PO₄, O₂: mg/l (ppm)
- CO₂: no unit (direct number)
- Temperature: °C

**Acceptance Criteria:**
- [ ] Graphs render with correct colors
- [ ] Y-axis shows values with units
- [ ] X-axis shows dates
- [ ] Hover tooltips work
- [ ] Graphs are responsive
- [ ] Graphs display in grid layout