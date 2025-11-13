import React, { ReactNode } from 'react';
import { useTheme } from '../../contexts/ThemeContext';

interface GlassButtonProps {
  children: ReactNode;
  onClick?: () => void;
  variant?: 'primary' | 'secondary';
  className?: string;
  type?: 'button' | 'submit' | 'reset';
}

export const GlassButton: React.FC<GlassButtonProps> = ({
  children,
  onClick,
  variant = 'primary',
  className = '',
  type = 'button'
}) => {
  const { theme } = useTheme();

  const baseClasses = 'px-6 py-3 rounded-lg backdrop-blur-[10px] border transition-all duration-300 font-medium hover:scale-105 active:scale-95 cursor-pointer';

  const variantClasses = variant === 'primary'
    ? theme === 'light'
      ? 'bg-white/20 border-white/30 hover:bg-white/30 text-white shadow-glass'
      : 'bg-white/10 border-white/20 hover:bg-white/20 text-white shadow-glass-dark'
    : theme === 'light'
      ? 'bg-white/10 border-white/20 hover:bg-white/20 text-white shadow-glass'
      : 'bg-black/20 border-white/10 hover:bg-black/30 text-white shadow-glass-dark';

  return (
    <button
      type={type}
      onClick={onClick}
      className={`${baseClasses} ${variantClasses} ${className}`}
    >
      {children}
    </button>
  );
};
