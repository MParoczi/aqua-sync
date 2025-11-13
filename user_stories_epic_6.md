## Epic 6: Filters (Eheim Integration) - REVISED

### US-019: Implement mDNS Device Discovery Service
**As a** developer
**I want** to discover Eheim filters on the local network via mDNS
**So that** the app can detect available devices automatically

**Technical Details:**
- Create `EheimDiscoveryService` in Electron main process
- Install `bonjour` library: `yarn add bonjour @types/bonjour`
- Search for `_http._tcp` services containing "eheim"
- Primary hostname: `eheimdigital.local`
- Extract IP addresses and MAC addresses from discovered services
- Discovery timeout: 10 seconds
- IPC handler: `eheim:discover` returns array of discovered devices
- Each device includes:
  ```typescript
  {
    hostname: string;      // e.g., "eheimdigital.local"
    ipAddress: string;     // e.g., "192.168.2.5"
    macAddress: string;    // from device response
    model: string;         // detected from API response
    port: number;          // typically 80
  }
  ```
- Handle no devices found gracefully
- Provide manual IP entry fallback

**Acceptance Criteria:**
- [ ] mDNS discovery finds Eheim devices on network
- [ ] Discovery completes within 10 seconds
- [ ] IP addresses are correctly extracted
- [ ] MAC addresses are retrieved via initial API call
- [ ] Empty array returned if no devices found
- [ ] Manual IP entry option available
- [ ] Discovery can be retriggered on demand

---

### US-019a: Implement WebSocket Connection Manager
**As a** developer
**I want** to manage persistent WebSocket connections to Eheim filters
**So that** the app receives real-time status updates

**Technical Details:**
- Create `EheimWebSocketService` in main process
- Install `ws` library: `yarn add ws @types/ws`
- WebSocket URL: `ws://[device-ip]/ws`
- Maintain one connection per connected filter
- Connection lifecycle:
  - Connect on device added
  - Auto-reconnect on disconnect (exponential backoff)
  - Close on device removed or app quit
- Parse incoming messages:
  - `FILTER_DATA` - Real-time filter status (primary)
  - `USRDTA` - User data including firmware version
  - `MESH_NETWORK` - Device topology (for multi-device setups)
- Send initial command after connection:
  ```javascript
  ws.send(JSON.stringify({
    title: 'GET_FILTER_DATA',
    from: 'USER',
    to: 'MASTER'
  }));
  ```
- Broadcast updates to renderer via IPC: `eheim:status-update`
- Handle connection errors gracefully (device offline)
- Log all WebSocket messages in development mode

**Acceptance Criteria:**
- [ ] WebSocket connects to device successfully
- [ ] `FILTER_DATA` messages are received and parsed
- [ ] Automatic reconnection works on disconnect
- [ ] Multiple devices can have simultaneous connections
- [ ] Renderer receives status updates via IPC
- [ ] Offline devices are detected and marked
- [ ] Connections close cleanly on app quit

---

### US-019b: Implement Eheim REST API Client
**As a** developer
**I want** a REST API client for sending commands to Eheim filters
**So that** the app can control filter operation

**Technical Details:**
- Create `EheimRestClient` class in main process
- Use `axios` or native `fetch` for HTTP requests
- Default authentication: `Basic YXBpOmFkbWlu` (api:admin)
- All requests to: `http://[device-ip]/api/filter`
- Standard headers:
  ```javascript
  {
    'Authorization': 'Basic YXBpOmFkbWlu',
    'Content-Type': 'application/json'
  }
  ```
- POST body always includes `to: macAddress`
- Implement POST-then-GET verification pattern:
  1. POST command to device
  2. Wait 1 second
  3. GET current status
  4. Verify parameters were accepted
- Handle HTTP errors (device offline, auth failure)
- Timeout: 5 seconds per request
- Optional: Support credential changes via `/api/changeauth`

**Acceptance Criteria:**
- [ ] REST client sends POST requests successfully
- [ ] Authentication header is included
- [ ] MAC address routing works (`to` parameter)
- [ ] POST-then-GET verification implemented
- [ ] Timeout prevents hanging requests
- [ ] HTTP errors return user-friendly messages
- [ ] Connection to `eheimdigital.local` works

---

### US-020: Create Filters Page Empty State
**As a** user
**I want** to see instructions when no filters are connected
**So that** I know how to add one

