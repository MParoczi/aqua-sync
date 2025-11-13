## Epic 6: Filters (Eheim Integration)

### US-019: Implement Eheim API Authentication
**As a** developer  
**I want** to authenticate with the Eheim API  
**So that** the app can discover and control filters

**Technical Details:**
- Create `EheimService` in Electron main process
- Implement one-time setup flow:
    - Prompt user for Eheim credentials on first use
    - Store encrypted credentials in `settings.json`
    - Use credentials for all API calls
- API endpoints:
    - User data: `https://api.eheimdigital.com/docs/eheim_digital_api/userdata`
    - Mesh liste: `https://api.eheimdigital.com/docs/eheim_digital_api/mesh-liste`
    - Filter status: `https://api.eheimdigital.com/docs/eheim_digital_api/professionel-5-e-status`
    - Pump control: `https://api.eheimdigital.com/docs/eheim_digital_api/professionel-5-pump`
    - Constant Flow: `https://api.eheimdigital.com/docs/eheim_digital_api/professionel-5-econstant-mode`
    - Bio Mode: `https://api.eheimdigital.com/docs/eheim_digital_api/professionel-5-bio-mode`
- Handle authentication errors gracefully

**Acceptance Criteria:**
- [ ] User can input Eheim credentials
- [ ] Credentials are stored securely
- [ ] API authentication works
- [ ] Authentication errors show user-friendly messages

---

### US-020: Create Filters Page Empty State
**As a** user  
**I want** to see instructions when no filters are connected  
**So that** I know how to add one

**Technical Details:**
- Create `FiltersContent` component
- Empty state:
    - Message: "Connect your Eheim filter!"
    - Prominent "Connect filter" button
- When filters exist:
    - Display filter cards in grid
    - Show "Add new filter" button in top-right

**Acceptance Criteria:**
- [ ] Empty state shows when no filters
- [ ] "Connect filter" button is prominent
- [ ] Button opens filter connection modal
- [ ] Grid shows when filters exist
- [ ] "Add new filter" button appears when filters exist

---

### US-021: Implement Filter Discovery Modal (Step 1)
**As a** user  
**I want** to discover available Eheim filters on my network  
**So that** I can add them to my aquarium

**Technical Details:**
- Create two-step modal: `FilterConnectionModal`
- **Step 1: Discovery**
    - Show loading indicator: "Searching for Eheim filters..."
    - Call Eheim API to list available filters
    - Filter out devices already assigned to other aquariums
    - Display list of available filters with:
        - Filter name/model
        - Selection indicator (radio/checkbox)
    - Show error if no filters found or API fails
    - Buttons: "Cancel", "Next" (disabled if none selected)
    - Progress indicator: "1 of 2"

**Acceptance Criteria:**
- [ ] Modal opens showing loading state
- [ ] API call discovers filters on network
- [ ] Already-assigned filters are excluded
- [ ] Available filters display in list
- [ ] User can select one filter
- [ ] "Next" button is disabled until selection
- [ ] "Next" proceeds to Step 2
- [ ] "Cancel" closes modal
- [ ] Error messages show if API fails

---

### US-022: Implement Filter Naming Modal (Step 2)
**As a** user  
**I want** to give my filter a custom name  
**So that** I can easily identify it

**Technical Details:**
- **Step 2: Naming**
    - Display selected filter's model/name (read-only)
    - Show basic filter parameters (from API)
    - Name input field (default: "Eheim Filter")
    - Buttons: "Back", "Save"
    - Progress indicator: "2 of 2"
- On Save:
    - Create device record:
```typescript
interface Device {
  id: string;
  aquariumId: string;
  type: 'filter' | 'lamp';
  brand: 'eheim' | 'chihiros';
  externalId: string; // API device ID
  name: string;
  model: string;
  status: DeviceStatus;
  config: any; // Brand-specific config
  createdAt: string;
}
```
- Save to `devices.json`
- Close modal
- Show success toast
- Refresh Filters page

**Acceptance Criteria:**
- [ ] Step 2 shows selected filter info
- [ ] Name input has sensible default
- [ ] "Back" returns to Step 1
- [ ] "Save" creates device record
- [ ] Modal closes after save
- [ ] Success toast appears
- [ ] Filters page shows new filter
- [ ] Filter appears on Dashboard

---

### US-023: Display Filter Cards on Filters Page
**As a** user  
**I want** to see my connected filters as cards  
**So that** I can view their status at a glance

**Technical Details:**
- Query filters for current aquarium
- Display filter cards in grid (typically 1-2 filters)
- Each card shows:
    - Filter name
    - Connection status
    - Active mode (e.g., "Bio mode - ON")
- Cards are clickable to view details
- Larger cards than typical grid items (account for 1-2 items max)

