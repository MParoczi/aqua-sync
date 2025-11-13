import { app, BrowserWindow, ipcMain, protocol, net } from 'electron';
import path from 'node:path';
import started from 'electron-squirrel-startup';
import * as dataService from './services/dataService';
import * as fileService from './services/fileService';
import * as eheimService from './services/eheimDiscoveryService';

// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (started) {
  app.quit();
}

const createWindow = () => {
  // Create the browser window.
  const mainWindow = new BrowserWindow({
    width: 1200,
    height: 800,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
    },
  });

  // and load the index.html of the app.
  if (MAIN_WINDOW_VITE_DEV_SERVER_URL) {
    mainWindow.loadURL(MAIN_WINDOW_VITE_DEV_SERVER_URL);
  } else {
    mainWindow.loadFile(
      path.join(__dirname, `../renderer/${MAIN_WINDOW_VITE_NAME}/index.html`),
    );
  }

  // Open the DevTools.
  mainWindow.webContents.openDevTools();
};

// ============================================
// Custom Protocol Handler for Thumbnails
// ============================================

/**
 * Register custom protocol handler for serving thumbnail images
 * This allows the renderer to load local files using aquasync://thumbnails/{filename}
 * without violating same-origin security policies
 */
app.whenReady().then(() => {
  // Register custom protocol handler
  protocol.handle('aquasync', async (request) => {
    try {
      // Parse URL: aquasync://thumbnails/{filename}
      const url = new URL(request.url);

      // Security: Only allow 'thumbnails' hostname
      if (url.hostname !== 'thumbnails') {
        return new Response('Not found', {
          status: 404,
          headers: { 'content-type': 'text/plain' }
        });
      }

      // Get filename (e.g., "uuid.jpg")
      const filename = url.pathname.slice(1); // Remove leading '/'

      // Security: Prevent directory traversal attacks
      if (!filename || filename.includes('..') || filename.includes('/') || filename.includes('\\')) {
        return new Response('Invalid filename', {
          status: 400,
          headers: { 'content-type': 'text/plain' }
        });
      }

      // Get full path to thumbnail
      const fullPath = path.join(fileService.getThumbnailsPath(), filename);

      // Normalize path for Windows compatibility
      const normalizedPath = path.normalize(fullPath);

      // Return file using net.fetch with file:// protocol
      return net.fetch(`file:///${normalizedPath.replace(/\\/g, '/')}`);
    } catch (err) {
      console.error('Protocol handler error:', err);
      return new Response('Internal server error', {
        status: 500,
        headers: { 'content-type': 'text/plain' }
      });
    }
  });

  // Create the main window
  createWindow();
});

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and their menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  // On OS X it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});

// In this file you can include the rest of your app's specific main process
// code. You can also put them in separate files and import them here.

// ============================================
// IPC Handlers for Data Persistence (US-003)
// ============================================

// Aquarium IPC handlers
ipcMain.handle('data:getAquariums', async () => {
  return await dataService.getAquariums();
});

ipcMain.handle('data:getAquarium', async (_event, id: string) => {
  return await dataService.getAquarium(id);
});

ipcMain.handle('data:createAquarium', async (_event, aquarium) => {
  return await dataService.createAquarium(aquarium);
});

ipcMain.handle('data:updateAquarium', async (_event, id: string, updates) => {
  return await dataService.updateAquarium(id, updates);
});

ipcMain.handle('data:deleteAquarium', async (_event, id: string) => {
  return await dataService.deleteAquarium(id);
});

// Device IPC handlers
ipcMain.handle('data:getDevices', async (_event, aquariumId?: string) => {
  return await dataService.getDevices(aquariumId);
});

ipcMain.handle('data:getDevice', async (_event, id: string) => {
  return await dataService.getDevice(id);
});

ipcMain.handle('data:createDevice', async (_event, device) => {
  return await dataService.createDevice(device);
});

ipcMain.handle('data:updateDevice', async (_event, id: string, updates) => {
  return await dataService.updateDevice(id, updates);
});

ipcMain.handle('data:deleteDevice', async (_event, id: string) => {
  return await dataService.deleteDevice(id);
});

// Water Test IPC handlers
ipcMain.handle('data:getWaterTests', async (_event, aquariumId?: string) => {
  return await dataService.getWaterTests(aquariumId);
});

ipcMain.handle('data:getWaterTest', async (_event, id: string) => {
  return await dataService.getWaterTest(id);
});

ipcMain.handle('data:createWaterTest', async (_event, waterTest) => {
  return await dataService.createWaterTest(waterTest);
});

ipcMain.handle('data:updateWaterTest', async (_event, id: string, updates) => {
  return await dataService.updateWaterTest(id, updates);
});

ipcMain.handle('data:deleteWaterTest', async (_event, id: string) => {
  return await dataService.deleteWaterTest(id);
});

// Settings IPC handlers
ipcMain.handle('data:getSettings', async () => {
  return await dataService.getSettings();
});

ipcMain.handle('data:updateSettings', async (_event, settings) => {
  return await dataService.updateSettings(settings);
});

// Aquarium-specific settings IPC handlers (US-014)
ipcMain.handle('data:getAquariumSettings', async (_event, aquariumId: string) => {
  return await dataService.getAquariumSettings(aquariumId);
});

ipcMain.handle('data:updateAquariumSettings', async (_event, aquariumId: string, settings) => {
  return await dataService.updateAquariumSettings(aquariumId, settings);
});

// Utility IPC handlers
ipcMain.handle('data:getDataPath', async () => {
  return { success: true, data: dataService.getDataPath() };
});

// ============================================
// IPC Handlers for File Operations (US-006)
// ============================================

// File operation handlers
ipcMain.handle('files:copyThumbnail', async (_event, buffer: Buffer, originalName: string) => {
  return await fileService.copyThumbnail(buffer, originalName);
});

ipcMain.handle('files:getThumbnailPath', async (_event, filename: string) => {
  return await fileService.getThumbnailPath(filename);
});

// ============================================
// IPC Handlers for Eheim Integration (US-019)
// ============================================

// Eheim discovery handlers
ipcMain.handle('eheim:discover', async () => {
  return await eheimService.discoverDevices();
});

ipcMain.handle('eheim:connectManual', async (_event, ipAddress: string, port: number) => {
  return await eheimService.connectManual(ipAddress, port);
});

ipcMain.handle('eheim:getDeviceInfo', async (_event, ipAddress: string) => {
  return await eheimService.getDeviceInfo(ipAddress, 80);
});
