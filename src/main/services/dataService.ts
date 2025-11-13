/**
 * Data Service for Local File-Based Storage
 *
 * This service manages JSON file storage for aquariums, devices, water tests, and settings.
 * All data is stored in the user's AppData folder using app.getPath('userData').
 */

import { app } from 'electron';
import * as fs from 'fs/promises';
import * as path from 'path';
import { randomUUID } from 'crypto';
import type {
  Aquarium,
  Device,
  WaterTest,
  AppSettings,
  IpcResult,
} from '../../shared/types';

/**
 * File names for data storage
 */
const DATA_FILES = {
  AQUARIUMS: 'aquariums.json',
  DEVICES: 'devices.json',
  WATER_TESTS: 'water-tests.json',
  SETTINGS: 'settings.json',
} as const;

/**
 * Default application settings
 */
const DEFAULT_SETTINGS: AppSettings = {
  theme: 'system',
  notifications: {
    enabled: true,
    maintenanceReminders: true,
    waterTestReminders: true,
  },
  units: {
    temperature: 'celsius',
    volume: 'liter',
    dimensions: 'cm',
  },
  autoBackup: {
    enabled: false,
    intervalDays: 7,
  },
};

/**
 * Get the data directory path
 */
export function getDataPath(): string {
  return app.getPath('userData');
}

/**
 * Get the full path for a data file
 */
function getFilePath(fileName: string): string {
  return path.join(getDataPath(), fileName);
}

/**
 * Ensure data directory exists
 */
async function ensureDataDirectory(): Promise<void> {
  const dataPath = getDataPath();
  try {
    await fs.access(dataPath);
  } catch {
    await fs.mkdir(dataPath, { recursive: true });
  }
}

/**
 * Read data from a JSON file
 */
async function readJsonFile<T>(fileName: string, defaultValue: T): Promise<T> {
  await ensureDataDirectory();
  const filePath = getFilePath(fileName);

  try {
    const data = await fs.readFile(filePath, 'utf-8');
    return JSON.parse(data) as T;
  } catch (error) {
    // If file doesn't exist or is invalid, return default value and create file
    if ((error as NodeJS.ErrnoException).code === 'ENOENT' || error instanceof SyntaxError) {
      await writeJsonFile(fileName, defaultValue);
      return defaultValue;
    }
    throw error;
  }
}

/**
 * Write data to a JSON file
 */
async function writeJsonFile<T>(fileName: string, data: T): Promise<void> {
  await ensureDataDirectory();
  const filePath = getFilePath(fileName);
  const jsonData = JSON.stringify(data, null, 2);
  await fs.writeFile(filePath, jsonData, 'utf-8');
}

/**
 * Wrap result in IpcResult format
 */
function success<T>(data: T): IpcResult<T> {
  return { success: true, data };
}

/**
 * Wrap error in IpcResult format
 */
function error<T>(errorMessage: string): IpcResult<T> {
  return { success: false, error: errorMessage };
}

// ============================================
// Aquarium CRUD Operations
// ============================================

export async function getAquariums(): Promise<IpcResult<Aquarium[]>> {
  try {
    const aquariums = await readJsonFile<Aquarium[]>(DATA_FILES.AQUARIUMS, []);
    return success(aquariums);
  } catch (err) {
    return error((err as Error).message);
  }
}

export async function getAquarium(id: string): Promise<IpcResult<Aquarium | null>> {
  try {
    const aquariums = await readJsonFile<Aquarium[]>(DATA_FILES.AQUARIUMS, []);
    const aquarium = aquariums.find((a) => a.id === id) || null;
    return success(aquarium);
  } catch (err) {
    return error((err as Error).message);
  }
}

export async function createAquarium(
  aquariumData: Omit<Aquarium, 'id' | 'createdAt'>
): Promise<IpcResult<Aquarium>> {
  try {
    const aquariums = await readJsonFile<Aquarium[]>(DATA_FILES.AQUARIUMS, []);

    const newAquarium: Aquarium = {
      ...aquariumData,
      id: randomUUID(),
      createdAt: new Date().toISOString(),
    };

    aquariums.push(newAquarium);
    await writeJsonFile(DATA_FILES.AQUARIUMS, aquariums);

    return success(newAquarium);
  } catch (err) {
    return error((err as Error).message);
  }
}

