/**
 * TypeScript declarations for window.electron API
 * This file extends the Window interface to include the electron API
 * exposed via the preload script
 */

import type { ElectronAPI } from '../shared/types';

declare global {
  interface Window {
    electron: ElectronAPI;
  }
}

export {};
