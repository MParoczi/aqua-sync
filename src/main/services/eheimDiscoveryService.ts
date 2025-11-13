/**
 * Eheim Discovery Service for US-019
 *
 * This service manages mDNS discovery of Eheim Professional 5e filters on the local network.
 * Uses bonjour-service for mDNS/DNS-SD discovery and axios for HTTP API calls.
 */

import Bonjour from 'bonjour-service';
import axios, { AxiosError } from 'axios';
import type {
  DiscoveredEheimDevice,
  EheimUserData,
  EheimFilterData,
  IpcResult,
} from '../../shared/types';

/**
 * Default Eheim API authentication (api:admin)
 * BASE64 encoded as YXBpOmFkbWlu
 */
const DEFAULT_AUTH_HEADER = 'Basic YXBpOmFkbWlu';

/**
 * Discovery timeout in milliseconds (10 seconds per US-019)
 */
const DISCOVERY_TIMEOUT = 10000;

/**
 * HTTP request timeout in milliseconds
 */
const HTTP_TIMEOUT = 5000;

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

/**
 * Discover Eheim devices on the local network via mDNS
 * Returns array of discovered devices with IP addresses
 */
export async function discoverDevices(): Promise<IpcResult<DiscoveredEheimDevice[]>> {
  return new Promise((resolve) => {
    const bonjour = new Bonjour();
    const discoveredDevices: DiscoveredEheimDevice[] = [];
    const seenIPs = new Set<string>();

    console.log('[Eheim Discovery] Starting mDNS scan for Eheim devices...');

    // Browse for HTTP services
    const browser = bonjour.find({ type: 'http' });

    browser.on('up', async (service) => {
      // Filter for Eheim devices
      const serviceName = service.name?.toLowerCase() || '';
      const hostname = service.host?.toLowerCase() || '';

      if (serviceName.includes('eheim') || hostname.includes('eheim')) {
        console.log('[Eheim Discovery] Found potential Eheim device:', {
          name: service.name,
          host: service.host,
          addresses: service.addresses,
          port: service.port,
        });

        // Get first valid IPv4 address
        const ipAddress = service.addresses?.find(
          (addr) => addr.includes('.') && !addr.startsWith('169.254')
        );

        if (ipAddress && !seenIPs.has(ipAddress)) {
          seenIPs.add(ipAddress);

          const device: DiscoveredEheimDevice = {
            hostname: service.host || 'eheimdigital.local',
            ipAddress,
            port: service.port || 80,
          };

          // Try to get device info (MAC, firmware, model)
          try {
            const deviceInfo = await getDeviceInfo(ipAddress, device.port);
            if (deviceInfo.success && deviceInfo.data) {
              device.macAddress = deviceInfo.data.macAddress;
              device.firmwareVersion = `S${deviceInfo.data.revision[0]}`;
            }
          } catch (err) {
            console.warn('[Eheim Discovery] Could not fetch device info:', err);
          }

          discoveredDevices.push(device);
          console.log('[Eheim Discovery] Added device:', device);
        }
      }
    });

    // Stop discovery after timeout
    setTimeout(() => {
      browser.stop();
      bonjour.destroy();

      console.log(`[Eheim Discovery] Scan complete. Found ${discoveredDevices.length} device(s).`);
      resolve(success(discoveredDevices));
    }, DISCOVERY_TIMEOUT);
  });
}

/**
 * Connect to a device manually using IP address and port
 * Tests connectivity and retrieves device information
 */
