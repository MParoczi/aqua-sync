# AquaSync - Speckit Prompts

## Constitution Prompt

Run this first with `/speckit.constitution` before any specify/plan pairs.

```
AquaSync is a Windows desktop application for managing and controlling home aquariums. It is built with WinUI3, .NET 10, and C# 14, targeting Windows 10/11. The application must look and feel like a native Windows 11 application — as if Microsoft developed it.

Core principles:

1. Windows-Native Design: Every UI element must use standard WinUI3 controls and follow the Windows 11 design language. Use Mica/acrylic backdrop, NavigationView for sidebar navigation, ContentDialog for modal forms, InfoBar for alerts, and Fluent Design icons. No custom styling that deviates from the Windows 11 aesthetic. No third-party UI control libraries unless absolutely necessary (e.g., charting).

2. Local-First Data: All application data is stored locally on the user's machine as JSON files under %LOCALAPPDATA%/AquaSync/. There is no cloud sync, no remote database, no network-dependent data storage. Use System.Text.Json for serialization. The application must function fully offline except when communicating with physical aquarium devices on the local network or via Bluetooth.

3. MVVM Architecture: All UI logic follows the Model-View-ViewModel pattern using CommunityToolkit.Mvvm and Microsoft.Extensions.DependencyInjection. ViewModels inherit from ViewModelBase (which extends ObservableObject). Use manual SetProperty pattern for observable properties — do not use [ObservableProperty] attribute on fields as it is not compatible with WinUI3 AOT requirements. Views resolve ViewModels from DI via App.GetService<T>(). Services are registered in App.xaml.cs.

4. Device Integration via Existing Libraries: Hardware control for Chihiros LED lamps (BLE) and Eheim Professionel 5E filters (WebSocket) uses the existing AquaSync.Chihiros and AquaSync.Eheim libraries. These libraries are referenced as project dependencies — never duplicate or rewrite their functionality. Chihiros uses event-based patterns (marshal to UI thread via DispatcherQueue). Eheim uses System.Reactive IObservable patterns (marshal via ObserveOn). Both use async/await with CancellationToken support.

5. Minimal Dependencies: Keep NuGet dependencies to a minimum. The core stack is: Microsoft.WindowsAppSDK, CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Hosting. Add packages only when the framework does not provide the functionality natively (e.g., a charting library for water parameter graphs). Never add an ORM, database engine, or heavy framework. Prefer System.Text.Json over Newtonsoft.Json. Prefer built-in .NET APIs over wrapper libraries.

6. Single-Aquarium Context: After launch, the user selects an aquarium profile from a grid of cards. All subsequent pages (Dashboard, Lamps, Filters, Equipment, Water Parameters, Maintenance, Gallery, Fertilizers, Plants) are scoped to that selected aquarium. There is no multi-aquarium view within the shell — the user must return to the selector to switch. Settings is the only page that is global (not aquarium-scoped).

7. English Only: The application is English-only. Do not implement localization infrastructure, resource files, or x:Uid bindings. Use hardcoded English strings in XAML and code. Localization may be added in a future phase but should not influence current architecture decisions.

Technology stack:
- Runtime: .NET 10 (net10.0-windows10.0.19041.0)
- Language: C# 14 with nullable reference types enabled, file-scoped namespaces, sealed concrete classes
- UI Framework: WinUI3 via Windows App SDK 1.7
- MVVM: CommunityToolkit.Mvvm 8.4
- DI: Microsoft.Extensions.DependencyInjection + Hosting
- Data: JSON files via System.Text.Json
- Device Libraries: AquaSync.Chihiros (BLE, net10.0-windows10.0.19041.0), AquaSync.Eheim (WebSocket/System.Reactive, net10.0)
- Build: SDK-style projects, Directory.Build.props for shared settings, TreatWarningsAsErrors enabled

Project structure:
- AquaSync.sln contains three projects: AquaSync.Chihiros (library), AquaSync.Eheim (library), AquaSync.App (WinUI3 desktop app)
- AquaSync.App follows: Views/, ViewModels/, Models/, Services/, Contracts/Services/, Helpers/, Converters/, Assets/
- Navigation: MainWindow hosts a root Frame that switches between AquariumSelectorPage and ShellPage. ShellPage contains a NavigationView with a content Frame managed by INavigationService/IPageService
- Data storage: IDataService reads/writes JSON files, DataService implementation uses SemaphoreSlim for thread safety

Constraints:
- One equipment item (lamp, filter, or other) is assigned to exactly one aquarium — no sharing
- Water parameter ideal ranges are global constants per aquarium type — users cannot override them
- Photos are stored at original resolution with no compression
- No test reminders, no dosing event logging, no water change tracking
- No templates for aquarium profiles
- No tags or categories for organizing aquariums
```

