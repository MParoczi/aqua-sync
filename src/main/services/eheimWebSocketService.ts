/**
 * Eheim WebSocket Service for US-019a
 *
 * This service manages persistent WebSocket connections to Eheim Professional 5e filters.
 * It handles connection lifecycle, auto-reconnection, message parsing, and broadcasting
 * updates to the renderer process.
 */

import WebSocket from 'ws';
import { BrowserWindow } from 'electron';
import type {
  EheimFilterData,
  EheimUserData,
  IpcResult,
} from '../../shared/types';

/**
 * Default Eheim API authentication (api:admin)
 */
const DEFAULT_AUTH_HEADER = 'Basic YXBpOmFkbWlu';

/**
 * Connection timeout in milliseconds (30 seconds)
 * If no messages received within this time, connection is considered stale
 */
const CONNECTION_TIMEOUT = 30000;

/**
 * Initial reconnection delay in milliseconds
 */
const INITIAL_RECONNECT_DELAY = 5000; // 5 seconds

/**
 * Maximum reconnection delay in milliseconds
 */
const MAX_RECONNECT_DELAY = 60000; // 60 seconds

/**
 * Reconnection backoff multiplier
 */
const RECONNECT_BACKOFF = 2;

/**
 * WebSocket message types we handle
 */
type WebSocketMessageType = 'FILTER_DATA' | 'USRDTA' | 'MESH_NETWORK';

/**
 * WebSocket message base structure
 */
interface WebSocketMessage {
  title: WebSocketMessageType;
  [key: string]: unknown;
}

/**
 * Connection state for a single device
 */
interface DeviceConnection {
  deviceId: string;
  ipAddress: string;
  macAddress?: string;
  ws: WebSocket | null;
  reconnectTimer: NodeJS.Timeout | null;
  reconnectDelay: number;
  lastMessageTime: number;
  isReconnecting: boolean;
  shouldReconnect: boolean; // Flag to control reconnection
}

/**
 * Singleton WebSocket manager
 */
class EheimWebSocketManager {
  private connections: Map<string, DeviceConnection> = new Map();
  private isDevelopment = process.env.NODE_ENV === 'development';

  /**
   * Subscribe to a device's WebSocket updates
   * Creates a new WebSocket connection and starts listening for updates
   */
  public subscribe(deviceId: string, ipAddress: string, macAddress?: string): IpcResult<boolean> {
    try {
      // Check if already connected
      if (this.connections.has(deviceId)) {
        console.log(`[Eheim WebSocket] Already subscribed to device ${deviceId}`);
        return { success: true, data: true };
      }

      console.log(`[Eheim WebSocket] Subscribing to device ${deviceId} at ${ipAddress}`);

      // Create connection record
      const connection: DeviceConnection = {
        deviceId,
        ipAddress,
        macAddress,
        ws: null,
        reconnectTimer: null,
        reconnectDelay: INITIAL_RECONNECT_DELAY,
        lastMessageTime: Date.now(),
        isReconnecting: false,
        shouldReconnect: true,
      };

      this.connections.set(deviceId, connection);

      // Start connection
      this.connect(connection);

      return { success: true, data: true };
    } catch (err) {
      const error = err as Error;
      console.error(`[Eheim WebSocket] Failed to subscribe to device ${deviceId}:`, error.message);
      return { success: false, error: error.message };
    }
  }

  /**
   * Unsubscribe from a device's WebSocket updates
   * Closes the connection and prevents reconnection
   */
  public unsubscribe(deviceId: string): IpcResult<boolean> {
    try {
      const connection = this.connections.get(deviceId);

      if (!connection) {
        console.log(`[Eheim WebSocket] Device ${deviceId} not subscribed`);
        return { success: true, data: true };
      }

      console.log(`[Eheim WebSocket] Unsubscribing from device ${deviceId}`);

      // Disable reconnection
      connection.shouldReconnect = false;

      // Clear reconnection timer
      if (connection.reconnectTimer) {
        clearTimeout(connection.reconnectTimer);
        connection.reconnectTimer = null;
      }

      // Close WebSocket
      if (connection.ws) {
        connection.ws.close();
        connection.ws = null;
      }

      // Remove from connections map
      this.connections.delete(deviceId);

      return { success: true, data: true };
    } catch (err) {
      const error = err as Error;
      console.error(`[Eheim WebSocket] Failed to unsubscribe from device ${deviceId}:`, error.message);
      return { success: false, error: error.message };
    }
  }

