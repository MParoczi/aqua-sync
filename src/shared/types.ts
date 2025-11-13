// Shared types between main and renderer processes

export interface AppInfo {
  name: string;
  version: string;
}

// ============================================
// Data Models for US-003: Local Data Persistence
// ============================================

/**
 * Aquarium configuration
 */
export interface Aquarium {
  id: string; // GUID
  name: string;
  type: 'freshwater' | 'marine';
  dimensions: {
    width: number;
    length: number;
    height: number;
    unit: 'cm' | 'inch';
  };
  volume: {
    value: number;
    unit: 'liter' | 'gallon';
    isCustom: boolean;
  };
  startDate: string; // ISO date
  thumbnailPath?: string;
  createdAt: string; // ISO date
  updatedAt?: string; // ISO date
}

/**
 * Device types
 */
export type DeviceType = 'filter' | 'lamp';
export type DeviceStatus = 'connecting' | 'connected' | 'offline' | 'error';

/**
 * Base device interface
 */
export interface Device {
  id: string; // GUID
  aquariumId: string; // Reference to Aquarium
  type: DeviceType;
  name: string;
  manufacturer: 'eheim' | 'chihiros' | 'other';
  model: string;
  status: DeviceStatus;
  bluetoothAddress?: string;
  macAddress?: string; // MAC address for Eheim devices (US-019)
  ipAddress?: string; // IP address for Eheim devices (US-019)
  firmwareVersion?: string; // Firmware version (e.g., "S2037") (US-019)
  lastSeen?: string; // ISO date - last time device was seen online (US-019)
  config?: Record<string, unknown>; // Device-specific configuration (US-019)
  createdAt: string; // ISO date
  updatedAt?: string; // ISO date
}

/**
 * Filter-specific device
 */
export interface FilterDevice extends Device {
  type: 'filter';
  maintenanceSchedule?: {
    lastMaintenance: string; // ISO date
    nextMaintenance: string; // ISO date
    intervalDays: number;
  };
  flowRate?: number; // in liters per hour
}

/**
 * Lamp-specific device
 */
export interface LampDevice extends Device {
  type: 'lamp';
  schedule?: {
    enabled: boolean;
    onTime: string; // HH:mm format
    offTime: string; // HH:mm format
  };
  brightness?: number; // 0-100
  color?: {
    red: number; // 0-255
    green: number; // 0-255
    blue: number; // 0-255
  };
}

/**
 * Water test parameter types
 */
export type WaterParameterType =
  | 'temperature'
  | 'ph'
  | 'ammonia'
  | 'nitrite'
  | 'nitrate'
  | 'hardness'
  | 'alkalinity'
  | 'phosphate'
  | 'salinity'; // for marine aquariums

/**
 * Water test measurement (US-018)
 * One measurement per record for simplified querying and graphing
 */
export interface WaterTest {
  id: string; // GUID
  aquariumId: string; // Reference to Aquarium
  parameter: WaterParameterOption; // The parameter being measured
  value: number; // The measured value
  unit: string; // Unit of measurement (e.g., 'pH', 'mg/l', '°C')
  measuredAt: string; // ISO datetime when measurement was taken
  createdAt: string; // ISO datetime when record was created
}

/**
 * Water parameter types for US-014 graph selector
 */
export type WaterParameterOption =
  | 'pH'
  | 'GH'
  | 'KH'
  | 'NO₂'
  | 'NO₃'
  | 'NH₄'
  | 'Fe'
  | 'Cu'
  | 'SiO₂'
  | 'PO₄'
  | 'CO₂'
  | 'O₂'
  | 'Temperature';

/**
 * Aquarium-specific settings
 */
export interface AquariumSettings {
  selectedWaterParameters: WaterParameterOption[];
}

/**
 * Application settings
 */
export interface AppSettings {
  theme: 'light' | 'dark' | 'system';
  notifications: {
    enabled: boolean;
    maintenanceReminders: boolean;
    waterTestReminders: boolean;
  };
  units: {
    temperature: 'celsius' | 'fahrenheit';
    volume: 'liter' | 'gallon';
    dimensions: 'cm' | 'inch';
  };
  dataPath?: string; // Custom data storage path
  lastBackup?: string; // ISO date
  autoBackup: {
    enabled: boolean;
    intervalDays: number;
  };
  // Aquarium-specific settings (US-014)
  aquariumSettings: {
    [aquariumId: string]: AquariumSettings;
  };
}

// ============================================
// IPC API Types
// ============================================

/**
 * Result wrapper for IPC operations
 */
export interface IpcResult<T> {
  success: boolean;
  data?: T;
  error?: string;
}

/**
 * CRUD operations for data persistence
 */
export interface DataAPI {
  // Aquariums
  getAquariums: () => Promise<IpcResult<Aquarium[]>>;
  getAquarium: (id: string) => Promise<IpcResult<Aquarium | null>>;
  createAquarium: (aquarium: Omit<Aquarium, 'id' | 'createdAt'>) => Promise<IpcResult<Aquarium>>;
  updateAquarium: (id: string, aquarium: Partial<Aquarium>) => Promise<IpcResult<Aquarium>>;
  deleteAquarium: (id: string) => Promise<IpcResult<boolean>>;

