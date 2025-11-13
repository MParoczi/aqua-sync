import React, { useState } from 'react';
import { useTheme } from './contexts/ThemeContext';
import { GlassCard, GlassButton, GlassModal } from './components/common';
import './App.css';

function App() {
  const { theme, toggleTheme } = useTheme();
  const [isModalOpen, setIsModalOpen] = useState(false);

  return (
    <div className="min-h-screen p-8 flex flex-col items-center justify-center gap-8">
      <GlassCard className="p-8 max-w-2xl w-full">
        <h1 className="text-4xl font-bold text-white mb-4 text-center">
          Welcome to AquaSync
        </h1>
        <p className="text-white/90 text-center mb-6">
          Your aquatic equipment management solution with glassmorphism design
        </p>

        <div className="space-y-4">
          <div className={`p-4 rounded-lg ${theme === 'light' ? 'bg-white/10' : 'bg-black/20'}`}>
            <h2 className="text-xl font-semibold text-white mb-2">Current Theme</h2>
            <p className="text-white/80">
              Active theme: <span className="font-bold capitalize">{theme}</span>
            </p>
            <p className="text-white/70 text-sm mt-2">
              Theme automatically follows system preferences and updates in real-time
            </p>
          </div>

          <div className={`p-4 rounded-lg ${theme === 'light' ? 'bg-white/10' : 'bg-black/20'}`}>
            <h2 className="text-xl font-semibold text-white mb-2">Tech Stack</h2>
            <ul className="text-white/80 space-y-1">
              <li>✓ Electron + React + TypeScript + Vite</li>
              <li>✓ Tailwind CSS with glassmorphism utilities</li>
              <li>✓ Auto theme detection with matchMedia</li>
              <li>✓ Hot reload enabled for development</li>
            </ul>
          </div>
        </div>
      </GlassCard>

      <div className="flex gap-4 flex-wrap justify-center">
        <GlassButton variant="primary" onClick={toggleTheme}>
          Toggle Theme (Current: {theme})
        </GlassButton>
        <GlassButton variant="secondary" onClick={() => setIsModalOpen(true)}>
          Open Modal Demo
        </GlassButton>
      </div>

      <GlassCard className="p-6 max-w-xl w-full">
        <h2 className="text-2xl font-semibold text-white mb-4">Glassmorphism Components</h2>
        <div className="space-y-3 text-white/80">
          <div className="flex items-center gap-2">
            <span className="text-green-400">✓</span>
            <span>GlassCard - Flexible container with glassmorphism effect</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-green-400">✓</span>
            <span>GlassButton - Interactive button with hover effects</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-green-400">✓</span>
            <span>GlassModal - Accessible modal with backdrop blur</span>
          </div>
        </div>
      </GlassCard>

      <GlassModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="Glassmorphism Modal Demo"
      >
        <div className="space-y-4 text-white/90">
          <p>
            This is a demonstration of the GlassModal component with glassmorphism styling.
          </p>
          <p>
            Features include:
          </p>
          <ul className="list-disc list-inside space-y-2 ml-4">
            <li>Backdrop blur effect</li>
            <li>Escape key to close</li>
            <li>Click outside to dismiss</li>
            <li>Smooth transitions</li>
            <li>Accessible ARIA labels</li>
            <li>Theme-aware styling</li>
          </ul>
          <div className="flex gap-3 mt-6">
            <GlassButton variant="primary" onClick={() => setIsModalOpen(false)}>
              Close Modal
            </GlassButton>
            <GlassButton variant="secondary" onClick={toggleTheme}>
              Toggle Theme
            </GlassButton>
          </div>
        </div>
      </GlassModal>
    </div>
  );
}

export default App;