**Technical Details:**
- Replace placeholder in `FiltersContent.tsx`
- Empty state (no filters):
  - Icon: Filter or wave icon
  - Message: "Connect your Eheim filter to get started!"
  - Subtitle: "Manage filter modes, flow rates, and monitor performance"
  - Prominent "Discover Filters" button (glassmorphism)
- When filters exist:
  - Display filter cards in grid (1-2 columns max)
  - Show "Add New Filter" button in top-right corner
  - Filter count indicator
- Loading state while fetching devices
- Error state if fetch fails

**Acceptance Criteria:**
- [ ] Empty state shows when no filters connected
- [ ] "Discover Filters" button opens discovery modal
- [ ] Grid displays when filters exist
- [ ] "Add New Filter" button appears when filters exist
- [ ] Loading state shows during data fetch
- [ ] Error state shows helpful message

---

### US-021: Implement Filter Discovery Modal (Step 1)
**As a** user
**I want** to discover available Eheim filters on my network
**So that** I can add them to my aquarium

**Technical Details:**
- Create `FilterDiscoveryModal` component (2-step modal)
- **Step 1: Network Scanning**
  - Show loading state: "Scanning network for Eheim filters..." with spinner
  - Call `window.electron.eheim.discover()` IPC handler
  - Display discovered devices in list:
    - Device hostname (eheimdigital.local)
    - IP address
    - Model (if detected)
    - Radio button for selection
  - Filter out devices already assigned to other aquariums (check by MAC)
  - Show count: "Found X filters"
  - Error states:
    - No devices found: "No Eheim filters found. Ensure your filter is connected to WiFi and try again."
    - Network error: "Network scan failed. Check your WiFi connection."
  - Manual entry option: "Enter IP manually" link
  - Buttons: "Cancel", "Next" (disabled until selection)
  - Progress: "Step 1 of 3"

**Acceptance Criteria:**
- [ ] Modal opens and starts scanning automatically
- [ ] Loading spinner shows during scan (10s max)
- [ ] Discovered filters display in list
- [ ] Already-connected filters are excluded
- [ ] User can select one filter (radio buttons)
- [ ] "Next" disabled until selection made
- [ ] Manual IP entry works as fallback
- [ ] Cancel closes modal without changes
- [ ] Error messages are user-friendly

---

### US-022: Fetch Filter Details and Firmware Version (Step 2)
**As a** developer
**I want** to retrieve filter model and firmware version
**So that** the app can validate compatibility and display correct settings

**Technical Details:**
- **Step 2: Device Information Retrieval**
  - After device selected in Step 1, show loading: "Connecting to filter..."
  - Establish WebSocket connection to device
  - Send `GET_USRDTA` command to retrieve:
    ```json
    {
      "title": "USRDTA",
      "revision": [2037, 1025],  // [master, client] firmware versions
      "macAddress": "A8:48:FA:D7:A0:F7",
      "latestAvailableRevision": [2037, 1025],
      "firmwareAvailable": 0
    }
    ```
  - Send `GET_FILTER_DATA` to retrieve model-specific info
  - Detect model from frequency ranges:
    - 5e 350: max ~72 Hz (3500-7100 Hz range)
    - 5e 450: max ~76 Hz
    - 5e 700/600T: max ~80 Hz
  - Validate firmware version:
    - Master minimum: S2037
    - Client minimum: S1025
    - Show warning if outdated (not blocking)
  - Extract current configuration:
    - `filterActive`, `pumpMode`, `rotorSpeed`
    - Bio mode settings (if active)
  - Display firmware warning modal if needed:
    - "Outdated firmware detected (S[VERSION]). Some features may not work correctly. Please update to S2037 or later."
    - Link to: https://eheim.com/en_GB/support/downloads/
    - Options: "Update Later" (proceed), "Cancel" (abort connection)

**Acceptance Criteria:**
- [ ] WebSocket connection established
- [ ] Firmware version retrieved from `USRDTA`
- [ ] Model detected from frequency data
- [ ] MAC address extracted correctly
- [ ] Outdated firmware shows warning (non-blocking)
- [ ] Update link opens in browser
- [ ] User can proceed despite firmware warning
- [ ] Connection errors show helpful messages

---