  /**
   * Unsubscribe from all devices
   * Called when app is quitting
   */
  public unsubscribeAll(): void {
    console.log('[Eheim WebSocket] Unsubscribing from all devices');

    for (const [deviceId] of this.connections) {
      this.unsubscribe(deviceId);
    }
  }

  /**
   * Get connection status for a device
   */
  public getConnectionStatus(deviceId: string): 'connected' | 'connecting' | 'offline' {
    const connection = this.connections.get(deviceId);

    if (!connection) {
      return 'offline';
    }

    if (connection.ws && connection.ws.readyState === WebSocket.OPEN) {
      // Check if we've received messages recently
      const timeSinceLastMessage = Date.now() - connection.lastMessageTime;
      if (timeSinceLastMessage > CONNECTION_TIMEOUT) {
        return 'offline';
      }
      return 'connected';
    }

    if (connection.isReconnecting) {
      return 'connecting';
    }

    return 'offline';
  }

  /**
   * Connect to a device via WebSocket
   */
  private connect(connection: DeviceConnection): void {
    try {
      const wsUrl = `ws://${connection.ipAddress}/ws`;
      console.log(`[Eheim WebSocket] Connecting to ${wsUrl} for device ${connection.deviceId}`);

      connection.isReconnecting = true;
      connection.ws = new WebSocket(wsUrl);

      // Connection opened
      connection.ws.on('open', () => {
        console.log(`[Eheim WebSocket] Connected to device ${connection.deviceId}`);
        connection.isReconnecting = false;
        connection.reconnectDelay = INITIAL_RECONNECT_DELAY; // Reset backoff
        connection.lastMessageTime = Date.now();

        // Send initial command to get filter data
        this.sendCommand(connection, {
          title: 'GET_FILTER_DATA',
          from: 'USER',
          to: 'MASTER',
        });

        // Broadcast connection status to renderer
        this.broadcastStatusUpdate(connection.deviceId, {
          type: 'connection',
          status: 'connected',
        });
      });

      // Message received
      connection.ws.on('message', (data: WebSocket.Data) => {
        connection.lastMessageTime = Date.now();

        try {
          const message = JSON.parse(data.toString()) as WebSocketMessage;

          if (this.isDevelopment) {
            console.log(`[Eheim WebSocket] Message from ${connection.deviceId}:`, message);
          }

          // Parse and broadcast message based on type
          this.handleMessage(connection, message);
        } catch (err) {
          console.error(`[Eheim WebSocket] Failed to parse message from ${connection.deviceId}:`, err);
        }
      });

      // Connection closed
      connection.ws.on('close', (code, reason) => {
        console.log(`[Eheim WebSocket] Disconnected from device ${connection.deviceId}. Code: ${code}, Reason: ${reason || 'N/A'}`);
        connection.ws = null;
        connection.isReconnecting = false;

        // Broadcast offline status to renderer
        this.broadcastStatusUpdate(connection.deviceId, {
          type: 'connection',
          status: 'offline',
        });

        // Attempt reconnection if enabled
        if (connection.shouldReconnect) {
          this.scheduleReconnect(connection);
        }
      });

      // Connection error
      connection.ws.on('error', (error) => {
        console.error(`[Eheim WebSocket] Error on device ${connection.deviceId}:`, error.message);

        // Broadcast error status to renderer
        this.broadcastStatusUpdate(connection.deviceId, {
          type: 'error',
          status: 'error',
          error: error.message,
        });
      });
    } catch (err) {
      console.error(`[Eheim WebSocket] Failed to create WebSocket for device ${connection.deviceId}:`, err);
      connection.isReconnecting = false;

      // Schedule reconnection
      if (connection.shouldReconnect) {
        this.scheduleReconnect(connection);
      }
    }
  }