---

## Specify/Plan Prompt Pairs

Each feature below is a self-contained specify/plan pair designed to be executed sequentially with `/speckit.specify` followed by `/speckit.plan`. Features are ordered by dependency — earlier features must be implemented before later ones.

---

## Feature 001: Aquarium Profiles & Selector

### Specify Prompt

```
The AquaSync application needs an aquarium profile management system as its core feature. When the application launches, the user sees a grid of aquarium profile cards with thumbnail photos. From this screen the user can create a new aquarium profile, or select an existing one to enter its management dashboard.

When creating a new aquarium profile the user fills in: name, volume, dimensions (length x width x height), aquarium type (freshwater, saltwater, or brackish), one or more substrate types, setup date, and an optional description/notes field. The user can also upload a thumbnail photo during creation — if no photo is uploaded, a default aquarium graphic is used.

Volume uses a unit of measurement toggle between liters and gallons. The chosen UOM is locked to that profile — all volume values throughout the application for that profile are displayed and calculated in the selected UOM. Dimensions use a toggle between centimeters and inches, also locked per profile.

The user registers substrates and additives as part of the aquarium profile. Each substrate/additive entry has: brand, product name, type (substrate, additive, or soil cap), layer depth, date added, and notes. Multiple substrate/additive entries are supported per profile (e.g., base layer of Power Sand + top layer of Amazonia + root tabs).

Aquarium profiles can be archived (decommissioned but history preserved) or permanently deleted. Archived profiles appear visually distinct in the selector grid. The grid of cards shows the aquarium thumbnail, name, and key details at a glance.

After selecting an aquarium from the grid, the user is taken to that aquarium's management shell with a sidebar navigation containing: Dashboard, Lamps, Filters, Other Equipment, Water Parameters, Maintenance, Gallery, Fertilizers, Plants, and Settings.
```

### Plan Prompt

```
Use the existing AquaSync.App WinUI3 project structure with MVVM pattern (CommunityToolkit.Mvvm, ViewModelBase, manual SetProperty pattern for observable properties). Use Microsoft.Extensions.DependencyInjection for service registration in App.xaml.cs.

Data storage uses the existing IDataService/DataService which stores JSON files under %LOCALAPPDATA%/AquaSync/. Aquarium profiles should be stored as individual JSON files in an "aquariums" folder, identified by GUID. Create an Aquarium model with all profile fields.

The AquariumSelectorPage already exists as a placeholder — implement it with a GridView of aquarium cards. The ShellPage with NavigationView sidebar already exists with all menu items wired. Implement the navigation flow: AquariumSelectorPage → ShellPage (via MainWindow's RootFrame).

For substrate/additive management within the profile, use a dialog or inline editing section on the profile creation/edit form.

Photos are stored as files in %LOCALAPPDATA%/AquaSync/gallery/{aquarium-id}/. The thumbnail reference is stored in the profile JSON.

Water parameter ideal ranges should be defined as global constants per aquarium type (freshwater/saltwater/brackish) — these are not user-editable.

Follow Windows 11 design patterns: use WinUI3 controls (NavigationView, GridView, ContentDialog, ComboBox, ToggleSwitch, CalendarDatePicker). The app uses Mica backdrop already configured on MainWindow.
```

---

## Feature 002: Settings

### Specify Prompt

