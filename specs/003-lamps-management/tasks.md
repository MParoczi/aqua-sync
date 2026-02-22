# Tasks: Lamps Management

**Input**: Design documents from `/specs/003-lamps-management/`
**Prerequisites**: plan.md ✓, spec.md ✓, data-model.md ✓, contracts/ ✓, research.md ✓, quickstart.md ✓
**Tests**: No test tasks — no test infrastructure in `AquaSync.App` (out of scope per plan.md)
**Organization**: Tasks grouped by user story for independent implementation and testing of each increment.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (US1–US7)
- Exact file paths included in all descriptions

---

## Phase 1: Setup

**Purpose**: Confirm the baseline compiles before any new files are added (1 task).

- [x] T001 Verify clean baseline: run `dotnet build AquaSync.App/AquaSync.App.csproj -r win-x64` and confirm zero errors and zero warnings before adding any new files

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Data models, service contract, service implementation, and core DI registration that ALL user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T002 [P] Create `LampMode` enum in `AquaSync.App/Models/LampMode.cs`: file-scoped namespace `AquaSync.App.Models`; apply `[JsonConverter(typeof(JsonStringEnumConverter))]`; `public enum LampMode { Off, Manual, Automatic }`

- [x] T003 [P] Create `ScheduleConfiguration` model in `AquaSync.App/Models/ScheduleConfiguration.cs`: file-scoped namespace `AquaSync.App.Models`; `public sealed class ScheduleConfiguration`; properties: `TimeOnly Sunrise`, `TimeOnly Sunset`, `int RampUpMinutes`, `Dictionary<string, byte> ChannelBrightness` (init `[]`), `Weekday ActiveDays` (from `AquaSync.Chihiros`)

- [x] T004 Create `LampConfiguration` model in `AquaSync.App/Models/LampConfiguration.cs`: file-scoped namespace; `public sealed class LampConfiguration`; properties: `Guid Id`, `Guid AquariumId`, `string BluetoothAddress` (12-char uppercase hex, e.g. `"A1B2C3D4E5F6"`), `string DeviceName`, `string ModelName` (empty string = unmanaged lamp), `LampMode Mode` (default `LampMode.Off`), `Dictionary<string, byte> ManualBrightness` (init `[]`), `ScheduleConfiguration? Schedule`, `DateTimeOffset CreatedAt`

- [x] T005 Create `ILampService` interface in `AquaSync.App/Contracts/Services/ILampService.cs`: file-scoped namespace `AquaSync.App.Contracts.Services`; all 11 members per `specs/003-lamps-management/contracts/ILampService.md`: `GetLampsForAquariumAsync`, `IsAddressAssignedAsync`, `AddLampAsync`, `RemoveLampAsync`, `SaveModeAsync`, `SaveManualBrightnessAsync`, `SaveScheduleAsync`, `ScanAsync`, `GetProfileForModel`, `ConnectAsync`, `DisconnectAsync`; add required `using` directives for `AquaSync.Chihiros` types (`DiscoveredDevice`, `IChihirosDevice`, `DeviceProfile`)

- [x] T006 Implement `LampService` in `AquaSync.App/Services/LampService.cs`: `public sealed class LampService : ILampService`; file-scoped namespace; constructor injects `IDataService`, `IDeviceScanner`; internal `Dictionary<string, IChihirosDevice> _connections` keyed by `BluetoothAddress`; implement all 11 members: `GetLampsForAquariumAsync` → `IDataService.ReadAllAsync<LampConfiguration>("lamps")`, filter by `AquariumId`, order by `CreatedAt` ascending; `IsAddressAssignedAsync` → scan all lamp JSON files for matching address; `AddLampAsync` → call `IsAddressAssignedAsync`, throw `InvalidOperationException` if true, construct `new LampConfiguration { Id = Guid.NewGuid(), AquariumId = aquariumId, BluetoothAddress = device.BluetoothAddress.ToString("X12"), DeviceName = device.Name, ModelName = device.MatchedProfile?.ModelName ?? string.Empty, Mode = LampMode.Off, CreatedAt = DateTimeOffset.UtcNow }`, save via `IDataService.SaveAsync`; `RemoveLampAsync` → look up lamp, call `DisconnectAsync`, call `IDataService.DeleteAsync`; `SaveModeAsync` / `SaveManualBrightnessAsync` → load + mutate + save; `SaveScheduleAsync` → validate (sunrise < sunset, gap ≥ ramp+1 min, ramp in [0,150], `ActiveDays != Weekday.None`), throw `ArgumentException` with specific message per FR-024 on each violation, then save; `GetProfileForModel` → `DeviceProfiles.MatchFromName(modelName)` (check exact method in `AquaSync.Chihiros/README.md`); `ConnectAsync` → call `GetProfileForModel`, return null if null (unmanaged), build `new ChihirosDevice(Convert.ToUInt64(address, 16), name, profile)`, call `device.ConnectAsync(ct)`, cache under address, subscribe `device.Disconnected += (_, _) => _connections.Remove(address)`; `DisconnectAsync` → evict from cache, call `device.DisconnectAsync()` if found; `ScanAsync` → `_scanner.ScanAsync(TimeSpan.FromMinutes(10), progress, cancellationToken)`

- [x] T007 Register `IDeviceScanner` and `ILampService` singletons in `AquaSync.App/App.xaml.cs`: add `services.AddSingleton<IDeviceScanner, DeviceScanner>()` and `services.AddSingleton<ILampService, LampService>()` to the `ConfigureServices` method; add required `using` directives for Chihiros types

**Checkpoint**: Run `dotnet build AquaSync.App/AquaSync.App.csproj -r win-x64` — new types must compile cleanly before proceeding to any user story phase.

