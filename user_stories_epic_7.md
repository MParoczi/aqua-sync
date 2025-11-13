## Epic 7: Lamps (Chihiros Integration)

### US-029: Implement Chihiros Bluetooth Service
**As a** developer  
**I want** to discover and control Chihiros lamps via Bluetooth  
**So that** users can manage their lighting

**Technical Details:**
- Create `ChihirosService` in Electron main process
- Use Node.js Bluetooth library (e.g., `noble` or `@abandonware/noble`)
- Reference GitHub repo: `https://github.com/TheMicDiet/chihiros-led-control`
- Implement:
    - Bluetooth scanning for Chihiros devices
    - Pairing and connection
    - Send commands for on/off, brightness, LED channels
    - Read device status
- Handle connection errors gracefully

**Acceptance Criteria:**
- [ ] Bluetooth scanning works
- [ ] Can connect to Chihiros lamps
- [ ] Can send on/off commands
- [ ] Can control brightness/LED channels
- [ ] Can read device status
- [ ] Connection errors are handled

---

### US-030: Create Lamps Page Empty State
**As a** user  
**I want** to see instructions when no lamps are connected  
**So that** I know how to add one

**Technical Details:**
- Create `LampsContent` component
- Empty state:
    - Message: "Connect your Chihiros lamp!"
    - Prominent "Connect lamp" button
- When lamps exist:
    - Display lamp cards in grid
    - Show "Add new lamp" button in top-right

**Acceptance Criteria:**
- [ ] Empty state shows when no lamps
- [ ] "Connect lamp" button is prominent
- [ ] Button opens lamp connection modal
- [ ] Grid shows when lamps exist
- [ ] "Add new lamp" button appears when lamps exist

---

### US-031: Implement Lamp Discovery Modal (Step 1)
**As a** user  
**I want** to discover available Chihiros lamps via Bluetooth  
**So that** I can add them to my aquarium

**Technical Details:**
- Create two-step modal: `LampConnectionModal`
- **Step 1: Discovery**
    - Show loading indicator: "Searching for Chihiros lamps..."
    - Scan for Bluetooth devices
    - Filter for Chihiros-specific identifiers
    - Exclude lamps already assigned to other aquariums
    - Display list of available lamps
    - Buttons: "Cancel", "Next" (disabled if none selected)
    - Progress indicator: "1 of 2"

**Acceptance Criteria:**
- [ ] Modal opens showing loading state
- [ ] Bluetooth scan discovers lamps
- [ ] Already-assigned lamps are excluded
- [ ] Available lamps display in list
- [ ] User can select one lamp
- [ ] "Next" button is disabled until selection
- [ ] "Next" proceeds to Step 2
- [ ] "Cancel" closes modal
- [ ] Error messages show if scan fails

---

### US-032: Implement Lamp Naming Modal (Step 2)
**As a** user  
**I want** to give my lamp a custom name  
**So that** I can easily identify it

**Technical Details:**
- **Step 2: Naming**
    - Display selected lamp's model (read-only)
    - Name input field (default: "Chihiros Lamp")
    - Buttons: "Back", "Save"
    - Progress indicator: "2 of 2"
- On Save:
    - Create device record in `devices.json`
    - Close modal
    - Show success toast
    - Refresh Lamps page

**Acceptance Criteria:**
- [ ] Step 2 shows selected lamp info
- [ ] Name input has sensible default
- [ ] "Back" returns to Step 1
- [ ] "Save" creates device record
- [ ] Modal closes after save
- [ ] Success toast appears
- [ ] Lamps page shows new lamp
- [ ] Lamp appears on Dashboard

---

### US-033: Display Lamp Cards on Lamps Page
**As a** user  
**I want** to see my connected lamps as cards  
**So that** I can view their status at a glance

**Technical Details:**
- Query lamps for current aquarium
- Display lamp cards in grid (typically 1-2 lamps)
- Each card shows:
    - Lamp name
    - Connection status
    - Active mode (e.g., "Automatic mode - ON")
- Cards are clickable to view details
- Larger cards than typical grid items (account for 1-2 items max)

**Acceptance Criteria:**
- [ ] Lamp cards display in grid
- [ ] Cards show name, status, and mode
- [ ] Cards have appropriate size for 1-2 items
- [ ] Clicking card opens lamp detail view
- [ ] Status polling updates cards every 5 seconds

---

### US-034: Implement Lamp Detail View
**As a** user  
**I want** to view and control a specific lamp  
**So that** I can manage its operation

