import React, { useState } from 'react';
import { GlassCard } from './common';
import {
  ALL_PARAMETERS,
  PARAMETER_COLORS,
  PARAMETER_DISPLAY_NAMES,
} from '../../shared/waterParameters';
import type { WaterParameterOption } from '../../shared/types';

/**
 * Water parameter card component (US-016)
 * Displays a single water parameter with its color and name
 */
interface ParameterCardProps {
  parameter: WaterParameterOption;
  onClick: (parameter: WaterParameterOption) => void;
}

const ParameterCard: React.FC<ParameterCardProps> = ({
  parameter,
  onClick,
}) => {
  const color = PARAMETER_COLORS[parameter];
  const displayName = PARAMETER_DISPLAY_NAMES[parameter];

  return (
    <GlassCard
      className="p-6 cursor-pointer transition-all duration-300 hover:scale-105 hover:shadow-lg group"
      onClick={() => onClick(parameter)}
    >
      {/* Color indicator bar at top */}
      <div
        className="h-1.5 w-full rounded-full mb-4 transition-all duration-300 group-hover:h-2"
        style={{ backgroundColor: color }}
      />

      {/* Parameter name */}
      <h3
        className="text-xl font-semibold text-center transition-all duration-300"
        style={{ color }}
      >
        {displayName}
      </h3>

      {/* Hover effect: subtle glow */}
      <div
        className="absolute inset-0 rounded-lg opacity-0 group-hover:opacity-20 transition-opacity duration-300 pointer-events-none"
        style={{
          background: `radial-gradient(circle at center, ${color}, transparent)`,
        }}
      />
    </GlassCard>
  );
};

/**
 * Tests Content component (US-016)
 * Displays all water test parameter cards in a responsive grid
 */
export const TestsContent: React.FC = () => {
  const [selectedParameter, setSelectedParameter] =
    useState<WaterParameterOption | null>(null);

  const handleParameterClick = (parameter: WaterParameterOption) => {
    setSelectedParameter(parameter);
    // TODO (US-017): Open measurement modal
    console.log('Parameter clicked:', parameter);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-3xl font-bold text-white mb-2">Water Tests</h2>
        <p className="text-white/60">
          Select a parameter to record a new measurement
        </p>
      </div>

      {/* Parameter cards grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
        {ALL_PARAMETERS.map((parameter) => (
          <ParameterCard
            key={parameter}
            parameter={parameter}
            onClick={handleParameterClick}
          />
        ))}
      </div>

      {/* Debug info (will be removed in US-017) */}
      {selectedParameter && (
        <GlassCard className="p-4">
          <p className="text-white/70 text-sm">
            Selected parameter: <strong>{selectedParameter}</strong> (Modal
            will open here in US-017)
          </p>
        </GlassCard>
      )}
    </div>
  );
};