---

## Phase 3: User Story 1 — Discover and Add a Lamp (Priority: P1) 🎯 MVP

**Goal**: Users can initiate a BLE scan from the Lamps page, see Chihiros devices streaming in real time with model name and signal strength, select an available device, and add it to the current aquarium where it appears in the list.

**Independent Test**: Navigate to Lamps page → tap Add → scan dialog opens → discovered devices appear in real time → select one → confirm → lamp appears in list.

- [x] T008 [US1] Update `LampsViewModel` in `AquaSync.App/ViewModels/LampsViewModel.cs`: inject `ILampService`, `IAquariumContext`, `INavigationService` via constructor; add `ObservableCollection<LampConfiguration> Lamps` (init `[]`), `bool IsBusy` (SetProperty), computed `bool IsEmpty` (`Lamps.Count == 0`, raise in `Lamps` collection-changed handler); implement `INavigationAware.OnNavigatedTo` → call `LoadLampsAsync(aquariumContext.CurrentAquarium.Id)`; `LoadLampsAsync` sets `IsBusy = true`, calls `ILampService.GetLampsForAquariumAsync`, clears and repopulates `Lamps`, sets `IsBusy = false`; add `RelayCommand OpenAddLampDialogCommand`: constructs `AddLampDialog(lampService, aquariumId)`, calls `dialog.ShowAsync()`, on `ContentDialogResult.Primary` calls `ILampService.AddLampAsync(aquariumId, dialog.SelectedDevice!)` then `LoadLampsAsync`

- [x] T009 [P] [US1] Create `AddLampDialog.xaml` in `AquaSync.App/Views/AddLampDialog.xaml`: `ContentDialog` with `Title="Add Lamp"`, `PrimaryButtonText="Add"` (initially disabled via `IsPrimaryButtonEnabled=False`), `CloseButtonText="Cancel"`; fix dialog width via `<ContentDialog.Resources><x:Double x:Key="ContentDialogMinWidth">500</x:Double><x:Double x:Key="ContentDialogMaxWidth">500</x:Double></ContentDialog.Resources>`; content `StackPanel`: (1) `ProgressRing x:Name="ScanningRing"`, (2) `TextBlock x:Name="ScanningText"` "Scanning for nearby Chihiros devices…", (3) `ListView x:Name="DeviceList"` with `DataTemplate`: outer `Grid` (2 cols) — Col0 `StackPanel` (TextBlock `Name` Bold, TextBlock `ModelName` Subtle; ModelName shows "Unknown Model" when empty), Col1 FontIcon signal-bars placeholder; each entry `Opacity="0.5"` and `IsEnabled="False"` when unavailable; (4) `TextBlock x:Name="AlreadyAssignedNote"` "Already assigned to another aquarium" below list, initially `Visibility=Collapsed`; (5) `InfoBar x:Name="BluetoothErrorBar"` for FR-033 Bluetooth unavailable error

- [x] T010 [US1] Implement `AddLampDialog.xaml.cs` in `AquaSync.App/Views/AddLampDialog.xaml.cs`: constructor accepts `ILampService lampService, Guid currentAquariumId`; `DiscoveredDevice? SelectedDevice { get; private set; }` property; inner class `DiscoveredDeviceItem { DiscoveredDevice Device; bool IsAvailable; }`; `ObservableCollection<DiscoveredDeviceItem> Devices`; on `Opened` event: attempt to detect Bluetooth availability (catch `UnauthorizedAccessException` or `BluetoothAdapter` state per WinRT APIs) — on unavailable: hide scan UI, open `BluetoothErrorBar` with message directing user to system settings (FR-033), return without starting scan; create `CancellationTokenSource _cts`; create `IProgress<DiscoveredDevice>` via `new Progress<DiscoveredDevice>(OnDeviceDiscovered)` on UI thread (captures `SynchronizationContext.Current`); `OnDeviceDiscovered` callback: FR-040 — if `Devices` already has item with same `BluetoothAddress`, update its signal properties; else check `await lampService.IsAddressAssignedAsync(address)`, determine `IsAvailable = !isAssigned || isCurrentAquarium` (FR-034: if assigned to current aquarium, `IsAvailable=false` with "already in this aquarium" tooltip), add to `Devices`; `DeviceList.SelectionChanged` → update `SelectedDevice`, set `IsPrimaryButtonEnabled = SelectedDevice?.IsAvailable == true`; on `PrimaryButtonClick` override: cancel `_cts`; on `CloseButtonClick` or dialog close: cancel `_cts`, dispose

- [x] T011 [US1] Implement `LampsPage.xaml` in `AquaSync.App/Views/LampsPage.xaml` (replacing existing placeholder): root `Grid` (3 rows: Auto, *, Auto); **Row 0** header `Grid` (2 cols, VerticalAlignment=Center, Margin bottom): Col0 `TextBlock "Lamps"` TitleTextBlockStyle; Col1 `Button` right-aligned, Content = `FontIcon Glyph="&#xE710;"` (Add symbol), `Command="{x:Bind ViewModel.OpenAddLampDialogCommand}"`; **Row 1** content: `ListView x:Name="LampsList"` `ItemsSource="{x:Bind ViewModel.Lamps, Mode=OneWay}"` placeholder `DataTemplate` (single `TextBlock` bound to `ModelName` — full template added in Phase 4 T012); empty-state `StackPanel` (TextBlock "No lamps assigned. Tap + to add one.", Visibility bound to `IsEmpty`); **Row 2**: `InfoBar x:Name="ErrorBar"` Severity=Error (for future error states)

**Checkpoint**: Run app → navigate to Lamps → tap Add → scan dialog opens and real-time devices stream in → confirm one → lamp appears in list (ModelName visible).