export async function updateAquarium(
  id: string,
  updates: Partial<Aquarium>
): Promise<IpcResult<Aquarium>> {
  try {
    const aquariums = await readJsonFile<Aquarium[]>(DATA_FILES.AQUARIUMS, []);
    const index = aquariums.findIndex((a) => a.id === id);

    if (index === -1) {
      return error('Aquarium not found');
    }

    const updatedAquarium: Aquarium = {
      ...aquariums[index],
      ...updates,
      id, // Ensure ID cannot be changed
      updatedAt: new Date().toISOString(),
    };

    aquariums[index] = updatedAquarium;
    await writeJsonFile(DATA_FILES.AQUARIUMS, aquariums);

    return success(updatedAquarium);
  } catch (err) {
    return error((err as Error).message);
  }
}

export async function deleteAquarium(id: string): Promise<IpcResult<boolean>> {
  try {
    const aquariums = await readJsonFile<Aquarium[]>(DATA_FILES.AQUARIUMS, []);
    const filteredAquariums = aquariums.filter((a) => a.id !== id);

    if (filteredAquariums.length === aquariums.length) {
      return error('Aquarium not found');
    }

    await writeJsonFile(DATA_FILES.AQUARIUMS, filteredAquariums);

    // Also delete associated devices and water tests
    await deleteDevicesByAquariumId(id);
    await deleteWaterTestsByAquariumId(id);

    return success(true);
  } catch (err) {
    return error((err as Error).message);
  }
}

// ============================================
// Device CRUD Operations
// ============================================

export async function getDevices(aquariumId?: string): Promise<IpcResult<Device[]>> {
  try {
    const devices = await readJsonFile<Device[]>(DATA_FILES.DEVICES, []);
    const filteredDevices = aquariumId
      ? devices.filter((d) => d.aquariumId === aquariumId)
      : devices;
    return success(filteredDevices);
  } catch (err) {
    return error((err as Error).message);
  }
}

export async function getDevice(id: string): Promise<IpcResult<Device | null>> {
  try {
    const devices = await readJsonFile<Device[]>(DATA_FILES.DEVICES, []);
    const device = devices.find((d) => d.id === id) || null;
    return success(device);
  } catch (err) {
    return error((err as Error).message);
  }
}

export async function createDevice(
  deviceData: Omit<Device, 'id' | 'createdAt'>
): Promise<IpcResult<Device>> {
  try {
    const devices = await readJsonFile<Device[]>(DATA_FILES.DEVICES, []);

    const newDevice: Device = {
      ...deviceData,
      id: randomUUID(),
      createdAt: new Date().toISOString(),
    };

    devices.push(newDevice);
    await writeJsonFile(DATA_FILES.DEVICES, devices);

    return success(newDevice);
  } catch (err) {
    return error((err as Error).message);
  }
}

export async function updateDevice(
  id: string,
  updates: Partial<Device>
): Promise<IpcResult<Device>> {
  try {
    const devices = await readJsonFile<Device[]>(DATA_FILES.DEVICES, []);
    const index = devices.findIndex((d) => d.id === id);

    if (index === -1) {
      return error('Device not found');
    }

    const updatedDevice: Device = {
      ...devices[index],
      ...updates,
      id, // Ensure ID cannot be changed
      updatedAt: new Date().toISOString(),
    };

    devices[index] = updatedDevice;
    await writeJsonFile(DATA_FILES.DEVICES, devices);

    return success(updatedDevice);
  } catch (err) {
    return error((err as Error).message);
  }
}

export async function deleteDevice(id: string): Promise<IpcResult<boolean>> {
  try {
    const devices = await readJsonFile<Device[]>(DATA_FILES.DEVICES, []);
    const filteredDevices = devices.filter((d) => d.id !== id);

    if (filteredDevices.length === devices.length) {
      return error('Device not found');
    }

    await writeJsonFile(DATA_FILES.DEVICES, filteredDevices);
    return success(true);
  } catch (err) {
    return error((err as Error).message);
  }
}

/**
 * Helper function to delete all devices for a specific aquarium
 */