### US-023: Implement Filter Naming Modal (Step 3)
**As a** user
**I want** to give my filter a custom name
**So that** I can easily identify it

**Technical Details:**
- **Step 3: Naming and Saving**
  - Display detected info (read-only):
    - Model: "Eheim Professional 5e 350"
    - IP Address: "192.168.2.5"
    - MAC Address: "A8:48:FA:D7:A0:F7"
    - Firmware: "S2037" (with warning icon if outdated)
  - Name input field:
    - Label: "Filter Name"
    - Default: "Eheim Filter" or "Main Filter"
    - Max length: 50 characters
    - Validation: Required, no special characters except space/-/_
  - Buttons: "Back" (return to Step 1), "Save & Connect"
  - Progress: "Step 3 of 3"
- On Save:
  - Create device record using existing `Device` interface:
    ```typescript
    {
      id: uuid(),
      aquariumId: currentAquarium.id,
      type: 'filter',
      manufacturer: 'eheim',
      name: userInput,
      model: detectedModel,  // e.g., "Professional 5e 350"
      status: 'connected',
      macAddress: deviceMAC,
      ipAddress: deviceIP,
      firmwareVersion: deviceFirmware,
      config: {
        filterActive: currentState.filterActive,
        pumpMode: currentState.pumpMode,
        rotorSpeed: currentState.rotorSpeed,
        // ... other Eheim-specific config
      },
      lastSeen: new Date().toISOString(),
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    }
    ```
  - Save via `window.electron.data.createDevice()`
  - Initialize WebSocket connection for this device
  - Close modal
  - Show success toast: "Filter connected successfully!"
  - Navigate to Filters page
  - Refresh Dashboard connected devices

**Acceptance Criteria:**
- [ ] Step 3 displays detected device info
- [ ] Name input has sensible default
- [ ] Name validation prevents empty/invalid names
- [ ] "Back" returns to Step 1 without losing data
- [ ] "Save & Connect" creates device record
- [ ] Device appears on Filters page immediately
- [ ] Device appears on Dashboard connected devices
- [ ] WebSocket connection starts automatically
- [ ] Success toast confirms addition
- [ ] Modal closes after successful save
- [ ] Error handling for save failures

---

### US-024: Display Filter Cards on Filters Page
**As a** user
**I want** to see my connected filters as cards
**So that** I can view their status at a glance

**Technical Details:**
- Create `FilterCard.tsx` component
- Query filters for current aquarium (type='filter', manufacturer='eheim')
- Display cards in grid:
  - 1 filter: Full width (max 600px centered)
  - 2+ filters: 2 columns on desktop, 1 on mobile
- Each card shows:
  - **Header:**
    - Filter name (large, bold)
    - Status indicator (dot): green=connected, yellow=connecting, red=offline
  - **Body:**
    - Model name (smaller text)
    - Current mode badge: "Manual" | "Pulse" | "Constant Flow" | "Bio Mode"
    - Active flow rate: "850 L/h" (from real-time data)
    - Current frequency: "5400 Hz" (from WebSocket)
  - **Footer:**
    - Last updated: "2 seconds ago"
    - Context menu (3-dot icon): Edit name, Remove filter
- Card styling:
  - Glassmorphism with mode-specific accent color
  - Hover effect: slight scale + shadow
  - Click opens filter detail view
- Real-time updates via WebSocket `FILTER_DATA` messages
- Loading skeleton while fetching devices
- Error state if no connection

**Acceptance Criteria:**
- [ ] Filter cards display in grid layout
- [ ] Cards show name, model, status, mode, flow rate
- [ ] Status indicator colors match connection state
- [ ] Real-time updates reflected (flow, frequency)
- [ ] Clicking card opens detail view
- [ ] Context menu allows edit/remove
- [ ] Appropriate card sizing for 1-2 filters
- [ ] Loading state shows during fetch
- [ ] Offline filters clearly indicated

---

### US-025: Implement Filter Detail View
**As a** user
**I want** to view and control a specific filter
**So that** I can manage its operation

**Technical Details:**
- Create `FilterDetailView.tsx` component
- Replace Filters content area when card clicked
- **Header Section:**
  - Back button (top-left arrow) returns to filter list
  - Filter name (editable inline with pencil icon)
  - Model name subtitle
  - Connection status badge
