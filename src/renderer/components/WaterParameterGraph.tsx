import React from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  CartesianGrid,
} from 'recharts';
import { GlassCard } from './common';
import type { WaterParameterOption } from '../../shared/types';
import { PARAMETER_COLORS, PARAMETER_UNITS } from '../../shared/waterParameters';

interface WaterParameterGraphProps {
  parameter: WaterParameterOption;
  data: Array<{
    date: string; // ISO date string
    value: number;
  }>;
}

/**
 * Format date for X-axis display (YYYY.MM.DD)
 */
const formatDate = (isoDate: string): string => {
  const date = new Date(isoDate);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}.${month}.${day}`;
};

/**
 * Format date for tooltip (more readable format)
 */
const formatTooltipDate = (isoDate: string): string => {
  const date = new Date(isoDate);
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
};

/**
 * Custom tooltip component with glassmorphism styling
 */
interface TooltipProps {
  active?: boolean;
  payload?: Array<{
    name: string;
    payload: {
      date: string;
      value: number;
    };
  }>;
}

const CustomTooltip: React.FC<TooltipProps> = ({ active, payload }) => {
  if (active && payload && payload.length > 0) {
    const data = payload[0].payload;
    const parameter = payload[0].name as WaterParameterOption;
    const unit = PARAMETER_UNITS[parameter];

    return (
      <div className="px-3 py-2 rounded-lg border border-white/20 bg-white/10 backdrop-blur-md">
        <p className="text-white text-sm font-medium">
          {formatTooltipDate(data.date)}
        </p>
        <p className="text-white/90 text-sm">
          {data.value}
          {unit && ` ${unit}`}
        </p>
      </div>
    );
  }

  return null;
};

/**
 * Water parameter line graph component (US-015)
 * Displays a single water parameter's values over time
 */
export const WaterParameterGraph: React.FC<WaterParameterGraphProps> = ({
  parameter,
  data,
}) => {
  const color = PARAMETER_COLORS[parameter];
  const unit = PARAMETER_UNITS[parameter];

  // Sort data by date (oldest to newest) as per US-015
  const sortedData = [...data].sort(
    (a, b) => new Date(a.date).getTime() - new Date(b.date).getTime()
  );

  // Transform data for Recharts
  const chartData = sortedData.map((point) => ({
    date: point.date,
    dateFormatted: formatDate(point.date),
    value: point.value,
  }));

  return (
    <GlassCard className="p-4">
      {/* Parameter Name */}
      <h4 className="text-white font-semibold text-lg mb-4">
        {parameter}
        {unit && <span className="text-white/60 text-sm ml-2">({unit})</span>}
      </h4>

      {/* Graph */}
      {chartData.length === 0 ? (
        <div className="h-64 flex items-center justify-center text-white/60">
          <p>No data available</p>
        </div>
      ) : (
        <ResponsiveContainer width="100%" height={250}>
          <LineChart
            data={chartData}
            margin={{ top: 5, right: 20, left: 0, bottom: 5 }}
          >
            {/* Grid with subtle styling */}
            <CartesianGrid
              strokeDasharray="3 3"
              stroke="rgba(255, 255, 255, 0.1)"
            />

            {/* X-axis: Dates */}
            <XAxis
              dataKey="dateFormatted"
              stroke="rgba(255, 255, 255, 0.6)"
              tick={{ fill: 'rgba(255, 255, 255, 0.7)', fontSize: 12 }}
              tickLine={{ stroke: 'rgba(255, 255, 255, 0.3)' }}
            />

            {/* Y-axis: Values with unit */}
            <YAxis
              stroke="rgba(255, 255, 255, 0.6)"
              tick={{ fill: 'rgba(255, 255, 255, 0.7)', fontSize: 12 }}
              tickLine={{ stroke: 'rgba(255, 255, 255, 0.3)' }}
              label={{
                value: unit,
                angle: -90,
                position: 'insideLeft',
                fill: 'rgba(255, 255, 255, 0.7)',
                fontSize: 12,
              }}
            />

            {/* Tooltip */}
            <Tooltip content={<CustomTooltip />} />

            {/* Line */}
            <Line
              type="monotone"
              dataKey="value"
              name={parameter}
              stroke={color}
              strokeWidth={2}
              dot={{ fill: color, r: 4 }}
              activeDot={{ r: 6, fill: color }}
            />
          </LineChart>
        </ResponsiveContainer>
      )}
    </GlassCard>
  );
};