---

## Phase 4: User Story 2 — View Lamps List and Status (Priority: P2)

**Goal**: Each lamp entry shows model name, device name, operational mode, and connection state (ordered oldest-first). Tapping a lamp navigates to its detail view.

**Independent Test**: With lamps pre-added, open Lamps page — verify each entry shows model, device name, status text, entries ordered by `CreatedAt` ascending, and tapping one navigates away.

- [x] T012 [US2] Complete `LampsPage.xaml` lamp entry `DataTemplate` in `AquaSync.App/Views/LampsPage.xaml`: replace placeholder DataTemplate with full template — outer `Grid` (2 cols, VerticalAlignment=Center, Padding=12 8): Col0 `StackPanel` (TextBlock `ModelName` SemiLightWeight, TextBlock `DeviceName` Subtle `TextTrimming=CharacterEllipsis` per FR-041); Col1 `StackPanel` RightAligned (TextBlock `Mode` text, TextBlock connection state "Connected"/"Disconnected" Subtle); wire `ListView.IsItemClickEnabled=True` and `ItemClick` event to `ViewModel.SelectLampCommand` passing the clicked `LampConfiguration` as parameter

- [x] T013 [US2] Add `SelectLampCommand` to `LampsViewModel` in `AquaSync.App/ViewModels/LampsViewModel.cs`: `RelayCommand<LampConfiguration> SelectLampCommand` that calls `_navigationService.NavigateTo(typeof(LampDetailViewModel).FullName!, lamp.Id)` passing the lamp `Guid` as the navigation parameter

**Checkpoint**: Lamp entries show full status → tapping navigates (to a blank shell page initially, completed in Phase 5).

---

## Phase 5: User Story 3 — Manually Control Lamp Brightness (Priority: P3)

**Goal**: Opening a lamp's detail view connects to the device and shows per-channel manual brightness sliders matching the device profile. Adjusting a slider persists the value locally and sends it to the device in real time. Disconnection disables all controls.

**Independent Test**: Select a lamp → detail view opens → sliders shown per device profile channels → moving a slider updates the physical device → unplugging device disables all controls and shows InfoBar.

- [x] T014 [P] [US3] Register `LampDetailViewModel` and `LampDetailPage` for DI and navigation in `AquaSync.App/App.xaml.cs` and `AquaSync.App/Services/PageService.cs`: add `services.AddTransient<LampDetailViewModel>()` and `services.AddTransient<LampDetailPage>()` to `App.xaml.cs ConfigureServices`; add `Configure<LampDetailViewModel, LampDetailPage>()` to `PageService` constructor alongside existing route configurations

- [x] T015 [US3] Create `LampDetailViewModel` in `AquaSync.App/ViewModels/LampDetailViewModel.cs`: `public sealed class LampDetailViewModel : ViewModelBase, INavigationAware`; inject `ILampService`, `DispatcherQueue` via constructor (capture `DispatcherQueue.GetForCurrentThread()` at construction); inner `public sealed class ChannelSlider` with `ColorChannel Channel`, `string Label`, `byte Value` (SetProperty); observable properties: `LampConfiguration? Lamp`, `IChihirosDevice? Device`, `bool IsConnected`, `bool IsConnecting`, `bool IsUnmanaged` (returns `string.IsNullOrEmpty(Lamp?.ModelName)`), `ObservableCollection<ChannelSlider> Channels` (init `[]`), `bool IsBrightnessApplying`, `string? ErrorMessage`, `bool IsErrorOpen`; `RelayCommand<ChannelSlider> ApplyBrightnessCommand`: update `slider.Value` in `Lamp!.ManualBrightness[channel.Channel.ToString()]` and call `ILampService.SaveManualBrightnessAsync(lampId, Lamp.ManualBrightness)` unconditionally; attempt `await Device!.SetBrightnessAsync(channel.Channel, slider.Value, ct)`, on `ChihirosException` set `ErrorMessage` and `IsErrorOpen = true` (local value already saved — not reverted); `INavigationAware.OnNavigatedTo(object parameter)`: parse `Guid lampId = (Guid)parameter`; call `ILampService.GetLampsForAquariumAsync` filtered by id (or add single-lamp getter if available) to load `Lamp`; set `IsConnecting = true`; call `ILampService.ConnectAsync(Lamp, ct)` → store as `Device`; if non-null populate `Channels` from `DeviceProfile.Channels` (via `ILampService.GetProfileForModel(Lamp.ModelName)`); set `IsConnected = Device != null`, `IsConnecting = false`; subscribe `Device!.Disconnected += OnDeviceDisconnected`; `OnDeviceDisconnected`: `_dispatcherQueue.TryEnqueue(() => { IsConnected = false; IsErrorOpen = true; ErrorMessage = "Device is unreachable."; })`; `INavigationAware.OnNavigatedFrom()`: unsubscribe `Disconnected`, call `ILampService.DisconnectAsync(Lamp!.BluetoothAddress)`, clear `Device`

- [x] T016 [P] [US3] Create `LampDetailPage.xaml.cs` in `AquaSync.App/Views/LampDetailPage.xaml.cs`: minimal code-behind; constructor receives `LampDetailViewModel viewModel` via DI and assigns `DataContext = viewModel`; expose `ViewModel` property for `x:Bind`

