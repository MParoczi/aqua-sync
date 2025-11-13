import React from 'react';
import { GlassCard } from './common';
import type { Aquarium } from '../../shared/types';

interface DashboardContentProps {
  aquarium: Aquarium;
}

/**
 * Format date to YYYY.MM.DD format
 */
const formatDate = (isoDate: string): string => {
  const date = new Date(isoDate);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}.${month}.${day}`;
};

/**
 * Calculate days since start date
 */
const calculateDaysSinceStart = (startDate: string): number => {
  const start = new Date(startDate);
  const today = new Date();
  const diffTime = Math.abs(today.getTime() - start.getTime());
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  return diffDays;
};

/**
 * Format dimensions with units
 */
const formatDimensions = (dimensions: Aquarium['dimensions']): string => {
  const { width, length, height, unit } = dimensions;
  return `${width} × ${length} × ${height} ${unit}`;
};

/**
 * Format volume with units
 */
const formatVolume = (volume: Aquarium['volume']): string => {
  const { value, unit } = volume;
  const unitDisplay = unit === 'liter' ? 'L' : 'gal';
  return `${value} ${unitDisplay}`;
};

/**
 * Format aquarium type with proper capitalization
 */
const formatType = (type: Aquarium['type']): string => {
  return type === 'freshwater' ? 'Freshwater' : 'Marine';
};

export const DashboardContent: React.FC<DashboardContentProps> = ({ aquarium }) => {
  const daysSinceStart = calculateDaysSinceStart(aquarium.startDate);
  const formattedStartDate = formatDate(aquarium.startDate);

  return (
    <div className="space-y-6">
      {/* Dashboard Title */}
      <div>
        <h2 className="text-3xl font-bold text-white mb-2">Dashboard</h2>
        <p className="text-white/60">Overview of your aquarium</p>
      </div>

      {/* Aquarium Information Card */}
      <GlassCard className="p-6">
        <div className="flex gap-6">
          {/* Left side - Aquarium Information */}
          <div className="flex-1 space-y-4">
            <div>
              <h3 className="text-2xl font-bold text-white mb-4">{aquarium.name}</h3>
            </div>

            <div className="grid grid-cols-2 gap-4">
              {/* Type */}
              <div>
                <p className="text-white/60 text-sm mb-1">Type</p>
                <p className="text-white font-medium">{formatType(aquarium.type)}</p>
              </div>

              {/* Volume */}
              <div>
                <p className="text-white/60 text-sm mb-1">Volume</p>
                <p className="text-white font-medium">{formatVolume(aquarium.volume)}</p>
              </div>

              {/* Dimensions */}
              <div>
                <p className="text-white/60 text-sm mb-1">Dimensions</p>
                <p className="text-white font-medium">{formatDimensions(aquarium.dimensions)}</p>
              </div>

              {/* Start Date */}
              <div>
                <p className="text-white/60 text-sm mb-1">Start Date</p>
                <p className="text-white font-medium">{formattedStartDate}</p>
              </div>

              {/* Days Since Start */}
              <div className="col-span-2">
                <p className="text-white/60 text-sm mb-1">Days Running</p>
                <p className="text-white font-medium text-lg">
                  {daysSinceStart} {daysSinceStart === 1 ? 'day' : 'days'}
                </p>
              </div>
            </div>
          </div>

          {/* Right side - Thumbnail */}
          <div className="w-48 h-48 flex-shrink-0">
            {aquarium.thumbnailPath ? (
              <img
                src={aquarium.thumbnailPath}
                alt={aquarium.name}
                className="w-full h-full object-cover rounded-lg border border-white/20"
              />
            ) : (
              <div className="w-full h-full rounded-lg border border-white/20 bg-white/5 flex items-center justify-center">
                <svg
                  className="w-16 h-16 text-white/40"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={1.5}
                    d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                  />
                </svg>
              </div>
            )}
          </div>
        </div>
      </GlassCard>

      {/* Placeholder sections for future user stories */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <GlassCard className="p-6">
          <h3 className="text-xl font-bold text-white mb-4">Connected Devices</h3>
          <p className="text-white/70">
            Device status cards will be implemented in US-013
          </p>
        </GlassCard>

        <GlassCard className="p-6">
          <h3 className="text-xl font-bold text-white mb-4">Quick Stats</h3>
          <p className="text-white/70">
            Statistics and quick actions will be added in future user stories
          </p>
        </GlassCard>
      </div>

      {/* Water Parameters Section Placeholder */}
      <GlassCard className="p-6">
        <h3 className="text-xl font-bold text-white mb-4">Water Parameters</h3>
        <p className="text-white/70">
          Water parameter graphs will be implemented in US-014 and US-015
        </p>
      </GlassCard>
    </div>
  );
};