- **Main Controls:**
  - Large On/Off toggle switch (glassmorphism)
    - Calls `window.electron.eheim.setPower(deviceId, isOn)`
    - POST: `{ filterActive: 1|0, to: macAddress }`
    - Optimistic UI update with rollback on error
  - Current status display:
    - "Filter is ON/OFF"
    - Frequency: "5400 Hz / 7100 Hz max"
    - Running time: "1,234 hours" (from `runTime` field)
- **Mode Selection (4 buttons in grid):**
  - Button layout: 2x2 grid
  - Each button shows:
    - Mode icon
    - Mode name
    - Gear icon (opens config modal)
    - Active state highlight
  - Modes:
    1. **Manual Mode** - Direct speed control
    2. **Pulse Mode** - Intermittent flow (if supported)
    3. **Constant Flow** - Auto-adjusting for pollution
    4. **Bio Mode** - Day/night cycles
  - Only one mode can be active (mutually exclusive)
  - Clicking mode button activates it (with confirmation if switching)
  - Gear icon opens mode configuration modal
- **Status Information Panel:**
  - Current mode details
  - Service hours remaining (if available)
  - Pollution grade (from `dfsFaktor` - optional, can be unreliable)
  - Last maintenance date (user-entered, future feature)
- Loading state during commands
- Error toasts for failed operations

**Acceptance Criteria:**
- [ ] Detail view shows when filter card clicked
- [ ] Back button returns to filter grid
- [ ] Filter name is editable inline
- [ ] On/Off toggle controls power successfully
- [ ] Toggle state reflects real device state
- [ ] All 4 mode buttons visible
- [ ] Active mode is highlighted
- [ ] Gear icons open mode config modals
- [ ] Mode switching works correctly
- [ ] Frequency and runtime display accurately
- [ ] Loading states show during operations
- [ ] Error handling with user-friendly messages

---

### US-026: Implement Manual Mode Configuration
**As a** user
**I want** to set my filter's speed manually
**So that** I have direct control over flow rate

**Technical Details:**
- Create `ManualModeModal.tsx` component
- Opens from gear icon on Manual Mode button
- **Display:**
  - Modal title: "Manual Mode Settings"
  - Current frequency display: "5400 Hz"
  - Speed slider (0-10 discrete steps)
    - Labels: "Min (0)" to "Max (10)"
    - Show equivalent flow rate: "450 L/h" (model-specific conversion)
    - Show equivalent frequency: "~4200 Hz"
    - Use discrete steps (not continuous)
  - Alternative percentage display: "60%" (optional)
  - Buttons: "Cancel", "Apply"
- Model-specific flow rate conversion:
  ```javascript
  const flowRates = {
    '5e 350': { min: 150, max: 1500 },
    '5e 450': { min: 350, max: 1700 },
    '5e 700': { min: 400, max: 1850 },
    '5e 600T': { min: 400, max: 1850 }
  };
  ```
- On Apply:
  - POST to `/api/filter`:
    ```json
    {
      "to": "A8:48:FA:D7:A0:F7",
      "pumpMode": 1,
      "rotorSpeed": 6,  // 0-10 value
      "filterActive": 1
    }
    ```
  - Wait 1 second
  - Verify via WebSocket `FILTER_DATA` message
  - Show success toast: "Manual mode activated"
  - Close modal
  - Update detail view to show Manual mode active
- Handle errors (device offline, invalid value)

**Acceptance Criteria:**
- [ ] Modal opens from Manual Mode gear icon
- [ ] Slider has 11 discrete positions (0-10)
- [ ] Flow rate updates as slider moves
- [ ] Model-specific flow rates display correctly
- [ ] Current setting loads when modal opens
- [ ] Apply button sends command successfully
- [ ] Settings are verified after POST
- [ ] Success toast appears on successful change
- [ ] Modal closes after apply
- [ ] Cancel closes without changing
- [ ] Error handling for failed commands

---

### US-027: Implement Constant Flow Mode Configuration
**As a** user
**I want** to configure my filter's Constant Flow mode
**So that** it maintains steady output despite pollution buildup