- [x] T017 [US3] Create `LampDetailPage.xaml` in `AquaSync.App/Views/LampDetailPage.xaml`: outer `ScrollViewer` > `StackPanel` (Spacing=24, Padding=24); **Header** `Grid` (2 cols, Margin=0 0 0 8): Col0 Back `Button` (Command wired to `INavigationService.GoBack()` via ViewModel or code-behind, FontIcon ChevronLeft `&#xE76B;`), Col1 `StackPanel` (TextBlock `Lamp.ModelName` TitleTextBlockStyle, TextBlock `Lamp.DeviceName` Subtle `TextTrimming=CharacterEllipsis`, TextBlock connection state "Connected"/"Disconnected" bound to `IsConnected`); **Connecting overlay** `Grid` (Visibility bound to `IsConnecting`): centered `ProgressRing IsActive=True`; **Unmanaged InfoBar**: `InfoBar IsOpen="{x:Bind ViewModel.IsUnmanaged, Mode=OneWay}" Severity=Warning Title="Unsupported Model" Message="This lamp model is not yet supported. Controls are unavailable."` + TextBlock `Lamp.DeviceName` in Message (or combined); **Disconnect InfoBar**: `InfoBar Severity=Warning` `IsOpen` bound to computed `!IsConnected && !IsConnecting`; **"Manual Brightness" TextBlock** section heading (Visibility bound to `!IsUnmanaged`); `ItemsControl ItemsSource="{x:Bind ViewModel.Channels, Mode=OneWay}"` DataTemplate: `Grid` (3 cols — TextBlock `Label` width=Auto, `Slider` Minimum=0 Maximum=100 `Value="{x:Bind Value, Mode=TwoWay}"` width=*, TextBlock `Value%` width=Auto); Slider `PointerCaptureLost` event fires `ViewModel.ApplyBrightnessCommand.Execute(slider)`; `IsEnabled` on all interactive elements bound to `IsConnected && !IsConnecting`; **Error InfoBar**: `IsOpen="{x:Bind ViewModel.IsErrorOpen, Mode=TwoWay}" Severity=Error Message="{x:Bind ViewModel.ErrorMessage, Mode=OneWay}"`

**Checkpoint**: Tap a lamp → connecting animation shows → sliders appear matching the device's channels → adjust slider → physical device responds → disconnect device → controls disabled, InfoBar appears.

---

## Phase 6: User Story 4 — Configure an Automated Daily Schedule (Priority: P4)

**Goal**: Schedule editor with Canvas timeline (off/ramp/on zones), time-picker inputs for sunrise/sunset, ramp-up NumberBox, weekday checkboxes, and per-channel peak brightness sliders. Saving validates all fields and programs the schedule onto the device so it runs autonomously.

**Independent Test**: Open schedule editor for a connected lamp → set sunrise/sunset via time pickers → handles appear on timeline → configure ramp-up/days/brightness → Save → device runs schedule when app is closed.

- [x] T018 [P] [US4] Create `ScheduleEditorControl.xaml` in `AquaSync.App/Controls/ScheduleEditorControl.xaml`: `UserControl` with `x:Class="AquaSync.App.Controls.ScheduleEditorControl"`; root `Grid` (2 rows — Row 0 `Canvas x:Name="TimelineCanvas" Height="48" SizeChanged="OnSizeChanged"`, Row 1 time-label row); inside Canvas: `Rectangle x:Name="OffLeft"` (dark muted fill), `Rectangle x:Name="RampSegment"` (LinearGradientBrush horizontal from off-color to accent-color), `Rectangle x:Name="OnSegment"` (accent fill), `Rectangle x:Name="OffRight"` (dark muted fill), `Ellipse x:Name="SunriseHandle" Width="16" Height="16"` (accent fill, `PointerPressed`, `PointerMoved`, `PointerReleased` handlers), `Ellipse x:Name="SunsetHandle" Width="16" Height="16"` (same handlers); Row 1 `Grid` (2 cols): `TextBlock x:Name="SunriseLabel" HorizontalAlignment=Left`, `TextBlock x:Name="SunsetLabel" HorizontalAlignment=Right`; blank state (SunriseTime and SunsetTime null): OffLeft fills full width, handles `Visibility=Collapsed`

- [x] T019 [US4] Implement `ScheduleEditorControl.xaml.cs` in `AquaSync.App/Controls/ScheduleEditorControl.xaml.cs`: register three `DependencyProperty` fields using `DependencyProperty.Register`: `SunriseTimeProperty` (`TimeOnly?`, default `null`, `PropertyChangedCallback` → `RedrawTimeline`), `SunsetTimeProperty` (`TimeOnly?`, default `null`, callback → `RedrawTimeline`), `RampUpMinutesProperty` (`int`, default `0`, callback → `RedrawTimeline`); CLR wrapper properties `SunriseTime`, `SunsetTime`, `RampUpMinutes`; `Ellipse? _draggingHandle`; `OnPointerPressed`: hit-test whether pointer is on SunriseHandle or SunsetHandle, set `_draggingHandle`, call `CapturePointer(e.Pointer)`; `OnPointerMoved`: if `_draggingHandle != null`, compute `double fraction = e.GetCurrentPoint(TimelineCanvas).Position.X / TimelineCanvas.ActualWidth`, clamp [0,1], `int totalMinutes = (int)(fraction * 1440)`, `TimeOnly t = new TimeOnly(totalMinutes / 60, totalMinutes % 60)`, set `SunriseTime` or `SunsetTime` on DP; call `RedrawTimeline()`; `OnPointerReleased`: release pointer capture, set `_draggingHandle = null`; `RedrawTimeline()`: early-return if `TimelineCanvas.ActualWidth == 0`; if `SunriseTime == null && SunsetTime == null`: OffLeft fills full width (Canvas.SetLeft=0, Width=ActualWidth), all other segments Width=0, handles Collapsed; else: compute pixel X for sunrise (`.TotalMinutes / 1440.0 * ActualWidth`), ramp end, sunset; set OffLeft width=sunriseX, RampSegment left=sunriseX width=rampWidth (hidden when RampUpMinutes==0), OnSegment left=rampEndX width=onWidth, OffRight left=sunsetX width=remainderWidth; position handles (Canvas.SetLeft, Canvas.SetTop for vertical centering); update label text; `OnSizeChanged` calls `RedrawTimeline()`

