// See the Electron documentation for details on how to use preload scripts:
// https://www.electronjs.org/docs/latest/tutorial/process-model#preload-scripts

import { contextBridge, ipcRenderer } from 'electron';
import type { ElectronAPI } from '../shared/types';

/**
 * Expose IPC API to renderer process via contextBridge
 * This allows the renderer to communicate with the main process securely
 */
const electronAPI: ElectronAPI = {
  data: {
    // Aquarium operations
    getAquariums: () => ipcRenderer.invoke('data:getAquariums'),
    getAquarium: (id: string) => ipcRenderer.invoke('data:getAquarium', id),
    createAquarium: (aquarium) => ipcRenderer.invoke('data:createAquarium', aquarium),
    updateAquarium: (id: string, updates) => ipcRenderer.invoke('data:updateAquarium', id, updates),
    deleteAquarium: (id: string) => ipcRenderer.invoke('data:deleteAquarium', id),

    // Device operations
    getDevices: (aquariumId?: string) => ipcRenderer.invoke('data:getDevices', aquariumId),
    getDevice: (id: string) => ipcRenderer.invoke('data:getDevice', id),
    createDevice: (device) => ipcRenderer.invoke('data:createDevice', device),
    updateDevice: (id: string, updates) => ipcRenderer.invoke('data:updateDevice', id, updates),
    deleteDevice: (id: string) => ipcRenderer.invoke('data:deleteDevice', id),

    // Water test operations
    getWaterTests: (aquariumId?: string) => ipcRenderer.invoke('data:getWaterTests', aquariumId),
    getWaterTest: (id: string) => ipcRenderer.invoke('data:getWaterTest', id),
    createWaterTest: (waterTest) => ipcRenderer.invoke('data:createWaterTest', waterTest),
    updateWaterTest: (id: string, updates) => ipcRenderer.invoke('data:updateWaterTest', id, updates),
    deleteWaterTest: (id: string) => ipcRenderer.invoke('data:deleteWaterTest', id),

    // Settings operations
    getSettings: () => ipcRenderer.invoke('data:getSettings'),
    updateSettings: (settings) => ipcRenderer.invoke('data:updateSettings', settings),

    // Utility operations
    getDataPath: () => ipcRenderer.invoke('data:getDataPath'),
  },
  files: {
    // File operations
    copyThumbnail: async (file: File) => {
      // Convert File to Buffer for IPC transmission
      const buffer = await file.arrayBuffer();
      return ipcRenderer.invoke('files:copyThumbnail', Buffer.from(buffer), file.name);
    },
    getThumbnailPath: (filename: string) => ipcRenderer.invoke('files:getThumbnailPath', filename),
  },
};

// Expose the API to the renderer process
contextBridge.exposeInMainWorld('electron', electronAPI);
