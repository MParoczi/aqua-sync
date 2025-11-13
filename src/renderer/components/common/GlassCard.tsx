import React, { ReactNode } from 'react';
import { useTheme } from '../../contexts/ThemeContext';

interface GlassCardProps extends React.HTMLAttributes<HTMLDivElement> {
  children: ReactNode;
  className?: string;
}

export const GlassCard: React.FC<GlassCardProps> = ({ children, className = '', ...props }) => {
  const { theme } = useTheme();

  const baseClasses = 'rounded-lg backdrop-blur-[10px] border transition-all duration-300';
  const themeClasses = theme === 'light'
    ? 'bg-white/10 border-white/20 shadow-glass'
    : 'bg-black/20 border-white/10 shadow-glass-dark';

  return (
    <div className={`${baseClasses} ${themeClasses} ${className}`} {...props}>
      {children}
    </div>
  );
};