```
The application needs a Settings page accessible from the navigation sidebar footer. Settings are global — they are not scoped to any specific aquarium profile.

The Settings page includes:

1. Default Unit of Measurement preferences: the user can set a default volume UOM (liters or gallons) and a default dimension UOM (centimeters or inches). These defaults are pre-selected when creating a new aquarium profile but can be changed per profile during creation.

2. Data folder location: displays the current path where application data is stored. The user can browse to select a different folder location. If the folder is changed, existing data should be moved to the new location.

3. Theme: the user can choose between "Follow system theme" (default), "Always light", or "Always dark". The theme change takes effect immediately without restarting the app.

4. Data export: the user can export all application data (aquarium profiles, water parameter logs, maintenance logs, equipment configurations, fertilizer plans, plant records, and photos) as a ZIP archive to a location of their choice.

5. About section: displays the application name, version number, and a brief description.
```

### Plan Prompt

```
Use the existing SettingsPage and SettingsViewModel placeholders in the AquaSync.App project. Store settings in a JSON file via the existing IDataService at the path "settings/app-settings.json".

Create an AppSettings model with properties for default volume UOM, default dimension UOM, theme preference, and data folder path.

For theme switching, use the WinUI3 FrameworkElement.RequestedTheme property on the root element. Expose an ISettingsService that loads settings at app startup and provides them to other services.

For data export, use System.IO.Compression.ZipFile to create a ZIP archive of the entire %LOCALAPPDATA%/AquaSync/ directory.

For data folder migration, copy all files from the old location to the new location and update the settings. Use a ContentDialog to confirm before migration.

Register ISettingsService as a singleton in App.xaml.cs. Other services and ViewModels that need settings (like the aquarium profile creation form for default UOM) should inject ISettingsService.
```

---

## Feature 003: Chihiros Lamp Management

### Specify Prompt

```
The Lamps page allows the user to manage Chihiros aquarium LED lamps assigned to the currently selected aquarium profile. Multiple lamps can be assigned to a single aquarium.

Adding a new lamp: the user initiates a Bluetooth Low Energy scan to discover nearby Chihiros devices. Discovered devices are shown in a list with their model name and signal strength. The user selects a device to pair and assign it to the current aquarium. One lamp can only be assigned to one aquarium — if a lamp is already assigned elsewhere, it cannot be added.

The Lamps page shows a list of all lamps assigned to the current aquarium. Each lamp entry displays: model name, device name, and current status.

Selecting a lamp opens its detail view with:

1. Manual brightness controls: individual sliders for each color channel supported by the specific lamp model. For example, a WRGB II Pro shows Red, Green, Blue, and White sliders, while an A II shows only a White slider. The channel list adapts to the lamp's device profile.

2. Visual schedule editor: a graphical timeline bar representing a 24-hour period. The user can visually set sunrise and sunset times by dragging handles on the timeline. A ramp-up duration control (0-150 minutes) sets the fade-in period during sunrise. Per-channel brightness sliders set the brightness levels during the "on" period. Weekday checkboxes select which days the schedule is active. Only one schedule per device is supported. When saved, the schedule is programmed directly onto the device — the device runs it autonomously without the app needing to stay connected.

3. Device clock synchronization: syncs the lamp's internal clock with the system time.

On the Dashboard page, each lamp has a status card showing: current on/off state with a toggle to switch between Off, Manual, and Automatic modes.

Removing a lamp from the aquarium permanently deletes its configuration. To move a lamp to another aquarium, the user must delete and re-add it.
```

### Plan Prompt