- [x] T020 [P] [US4] Create `WeekdayFlagConverter` in `AquaSync.App/Converters/WeekdayFlagConverter.cs` (create `Converters/` folder): `public sealed class WeekdayFlagConverter : IValueConverter`; `Convert`: `Weekday current = (Weekday)value; Weekday flag = Enum.Parse<Weekday>((string)parameter); return current.HasFlag(flag);`; `ConvertBack`: `Weekday current = (Weekday)value; Weekday flag = Enum.Parse<Weekday>((string)parameter); bool isChecked = (bool)targetType; return current.HasFlag(flag) ? current & ~flag : current | flag;` — Note: since WinUI3 `ConvertBack` doesn't pass the current value, bind with `Mode=TwoWay` using a `ViewModel` intermediary property or use `ConverterParameter` to pass the flag; the ViewModel's `ScheduleActiveDays` property setter handles the toggle via bitwise operations

- [x] T021 [US4] Add schedule state and `SaveScheduleCommand` to `LampDetailViewModel` in `AquaSync.App/ViewModels/LampDetailViewModel.cs`: add properties `TimeOnly? ScheduleSunrise`, `TimeOnly? ScheduleSunset`, `int ScheduleRampUpMinutes` (0–150), `Weekday ScheduleActiveDays`, `Dictionary<string, byte> ScheduleChannelBrightness` (init `[]`), `bool IsSavingSchedule`, `string? ValidationMessage`; computed `bool IsScheduleValid`: both times set AND `ScheduleSunrise < ScheduleSunset` AND `(ScheduleSunset.ToTimeSpan() - ScheduleSunrise.ToTimeSpan()).TotalMinutes >= ScheduleRampUpMinutes + 1` AND `ScheduleRampUpMinutes` in [0,150] AND `ScheduleActiveDays != Weekday.None`; in `OnNavigatedTo` if `Lamp.Schedule != null` populate schedule fields from it; `RelayCommand SaveScheduleCommand`: if not `IsScheduleValid` set `ValidationMessage` with per-issue text and return (FR-024 distinct messages: "Sunrise must be earlier than sunset", "At least one day must be selected", etc.); set `IsSavingSchedule=true`; build `ScheduleConfiguration { Sunrise = ScheduleSunrise.Value, Sunset = ScheduleSunset.Value, RampUpMinutes = ScheduleRampUpMinutes, ChannelBrightness = ScheduleChannelBrightness, ActiveDays = ScheduleActiveDays }`; call `ILampService.SaveScheduleAsync(Lamp.Id, config)` (service re-validates, throws `ArgumentException` on failure); build `LightSchedule` from config and call `Device!.AddScheduleAsync(lightSchedule, ct)` (check `AquaSync.Chihiros/README.md` for exact `LightSchedule` constructor); on BLE failure catch `ChihirosException`, set `ErrorMessage` and `IsErrorOpen=true` (local save already committed); always set `IsSavingSchedule=false`

- [x] T022 [US4] Add schedule section to `LampDetailPage.xaml` in `AquaSync.App/Views/LampDetailPage.xaml`: after the Manual Brightness `ItemsControl`; **"Schedule" section heading** TextBlock; `controls:ScheduleEditorControl SunriseTime="{x:Bind ViewModel.ScheduleSunrise, Mode=TwoWay}" SunsetTime="{x:Bind ViewModel.ScheduleSunset, Mode=TwoWay}" RampUpMinutes="{x:Bind ViewModel.ScheduleRampUpMinutes, Mode=OneWay}"`; **Sunrise TimePicker** (WinUI3 `TimePicker` or `TimePickerFlyout`, FR-042 primary entry) bound two-way to `ViewModel.ScheduleSunrise`; **Sunset TimePicker** bound two-way to `ViewModel.ScheduleSunset`; **"Ramp-Up Duration (minutes)"** label + `NumberBox Minimum="0" Maximum="150" Value="{x:Bind ViewModel.ScheduleRampUpMinutes, Mode=TwoWay}"`; **"Active Days"** label; weekday `CheckBox` row: 7 CheckBoxes with `Content` Mon/Tue/Wed/Thu/Fri/Sat/Sun (FR-043 Monday-to-Sunday order), each `IsChecked` bound to `ViewModel.ScheduleActiveDays` via `WeekdayFlagConverter` with `ConverterParameter="Monday"` / `"Tuesday"` etc.; **"Schedule Brightness" TextBlock** section heading; per-channel brightness sliders for schedule peak levels (same `ItemsControl` pattern as Manual Brightness but bound to `ScheduleChannelBrightness`); **Save Schedule `Button`** `IsEnabled="{x:Bind ViewModel.IsScheduleValid, Mode=OneWay}"` `Command="{x:Bind ViewModel.SaveScheduleCommand}"`, with inline `ProgressRing IsActive="{x:Bind ViewModel.IsSavingSchedule, Mode=OneWay}"`; **ValidationMessage TextBlock** Foreground=SystemFillColorCriticalBrush, Visibility bound to ValidationMessage not-null; entire schedule section `Visibility` bound to `!IsUnmanaged`; add `xmlns:controls="using:AquaSync.App.Controls"` and `xmlns:converters="using:AquaSync.App.Converters"` to page root

**Checkpoint**: Open schedule editor → set sunrise via time picker → handle appears on timeline and ramp gradient renders → configure all fields → Save → schedule programmed on device.