async function deleteDevicesByAquariumId(aquariumId: string): Promise<void> {
  const devices = await readJsonFile<Device[]>(DATA_FILES.DEVICES, []);
  const filteredDevices = devices.filter((d) => d.aquariumId !== aquariumId);
  await writeJsonFile(DATA_FILES.DEVICES, filteredDevices);
}

// ============================================
// Water Test CRUD Operations
// ============================================

export async function getWaterTests(aquariumId?: string): Promise<IpcResult<WaterTest[]>> {
  try {
    const waterTests = await readJsonFile<WaterTest[]>(DATA_FILES.WATER_TESTS, []);
    const filteredTests = aquariumId
      ? waterTests.filter((w) => w.aquariumId === aquariumId)
      : waterTests;
    return success(filteredTests);
  } catch (err) {
    return error((err as Error).message);
  }
}

export async function getWaterTest(id: string): Promise<IpcResult<WaterTest | null>> {
  try {
    const waterTests = await readJsonFile<WaterTest[]>(DATA_FILES.WATER_TESTS, []);
    const waterTest = waterTests.find((w) => w.id === id) || null;
    return success(waterTest);
  } catch (err) {
    return error((err as Error).message);
  }
}

export async function createWaterTest(
  waterTestData: Omit<WaterTest, 'id' | 'createdAt'>
): Promise<IpcResult<WaterTest>> {
  try {
    const waterTests = await readJsonFile<WaterTest[]>(DATA_FILES.WATER_TESTS, []);

    const newWaterTest: WaterTest = {
      ...waterTestData,
      id: randomUUID(),
      createdAt: new Date().toISOString(),
    };

    waterTests.push(newWaterTest);
    await writeJsonFile(DATA_FILES.WATER_TESTS, waterTests);

    return success(newWaterTest);
  } catch (err) {
    return error((err as Error).message);
  }
}

export async function updateWaterTest(
  id: string,
  updates: Partial<WaterTest>
): Promise<IpcResult<WaterTest>> {
  try {
    const waterTests = await readJsonFile<WaterTest[]>(DATA_FILES.WATER_TESTS, []);
    const index = waterTests.findIndex((w) => w.id === id);

    if (index === -1) {
      return error('Water test not found');
    }

    const updatedWaterTest: WaterTest = {
      ...waterTests[index],
      ...updates,
      id, // Ensure ID cannot be changed
    };

    waterTests[index] = updatedWaterTest;
    await writeJsonFile(DATA_FILES.WATER_TESTS, waterTests);

    return success(updatedWaterTest);
  } catch (err) {
    return error((err as Error).message);
  }
}

export async function deleteWaterTest(id: string): Promise<IpcResult<boolean>> {
  try {
    const waterTests = await readJsonFile<WaterTest[]>(DATA_FILES.WATER_TESTS, []);
    const filteredTests = waterTests.filter((w) => w.id !== id);

    if (filteredTests.length === waterTests.length) {
      return error('Water test not found');
    }

    await writeJsonFile(DATA_FILES.WATER_TESTS, filteredTests);
    return success(true);
  } catch (err) {
    return error((err as Error).message);
  }
}

/**
 * Helper function to delete all water tests for a specific aquarium
 */
async function deleteWaterTestsByAquariumId(aquariumId: string): Promise<void> {
  const waterTests = await readJsonFile<WaterTest[]>(DATA_FILES.WATER_TESTS, []);
  const filteredTests = waterTests.filter((w) => w.aquariumId !== aquariumId);
  await writeJsonFile(DATA_FILES.WATER_TESTS, filteredTests);
}

// ============================================
// Settings Operations
// ============================================

export async function getSettings(): Promise<IpcResult<AppSettings>> {
  try {
    const settings = await readJsonFile<AppSettings>(DATA_FILES.SETTINGS, DEFAULT_SETTINGS);
    return success(settings);
  } catch (err) {
    return error((err as Error).message);
  }
}

export async function updateSettings(
  updates: Partial<AppSettings>
): Promise<IpcResult<AppSettings>> {
  try {
    const settings = await readJsonFile<AppSettings>(DATA_FILES.SETTINGS, DEFAULT_SETTINGS);

    const updatedSettings: AppSettings = {
      ...settings,
      ...updates,
    };

    await writeJsonFile(DATA_FILES.SETTINGS, updatedSettings);
    return success(updatedSettings);
  } catch (err) {
    return error((err as Error).message);
  }
}