```
Use the existing AquaSync.Chihiros library which provides: IDeviceScanner for BLE discovery, IChihirosDevice for device control (connect, brightness, scheduling, on/off, auto mode), DeviceProfiles for model identification with per-model channel mappings, LightSchedule for schedule data, and Weekday enum for day selection.

The Chihiros library uses BLE via WinRT APIs and communicates through Nordic UART Service. Devices are identified by BLE address and name prefix matching. The library targets net10.0-windows10.0.19041.0.

Key integration points:
- IDeviceScanner.ScanByServiceAsync() for discovery with IProgress<DiscoveredDevice>
- IChihirosDevice.ConnectAsync() / DisconnectAsync() for connection management
- IChihirosDevice.SetBrightnessAsync(ColorChannel, byte) for manual control (0-100)
- IChihirosDevice.TurnOnAsync() / TurnOffAsync() for power control
- IChihirosDevice.EnableAutoModeAsync() to switch to schedule mode
- IChihirosDevice.AddScheduleAsync(LightSchedule) to program schedules onto device
- DeviceProfile.Channels gives the list of supported ColorChannels per model

Store lamp configuration (BLE address, name, model, schedule settings) in JSON via IDataService. The actual device control happens via BLE at runtime.

Events fire on background threads — use DispatcherQueue.TryEnqueue() to marshal UI updates.

For the visual schedule editor, create a custom WinUI3 control with a Canvas-based 24-hour timeline bar. Use pointer manipulation events for dragging sunrise/sunset handles.

Use the existing LampsPage and LampsViewModel placeholders. Register device-related services (lamp management service wrapping IDeviceScanner and IChihirosDevice) in DI.
```

---

## Feature 004: Eheim Filter Management

### Specify Prompt

```
The Filters page allows the user to manage Eheim Professionel 5E filters assigned to the currently selected aquarium profile. Multiple filters can be assigned to a single aquarium.

Adding a new filter: the user initiates a network scan to discover Eheim Digital hubs on the local network. Once a hub is found, the app connects and displays all filters on the hub's mesh network. The user selects a filter to assign to the current aquarium. One filter can only be assigned to one aquarium.

The Filters page shows a list of all filters assigned to the current aquarium. Each filter entry displays: model name and current status (running/stopped).

Selecting a filter opens its detail view with full mode configuration:

1. Manual Mode: the user sets a pump frequency directly using a slider. The available frequency range depends on the filter model (e.g., 35-80 Hz for Filter700).

2. Constant Flow Mode: the user selects from predefined flow rate options. The available flow rates depend on the filter model.

3. Pulse Mode: the user configures high pulse flow rate, low pulse flow rate, high pulse duration, and low pulse duration. The filter alternates between high and low flow.

4. Bio (Day/Night) Mode: the user sets a daytime flow rate, a nighttime flow rate, a day start time, and a night start time. The filter automatically switches between day and night flow.

On the Dashboard page, each filter has a status card showing: running/stopped state with an on/off toggle, and the ability to select between all four operating modes.

Removing a filter from the aquarium permanently deletes its configuration. To move a filter to another aquarium, the user must delete and re-add it.
```

### Plan Prompt

```
Use the existing AquaSync.Eheim library which provides: IEheimDiscoveryService for mDNS hub discovery, IEheimHub for WebSocket connection and device enumeration, IEheimFilter for filter control with all 4 modes.

The Eheim library uses WebSocket communication to an Eheim Digital hub (ws://{host}/ws). The hub manages a mesh network of filters. The library uses System.Reactive — all filter state properties are IObservable<T> backed by BehaviorSubject<T>.

Key integration points:
- EheimDiscoveryService.Scan() returns IObservable<DiscoveredHub>
- IEheimHub.ConnectAsync() / DisconnectAsync() for hub connection
- IEheimHub.DeviceDiscovered (IObservable<IEheimDevice>) for filter discovery
- IEheimFilter properties: IsActive, CurrentSpeed, FilterMode, ServiceHours, OperatingTime (all IObservable<T>)
- IEheimFilter.SetActiveAsync(bool) for on/off
- IEheimFilter.SetFilterModeAsync(FilterMode) to switch modes
- Mode-specific methods: SetManualSpeedAsync, SetConstantFlowAsync, SetDaySpeedAsync/SetNightSpeedAsync/SetDayStartTimeAsync/SetNightStartTimeAsync, SetHighPulseSpeedAsync/SetLowPulseSpeedAsync/SetHighPulseTimeAsync/SetLowPulseTimeAsync
- FlowRateTable provides model-specific flow rates and frequency ranges
- EheimFilterModel enum: Filter350, Filter450, Filter600T, Filter700

Store filter configuration (hub host, MAC address, model, mode settings) in JSON via IDataService.

Since properties are IObservable<T>, use System.Reactive.Linq.ObserveOn() with DispatcherQueue to marshal to UI thread when subscribing in ViewModels.

Use the existing FiltersPage and FiltersViewModel placeholders. Create a filter management service wrapping hub connection and filter device management, registered as singleton in DI.
```

