## Epic 2: Aquarium Management (Landing Page)

### US-005: Create Empty State Landing Page
**As a** new user  
**I want** to see a welcome screen when I first open the app  
**So that** I understand how to get started

**Technical Details:**
- Create `LandingPage` component
- Detect if any aquariums exist using data service
- Show centered welcome message: "Welcome to Aqua Sync" + "Create your first aquarium!"
- Implement "Create new aquarium" button with hover effects
- Button opens aquarium creation modal

**Acceptance Criteria:**
- [ ] Welcome screen shows when no aquariums exist
- [ ] "Create new aquarium" button is prominent and centered
- [ ] Button has hover/focus states with glassmorphism effects
- [ ] Modal opens when button is clicked

---

### US-006: Implement Aquarium Creation Modal
**As a** user  
**I want** to create a new aquarium with all its details  
**So that** I can start managing my aquarium

**Technical Details:**
- Create `AquariumModal` component with glassmorphism styling
- Form fields:
    - Name (required, text input)
    - Type (required, radio buttons: Freshwater/Marine)
    - Dimensions: Width, Length, Height (required, number inputs)
    - Unit selector for dimensions (radio: Centimetre/Inch)
    - Volume (auto-calculated, editable)
    - Unit selector for volume (radio: Litre/Gallon)
    - Start date (required, date picker)
    - Thumbnail (optional, file upload)
- Auto-calculate volume: `width × length × height` (convert to litres/gallons)
- Allow manual volume override
- Generate GUID for new aquarium using `crypto.randomUUID()`
- Handle image upload: copy to `userData/thumbnails/` folder
- Show validation errors inline

**Volume Calculation:**
```typescript
// If dimensions in cm, volume in litres: (W × L × H) / 1000
// If dimensions in inches, volume in gallons: (W × L × H) / 231
```

**Acceptance Criteria:**
- [ ] Modal opens with all fields
- [ ] Volume auto-calculates when dimensions change
- [ ] Unit conversion works correctly
- [ ] User can override auto-calculated volume
- [ ] Form validation works (all required fields)
- [ ] Image upload copies file to userData folder
- [ ] "Save" creates aquarium and closes modal
- [ ] "Cancel" closes modal without saving
- [ ] Success toast appears after saving

---

### US-007: Display Aquarium Grid on Landing Page
**As a** user  
**I want** to see all my aquariums in a grid layout  
**So that** I can quickly view and select them

**Technical Details:**
- Create `AquariumGrid` component
- Display aquarium cards in responsive grid (3-4 columns)
- Each card shows:
    - Thumbnail image (or default placeholder)
    - Aquarium name
    - Type (Freshwater/Marine) with icon
- Sort aquariums by `createdAt` (oldest first)
- Add "Add new aquarium" button in top-right corner
- Card hover effects with glassmorphism

**Acceptance Criteria:**
- [ ] Grid displays all aquariums
- [ ] Cards show thumbnail, name, and type
- [ ] Default image used if no thumbnail uploaded
- [ ] Cards are sorted by creation date (oldest first)
- [ ] "Add new aquarium" button visible in top-right
- [ ] Cards have hover effects

---

### US-008: Implement Aquarium Context Menu
**As a** user  
**I want** to edit or delete aquariums via right-click  
**So that** I can manage my aquarium list

**Technical Details:**
- Implement right-click context menu on aquarium cards
- Menu options: "Edit", "Delete"
- **Edit**: Opens `AquariumModal` pre-filled with aquarium data
- **Delete**: Opens confirmation modal
- Update aquarium: Keep same `id` and `createdAt`, preserve grid position
- Delete aquarium: Remove from storage, show success toast

**Delete Confirmation Modal:**
- Message: "Are you sure you want to delete [Aquarium Name]?"
- Buttons: "Cancel", "Delete" (danger style)

**Acceptance Criteria:**
- [ ] Right-click opens context menu
- [ ] Edit opens modal with pre-filled data
- [ ] Edited aquarium updates in grid (same position)
- [ ] Delete shows confirmation modal
- [ ] Delete confirmation removes aquarium from grid
- [ ] Success/error toasts show for operations

---

### US-009: Navigate to Main Page from Aquarium Card
**As a** user  
**I want** to click an aquarium card to open its main page  
**So that** I can manage that specific aquarium

**Technical Details:**
- Implement click handler on aquarium cards (left-click)
- Store selected aquarium ID in app state (React Context or Zustand)
- Navigate to `MainPage` component
- Pass aquarium ID to MainPage

**Acceptance Criteria:**
- [ ] Clicking aquarium card navigates to MainPage
- [ ] Selected aquarium data is available in MainPage
- [ ] Navigation is smooth (no flicker)