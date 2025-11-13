import React, { ReactNode, useEffect } from 'react';
import { useTheme } from '../../contexts/ThemeContext';

interface GlassModalProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  children: ReactNode;
  className?: string;
}

export const GlassModal: React.FC<GlassModalProps> = ({
  isOpen,
  onClose,
  title,
  children,
  className = ''
}) => {
  const { theme } = useTheme();

  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen) {
        onClose();
      }
    };

    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  const modalClasses = theme === 'light'
    ? 'bg-white/10 border-white/20 shadow-glass'
    : 'bg-black/20 border-white/10 shadow-glass-dark';

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4 backdrop-blur-sm bg-black/30"
      onClick={onClose}
      role="dialog"
      aria-modal="true"
      aria-labelledby={title ? 'modal-title' : undefined}
    >
      <div
        className={`relative rounded-lg backdrop-blur-[10px] border transition-all duration-300 max-w-2xl w-full max-h-[90vh] overflow-y-auto ${modalClasses} ${className}`}
        onClick={(e) => e.stopPropagation()}
      >
        {title && (
          <div className="px-6 py-4 border-b border-white/10">
            <h2
              id="modal-title"
              className={`text-2xl font-semibold ${theme === 'light' ? 'text-white' : 'text-white'}`}
            >
              {title}
            </h2>
          </div>
        )}
        <div className="px-6 py-4">
          {children}
        </div>
        <button
          onClick={onClose}
          className="absolute top-4 right-4 text-white hover:text-gray-300 transition-colors"
          aria-label="Close modal"
        >
          <svg
            className="w-6 h-6"
            fill="none"
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth="2"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path d="M6 18L18 6M6 6l12 12"></path>
          </svg>
        </button>
      </div>
    </div>
  );
};