---

## Phase 7: User Story 5 — Control Lamp Mode from Dashboard (Priority: P5)

**Goal**: Dashboard shows a lamp status card per assigned lamp with its derived on/off state and an Off | Manual | Automatic segmented control. Switching modes persists the change and sends the appropriate BLE command.

**Independent Test**: Open Dashboard → each assigned lamp has a card with its current mode pre-selected → switch a lamp to Manual → device activates at stored brightness (FR-039).

- [x] T023 [US5] Update `DashboardViewModel` in `AquaSync.App/ViewModels/DashboardViewModel.cs`: inject `ILampService` (add to constructor); add inner `public sealed class LampCardViewModel : ViewModelBase` with `Guid LampId`, `string DisplayName`, `string ModelName`, `LampMode CurrentMode` (SetProperty), `bool IsConnecting` (SetProperty), computed `bool IsOn` (`CurrentMode == LampMode.Manual || CurrentMode == LampMode.Automatic`); `ObservableCollection<LampCardViewModel> LampCards` (init `[]`); computed `bool HasLamps` (`LampCards.Count > 0`); in `OnNavigatedTo` (or wherever Dashboard loads aquarium data) call `ILampService.GetLampsForAquariumAsync(aquariumId)` and populate `LampCards`; add `RelayCommand<(LampCardViewModel card, LampMode newMode)> SetLampModeCommand` (or pass both via a single VM parameter): (1) set `card.IsConnecting = true`; (2) call `ILampService.SaveModeAsync(card.LampId, newMode)`; (3) connect: `IChihirosDevice? device = await ILampService.ConnectAsync(lamp, ct)`; (4) send BLE command per mode — `LampMode.Off` → `device?.TurnOffAsync(ct)`, `LampMode.Manual` → `device?.TurnOnAsync(ct)` + FR-039: foreach channel in `lamp.ManualBrightness` (or 100% per `DeviceProfile.Channels` if empty) call `device.SetBrightnessAsync(channel, value, ct)`, `LampMode.Automatic` → `device?.EnableAutoModeAsync(ct)`; (5) `card.CurrentMode = newMode`; (6) `card.IsConnecting = false`; on any `ChihirosException` show error notification and reset `IsConnecting`; disconnect device after command (Dashboard uses transient connect-command-disconnect pattern per data-model.md)

- [x] T024 [P] [US5] Add lamp status cards section to `DashboardPage.xaml` in `AquaSync.App/Views/DashboardPage.xaml`: add as first visible section (before existing content): **"Lamps" TextBlock** section heading; `ItemsControl ItemsSource="{x:Bind ViewModel.LampCards, Mode=OneWay}"`; `DataTemplate`: `Border` card (rounded corners, shadow, Margin=0 0 0 8): inner `Grid` (2 rows): Row 0 `StackPanel` Horizontal (TextBlock `DisplayName` Bold, TextBlock `ModelName` Subtle); Row 1 `Grid` (2 cols): Col0 — segmented mode control (use `RadioButtons` with 3 `RadioButton` items Off/Manual/Automatic, or WinUI3 `SelectorBar` if available, bound to `CurrentMode`, `SelectionChanged` fires `ViewModel.SetLampModeCommand`), Col1 `ProgressRing IsActive="{x:Bind IsConnecting, Mode=OneWay}" Width="20" Height="20"`; FR-037 empty-state: **TextBlock** "No lamps assigned — go to Lamps to add one" (`Visibility` bound to `!ViewModel.HasLamps`)

**Checkpoint**: Open Dashboard → lamp cards visible → switch a lamp from Off to Manual → device turns on at stored brightness.

---

## Phase 8: User Story 6 — Remove a Lamp (Priority: P6)

**Goal**: Users can remove a lamp via a context menu action. A confirmation dialog names the lamp and warns of permanent deletion. Confirming disconnects any active BLE session, deletes the config, and removes it from the list.

**Independent Test**: Right-click a lamp in the list → Remove → confirmation dialog names the lamp and model → confirm → lamp gone from list → same device discoverable again via BLE scan.

- [ ] T025 [US6] Add `RemoveLampCommand` to `LampsViewModel` in `AquaSync.App/ViewModels/LampsViewModel.cs`: `RelayCommand<LampConfiguration> RemoveLampCommand`: construct and show `ContentDialog` on the current `XamlRoot` with `Title = "Remove Lamp"`, `Content = $"Remove \"{lamp.DeviceName}\" ({lamp.ModelName})?\n\nThis will permanently delete the lamp configuration and cannot be undone."`, `PrimaryButtonText = "Remove"`, `CloseButtonText = "Cancel"`; on `ContentDialogResult.Primary`: call `ILampService.DisconnectAsync(lamp.BluetoothAddress)`, then `ILampService.RemoveLampAsync(lamp.Id)`, then `Lamps.Remove(lamp)`; note: `ContentDialog.XamlRoot` must be set from the page — pass `XamlRoot` into the ViewModel or show from code-behind and call a ViewModel method on confirm

- [ ] T026 [US6] Add remove context menu to `LampsPage.xaml` in `AquaSync.App/Views/LampsPage.xaml`: add `ContextFlyout` to the `ListView` `DataTemplate` container: `<FlyoutBase.AttachedFlyout><MenuFlyout><MenuFlyoutItem Text="Remove" Command="{x:Bind ViewModel.RemoveLampCommand, Mode=OneWay}" CommandParameter="{x:Bind}"/></MenuFlyout></FlyoutBase.AttachedFlyout>`; trigger on right-click via `RightTapped` event calling `FlyoutBase.ShowAttachedFlyout(sender)` in code-behind or use `SwipeControl` with `SwipeItem` for touch-friendly alternative