---

## Feature 005: Other Equipment Management

### Specify Prompt

```
The Other Equipment page allows the user to track non-smart aquarium equipment assigned to the currently selected aquarium profile. These devices are not electronically controlled — the app only stores their information and tracks maintenance.

Multiple equipment items per category can be assigned to a single aquarium. One equipment item can only be assigned to one aquarium.

Supported equipment types: Heater, UV Sterilizer, Air Pump, CO2 System, Substrate Heater, Water Pump, Skimmer, UV Lamp, and Doser Pump.

When adding equipment, the user selects the type and fills in: brand, model, purchase date (optional), maintenance interval (in days), and notes (optional).

The Other Equipment page shows a list of all non-smart equipment assigned to the current aquarium, grouped by type. Each entry displays: type icon, brand, model, and days until next maintenance.

Selecting an equipment item opens its detail view where the user can edit all fields.

Removing equipment permanently deletes it. To move equipment to another aquarium, the user must delete and re-add it.

Non-smart equipment does not appear on the Dashboard. Maintenance tracking for these devices is handled on the Maintenance page.
```

### Plan Prompt

```
Use the existing OtherEquipmentPage and OtherEquipmentViewModel placeholders. Store equipment data in JSON via IDataService under "equipment/{aquarium-id}.json" as a list of equipment entries.

Create an Equipment model with: Id (GUID), Type (enum), Brand, Model, PurchaseDate (nullable DateOnly), MaintenanceIntervalDays (int), Notes, LastMaintenanceDate (nullable DateOnly). The equipment type enum should include all 9 types.

Use a ListView grouped by equipment type with GroupStyle headers. Use ContentDialog for add/edit forms. Use WinUI3 controls: ComboBox for type selection, TextBox for brand/model, CalendarDatePicker for dates, NumberBox for maintenance interval.

Calculate "days until next maintenance" from LastMaintenanceDate + MaintenanceIntervalDays. This calculation is also used by the Maintenance feature and Dashboard alerts.

Follow the existing MVVM pattern with manual SetProperty. Register any equipment-related services in DI in App.xaml.cs.
```

---

## Feature 006: Water Parameter Logging

### Specify Prompt

```
The Water Parameters page allows the user to log water test results for the currently selected aquarium profile. There are two ways to log parameters:

1. Test Session: the user starts a new testing session, selects which parameters they tested, records values for each, and at the end of the session adds optional notes summarizing the test (e.g., "tested after 30% water change"). The session is saved with a timestamp.

2. Quick Entry: the user records one or a few parameter values outside of a session. No notes can be attached to quick entries. Quick entries are saved with a timestamp.

The available parameters depend on the aquarium type:
- All types: Temperature, pH, Ammonia (NH3/NH4), Nitrite (NO2), Nitrate (NO3), KH, Phosphate (PO4), O2, Cu
- Freshwater and Brackish: GH, CO2
- Freshwater only: Iron (Fe)
- Saltwater and Brackish: Salinity
- Saltwater only: Calcium (Ca), Magnesium (Mg), Alkalinity, SiO2

Each recorded value is shown with a color-coded indicator based on global ideal ranges for the aquarium type: green (ideal), yellow (caution), or red (danger). These ranges are predefined and cannot be changed by the user.

The page also shows a history list of all test entries (both sessions and quick entries) in reverse chronological order. Each entry shows the date, parameters tested, values with color indicators, and any notes.

Separate line charts per parameter show all-time trends.
```

### Plan Prompt

