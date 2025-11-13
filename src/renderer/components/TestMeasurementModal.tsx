import React, { useState, useEffect, useMemo } from 'react';
import { GlassModal, GlassButton } from './common';
import { useToast } from '../contexts/ToastContext';
import { useAquarium } from '../contexts/AquariumContext';
import type { WaterParameterOption, WaterParameterType, WaterTest } from '../../shared/types';
import {
  PARAMETER_COLORS,
  PARAMETER_DISPLAY_NAMES,
  PARAMETER_UNITS,
  PARAMETER_RANGES,
} from '../../shared/waterParameters';

/**
 * Map WaterParameterOption to WaterParameterType
 * This is needed because the UI uses display names (e.g., 'pH', 'Temperature')
 * but the data storage uses type names (e.g., 'ph', 'temperature')
 */
const mapParameterOptionToType = (option: WaterParameterOption): WaterParameterType => {
  const mapping: Record<WaterParameterOption, WaterParameterType> = {
    'pH': 'ph',
    'GH': 'hardness',
    'KH': 'alkalinity',
    'NO₂': 'nitrite',
    'NO₃': 'nitrate',
    'NH₄': 'ammonia',
    'Fe': 'hardness', // Iron - using hardness as placeholder (not in original enum)
    'Cu': 'hardness', // Copper - using hardness as placeholder (not in original enum)
    'SiO₂': 'hardness', // Silica - using hardness as placeholder (not in original enum)
    'PO₄': 'phosphate',
    'CO₂': 'hardness', // CO2 - using hardness as placeholder (not in original enum)
    'O₂': 'hardness', // O2 - using hardness as placeholder (not in original enum)
    'Temperature': 'temperature',
  };
  return mapping[option];
};

/**
 * Check if a WaterParameterType matches a WaterParameterOption
 */
const isParameterMatch = (type: WaterParameterType, option: WaterParameterOption): boolean => {
  const typeStr = type.toLowerCase();
  const optionStr = option.toLowerCase();

  // Direct matches
  if (typeStr === optionStr) return true;

  // Special case mappings
  if (option === 'pH' && typeStr === 'ph') return true;
  if (option === 'Temperature' && typeStr === 'temperature') return true;
  if (option === 'GH' && typeStr === 'hardness') return true;
  if (option === 'KH' && typeStr === 'alkalinity') return true;
  if (option === 'NO₂' && typeStr === 'nitrite') return true;
  if (option === 'NO₃' && typeStr === 'nitrate') return true;
  if (option === 'NH₄' && typeStr === 'ammonia') return true;
  if (option === 'PO₄' && typeStr === 'phosphate') return true;

  return false;
};

interface TestMeasurementModalProps {
  isOpen: boolean;
  onClose: () => void;
  parameter: WaterParameterOption | null;
  onSaved?: () => void;
}

interface FormData {
  value: string;
  measuredAt: string; // ISO datetime-local format (YYYY-MM-DDTHH:mm)
}

interface FormErrors {
  value?: string;
  measuredAt?: string;
}

/**
 * Water Test Measurement Modal - US-017
 * Allows users to record water test measurements for a specific parameter
 */