**Technical Details:**
- Create `LampDetailView` component
- Replace Lamps content when lamp card clicked
- Show back button (top-left) to return to lamp list
- Display:
    - Lamp name (large)
    - Lamp model (below name)
    - Connection status
    - On/Off toggle button (large, prominent)
    - Automatic mode button with gear icon
    - Manual mode button with gear icon
- Toggle button controls lamp on/off via Bluetooth
- Mode buttons show active state (one active at a time)

**Acceptance Criteria:**
- [ ] Detail view shows when lamp clicked
- [ ] Back button returns to lamp list
- [ ] Lamp info displays correctly
- [ ] Toggle button turns lamp on/off
- [ ] Toggle state reflects actual lamp state
- [ ] Mode buttons show which is active
- [ ] Gear icons are visible on mode buttons

---

### US-035: Implement Automatic Mode Configuration
**As a** user  
**I want** to configure my lamp's Automatic mode  
**So that** it turns on/off and ramps automatically

**Technical Details:**
- Create `AutomaticModeModal` component
- Display:
    - Mode name: "Automatic mode settings"
    - **LED settings** (if lamp model supports RGBW):
        - Red slider (0-130%)
        - Green slider (0-130%)
        - Blue slider (0-130%)
        - White slider (0-130%)
        - Validation: Average of 4 LEDs ≤ 100%
    - **OR** (if lamp only supports dimming):
        - Single brightness slider (0-100%)
    - **Time settings:**
        - Start hour (time picker)
        - End hour (time picker)
    - **Ramp up/down:**
        - Slider: 0 to 2 hours, in 30-minute increments
        - Label shows selected duration
    - Buttons: "Cancel", "Save"
- Detect lamp model capabilities from Bluetooth device info
- Load current settings when modal opens
- On Save:
    - Send Bluetooth commands to configure schedule
    - Close modal
    - Show success/error toast

**Acceptance Criteria:**
- [ ] Modal opens from gear icon
- [ ] LED sliders show if model supports RGBW
- [ ] Single brightness slider shows if dimming only
- [ ] LED average validation works (4 LEDs average ≤ 100%)
- [ ] Time pickers work correctly
- [ ] Ramp slider works in 30-min increments (0-2h)
- [ ] Current settings load correctly
- [ ] Save applies settings via Bluetooth
- [ ] Success/error toast appears
- [ ] Modal closes after save
- [ ] Cancel closes without changing

---

### US-036: Implement Manual Mode Configuration
**As a** user  
**I want** to configure my lamp's Manual mode  
**So that** I can set a static brightness level

**Technical Details:**
- Create `ManualModeModal` component
- Display:
    - Mode name: "Manual mode settings"
    - **LED settings** (if lamp model supports RGBW):
        - Red slider (0-130%)
        - Green slider (0-130%)
        - Blue slider (0-130%)
        - White slider (0-130%)
        - Validation: Average of 4 LEDs ≤ 100%
    - **OR** (if lamp only supports dimming):
        - Single brightness slider (0-100%)
    - Buttons: "Cancel", "Save"
- No time/ramp settings (Manual mode is static)
- Load current settings when modal opens
- On Save:
    - Send Bluetooth commands to set brightness
    - Close modal
    - Show success/error toast

**Acceptance Criteria:**
- [ ] Modal opens from gear icon
- [ ] LED sliders show if model supports RGBW
- [ ] Single brightness slider shows if dimming only
- [ ] LED average validation works
- [ ] Current settings load correctly
- [ ] Save applies settings via Bluetooth
- [ ] Success/error toast appears
- [ ] Modal closes after save
- [ ] Cancel closes without changing

---

### US-037: Implement Lamp Mode Switching Logic
**As a** user  
**I want** my lamp to behave correctly based on selected mode  
**So that** lighting operates as expected

**Technical Details:**
- **Manual Mode:**
    - When activated: Lamp turns on immediately at configured brightness
    - When deactivated: Lamp turns off immediately
- **Automatic Mode:**
    - When activated: Lamp follows schedule
        - Turns on at start time with ramp-up
        - Stays at configured brightness during on-period
        - Turns off at end time with ramp-down
    - When deactivated: Lamp turns off immediately
- Only one mode active at a time
- Switching modes deactivates the other
- Send appropriate Bluetooth commands for mode switching

**Acceptance Criteria:**
- [ ] Only one mode can be active
- [ ] Activating a mode deactivates the other
- [ ] Manual mode turns lamp on/off immediately
- [ ] Automatic mode follows schedule with ramp
- [ ] UI reflects active mode correctly
- [ ] Mode switches via Bluetooth successfully