```
Use the existing WaterParametersPage and WaterParametersViewModel placeholders. Store water test data in JSON via IDataService under "water-parameters/{aquarium-id}.json" as a chronological list of test entries.

Create models: WaterTestEntry (Id, Timestamp, IsSession, Notes, list of ParameterReadings), ParameterReading (ParameterType enum, Value double, Unit string). Create a WaterParameterType enum with all parameters.

Define global ideal ranges as a static class/dictionary keyed by AquariumType and WaterParameterType, returning min/max for green, yellow, and red zones. This is read-only configuration, not stored in user data.

For the parameter selection by aquarium type, create a mapping of AquariumType → available WaterParameterTypes.

For the test session flow, use a multi-step UI: step 1 shows checkboxes for which parameters to test, step 2 shows input fields for selected parameters, step 3 shows a notes field. Use NumberBox for value entry.

For charts, use a charting library compatible with WinUI3. Evaluate WinUI3 Community Toolkit charting controls, LiveCharts2, or ScottPlot. Create separate line charts per parameter showing all data points over time.

For the history list, use a ListView with DataTemplateSelector to differentiate session entries (with notes) from quick entries.

Color-coded indicators can be implemented with a value converter that maps value + parameter type + aquarium type to a SolidColorBrush (green/yellow/red).
```

---

## Feature 007: Dashboard

### Specify Prompt

```
The Dashboard is the landing page after the user selects an aquarium from the selector. It provides an at-a-glance overview of the aquarium's current state.

The Dashboard contains the following sections:

1. Aquarium details card: displays the aquarium name, thumbnail photo, volume, type, substrate(s), and setup date.

2. Lamp status cards: one card per assigned Chihiros lamp showing current on/off state. The user can toggle between three modes directly from the dashboard: Off, Manual, and Automatic. When in Manual mode, the current brightness per channel is shown. When in Automatic mode, the active schedule info is shown.

3. Filter status cards: one card per assigned Eheim filter showing running/stopped state with an on/off toggle. The user can select between all four operating modes (Manual, Constant Flow, Pulse, Bio) directly from the dashboard.

4. Latest water test results: displays the most recent values for each tested parameter with color-coded health indicators (green/yellow/red).

5. Water parameter charts: separate line charts per parameter showing all-time trends.

6. Maintenance alerts: shows upcoming maintenance reminders starting 7 days before the due date, and overdue maintenance warnings. Alerts cover all equipment types (smart and non-smart).

Non-smart equipment does not have status cards on the dashboard.
```

### Plan Prompt

```
Use the existing DashboardPage and DashboardViewModel placeholders. The Dashboard aggregates data from multiple services — it does not own data but reads from equipment, water parameter, and maintenance services.

The DashboardViewModel should inject services for: aquarium profile data, lamp management, filter management, equipment tracking, water parameter data, and maintenance scheduling.

For lamp control on the dashboard, connect to Chihiros devices via BLE and expose toggle controls. Use the IChihirosDevice interface: TurnOffAsync() for Off, TurnOnAsync() + SetBrightnessAsync() for Manual, EnableAutoModeAsync() for Automatic. Display channel brightness using the device's Profile.Channels.

For filter control, connect to the Eheim hub and expose mode selection. Use IEheimFilter: SetActiveAsync() for on/off, SetFilterModeAsync() for mode changes. Subscribe to IObservable properties for live status updates.

Use an ItemsRepeater or GridView to dynamically render equipment status cards. Each card type (lamp, filter) uses a different DataTemplate.

For water parameter charts, reuse the same charting components from the Water Parameters feature.

For maintenance alerts, query all equipment (smart and non-smart) and calculate days until next maintenance. Filter for items due within 7 days or overdue. Display as InfoBar controls (warning severity for upcoming, error severity for overdue).

Layout: use a ScrollViewer containing a responsive grid layout with cards. Follow Windows 11 card-based design patterns.
```

---

## Feature 008: Maintenance Tracking

### Specify Prompt

```
The Maintenance page allows the user to track maintenance schedules and log maintenance activities for all equipment assigned to the currently selected aquarium.

The page shows a list view of all equipment grouped by device, with each entry showing: equipment name/model, last maintenance date, next maintenance due date, and a countdown (e.g., "in 5 days" or "3 days overdue").

For each equipment item, the user can log a maintenance session individually. A maintenance log entry includes: date (defaults to today), a description of what was done (free text), and optional notes. Multiple maintenance entries can be logged per equipment over time.

The maintenance history for each equipment item is viewable as a chronological list.

Maintenance reminders appear in the Dashboard alerts section starting 7 days before the due date. Overdue maintenance is highlighted.

For Eheim Professionel 5E filters specifically: when the user logs a maintenance session, the app automatically resets the service hour counter on the physical device.

Smart equipment (Chihiros lamps, Eheim filters) and non-smart equipment (heaters, pumps, etc.) all appear on this page with unified maintenance tracking.
```