**Technical Details:**
- Create `ConstantFlowModal.tsx` component
- Opens from gear icon on Constant Flow Mode button
- **Display:**
  - Modal title: "Constant Flow Mode Settings"
  - Description: "Automatically adjusts pump speed to maintain consistent flow as the filter loads with debris."
  - Target flow slider (0-10 discrete steps)
    - Label: "Target Flow Rate"
    - Show L/h or gal/h based on aquarium volume unit setting
    - Model-specific conversion (same as Manual mode)
    - Current frequency indicator: "Running at 5400 Hz"
  - Pollution compensation indicator:
    - Progress bar showing current frequency vs max frequency
    - "Filter is at 60% of maximum compensation"
    - Explanation: "Service recommended when approaching 100%"
  - Buttons: "Cancel", "Apply"
- On Apply:
  - POST to `/api/filter`:
    ```json
    {
      "to": "macAddress",
      "pumpMode": 3,
      "flowRate": 7,  // 0-10 discrete level (NOT L/h)
      "filterActive": 1
    }
    ```
  - Verify via WebSocket
  - Show success toast
  - Close modal
- Real-time frequency monitoring:
  - Listen to `FILTER_DATA` messages
  - Show `freq` (current) vs `freqSoll` (target)
  - Indicate when pollution compensation is active

**Acceptance Criteria:**
- [ ] Modal opens from Constant Flow gear icon
- [ ] Target flow slider has 0-10 discrete steps
- [ ] Flow rate displays in correct unit (L/h or gal/h)
- [ ] Model-specific flow rates show correctly
- [ ] Current setting loads on modal open
- [ ] Pollution compensation indicator updates in real-time
- [ ] Apply sends correct pumpMode (3) and flowRate
- [ ] Settings verified via WebSocket
- [ ] Success toast appears
- [ ] Modal closes after apply
- [ ] Cancel closes without changing
- [ ] Frequency monitoring shows live data

---

### US-028: Implement Bio Mode Configuration
**As a** user
**I want** to configure my filter's Bio Mode with day/night cycles
**So that** it simulates natural water flow patterns

**Technical Details:**
- Create `BioModeModal.tsx` component
- Opens from gear icon on Bio Mode button
- **Display:**
  - Modal title: "Bio Mode Settings (Sun/Moon Mode)"
  - Description: "Alternate between two flow rates on a schedule to simulate natural currents."
  - **Daytime Section:**
    - Label: "Daytime Settings"
    - Start time picker (HH:MM format, default: "08:00")
    - End time picker (HH:MM format, default: "20:00")
    - Flow rate slider (0-10 discrete, show L/h)
    - Duration indicator: "12 hours" (calculated)
  - **Nighttime Section:**
    - Label: "Nighttime Settings"
    - Time range (calculated): "20:00 - 08:00"
    - Duration: "12 hours" (calculated)
    - Flow rate slider (0-10 discrete, show L/h)
  - **Current Status:**
    - "Currently in: Day mode" or "Night mode"
    - "Next transition in: 3h 24m"
  - Buttons: "Cancel", "Apply"
- Time conversion:
  ```javascript
  const timeToMinutes = (timeStr) => {
    const [hours, minutes] = timeStr.split(':').map(Number);
    return hours * 60 + minutes;
  };
  ```
- On Apply:
  - POST to `/api/filter` with WebSocket title:
    ```json
    {
      "title": "START_NOCTURNAL_MODE",
      "to": "macAddress",
      "from": "USER",
      "pumpMode": 4,
      "dfs_soll_day": 8,  // 0-10 day flow level
      "dfs_soll_night": 3,  // 0-10 night flow level
      "end_time_night_mode": 480,  // minutes since midnight (08:00)
      "start_time_night_mode": 1200  // minutes since midnight (20:00)
    }
    ```
  - Verify via WebSocket `FILTER_DATA`:
    - Check `pumpMode: 4`
    - Verify `nm_dfs_soll_day`, `nm_dfs_soll_night`
    - Verify time settings
  - Show success toast
  - Close modal

**Acceptance Criteria:**
- [ ] Modal opens from Bio Mode gear icon
- [ ] Daytime start/end time pickers work
- [ ] Nighttime range calculated automatically
- [ ] Duration calculations update in real-time
- [ ] Both flow rate sliders work (0-10 discrete)
- [ ] Flow rates show in correct unit
- [ ] Current mode (day/night) displays correctly
- [ ] Next transition countdown accurate
- [ ] Current settings load on modal open
- [ ] Apply sends correct WebSocket command
- [ ] Settings verified after POST
- [ ] Success toast appears
- [ ] Modal closes after apply
- [ ] Cancel closes without changing
- [ ] Validation prevents end time before start time

