## Epic 8: Polish & Error Handling

### US-045: Implement Comprehensive Error Handling
**As a** user  
**I want** to see helpful error messages when things go wrong  
**So that** I understand what happened and how to fix it

**Technical Details:**
- Handle common error scenarios:
    - Network errors (API unreachable)
    - Device offline/disconnected
    - Bluetooth pairing failures
    - Invalid form inputs
    - File I/O errors
    - API authentication failures
- Show error toasts with clear messages
- Add retry mechanisms where appropriate
- Log errors to file for debugging

**Error Messages:**
- "Unable to connect to Eheim API. Please check your internet connection."
- "Filter is offline. Please check the device."
- "Failed to discover Chihiros lamps. Please ensure Bluetooth is enabled."
- "Invalid value. pH must be between 1 and 14."
- etc.

**Acceptance Criteria:**
- [ ] All API errors show user-friendly messages
- [ ] Device connection errors are handled gracefully
- [ ] Form validation errors are clear
- [ ] Users can retry failed operations
- [ ] Errors are logged for debugging

---

### US-046: Implement Loading States and Spinners
**As a** user  
**I want** to see loading indicators during operations  
**So that** I know the app is working

**Technical Details:**
- Add loading spinners for:
    - Device discovery (filters, lamps)
    - API calls (status, control commands)
    - Data loading (aquarium list, water tests)
    - File uploads (aquarium thumbnails)
- Use glassmorphism-styled spinners
- Disable buttons during operations
- Show progress text where appropriate

**Acceptance Criteria:**
- [ ] Loading spinners show during operations
- [ ] Buttons are disabled while loading
- [ ] Progress text is clear and helpful
- [ ] Spinners match glassmorphism design

---

### US-047: Implement Context Menu on Landing Page (Right-Click)
**As a** user  
**I want** a custom context menu on aquarium cards  
**So that** the right-click experience is polished

**Technical Details:**
- Implement custom context menu component with glassmorphism
- Prevent default browser context menu
- Position menu near cursor
- Close menu when clicking outside

**Acceptance Criteria:**
- [ ] Custom context menu appears on right-click
- [ ] Menu has glassmorphism styling
- [ ] Menu closes when clicking outside
- [ ] Default browser menu is suppressed

---

### US-048: Implement Auto-unit Conversion
**As a** user  
**I want** measurements to convert automatically when I change units  
**So that** I don't lose data when switching between metric/imperial

**Technical Details:**
- When user edits aquarium and changes dimension unit:
    - Convert width, length, height (cm ↔ inch)
    - Recalculate volume
- When user changes volume unit:
    - Convert volume value (L ↔ gal)
- Conversion formulas:
    - cm to inch: value / 2.54
    - inch to cm: value × 2.54
    - L to gal: value / 3.78541
    - gal to L: value × 3.78541
- Round to 2 decimal places

**Acceptance Criteria:**
- [ ] Changing dimension unit converts values
- [ ] Changing volume unit converts value
- [ ] Conversions are accurate (within rounding)
- [ ] UI updates immediately after conversion

---

### US-049: Add Keyboard Shortcuts
**As a** user  
**I want** keyboard shortcuts for common actions  
**So that** I can work more efficiently

**Technical Details:**
- Implement keyboard shortcuts:
    - `Ctrl+N`: Create new aquarium (from Landing Page)
    - `Ctrl+W`: Close modal (if open)
    - `Escape`: Close modal/context menu
    - `Ctrl+S`: Save form (in modals)
- Show shortcuts in tooltips where appropriate

**Acceptance Criteria:**
- [ ] Keyboard shortcuts work as specified
- [ ] Shortcuts are documented in UI (tooltips)
- [ ] Shortcuts don't conflict with system shortcuts

---

### US-050: Implement Default Aquarium Thumbnail
**As a** user  
**I want** a nice default image when I don't upload a thumbnail  
**So that** my aquarium cards look complete

**Technical Details:**
- Create or find a default aquarium image
- Store in app assets
- Use default image when `thumbnailPath` is not set
- Default image should match glassmorphism aesthetic

**Acceptance Criteria:**
- [ ] Default image is aesthetically pleasing
- [ ] Default image displays when no upload
- [ ] Default image matches app design

---

### US-051: Add "Days Running" Calculation to Dashboard
**As a** developer  
**I want** to calculate days since aquarium start  
**So that** users can track their tank's maturity

**Technical Details:**
- Calculate: `Math.floor((today - startDate) / (1000 * 60 * 60 * 24))`
- Display: "X days since start"
- Update daily (no need for real-time updates)

**Acceptance Criteria:**
- [ ] Days calculation is accurate
- [ ] Display updates daily
- [ ] Format is clear: "X days since start"

---

### US-052: Optimize Performance for Large Datasets
**As a** developer  
**I want** the app to perform well with many measurements  
**So that** users with long histories have a good experience

**Technical Details:**
- Limit water test queries to last 100 measurements per parameter
- Implement pagination for historical data table
- Use React.memo and useMemo for expensive computations
- Debounce graph updates
- Lazy load images

**Acceptance Criteria:**
- [ ] App remains responsive with 1000+ measurements
- [ ] Graphs render smoothly
- [ ] Tables paginate correctly
- [ ] Memory usage is reasonable

---

### US-053: Add Application Icon and Branding
**As a** designer/developer  
**I want** a professional app icon and branding  
**So that** the app looks polished

**Technical Details:**
- Design app icon (aquarium-themed)
- Set icon for:
    - Electron window
    - Windows taskbar
    - Desktop shortcut
- Add app name to window title bar
- Consistent branding throughout UI

**Acceptance Criteria:**
- [ ] App icon is professional and relevant
- [ ] Icon displays in all Windows contexts
- [ ] Window title shows "Aqua Sync"
- [ ] Branding is consistent

---

### US-054: Implement Build and Packaging
**As a** developer  
**I want** to package the app for Windows distribution  
**So that** users can install it easily

**Technical Details:**
- Configure Electron Builder for Windows
- Create installer:
    - NSIS installer or Squirrel.Windows
    - Include all dependencies
    - Set up proper file associations
- Code signing (optional, for production)
- Test installation and updates

**Acceptance Criteria:**
- [ ] App packages successfully
- [ ] Installer works on clean Windows 11 system
- [ ] App launches after installation
- [ ] All features work in packaged app
- [ ] Uninstaller works correctly