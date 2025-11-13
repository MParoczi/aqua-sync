import React, { useEffect, useState } from 'react';
import { Device } from '../../shared/types';
import { GlassCard } from './common/GlassCard';
import { DeviceCard } from './DeviceCard';

interface ConnectedDevicesProps {
  aquariumId: string;
}

export const ConnectedDevices: React.FC<ConnectedDevicesProps> = ({ aquariumId }) => {
  const [devices, setDevices] = useState<Device[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadDevices();
  }, [aquariumId]);

  const loadDevices = async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await window.electron.data.getDevices(aquariumId);

      if (result.success && result.data) {
        setDevices(result.data);
      } else {
        setError(result.error || 'Failed to load devices');
      }
    } catch (err) {
      setError('An error occurred while loading devices');
      console.error('Error loading devices:', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <GlassCard className="p-6">
        <h3 className="text-xl font-bold text-white mb-4">Connected Devices</h3>
        <div className="flex items-center justify-center py-8">
          <div className="text-white/70">Loading devices...</div>
        </div>
      </GlassCard>
    );
  }

  if (error) {
    return (
      <GlassCard className="p-6">
        <h3 className="text-xl font-bold text-white mb-4">Connected Devices</h3>
        <div className="flex items-center justify-center py-8">
          <div className="text-red-400">{error}</div>
        </div>
      </GlassCard>
    );
  }

  return (
    <div>
      <h3 className="text-xl font-bold text-white mb-4">Connected Devices</h3>

      {devices.length === 0 ? (
        <GlassCard className="p-6">
          <div className="flex items-center justify-center py-8">
            <p className="text-white/70 text-center">
              Your connected devices will appear here
            </p>
          </div>
        </GlassCard>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {devices.map((device) => (
            <DeviceCard key={device.id} device={device} />
          ))}
        </div>
      )}
    </div>
  );
};
