import React, { useState } from 'react';
import { GlassCard } from './common';
import { TestMeasurementModal } from './TestMeasurementModal';
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
 * Tests Content component (US-016, US-017)
 * Displays all water test parameter cards in a responsive grid
 * Opens measurement modal when a parameter is selected
 */
export const TestsContent: React.FC = () => {
  const [selectedParameter, setSelectedParameter] =
    useState<WaterParameterOption | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const handleParameterClick = (parameter: WaterParameterOption) => {
    setSelectedParameter(parameter);
    setIsModalOpen(true);
  };

  const handleModalClose = () => {
    setIsModalOpen(false);
    // Don't clear selectedParameter immediately to avoid UI flash
    setTimeout(() => setSelectedParameter(null), 300);
  };

  const handleMeasurementSaved = () => {
    // Modal will close automatically after save
    // Trigger dashboard refresh (US-018)
    window.dispatchEvent(new Event('refreshWaterTests'));
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

      {/* Measurement Modal (US-017) */}
      <TestMeasurementModal
        isOpen={isModalOpen}
        onClose={handleModalClose}
        parameter={selectedParameter}
        onSaved={handleMeasurementSaved}
      />
    </div>
  );
};
