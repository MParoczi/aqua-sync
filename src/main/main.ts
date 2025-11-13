import { app, BrowserWindow, ipcMain } from 'electron';
import path from 'node:path';
import started from 'electron-squirrel-startup';
import * as dataService from './services/dataService';
import * as fileService from './services/fileService';

// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (started) {
  app.quit();
}

const createWindow = () => {
  // Create the browser window.
  const mainWindow = new BrowserWindow({
    width: 800,
    height: 600,
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

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', createWindow);

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
