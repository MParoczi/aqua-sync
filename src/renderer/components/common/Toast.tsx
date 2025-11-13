import React, { useEffect, useState } from 'react';
import { useTheme } from '../../contexts/ThemeContext';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface ToastData {
  id: string;
  message: string;
  type: ToastType;
  duration?: number;
}

interface ToastProps {
  toast: ToastData;
  onDismiss: (id: string) => void;
}

const toastIcons: Record<ToastType, string> = {
  success: '✓',
  error: '✕',
  info: 'ℹ',
  warning: '⚠',
};

const toastColors: Record<ToastType, { light: string; dark: string }> = {
  success: {
    light: 'bg-green-500/20 border-green-400/40 text-green-100',
    dark: 'bg-green-500/30 border-green-400/50 text-green-200',
  },
  error: {
    light: 'bg-red-500/20 border-red-400/40 text-red-100',
    dark: 'bg-red-500/30 border-red-400/50 text-red-200',
  },
  info: {
    light: 'bg-blue-500/20 border-blue-400/40 text-blue-100',
    dark: 'bg-blue-500/30 border-blue-400/50 text-blue-200',
  },
  warning: {
    light: 'bg-yellow-500/20 border-yellow-400/40 text-yellow-100',
    dark: 'bg-yellow-500/30 border-yellow-400/50 text-yellow-200',
  },
};

export const Toast: React.FC<ToastProps> = ({ toast, onDismiss }) => {
  const { theme } = useTheme();
  const [isVisible, setIsVisible] = useState(false);
  const [isLeaving, setIsLeaving] = useState(false);

  useEffect(() => {
    // Animate in
    setTimeout(() => setIsVisible(true), 10);

    // Auto-dismiss
    const duration = toast.duration ?? 5000;
    const timer = setTimeout(() => {
      handleDismiss();
    }, duration);

    return () => clearTimeout(timer);
  }, [toast.id]);

  const handleDismiss = () => {
    setIsLeaving(true);
    setTimeout(() => {
      onDismiss(toast.id);
    }, 300); // Match animation duration
  };

  const colorClasses = toastColors[toast.type][theme];

  return (
    <div
      role="alert"
      aria-live="polite"
      aria-atomic="true"
      className={`
        flex items-start gap-3 p-4 min-w-[300px] max-w-[400px]
        rounded-lg backdrop-blur-[10px] border
        shadow-lg transition-all duration-300
        ${colorClasses}
        ${isVisible && !isLeaving ? 'translate-x-0 opacity-100' : 'translate-x-full opacity-0'}
      `}
    >
      <div
        className="flex-shrink-0 w-6 h-6 rounded-full flex items-center justify-center font-bold text-lg"
        aria-hidden="true"
      >
        {toastIcons[toast.type]}
      </div>
      <p className="flex-1 text-sm font-medium leading-relaxed">{toast.message}</p>
      <button
        onClick={handleDismiss}
        className="flex-shrink-0 w-5 h-5 rounded hover:bg-white/10 transition-colors flex items-center justify-center text-xs"
        aria-label="Dismiss notification"
      >
        ✕
      </button>
    </div>
  );
};