### Plan Prompt

```
Use the existing MaintenancePage and MaintenanceViewModel placeholders. Store maintenance logs in JSON via IDataService under "maintenance/{aquarium-id}.json" as a list of maintenance entries per equipment.

Create a MaintenanceEntry model: Id (GUID), EquipmentId (GUID), Date (DateOnly), Description (string), Notes (string nullable).

The maintenance page needs to aggregate equipment from three sources: lamp configurations, filter configurations, and other equipment entries. Create an IMaintenanceService that provides a unified list of all equipment with their maintenance status.

For next-due calculation: use LastMaintenanceDate (from the most recent maintenance log entry for that equipment) + MaintenanceIntervalDays. For Eheim filters, the maintenance interval can be derived from ServiceHours thresholds. For Chihiros lamps and non-smart equipment, use user-configured intervals.

For the Eheim service hour reset: when a maintenance entry is logged for an Eheim filter, connect to the hub and call the appropriate reset method on IEheimFilter. Note: check if ServiceHours can be reset via the existing protocol — if not, track the reset offset locally.

Use a ListView grouped by equipment with expandable sections showing maintenance history. Use ContentDialog for logging new maintenance entries with DatePicker, TextBox for description, and TextBox for notes.
```

---

## Feature 009: Photo Gallery

### Specify Prompt

```
The Gallery page allows the user to manage photos for the currently selected aquarium profile. It serves as a visual journal to track the aquarium's progression over time.

Photos are displayed in a simple chronological grid, with the most recent photos first.

The user can upload new photos at any time. Each photo can have an optional caption/note (e.g., "Day 1 - just planted", "3 months in - carpet growing nicely").

From the gallery, the user can select any photo to set it as the aquarium's thumbnail image that appears on the aquarium selector grid and the dashboard.

Photos are stored at their original resolution with no compression or resizing. There is no limit on the number of photos per aquarium.

The user can delete photos from the gallery. If a deleted photo was the current thumbnail, the aquarium reverts to the default graphic.
```

### Plan Prompt

```
Use the existing GalleryPage and GalleryViewModel placeholders. Store photo files in %LOCALAPPDATA%/AquaSync/gallery/{aquarium-id}/. Store photo metadata (filename, caption, date, isThumbnail flag) in JSON via IDataService under "gallery/{aquarium-id}.json".

Use a GridView with adaptive layout (ItemsWrapGrid) for the photo grid. Use BitmapImage for loading photos — consider using DecodePixelWidth for thumbnails in the grid to reduce memory usage while keeping originals at full resolution.

For photo upload, use FileOpenPicker with image file type filters (.jpg, .jpeg, .png, .bmp, .webp). Copy selected files to the gallery directory with a generated unique filename.

For setting a photo as thumbnail, update the aquarium profile JSON with the thumbnail file path. The aquarium selector grid and dashboard should reference this path.

For caption editing, use an inline TextBox or a flyout editor on the photo item.

For photo deletion, use a ContentDialog confirmation. Delete the file from disk and remove the metadata entry.

Use WinUI3 Image control within the GridView item template. Consider lazy loading for large galleries.
```

---

## Feature 010: Fertilizer Management

### Specify Prompt

```
The Fertilizers page allows the user to manage fertilizer dosing plans for the currently selected aquarium profile.

The user registers fertilizers with the following information: brand, product name, dosage amount (with unit, e.g., "2 ml" or "1 pump"), frequency (daily, every other day, weekly, bi-weekly, or custom days), and optional notes.

Multiple fertilizers can be active simultaneously for one aquarium (e.g., a macro fertilizer, a micro fertilizer, and an iron supplement, each with their own dosage and schedule).

A registered fertilizer and dosage combination remains "active" until the user registers a new entry for that product, effectively replacing the previous dosing plan. Historical entries are preserved and viewable — the user can see what dosing plans were used in the past and when they changed.

The Fertilizers page shows the currently active fertilizers at the top, and a history of past dosing plans below.

The fertilizer feature only stores dosing plans — it does not log individual dosing events or send reminders.
```

