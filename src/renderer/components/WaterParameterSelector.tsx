import React, { useState, useRef, useEffect } from 'react';
import type { WaterParameterOption } from '../../shared/types';
import { GlassCard } from './common';

interface WaterParameterSelectorProps {
  selectedParameters: WaterParameterOption[];
  onSelectionChange: (parameters: WaterParameterOption[]) => void;
}

/**
 * All available water parameters as per US-014
 */
const WATER_PARAMETERS: WaterParameterOption[] = [
  'pH',
  'GH',
  'KH',
  'NO₂',
  'NO₃',
  'NH₄',
  'Fe',
  'Cu',
  'SiO₂',
  'PO₄',
  'CO₂',
  'O₂',
  'Temperature',
];

/**
 * Multi-select dropdown for water parameters (US-014)
 */
export const WaterParameterSelector: React.FC<WaterParameterSelectorProps> = ({
  selectedParameters,
  onSelectionChange,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  const toggleParameter = (parameter: WaterParameterOption) => {
    const newSelection = selectedParameters.includes(parameter)
      ? selectedParameters.filter(p => p !== parameter)
      : [...selectedParameters, parameter];

    onSelectionChange(newSelection);
  };

  const toggleDropdown = () => {
    setIsOpen(!isOpen);
  };

  return (
    <div className="relative" ref={dropdownRef}>
      {/* Dropdown Trigger Button */}
      <button
        onClick={toggleDropdown}
        className="w-full px-4 py-3 rounded-lg border border-white/20 bg-white/10 backdrop-blur-md text-white text-left flex items-center justify-between hover:bg-white/15 transition-all"
      >
        <span>
          {selectedParameters.length === 0
            ? 'Select water parameters to display'
            : `${selectedParameters.length} parameter${selectedParameters.length === 1 ? '' : 's'} selected`}
        </span>
        <svg
          className={`w-5 h-5 text-white/60 transition-transform ${isOpen ? 'rotate-180' : ''}`}
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M19 9l-7 7-7-7"
          />
        </svg>
      </button>

      {/* Dropdown Menu */}
      {isOpen && (
        <div className="absolute z-50 w-full mt-2 rounded-lg border border-white/20 bg-white/10 backdrop-blur-md shadow-xl max-h-96 overflow-y-auto">
          <div className="p-2">
            {WATER_PARAMETERS.map((parameter) => {
              const isSelected = selectedParameters.includes(parameter);

              return (
                <button
                  key={parameter}
                  onClick={() => toggleParameter(parameter)}
                  className={`w-full px-4 py-2 rounded-md text-left flex items-center gap-3 transition-all ${
                    isSelected
                      ? 'bg-white/20 text-white'
                      : 'text-white/70 hover:bg-white/10 hover:text-white'
                  }`}
                >
                  {/* Checkbox */}
                  <div
                    className={`w-5 h-5 rounded border-2 flex items-center justify-center transition-all ${
                      isSelected
                        ? 'bg-blue-500 border-blue-500'
                        : 'border-white/40 bg-transparent'
                    }`}
                  >
                    {isSelected && (
                      <svg
                        className="w-3 h-3 text-white"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          strokeWidth={3}
                          d="M5 13l4 4L19 7"
                        />
                      </svg>
                    )}
                  </div>

                  <span className="font-medium">{parameter}</span>
                </button>
              );
            })}
          </div>

          {/* Clear All / Select All buttons */}
          <div className="border-t border-white/20 p-2 flex gap-2">
            <button
              onClick={() => onSelectionChange([])}
              className="flex-1 px-3 py-2 rounded-md text-sm text-white/70 hover:bg-white/10 hover:text-white transition-all"
            >
              Clear All
            </button>
            <button
              onClick={() => onSelectionChange([...WATER_PARAMETERS])}
              className="flex-1 px-3 py-2 rounded-md text-sm text-white/70 hover:bg-white/10 hover:text-white transition-all"
            >
              Select All
            </button>
          </div>
        </div>
      )}

      {/* Selected Parameters Display */}
      {selectedParameters.length > 0 && (
        <div className="mt-3 flex flex-wrap gap-2">
          {selectedParameters.map((parameter) => (
            <div
              key={parameter}
              className="px-3 py-1 rounded-full bg-white/10 border border-white/20 text-white text-sm flex items-center gap-2"
            >
              <span>{parameter}</span>
              <button
                onClick={() => toggleParameter(parameter)}
                className="text-white/60 hover:text-white transition-colors"
              >
                <svg
                  className="w-4 h-4"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M6 18L18 6M6 6l12 12"
                  />
                </svg>
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