---

### US-029: Implement Mode Switching Logic
**As a** user
**I want** only one filter mode active at a time
**So that** my filter operates correctly

**Technical Details:**
- Mode switching happens via `pumpMode` parameter:
  - `pumpMode: 1` = Manual
  - `pumpMode: 2` = Pulse
  - `pumpMode: 3` = Constant Flow
  - `pumpMode: 4` = Bio Mode
- Clicking any mode button:
  - If already active: Open config modal immediately
  - If different mode active:
    1. Show confirmation: "Switch to [New Mode]? Current [Old Mode] settings will be deactivated."
    2. On confirm: POST with new `pumpMode` value
    3. Open config modal for new mode
    4. Update UI to highlight new active mode
- Visual feedback:
  - Active mode: Darker glassmorphism, accent border, checkmark icon
  - Inactive modes: Lighter glassmorphism, no border
  - Transition animation: 200ms ease
- Real-time sync:
  - Listen to WebSocket `FILTER_DATA` messages
  - Update active mode if changed externally (via physical button or web interface)
  - Show notification: "Mode changed to [Mode] externally"
- Error handling:
  - If mode switch fails, rollback UI
  - Show error toast: "Failed to switch mode. Check connection."
  - Re-enable previous mode button

**Acceptance Criteria:**
- [ ] Only one mode can be active at a time
- [ ] Clicking inactive mode shows confirmation
- [ ] Confirmation dialog clear and user-friendly
- [ ] Mode switch sends correct API command
- [ ] Active mode button has distinct styling
- [ ] UI updates immediately (optimistic)
- [ ] Mode switches via API successfully verified
- [ ] External mode changes detected and reflected
- [ ] Error handling rolls back UI on failure
- [ ] Notification shows for external changes

---

### US-030: Implement Pulse Mode Configuration (Optional)
**As a** user
**I want** to configure my filter's Pulse mode
**So that** it creates intermittent flow patterns

**Technical Details:**
- Create `PulseModeModal.tsx` component (if Pulse mode is supported)
- **NOTE:** Pulse mode details are not well-documented in the API guide
- May need reverse engineering from actual device
- Likely parameters:
  - Pulse frequency (how often)
  - Pulse duration (how long each pulse)
  - Flow rate during pulse
  - Flow rate during rest period
- Placeholder implementation until confirmed:
  - Show coming soon message
  - Or basic on/off toggle only
  - POST: `{ pumpMode: 2, filterActive: 1 }`
- Further research needed from:
  - Community forums
  - Actual device testing
  - WebSocket message inspection

**Acceptance Criteria:**
- [ ] If supported: Modal implements pulse configuration
- [ ] If unsupported: Shows "Coming soon" or simple toggle
- [ ] Mode activates successfully (pumpMode: 2)
- [ ] Documentation updated when details confirmed

---

### US-031: Implement POST Verification Pattern
**As a** developer
**I want** to verify that device accepted configuration changes
**So that** the UI reflects actual device state

**Technical Details:**
- **Problem:** API returns HTTP 200 on successful auth/parsing, NOT on parameter acceptance
- Device validates params against min/max independently
- Out-of-range values silently rejected
- **Solution:** POST-then-GET verification pattern
- Implementation in `EheimRestClient`:
  ```javascript
  async setAndVerify(deviceIP, mac, params) {
    // 1. Send configuration
    await this.post(deviceIP, mac, params);

    // 2. Wait for device to process (1 second)
    await new Promise(resolve => setTimeout(resolve, 1000));

    // 3. Get current state via WebSocket or REST
    const currentState = await this.getCurrentState(deviceIP, mac);

    // 4. Compare sent vs actual
    const verification = {
      accepted: true,
      rejectedParams: []
    };

    for (const [key, value] of Object.entries(params)) {
      if (currentState[key] !== value) {
        verification.accepted = false;
        verification.rejectedParams.push(key);
      }
    }

    return verification;
  }
  ```
- Use verification in all config modals:
  - If accepted: Show success toast, close modal
  - If rejected: Show error toast with rejected params, keep modal open
  - Example: "Flow rate rejected. Value out of range for this model."