  /**
   * Schedule a reconnection attempt with exponential backoff
   */
  private scheduleReconnect(connection: DeviceConnection): void {
    // Clear any existing reconnect timer
    if (connection.reconnectTimer) {
      clearTimeout(connection.reconnectTimer);
    }

    console.log(`[Eheim WebSocket] Scheduling reconnect for device ${connection.deviceId} in ${connection.reconnectDelay}ms`);

    connection.reconnectTimer = setTimeout(() => {
      console.log(`[Eheim WebSocket] Attempting reconnect for device ${connection.deviceId}`);
      this.connect(connection);

      // Increase delay for next attempt (exponential backoff)
      connection.reconnectDelay = Math.min(
        connection.reconnectDelay * RECONNECT_BACKOFF,
        MAX_RECONNECT_DELAY
      );
    }, connection.reconnectDelay);
  }

  /**
   * Send a command to a device via WebSocket
   */
  private sendCommand(connection: DeviceConnection, command: Record<string, unknown>): void {
    if (connection.ws && connection.ws.readyState === WebSocket.OPEN) {
      connection.ws.send(JSON.stringify(command));

      if (this.isDevelopment) {
        console.log(`[Eheim WebSocket] Sent command to ${connection.deviceId}:`, command);
      }
    } else {
      console.warn(`[Eheim WebSocket] Cannot send command to ${connection.deviceId}: WebSocket not open`);
    }
  }

  /**
   * Handle incoming WebSocket messages
   */
  private handleMessage(connection: DeviceConnection, message: WebSocketMessage): void {
    switch (message.title) {
      case 'FILTER_DATA':
        this.handleFilterData(connection, message as unknown as EheimFilterData);
        break;

      case 'USRDTA':
        this.handleUserData(connection, message as unknown as EheimUserData);
        break;

      case 'MESH_NETWORK':
        this.handleMeshNetwork(connection, message);
        break;

      default:
        if (this.isDevelopment) {
          console.log(`[Eheim WebSocket] Unhandled message type from ${connection.deviceId}:`, message.title);
        }
    }
  }

  /**
   * Handle FILTER_DATA message
   * Contains real-time filter status (power, mode, frequency, flow)
   */
  private handleFilterData(connection: DeviceConnection, data: EheimFilterData): void {
    // Broadcast to renderer
    this.broadcastStatusUpdate(connection.deviceId, {
      type: 'filter_data',
      data,
    });
  }

  /**
   * Handle USRDTA message
   * Contains device info (MAC address, firmware version)
   */
  private handleUserData(connection: DeviceConnection, data: EheimUserData): void {
    // Update connection with MAC address if not set
    if (data.macAddress && !connection.macAddress) {
      connection.macAddress = data.macAddress;
    }

    // Broadcast to renderer
    this.broadcastStatusUpdate(connection.deviceId, {
      type: 'user_data',
      data,
    });
  }

  /**
   * Handle MESH_NETWORK message
   * Contains device topology for multi-device setups
   */
  private handleMeshNetwork(connection: DeviceConnection, data: WebSocketMessage): void {
    // Broadcast to renderer
    this.broadcastStatusUpdate(connection.deviceId, {
      type: 'mesh_network',
      data,
    });
  }

  /**
   * Broadcast status update to all renderer windows
   */
  private broadcastStatusUpdate(deviceId: string, payload: Record<string, unknown>): void {
    const windows = BrowserWindow.getAllWindows();

    for (const window of windows) {
      window.webContents.send('eheim:status-update', {
        deviceId,
        timestamp: new Date().toISOString(),
        ...payload,
      });
    }
  }
}

// Export singleton instance
export const websocketManager = new EheimWebSocketManager();

// Export functions for IPC handlers
export function subscribe(deviceId: string, ipAddress: string, macAddress?: string): IpcResult<boolean> {
  return websocketManager.subscribe(deviceId, ipAddress, macAddress);
}

export function unsubscribe(deviceId: string): IpcResult<boolean> {
  return websocketManager.unsubscribe(deviceId);
}

export function unsubscribeAll(): void {
  websocketManager.unsubscribeAll();
}

export function getConnectionStatus(deviceId: string): 'connected' | 'connecting' | 'offline' {
  return websocketManager.getConnectionStatus(deviceId);
}