export const TestMeasurementModal: React.FC<TestMeasurementModalProps> = ({
  isOpen,
  onClose,
  parameter,
  onSaved,
}) => {
  const { selectedAquarium } = useAquarium();
  const { showSuccess, showError } = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [historicalTests, setHistoricalTests] = useState<WaterTest[]>([]);
  const [formData, setFormData] = useState<FormData>({
    value: '',
    measuredAt: new Date().toISOString().slice(0, 16), // Format: YYYY-MM-DDTHH:mm
  });
  const [errors, setErrors] = useState<FormErrors>({});

  // Get parameter-specific data
  const parameterColor = parameter ? PARAMETER_COLORS[parameter] : '#fff';
  const parameterName = parameter ? PARAMETER_DISPLAY_NAMES[parameter] : '';
  const parameterUnit = parameter ? PARAMETER_UNITS[parameter] : '';
  const parameterRange = parameter ? PARAMETER_RANGES[parameter] : null;

  // Reset form when modal opens
  useEffect(() => {
    if (isOpen && parameter) {
      setFormData({
        value: '',
        measuredAt: new Date().toISOString().slice(0, 16),
      });
      setErrors({});
      loadHistoricalData();
    }
  }, [isOpen, parameter]);

  /**
   * Load historical measurements for the selected parameter
   */
  const loadHistoricalData = async () => {
    if (!selectedAquarium || !parameter) return;

    try {
      const result = await window.electron.data.getWaterTests(selectedAquarium.id);
      if (result.success && result.data) {
        // Filter tests that have the current parameter
        const filteredTests = result.data
          .filter((test) =>
            test.parameters.some((p) => isParameterMatch(p.type, parameter))
          )
          .map((test) => ({
            ...test,
            // Find the specific parameter value
            parameterValue: test.parameters.find((p) => isParameterMatch(p.type, parameter)),
          }))
          .sort((a, b) => {
            // Sort by testDate descending (newest first)
            return new Date(b.testDate).getTime() - new Date(a.testDate).getTime();
          })
          .slice(0, 10); // Show last 10 measurements

        setHistoricalTests(filteredTests);
      }
    } catch (err) {
      console.error('Failed to load historical tests:', err);
    }
  };

  /**
   * Validate form inputs
   */
  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    // Validate value
    if (!formData.value.trim()) {
      newErrors.value = 'Value is required';
    } else {
      const numValue = parseFloat(formData.value);
      if (isNaN(numValue)) {
        newErrors.value = 'Value must be a number';
      } else if (parameterRange) {
        if (numValue < parameterRange.min) {
          newErrors.value = `Value must be at least ${parameterRange.min}`;
        } else if (numValue > parameterRange.max) {
          newErrors.value = `Value must be at most ${parameterRange.max}`;
        }
      }
    }

    // Validate date
    if (!formData.measuredAt) {
      newErrors.measuredAt = 'Date and time are required';
    } else {
      const measuredDate = new Date(formData.measuredAt);
      const now = new Date();
      if (measuredDate > now) {
        newErrors.measuredAt = 'Date cannot be in the future';
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  /**
   * Handle form submission
   */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm() || !selectedAquarium || !parameter) {
      return;
    }

    setIsSubmitting(true);

    try {
      const numValue = parseFloat(formData.value);
      const measuredDate = new Date(formData.measuredAt);

      // Create water test record
      const waterTestData = {
        aquariumId: selectedAquarium.id,
        testDate: measuredDate.toISOString(),
        parameters: [
          {
            type: mapParameterOptionToType(parameter),
            value: numValue,
            unit: parameterUnit,
          },
        ],
      };

      const result = await window.electron.data.createWaterTest(waterTestData);

      if (result.success) {
        showSuccess(`${parameterName} measurement saved successfully`);
        onSaved?.();
        onClose();
      } else {
        showError(result.error || 'Failed to save measurement');
      }
    } catch (err) {
      console.error('Error saving water test:', err);
      showError('An error occurred while saving the measurement');
    } finally {
      setIsSubmitting(false);
    }
  };

  /**
   * Handle input changes
   */
  const handleInputChange = (field: keyof FormData, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    // Clear error when user starts typing
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  /**
   * Format date for display
   */
  const formatDateTime = (isoString: string): string => {
    const date = new Date(isoString);
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (!parameter) return null;

  return (
    <GlassModal isOpen={isOpen} onClose={onClose} className="max-w-3xl">
      <div className="space-y-6">
        {/* Header with parameter name and colored indicator */}
        <div className="flex items-center gap-4">
          <div
            className="w-2 h-12 rounded-full"
            style={{ backgroundColor: parameterColor }}
          />
          <div>
            <h2
              className="text-3xl font-bold"
              style={{ color: parameterColor }}
            >
              {parameterName}
            </h2>
            <p className="text-white/60 text-sm">
              Record a new measurement
            </p>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Value Input */}
          <div>
            <label className="block text-white font-medium mb-2">
              Value {parameterUnit && `(${parameterUnit})`}
            </label>
            <div className="flex gap-2 items-center">
              <input
                type="number"
                value={formData.value}
                onChange={(e) => handleInputChange('value', e.target.value)}
                step={parameterRange?.step || 0.01}
                min={parameterRange?.min}
                max={parameterRange?.max}
                className={`flex-1 px-4 py-3 bg-white/10 border ${
                  errors.value ? 'border-red-500' : 'border-white/20'
                } rounded-lg text-white placeholder-white/40 focus:outline-none focus:border-white/40 transition-colors`}
                placeholder={`Enter ${parameterName.toLowerCase()} value`}
                disabled={isSubmitting}
              />
              {parameterUnit && (
                <span className="text-white/60 font-medium min-w-[60px]">
                  {parameterUnit}
                </span>
              )}
            </div>
            {errors.value && (
              <p className="text-red-400 text-sm mt-1">{errors.value}</p>
            )}
            {parameterRange && !errors.value && (
              <p className="text-white/40 text-xs mt-1">
                Valid range: {parameterRange.min} - {parameterRange.max}
              </p>
            )}
          </div>

          {/* Date/Time Picker */}
          <div>
            <label className="block text-white font-medium mb-2">
              Date & Time
            </label>
            <input
              type="datetime-local"
              value={formData.measuredAt}
              onChange={(e) => handleInputChange('measuredAt', e.target.value)}
              max={new Date().toISOString().slice(0, 16)}
              className={`w-full px-4 py-3 bg-white/10 border ${
                errors.measuredAt ? 'border-red-500' : 'border-white/20'
              } rounded-lg text-white placeholder-white/40 focus:outline-none focus:border-white/40 transition-colors`}
              disabled={isSubmitting}
            />
            {errors.measuredAt && (
              <p className="text-red-400 text-sm mt-1">{errors.measuredAt}</p>
            )}
          </div>

          {/* Historical Measurements Table */}
          <div>
            <h3 className="text-white font-semibold mb-3">
              Historical Measurements
            </h3>
            <div className="bg-white/5 border border-white/10 rounded-lg overflow-hidden max-h-[300px] overflow-y-auto">
              {historicalTests.length === 0 ? (
                <div className="p-8 text-center text-white/40">
                  Historical measurement data will appear here
                </div>
              ) : (
                <table className="w-full">
                  <thead className="bg-white/5 sticky top-0">
                    <tr>
                      <th className="px-4 py-3 text-left text-white/80 font-medium text-sm">
                        Value
                      </th>
                      <th className="px-4 py-3 text-left text-white/80 font-medium text-sm">
                        Date & Time
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {historicalTests.map((test, index) => {
                      const paramValue = test.parameters.find(
                        (p) => p.type === parameter
                      );
                      return (
                        <tr
                          key={test.id}
                          className={`border-t border-white/5 ${
                            index % 2 === 0 ? 'bg-white/0' : 'bg-white/[0.02]'
                          }`}
                        >
                          <td className="px-4 py-3 text-white">
                            {paramValue?.value.toFixed(2)}{' '}
                            {parameterUnit && (
                              <span className="text-white/60">
                                {parameterUnit}
                              </span>
                            )}
                          </td>
                          <td className="px-4 py-3 text-white/80">
                            {formatDateTime(test.testDate)}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              )}
            </div>
          </div>

          {/* Buttons */}
          <div className="flex gap-3 justify-end pt-4">
            <GlassButton
              type="button"
              variant="secondary"
              onClick={onClose}
              disabled={isSubmitting}
            >
              Cancel
            </GlassButton>
            <GlassButton
              type="submit"
              variant="primary"
              disabled={isSubmitting}
              style={{
                backgroundColor: `${parameterColor}40`,
                borderColor: parameterColor,
              }}
            >
              {isSubmitting ? 'Saving...' : 'Save Measurement'}
            </GlassButton>
          </div>
        </form>
      </div>
    </GlassModal>
  );
};