export async function connectManual(
  ipAddress: string,
  port = 80
): Promise<IpcResult<DiscoveredEheimDevice>> {
  console.log(`[Eheim Discovery] Manual connection attempt: ${ipAddress}:${port}`);

  // Validate IP address format (basic IPv4 check)
  const ipRegex = /^(\d{1,3}\.){3}\d{1,3}$/;
  if (!ipRegex.test(ipAddress)) {
    return error('Invalid IP address format. Please enter a valid IPv4 address.');
  }

  // Validate private IP range (security)
  const octets = ipAddress.split('.').map(Number);
  const isPrivate =
    octets[0] === 10 ||
    (octets[0] === 172 && octets[1] >= 16 && octets[1] <= 31) ||
    (octets[0] === 192 && octets[1] === 168);

  if (!isPrivate) {
    return error('IP address must be in a private network range (10.x.x.x, 172.16-31.x.x, or 192.168.x.x).');
  }

  // Test connection
  try {
    const deviceInfo = await getDeviceInfo(ipAddress, port);

    if (!deviceInfo.success || !deviceInfo.data) {
      return error(deviceInfo.error || 'Could not connect to device');
    }

    const device: DiscoveredEheimDevice = {
      hostname: 'eheimdigital.local',
      ipAddress,
      port,
      macAddress: deviceInfo.data.macAddress,
      firmwareVersion: `S${deviceInfo.data.revision[0]}`,
    };

    console.log('[Eheim Discovery] Manual connection successful:', device);
    return success(device);
  } catch (err) {
    const axiosError = err as AxiosError;
    if (axiosError.code === 'ECONNREFUSED') {
      return error('Connection refused. Check IP address and ensure filter is powered on.');
    } else if (axiosError.code === 'ETIMEDOUT') {
      return error('Connection timed out. Ensure filter is on the same WiFi network.');
    } else if (axiosError.response?.status === 404) {
      return error('Device not found. This may not be an Eheim device.');
    } else if (axiosError.response?.status === 401 || axiosError.response?.status === 403) {
      return error('Authentication failed. Credentials may have been changed.');
    } else {
      return error(`Connection failed: ${axiosError.message || 'Unknown error'}`);
    }
  }
}

/**
 * Get device information via WebSocket and REST API
 * Retrieves MAC address, firmware version, and model
 */
export async function getDeviceInfo(
  ipAddress: string,
  port = 80
): Promise<IpcResult<EheimUserData>> {
  try {
    console.log(`[Eheim Discovery] Fetching device info from ${ipAddress}:${port}`);

    // For now, use REST API to test connectivity
    // In US-019a, we'll implement WebSocket for USRDTA messages
    const response = await axios.get(`http://${ipAddress}:${port}/api/filter`, {
      headers: {
        Authorization: DEFAULT_AUTH_HEADER,
      },
      timeout: HTTP_TIMEOUT,
    });

    console.log('[Eheim Discovery] API response:', response.data);

    // The REST API returns filter data, but we need USRDTA for MAC and firmware
    // For US-019, we'll simulate this data or extract from response
    // TODO: Implement proper WebSocket connection in US-019a to get USRDTA

    // Extract MAC address from response (from 'from' field or similar)
    const filterData = response.data as EheimFilterData;
    const macAddress = filterData.from || 'UNKNOWN';

    // For now, return a simulated USRDTA response
    // This will be properly implemented in US-019a with WebSocket
    const userData: EheimUserData = {
      title: 'USRDTA',
      macAddress,
      revision: [2037, 1025], // Placeholder - will be from WebSocket
      latestAvailableRevision: [2037, 1025],
      firmwareAvailable: 0,
    };

    return success(userData);
  } catch (err) {
    const axiosError = err as AxiosError;
    console.error('[Eheim Discovery] Failed to fetch device info:', axiosError.message);

    if (axiosError.code === 'ECONNREFUSED') {
      return error('Connection refused');
    } else if (axiosError.code === 'ETIMEDOUT') {
      return error('Connection timed out');
    } else if (axiosError.response?.status === 404) {
      return error('Device not found');
    } else if (axiosError.response?.status === 401 || axiosError.response?.status === 403) {
      return error('Authentication failed');
    } else {
      return error(axiosError.message || 'Unknown error');
    }
  }
}

/**
 * Test if a device is an Eheim filter by checking API endpoint
 */
export async function testConnection(ipAddress: string, port = 80): Promise<boolean> {
  try {
    const response = await axios.get(`http://${ipAddress}:${port}/api/filter`, {
      headers: {
        Authorization: DEFAULT_AUTH_HEADER,
      },
      timeout: HTTP_TIMEOUT,
      validateStatus: (status) => status === 200 || status === 401, // 401 means it's there but auth might be different
    });

    return response.status === 200 || response.status === 401;
  } catch {
    return false;
  }
}