**Acceptance Criteria:**
- [ ] Filter cards display in grid
- [ ] Cards show name, status, and mode
- [ ] Cards have appropriate size for 1-2 items
- [ ] Clicking card opens filter detail view
- [ ] Status polling updates cards every 5 seconds

---

### US-024: Implement Filter Detail View
**As a** user  
**I want** to view and control a specific filter  
**So that** I can manage its operation

**Technical Details:**
- Create `FilterDetailView` component
- Replace Filters content when filter card clicked
- Show back button (top-left) to return to filter list
- Display:
    - Filter name (large)
    - Filter model (below name)
    - Connection status
    - On/Off toggle button (large, prominent)
    - Bio Mode button with gear icon
    - Constant Flow Mode button with gear icon
- Toggle button controls pump on/off
- Mode buttons show active state (one active at a time)
- Call Eheim API to control pump

**Acceptance Criteria:**
- [ ] Detail view shows when filter clicked
- [ ] Back button returns to filter list
- [ ] Filter info displays correctly
- [ ] Toggle button turns filter on/off
- [ ] Toggle state reflects actual filter state
- [ ] Mode buttons show which is active
- [ ] Gear icons are visible on mode buttons

---

### US-025: Implement Device Status Polling
**As a** developer  
**I want** to poll filter status every 5 seconds  
**So that** the UI reflects current device state

**Technical Details:**
- Create polling service in Electron main process
- Poll every 5 seconds for all connected devices
- Call appropriate API endpoints:
    - Eheim: status endpoint
    - Chihiros: Bluetooth status query
- Update device status in memory
- Emit IPC events to renderer on status change
- Handle offline devices gracefully

**Acceptance Criteria:**
- [ ] Status polls every 5 seconds
- [ ] UI updates when status changes
- [ ] Offline devices are detected
- [ ] No excessive API calls
- [ ] Polling stops when app is minimized (optional optimization)

---

### US-026: Implement Constant Flow Mode Configuration
**As a** user  
**I want** to configure my filter's Constant Flow mode  
**So that** it runs at a steady flow rate

**Technical Details:**
- Create `ConstantFlowModal` component
- Display:
    - Mode name: "Constant Flow Mode"
    - Slider (0-14 scale)
    - Flow rate label next to slider (in L/h or gal/h based on aquarium volume unit)
    - Buttons: "Cancel", "Save"
- Map slider positions to flow rates based on Eheim API
- Load current setting from API when modal opens
- On Save:
    - Call Eheim Constant Flow API endpoint
    - Close modal
    - Show success/error toast

**Acceptance Criteria:**
- [ ] Modal opens from gear icon
- [ ] Slider shows current setting
- [ ] Flow rate updates as slider moves
- [ ] Correct unit displays (L/h or gal/h)
- [ ] Save applies setting via API
- [ ] Success/error toast appears
- [ ] Modal closes after save
- [ ] Cancel closes without changing

---

### US-027: Implement Bio Mode Configuration
**As a** user  
**I want** to configure my filter's Bio Mode with day/night cycles  
**So that** it simulates natural water flow patterns

**Technical Details:**
- Create `BioModeModal` component
- Display:
    - Mode name: "Bio Mode settings"
    - **Daytime section:**
        - Start hour (time picker)
        - End hour (time picker)
        - Flow rate slider (0-14 scale)
    - **Night time section:**
        - Label: "Night time" (calculated: hours not in daytime)
        - Flow rate slider (0-14 scale)
    - Buttons: "Cancel", "Save"
- Calculate night time automatically
- Load current settings from API
- On Save:
    - Call Eheim Bio Mode API endpoint
    - Close modal
    - Show success/error toast

**Acceptance Criteria:**
- [ ] Modal opens from gear icon
- [ ] Daytime start/end pickers work
- [ ] Night time is calculated automatically
- [ ] Both flow rate sliders work
- [ ] Current settings load correctly
- [ ] Save applies settings via API
- [ ] Success/error toast appears
- [ ] Modal closes after save
- [ ] Cancel closes without changing

---

### US-028: Implement Mode Switching Logic
**As a** user  
**I want** only one filter mode active at a time  
**So that** my filter operates correctly

**Technical Details:**
- Clicking Bio Mode button:
    - If not active: Activate Bio Mode via API, deactivate Constant Flow
    - Update UI to show Bio Mode active
- Clicking Constant Flow button:
    - If not active: Activate Constant Flow via API, deactivate Bio Mode
    - Update UI to show Constant Flow active
- Visual feedback: Active button has darker/highlighted styling
- API calls handle mode switching

**Acceptance Criteria:**
- [ ] Only one mode can be active
- [ ] Activating a mode deactivates the other
- [ ] UI reflects active mode correctly
- [ ] Mode switches via API successfully
- [ ] Error handling for failed mode switches