**Checkpoint**: Right-click a lamp → Remove appears → confirm → lamp gone → BLE scan can re-discover and re-add the same device.

---

## Phase 9: User Story 7 — Synchronize the Device Clock (Priority: P7)

**Goal**: Lamp detail view provides a Sync Clock button that writes the current system time to the device's internal clock. Success or failure is reported to the user.

**Independent Test**: Open a connected lamp's detail view → tap Sync Clock → no error shown (success) → tap while disconnected → button disabled.

- [ ] T027 [US7] Add `SyncClockCommand` to `LampDetailViewModel` in `AquaSync.App/ViewModels/LampDetailViewModel.cs`: add `bool IsClockSyncing` property (SetProperty); `RelayCommand SyncClockCommand` (enabled only when `IsConnected && !IsConnecting`): set `IsClockSyncing = true`; call the device clock-set method from `AquaSync.Chihiros` (verify exact name in `AquaSync.Chihiros/README.md` — likely `Device!.SetTimeAsync(DateTimeOffset.Now, ct)` or `SyncTimeAsync`); on success: clear `ErrorMessage`, leave `IsErrorOpen = false` (FR-026 success state); on `ChihirosException`: set `ErrorMessage` to clock sync failure description, `IsErrorOpen = true` (FR-026 error state); always: set `IsClockSyncing = false` in `finally`

- [ ] T028 [US7] Add clock sync section to `LampDetailPage.xaml` in `AquaSync.App/Views/LampDetailPage.xaml`: after the Schedule section; **"Device Clock" TextBlock** section heading; TextBlock "Synchronize the lamp's internal clock with system time."; `StackPanel Orientation=Horizontal`: `Button Content="Sync Clock" Command="{x:Bind ViewModel.SyncClockCommand}"` `IsEnabled="{x:Bind ViewModel.IsConnected, Mode=OneWay}"`, `ProgressRing IsActive="{x:Bind ViewModel.IsClockSyncing, Mode=OneWay}" Width="20" Height="20" Margin="8 0 0 0"`; entire clock section hidden (`Visibility=Collapsed`) when `IsUnmanaged`

**Checkpoint**: Detail view shows Sync Clock button → tap it while connected → no error InfoBar opens → button is disabled when device is disconnected.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Verification tasks that span multiple components and stories.

- [ ] T029 [P] Verify `INavigationAware` invocation: inspect `AquaSync.App/Services/NavigationService.cs` (and `AquaSync.App/Views/ShellPage.xaml.cs`) to confirm that `OnNavigatedTo(object parameter)` and `OnNavigatedFrom()` are called on the incoming/outgoing ViewModel when `NavigateTo` is invoked; if the invocation pattern differs from `LampDetailViewModel`'s expectations (e.g., parameter type mismatch, method not called), update `LampDetailViewModel.OnNavigatedTo` and `OnNavigatedFrom` accordingly

- [ ] T030 [P] Audit `DispatcherQueue.TryEnqueue` usage: review all BLE background-thread event handlers in `AquaSync.App/Services/LampService.cs` (Disconnected event callback in `ConnectAsync`) and `AquaSync.App/ViewModels/LampDetailViewModel.cs` (`OnDeviceDisconnected`) to confirm every UI property update is wrapped in `_dispatcherQueue.TryEnqueue(() => { ... })`; fix any missed marshalling that would cause cross-thread `ObservableCollection` exceptions

- [ ] T031 [P] Verify all `InfoBar` bindings: confirm `IsOpen`, `Severity`, and `Message` bindings in `AquaSync.App/Views/LampsPage.xaml`, `AquaSync.App/Views/LampDetailPage.xaml`, and `AquaSync.App/Views/AddLampDialog.xaml` surface correct error state from their respective ViewModels; run through each error path (BLE failure, BLE unavailable, device disconnect, schedule validation failure, clock sync failure) and confirm the correct InfoBar opens with a meaningful message

- [ ] T032 Final build and warning cleanup: run `dotnet build AquaSync.App/AquaSync.App.csproj -r win-x64` and resolve all compiler errors and warnings — `TreatWarningsAsErrors` is enabled; common issues to check: nullable reference type warnings on `Lamp?` / `Device?` access, missing `using` directives for new namespaces (`AquaSync.App.Models`, `AquaSync.App.Contracts.Services`, `AquaSync.App.Controls`, `AquaSync.App.Converters`), unused `using` directives, uninitialized properties in model classes

---

## Dependencies & Execution Order

### Phase Dependencies

| Phase | Depends On | Blocks |
|-------|-----------|--------|
| Phase 1 (Setup) | — | Phase 2 |
| Phase 2 (Foundational) | Phase 1 | **All user story phases** |
| Phase 3 (US1) | Phase 2 | Phase 4 |
| Phase 4 (US2) | Phase 3 | Phase 5 |
| Phase 5 (US3) | Phase 2 + Phase 4 | Phase 6, Phase 9 |
| Phase 6 (US4) | Phase 5 | — |
| Phase 7 (US5) | Phase 2 + Phase 3 | — |
| Phase 8 (US6) | Phase 3 | — |
| Phase 9 (US7) | Phase 5 | — |
| Phase 10 (Polish) | All prior phases | — |

### User Story Dependencies

- **US1 (Phase 3)**: No story dependencies — needs only Phase 2 foundational layer
- **US2 (Phase 4)**: Needs US1's `LampsPage` shell to add the full DataTemplate + navigation wiring
- **US3 (Phase 5)**: Needs US2's `SelectLampCommand` to navigate to the detail page
- **US4 (Phase 6)**: Needs US3's `LampDetailViewModel` base + `LampDetailPage` scaffold
- **US5 (Phase 7)**: Needs only Phase 2 (ILampService) + Phase 3 (lamps exist to show); independently implementable
- **US6 (Phase 8)**: Needs only Phase 3 (LampsViewModel + LampsPage); independently implementable
- **US7 (Phase 9)**: Needs US3's `LampDetailViewModel` base for `Device` access