- Log verification failures for debugging

**Acceptance Criteria:**
- [ ] All POST requests followed by verification
- [ ] 1-second delay after POST
- [ ] Current state retrieved via WebSocket
- [ ] Sent params compared against actual state
- [ ] Rejected params identified clearly
- [ ] User-friendly error messages for rejections
- [ ] Verification failures logged
- [ ] Modal stays open on rejection

---

### US-032: Display Firmware Version and Update Warnings
**As a** user
**I want** to be notified if my filter firmware is outdated
**So that** I can update it for optimal performance

**Technical Details:**
- Firmware info from WebSocket `USRDTA` message:
  ```json
  {
    "title": "USRDTA",
    "revision": [2037, 1025],  // [master, client]
    "latestAvailableRevision": [2037, 1025],
    "firmwareAvailable": 0
  }
  ```
- Minimum required versions:
  - Master: S2037
  - Client: S1025
- Display firmware version in Filter Detail View:
  - Small text below model name
  - "Firmware: S2037" (green checkmark if OK)
  - "Firmware: S2036 - Update Available" (yellow warning if outdated)
- Show warning modal on first connection if outdated:
  - Title: "Firmware Update Recommended"
  - Message: "Your filter is running firmware S[VERSION], which may have bugs affecting API stability. Please update to S2037 or later for the best experience."
  - Link: "Download firmware update" → https://eheim.com/en_GB/support/downloads/
  - Checkbox: "Don't show this again for this device"
  - Buttons: "Update Later", "Open Download Page"
- Store dismissed warnings in device config
- Check for new firmware versions:
  - Compare `revision` vs `latestAvailableRevision`
  - Show notification if `firmwareAvailable: 1`

**Acceptance Criteria:**
- [ ] Firmware version displayed in detail view
- [ ] Outdated firmware shows warning icon
- [ ] Warning modal appears on first connection (if outdated)
- [ ] Download link opens in default browser
- [ ] "Don't show again" prevents future warnings for that device
- [ ] Available updates detected from `USRDTA`
- [ ] Notification shows when new firmware available
- [ ] Minimum version check works correctly

---

### US-033: Handle Connection Errors and Offline Devices
**As a** user
**I want** to see clear status when my filter is offline
**So that** I can troubleshoot connectivity issues

**Technical Details:**
- Implement connection health monitoring:
  - WebSocket connection state tracking
  - Last message timestamp (timeout: 30 seconds)
  - Ping/pong heartbeat (optional)
- Device status states:
  - `connecting` - Initial connection attempt (yellow)
  - `connected` - Active WebSocket + recent messages (green)
  - `offline` - WebSocket closed or no messages for 30s (red)
  - `error` - Connection failed or API error (red with warning)
- Reconnection logic:
  - On disconnect: Attempt reconnect every 5s
  - Exponential backoff: 5s, 10s, 30s, 60s
  - Max retries: Infinite (until manual disconnect)
  - Reset backoff on successful connection
- UI indicators:
  - Status dot on filter card (color-coded)
  - Status badge in detail view
  - "Last seen" timestamp: "5 minutes ago"
  - Overlay on offline cards: "Offline - Reconnecting..."
- Error messages:
  - Network error: "Filter offline. Check WiFi connection."
  - Auth error: "Authentication failed. Credentials may have changed."
  - Timeout: "Connection timed out. Filter may be powered off."
- Offline mode:
  - Show last known state (grayed out)
  - Disable all controls
  - Show "Reconnect" button
- Toast notifications:
  - On disconnect: "Filter disconnected"
  - On reconnect: "Filter reconnected"
  - Throttle notifications (max 1 per minute)

**Acceptance Criteria:**
- [ ] Connection state tracked accurately
- [ ] 30-second timeout detects offline devices
- [ ] Automatic reconnection attempts work
- [ ] Exponential backoff implemented correctly
- [ ] Status indicators update in real-time
- [ ] Offline devices show last known state
- [ ] Controls disabled when offline
- [ ] Error messages are specific and helpful
- [ ] Toast notifications inform user of state changes
- [ ] Manual reconnect button works
- [ ] Infinite retry doesn't cause performance issues

---

### US-034: Optimize Performance for Multiple Filters
**As a** developer
**I want** to efficiently manage multiple WebSocket connections
**So that** the app remains responsive with multiple devices

