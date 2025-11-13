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
export type DeviceStatus = 'connected' | 'disconnected' | 'error';

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
 * Water test measurement
 */
export interface WaterTest {
  id: string; // GUID
  aquariumId: string; // Reference to Aquarium
  testDate: string; // ISO date
  parameters: {
    type: WaterParameterType;
    value: number;
    unit: string;
    status?: 'normal' | 'warning' | 'critical';
  }[];
  notes?: string;
  createdAt: string; // ISO date
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

  // Utility
  getDataPath: () => Promise<IpcResult<string>>;
}

/**
 * Window API exposed via preload
 */
export interface ElectronAPI {
  data: DataAPI;
}