  // Devices
  getDevices: (aquariumId?: string) => Promise<IpcResult<Device[]>>;
  getDevice: (id: string) => Promise<IpcResult<Device | null>>;
  createDevice: (device: Omit<Device, 'id' | 'createdAt'>) => Promise<IpcResult<Device>>;
  updateDevice: (id: string, device: Partial<Device>) => Promise<IpcResult<Device>>;
  deleteDevice: (id: string) => Promise<IpcResult<boolean>>;

  // Water Tests
  getWaterTests: (aquariumId?: string) => Promise<IpcResult<WaterTest[]>>;
  getWaterTest: (id: string) => Promise<IpcResult<WaterTest | null>>;
  createWaterTest: (waterTest: Omit<WaterTest, 'id' | 'createdAt'>) => Promise<IpcResult<WaterTest>>;
  updateWaterTest: (id: string, waterTest: Partial<WaterTest>) => Promise<IpcResult<WaterTest>>;
  deleteWaterTest: (id: string) => Promise<IpcResult<boolean>>;

  // Settings
  getSettings: () => Promise<IpcResult<AppSettings>>;
  updateSettings: (settings: Partial<AppSettings>) => Promise<IpcResult<AppSettings>>;

  // Aquarium-specific settings (US-014)
  getAquariumSettings: (aquariumId: string) => Promise<IpcResult<AquariumSettings>>;
  updateAquariumSettings: (aquariumId: string, settings: Partial<AquariumSettings>) => Promise<IpcResult<AquariumSettings>>;

  // Utility
  getDataPath: () => Promise<IpcResult<string>>;
}

/**
 * File operations API
 */
export interface FileAPI {
  // Copy uploaded thumbnail to userData/thumbnails folder
  copyThumbnail: (file: File) => Promise<IpcResult<string>>;
  // Get thumbnail path from userData
  getThumbnailPath: (filename: string) => Promise<IpcResult<string>>;
}

/**
 * Window API exposed via preload
 */
export interface ElectronAPI {
  data: DataAPI;
  files: FileAPI;
  eheim: EheimAPI;
}

// ============================================
// Eheim Integration Types (US-019)
// ============================================

/**
 * Discovered Eheim device from mDNS scan
 */
export interface DiscoveredEheimDevice {
  hostname: string; // e.g., "eheimdigital.local"
  ipAddress: string; // e.g., "192.168.2.5"
  port: number; // typically 80
  macAddress?: string; // Retrieved after initial API call
  model?: string; // Detected from API response
  firmwareVersion?: string; // e.g., "S2037"
}

/**
 * Eheim filter data from WebSocket FILTER_DATA message
 */
export interface EheimFilterData {
  title: 'FILTER_DATA';
  from: string; // MAC address
  filterActive: number; // 0 or 1
  freq: number; // Current frequency in Hz
  freqSoll: number; // Target frequency in Hz
  rotSpeed: number; // Rotation speed
  pumpMode: number; // 1=Manual, 2=Pulse, 3=Constant Flow, 4=Bio
  rotorSpeed?: number; // 0-10 scale for manual mode
  // Bio mode fields
  nm_dfs_soll_day?: number; // Day flow level (0-10)
  nm_dfs_soll_night?: number; // Night flow level (0-10)
  end_time_night_mode?: number; // Day start time in minutes since midnight
  start_time_night_mode?: number; // Night start time in minutes since midnight
  runTime?: number; // Total running hours
  dfsFaktor?: number; // Flow factor (internal)
}

/**
 * Eheim user data from WebSocket USRDTA message
 */
export interface EheimUserData {
  title: 'USRDTA';
  macAddress: string;
  revision: [number, number]; // [master, client] firmware versions
  latestAvailableRevision: [number, number];
  firmwareAvailable: number; // 0 or 1
}

/**
 * WebSocket status update event types (US-019a)
 */
export type EheimStatusUpdateType = 'connection' | 'filter_data' | 'user_data' | 'mesh_network' | 'error';

/**
 * WebSocket status update event (US-019a)
 */
export interface EheimStatusUpdate {
  deviceId: string;
  timestamp: string; // ISO datetime
  type: EheimStatusUpdateType;
  status?: 'connected' | 'connecting' | 'offline' | 'error';
  data?: EheimFilterData | EheimUserData | unknown;
  error?: string;
}

/**
 * Eheim API for IPC communication
 */
export interface EheimAPI {
  // Discovery
  discover: () => Promise<IpcResult<DiscoveredEheimDevice[]>>;
  connectManual: (ipAddress: string, port: number) => Promise<IpcResult<DiscoveredEheimDevice>>;

  // Device information
  getDeviceInfo: (ipAddress: string, macAddress?: string) => Promise<IpcResult<EheimUserData>>;

  // WebSocket operations (US-019a)
  subscribe: (deviceId: string, ipAddress: string, macAddress?: string) => Promise<IpcResult<boolean>>;
  unsubscribe: (deviceId: string) => Promise<IpcResult<boolean>>;
  getConnectionStatus: (deviceId: string) => Promise<IpcResult<'connected' | 'connecting' | 'offline'>>;

  // WebSocket status update listener (US-019a)
  onStatusUpdate: (callback: (event: EheimStatusUpdate) => void) => () => void;
}