**Technical Details:**
- **Current expectation:** Most users have 1-2 filters per aquarium
- **Optimization strategies:**
  - Single WebSocket manager instance
  - Connection pooling (one connection per device)
  - Message batching for UI updates (throttle to 60fps)
  - Debounce status updates (250ms)
  - Pause connections when app minimized (optional)
- WebSocket message handling:
  - Parse messages in main process (not renderer)
  - Only send relevant updates to renderer
  - Filter out duplicate messages
- Memory management:
  - Close connections for devices removed
  - Cleanup listeners on component unmount
  - Clear message queues periodically
- CPU optimization:
  - Don't poll REST API (WebSocket only)
  - Batch IPC messages to renderer
  - Use React.memo for filter cards
  - Virtualize large device lists (if needed)
- Network optimization:
  - Reuse HTTP connections (keep-alive)
  - Compress WebSocket messages (if supported)
  - Cache GET responses (5 seconds)
- Performance monitoring:
  - Log connection count
  - Track message rates
  - Monitor memory usage (dev mode)

**Acceptance Criteria:**
- [ ] WebSocket manager handles multiple connections
- [ ] No performance degradation with 2-3 devices
- [ ] UI updates throttled appropriately
- [ ] Memory leaks prevented (connections cleanup)
- [ ] App remains responsive during heavy updates
- [ ] Minimized app reduces network activity
- [ ] Performance metrics logged in dev mode

---

### US-035: Add Manual IP Entry for Discovery Fallback
**As a** user
**I want** to manually enter my filter's IP address
**So that** I can connect even if mDNS discovery fails

**Technical Details:**
- Add "Manual Entry" option in discovery modal
- Show input form:
  - IP address field (validation: IPv4 format)
  - Port field (default: 80)
  - "Test Connection" button
- On "Test Connection":
  - Attempt HTTP GET to `http://[ip]:[port]/api/filter`
  - Check for 401 (auth required) or 200 (success)
  - If successful: Proceed to fetch device details (Step 2)
  - If failed: Show error with suggestions
- Error suggestions:
  - "Connection refused" → "Check IP address and ensure filter is powered on"
  - "Timeout" → "Ensure filter is on the same WiFi network"
  - "404" → "This may not be an Eheim device"
- Save successful manual IPs for future quick connect
- Validation:
  - IP format: 192.168.x.x or 10.x.x.x (private ranges)
  - Port range: 1-65535
  - Non-empty fields
- Show helpful examples:
  - Placeholder: "192.168.1.100"
  - Info text: "Find your filter's IP in your router settings"

**Acceptance Criteria:**
- [ ] Manual entry option visible in discovery modal
- [ ] IP address validation works correctly
- [ ] Port field has sensible default (80)
- [ ] Test connection attempts HTTP request
- [ ] Success proceeds to device details step
- [ ] Failure shows specific error messages
- [ ] Private IP ranges enforced (security)
- [ ] Manual IPs saved for quick reconnect
- [ ] Helpful examples and guidance provided

---

## Summary of Changes

**User Stories Removed:**
- ❌ Old US-019 (Cloud API authentication - doesn't exist)
- ❌ Old US-025 (REST polling - anti-pattern)

**User Stories Significantly Revised:**
- ✏️ US-019 → Now mDNS Discovery Service
- ✏️ US-021 → Now focuses on UI for discovery, not API calls
- ✏️ US-025 → Now WebSocket connection manager (in US-019a)
- ✏️ US-026 → Corrected to 0-10 scale
- ✏️ US-027 → Corrected to 0-10 scale + implementation details
- ✏️ US-028 → Expanded to cover all 4 modes

**User Stories Added:**
- ✅ US-019a: WebSocket Connection Manager (critical)
- ✅ US-019b: REST API Client with verification pattern
- ✅ US-022: Firmware version detection (split from old US-021)
- ✅ US-026: Manual Mode configuration (was missing)
- ✅ US-030: Pulse Mode configuration (placeholder)
- ✅ US-031: POST verification pattern (API quirk)
- ✅ US-032: Firmware warnings and updates
- ✅ US-033: Connection error handling
- ✅ US-034: Performance optimization
- ✅ US-035: Manual IP entry fallback

**Total User Stories:** 19 (was 10) - Epic is now more realistic and detailed.
