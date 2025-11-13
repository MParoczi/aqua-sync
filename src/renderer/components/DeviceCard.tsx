import React from 'react';
import { Device, FilterDevice, LampDevice, DeviceStatus } from '../../shared/types';
import { GlassCard } from './common/GlassCard';

interface DeviceCardProps {
  device: Device;
}

const getStatusDisplay = (status: DeviceStatus): { text: string; colorClasses: string } => {
  switch (status) {
    case 'connecting':
      return {
        text: 'Connecting',
        colorClasses: 'text-yellow-400 bg-yellow-400/20',
      };
    case 'connected':
      return {
        text: 'Connected',
        colorClasses: 'text-green-400 bg-green-400/20',
      };
    case 'offline':
      return {
        text: 'Offline',
        colorClasses: 'text-red-400 bg-red-400/20',
      };
    case 'error':
      return {
        text: 'Connection Failed',
        colorClasses: 'text-red-500 bg-red-500/20',
      };
    default:
      return {
        text: 'Unknown',
        colorClasses: 'text-gray-400 bg-gray-400/20',
      };
  }
};

const getDeviceTypeDisplay = (type: string): string => {
  return type.charAt(0).toUpperCase() + type.slice(1);
};

const getActiveMode = (device: Device): string | null => {
  if (device.status !== 'connected') {
    return null;
  }

  if (device.type === 'filter') {
    const filterDevice = device as FilterDevice;
    if (filterDevice.flowRate) {
      return `Flow: ${filterDevice.flowRate} L/h`;
    }
    return 'Running';
  }

  if (device.type === 'lamp') {
    const lampDevice = device as LampDevice;
    if (lampDevice.schedule?.enabled) {
      return `Schedule: ${lampDevice.schedule.onTime} - ${lampDevice.schedule.offTime}`;
    }
    if (lampDevice.brightness !== undefined) {
      return `Brightness: ${lampDevice.brightness}%`;
    }
    return 'ON';
  }

  return null;
};

export const DeviceCard: React.FC<DeviceCardProps> = ({ device }) => {
  const status = getStatusDisplay(device.status);
  const deviceType = getDeviceTypeDisplay(device.type);
  const activeMode = getActiveMode(device);

  return (
    <GlassCard className="p-4 hover:scale-[1.02] transition-transform duration-200">
      <div className="flex flex-col gap-3">
        {/* Device Name */}
        <h4 className="text-lg font-semibold text-white truncate">
          {device.name}
        </h4>

        {/* Device Type */}
        <div className="flex items-center gap-2">
          <span className="text-sm text-white/60">Type:</span>
          <span className="text-sm text-white font-medium">{deviceType}</span>
        </div>

        {/* Status Badge */}
        <div className="flex items-center gap-2">
          <span className="text-sm text-white/60">Status:</span>
          <span
            className={`text-xs font-semibold px-2.5 py-1 rounded-full ${status.colorClasses}`}
          >
            {status.text}
          </span>
        </div>

        {/* Active Mode (only if connected) */}
        {activeMode && (
          <div className="flex items-center gap-2 pt-2 border-t border-white/10">
            <span className="text-sm text-white/60">Mode:</span>
            <span className="text-sm text-green-400 font-medium">{activeMode}</span>
          </div>
        )}

        {/* Manufacturer & Model */}
        <div className="text-xs text-white/40 mt-1">
          {device.manufacturer} {device.model}
        </div>
      </div>
    </GlassCard>
  );
};
