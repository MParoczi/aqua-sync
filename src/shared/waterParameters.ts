// Water parameter constants shared across the application

import { WaterParameterOption } from './types';

/**
 * Color definitions for water parameters (used in graphs and cards)
 * These colors provide visual consistency across the application
 */
export const PARAMETER_COLORS: Record<WaterParameterOption, string> = {
  pH: '#0099CC', // Cyan blue
  GH: '#8D7B65', // Brown (General Hardness)
  KH: '#B89B5E', // Tan (Carbonate Hardness)
  'NO₂': '#A347BA', // Purple (Nitrite)
  'NO₃': '#E05C2B', // Orange (Nitrate)
  'NH₄': '#7AC943', // Green (Ammonia)
  Fe: '#A63E14', // Dark brown (Iron)
  Cu: '#0097A7', // Teal (Copper)
  'SiO₂': '#C4B998', // Light tan (Silica)
  'PO₄': '#1FA75D', // Dark green (Phosphate)
  'CO₂': '#546E7A', // Gray (Carbon Dioxide)
  'O₂': '#42A5F5', // Light blue (Oxygen)
  Temperature: '#F39C12', // Amber/Orange
};

/**
 * Unit definitions for water parameters
 */
export const PARAMETER_UNITS: Record<WaterParameterOption, string> = {
  pH: '', // pH is unitless
  GH: '°dGH', // Degrees German Hardness
  KH: '°dKH', // Degrees Carbonate Hardness
  'NO₂': 'mg/l', // Milligrams per liter
  'NO₃': 'mg/l',
  'NH₄': 'mg/l',
  Fe: 'mg/l',
  Cu: 'mg/l',
  'SiO₂': 'mg/l',
  'PO₄': 'mg/l',
  'CO₂': '', // Usually expressed as ppm or mg/l, but often shown without unit
  'O₂': 'mg/l',
  Temperature: '°C',
};

/**
 * All available water parameters in order
 */
export const ALL_PARAMETERS: WaterParameterOption[] = [
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
 * Display names for water parameters (for UI)
 */
export const PARAMETER_DISPLAY_NAMES: Record<WaterParameterOption, string> = {
  pH: 'pH',
  GH: 'GH (General Hardness)',
  KH: 'KH (Carbonate Hardness)',
  'NO₂': 'NO₂ (Nitrite)',
  'NO₃': 'NO₃ (Nitrate)',
  'NH₄': 'NH₄ (Ammonia)',
  Fe: 'Fe (Iron)',
  Cu: 'Cu (Copper)',
  'SiO₂': 'SiO₂ (Silica)',
  'PO₄': 'PO₄ (Phosphate)',
  'CO₂': 'CO₂ (Carbon Dioxide)',
  'O₂': 'O₂ (Oxygen)',
  Temperature: 'Temperature',
};

/**
 * Value ranges for water parameters (US-017)
 * Used for input validation in measurement modal
 */
export interface ParameterRange {
  min: number;
  max: number;
  step: number; // Input step size
}

export const PARAMETER_RANGES: Record<WaterParameterOption, ParameterRange> = {
  pH: { min: 0, max: 14, step: 0.1 },
  GH: { min: 0, max: 30, step: 0.5 }, // °dGH
  KH: { min: 0, max: 30, step: 0.5 }, // °dKH
  'NO₂': { min: 0, max: 10, step: 0.01 }, // mg/l (should be close to 0)
  'NO₃': { min: 0, max: 200, step: 0.1 }, // mg/l
  'NH₄': { min: 0, max: 10, step: 0.01 }, // mg/l (should be close to 0)
  Fe: { min: 0, max: 5, step: 0.01 }, // mg/l
  Cu: { min: 0, max: 1, step: 0.001 }, // mg/l (toxic at low levels)
  'SiO₂': { min: 0, max: 50, step: 0.1 }, // mg/l
  'PO₄': { min: 0, max: 10, step: 0.01 }, // mg/l
  'CO₂': { min: 0, max: 50, step: 1 }, // mg/l or ppm
  'O₂': { min: 0, max: 20, step: 0.1 }, // mg/l
  Temperature: { min: 0, max: 50, step: 0.1 }, // °C
};