### Within Each Phase (Sequential Order)

- T002, T003 → T004 → T005 → T006 → T007 (models → interface → service → DI)
- T008 (ViewModel) before T011 (XAML binds to ViewModel)
- T009 (Dialog XAML) before T010 (Dialog code-behind references named elements)
- T012 (XAML DataTemplate) and T013 (ViewModel command) → both needed for end-to-end navigation
- T015 (LampDetailViewModel) before T017 (LampDetailPage XAML binds to it)
- T018 (ScheduleEditorControl XAML) before T019 (code-behind references named elements)
- T021 (LampDetailViewModel schedule additions) before T022 (LampDetailPage schedule section binds to them)
- T023 (DashboardViewModel) before T024 (DashboardPage XAML binds to it)

### Parallel Opportunities

- **T002 + T003**: `LampMode.cs` and `ScheduleConfiguration.cs` are independent files — write simultaneously
- **T008 + T009**: `LampsViewModel` and `AddLampDialog.xaml` touch different files — start together
- **T014 + T016**: DI registration and `LampDetailPage.xaml.cs` shell — different files, write together
- **T018 + T020**: `ScheduleEditorControl.xaml` and `WeekdayFlagConverter.cs` — independent, write together
- **T024 + T023**: `DashboardPage.xaml` shell can be written while `DashboardViewModel` is in progress
- **T029 + T030 + T031**: All polish verification tasks are read-only or independent file checks

---

## Parallel Execution Examples

### Phase 2 (Foundational)
```
Parallel: T002 (LampMode.cs) + T003 (ScheduleConfiguration.cs)
→ Then:   T004 (LampConfiguration.cs)
→ Then:   T005 (ILampService.cs)
→ Then:   T006 (LampService.cs)
→ Then:   T007 (App.xaml.cs DI)
```

### Phase 3 (US1)
```
Parallel: T008 (LampsViewModel) + T009 (AddLampDialog.xaml)
→ Then:   T010 (AddLampDialog.xaml.cs)
→ Then:   T011 (LampsPage.xaml)
```

### Phase 5 (US3)
```
Parallel: T014 (DI registration) + T016 (LampDetailPage.xaml.cs shell)
→ Then:   T015 (LampDetailViewModel) — can start alongside T014/T016
→ Then:   T017 (LampDetailPage.xaml)
```

### Phase 6 (US4)
```
Parallel: T018 (ScheduleEditorControl.xaml) + T020 (WeekdayFlagConverter.cs)
→ Then:   T019 (ScheduleEditorControl.xaml.cs)
Parallel: T021 (LampDetailViewModel schedule additions) — can start when T015 base exists
→ Then:   T022 (LampDetailPage.xaml schedule section) — needs T018-T021 all done
```

### Phase 10 (Polish)
```
Parallel: T029 + T030 + T031 (all independent verification tasks)
→ Then:   T032 (final build)
```

---

## Implementation Strategy

### MVP First (US1 Only)

1. Complete Phase 1: Setup (T001)
2. Complete Phase 2: Foundational (T002–T007) — **required before anything else**
3. Complete Phase 3: US1 — Discover and Add (T008–T011)
4. **STOP and validate**: Can you add a lamp and see it in the list?
5. Proceed to Phase 4+ in priority order

### Incremental Delivery

| After Phase | Deliverable | Stories Complete |
|-------------|-------------|-----------------|
| Phase 2 | Service layer + models | None (no UI) |
| Phase 3 | Add lamp flow | US1 |
| Phase 4 | Full list + navigation shell | US1, US2 |
| Phase 5 | Brightness control | US1, US2, US3 |
| Phase 6 | Schedule programming | US1–US4 |
| Phase 7 | Dashboard mode toggle | US1–US5 |
| Phase 8 | Lamp removal | US1–US6 |
| Phase 9 | Clock sync | All 7 stories |
| Phase 10 | Polished and verified | Full feature |

---

## Notes

- `[P]` = different files, no incomplete dependencies — safe to start simultaneously
- No test tasks — `AquaSync.App` has no test infrastructure (out of scope per plan.md)
- **Before starting T006**: verify exact `IDataService` method signatures for reading collections (`ReadAllAsync<T>(folder)`) in the existing `DataService.cs` implementation
- **Before T019/T027**: check `AquaSync.Chihiros/README.md` for exact `IChihirosDevice` method names — specifically the clock-set method and `AddScheduleAsync` signature / `LightSchedule` constructor
- **`DispatcherQueue` injection**: inject as `DispatcherQueue` singleton registered in DI (or capture via `DispatcherQueue.GetForCurrentThread()` in ViewModel constructor, which runs on UI thread)
- **All sealed classes**, file-scoped namespaces, manual `SetProperty` / `RelayCommand`, no `[ObservableProperty]` / `[RelayCommand]` attributes — enforced by CLAUDE.md conventions and `TreatWarningsAsErrors`
- **`WeekdayFlagConverter` two-way binding**: standard `IValueConverter.ConvertBack` receives the new `bool` value but not the current aggregate `Weekday`; the cleanest approach is to bind each `CheckBox.IsChecked` to an individual `bool` property on the ViewModel (e.g., `ScheduleMonday`, `ScheduleTuesday`, ...) that internally computes `ScheduleActiveDays`, avoiding the ConvertBack problem entirely — evaluate this tradeoff when implementing T020/T021