### Plan Prompt

```
Use the existing FertilizersPage and FertilizersViewModel placeholders. Store fertilizer data in JSON via IDataService under "fertilizers/{aquarium-id}.json".

Create a FertilizerPlan model: Id (GUID), Brand, ProductName, Dosage (string, free-form to accommodate "2 ml", "1 pump", etc.), Frequency (enum: Daily, EveryOtherDay, Weekly, BiWeekly, Custom), CustomFrequencyDays (nullable int, for custom frequency), Notes, StartDate (DateOnly), EndDate (nullable DateOnly, null means currently active).

When a new plan is registered for the same product (matched by Brand + ProductName), set the EndDate on the previous entry to today, then create the new entry with a null EndDate. This preserves history while marking the new plan as active.

Active plans: filter where EndDate is null. History: filter where EndDate is not null, ordered by EndDate descending.

Use a ListView with two sections (active and history). Use ContentDialog for adding new fertilizer plans. Use ComboBox for frequency selection with a conditional NumberBox for custom day interval.
```

---

## Feature 011: Plant Inventory

### Specify Prompt

```
The Plants page allows the user to track the plant inventory for the currently selected aquarium profile. Plants can be added and removed throughout the aquarium's lifetime.

When adding a plant, the user enters: species or common name, quantity, date added (defaults to today), and optional notes.

The Plants page shows all current plants in a list with their name, quantity, and date added.

Plants can be marked as "removed" or "dead" — this does not delete the record but moves it to a history section. The user can see which plants were in the aquarium in the past. Removed/dead plants show their removal date.

The user can edit plant details (update quantity, add notes) while a plant is active.

No photos are attached to individual plant entries. No placement information (foreground/midground/background) is tracked.
```

### Plan Prompt

```
Use the existing PlantsPage and PlantsViewModel placeholders. Store plant data in JSON via IDataService under "plants/{aquarium-id}.json".

Create a Plant model: Id (GUID), Name (string), Quantity (int), DateAdded (DateOnly), Notes (string nullable), Status (enum: Active, Removed, Dead), DateRemoved (nullable DateOnly).

When marking a plant as removed/dead, set the Status and DateRemoved fields. Keep the record in the same data file.

Display with a ListView split into two sections: "Current Plants" (Status == Active) and "History" (Status == Removed or Dead). History section shows the removal date and reason.

Use ContentDialog for adding and editing plants. Use a MenuFlyout or context menu on plant items for "Mark as Removed" and "Mark as Dead" actions with a DatePicker for removal date.

Follow the existing MVVM and service patterns. Create a plant service if needed, or manage data directly in the ViewModel using IDataService.
```

---

## Execution Order

Run these features in order, as later features depend on earlier ones:

| Order | Feature | Dependency |
|-------|---------|------------|
| 1 | 001 - Aquarium Profiles & Selector | None (foundation) |
| 2 | 002 - Settings | 001 (default UOM for profiles) |
| 3 | 003 - Chihiros Lamp Management | 001 (needs aquarium to assign to) |
| 4 | 004 - Eheim Filter Management | 001 (needs aquarium to assign to) |
| 5 | 005 - Other Equipment | 001 (needs aquarium to assign to) |
| 6 | 006 - Water Parameter Logging | 001 (needs aquarium type for param filtering) |
| 7 | 007 - Dashboard | 001, 003, 004, 005, 006, 008 (aggregates all) |
| 8 | 008 - Maintenance Tracking | 003, 004, 005 (needs all equipment types) |
| 9 | 009 - Photo Gallery | 001 (needs aquarium profile for thumbnail) |
| 10 | 010 - Fertilizer Management | 001 (scoped to aquarium) |
| 11 | 011 - Plant Inventory | 001 (scoped to aquarium) |

> **Note:** Feature 007 (Dashboard) depends on 008 (Maintenance) for alerts. Consider implementing 008 before 007, or implementing Dashboard alerts as a follow-up pass after 008 is complete.
