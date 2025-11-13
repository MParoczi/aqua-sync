import React, { useState, useEffect } from 'react';
import { useTheme } from './contexts/ThemeContext';
import { GlassCard, GlassButton, GlassModal } from './components/common';
import type { Aquarium } from '../shared/types';
import './App.css';

function App() {
  const { theme, toggleTheme } = useTheme();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [dataPath, setDataPath] = useState<string>('');
  const [aquariums, setAquariums] = useState<Aquarium[]>([]);
  const [isTestingData, setIsTestingData] = useState(false);

  // Load data path and aquariums on mount
  useEffect(() => {
    loadDataPath();
    loadAquariums();
  }, []);

  const loadDataPath = async () => {
    const result = await window.electron.data.getDataPath();
    if (result.success && result.data) {
      setDataPath(result.data);
    }
  };

  const loadAquariums = async () => {
    const result = await window.electron.data.getAquariums();
    if (result.success && result.data) {
      setAquariums(result.data);
    }
  };

  const testDataPersistence = async () => {
    setIsTestingData(true);
    try {
      // Create a test aquarium
      const testAquarium = {
        name: 'Test Aquarium',
        type: 'freshwater' as const,
        dimensions: {
          width: 100,
          length: 50,
          height: 60,
          unit: 'cm' as const,
        },
        volume: {
          value: 300,
          unit: 'liter' as const,
          isCustom: false,
        },
        startDate: new Date().toISOString(),
      };

      const createResult = await window.electron.data.createAquarium(testAquarium);
      if (createResult.success) {
        console.log('✅ Test aquarium created:', createResult.data);
        // Reload aquariums
        await loadAquariums();
      } else {
        console.error('❌ Failed to create test aquarium:', createResult.error);
      }
    } catch (error) {
      console.error('❌ Error testing data persistence:', error);
    } finally {
      setIsTestingData(false);
    }
  };

  const deleteAquarium = async (id: string) => {
    const result = await window.electron.data.deleteAquarium(id);
    if (result.success) {
      console.log('✅ Aquarium deleted');
      await loadAquariums();
    } else {
      console.error('❌ Failed to delete aquarium:', result.error);
    }
  };

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

      <GlassCard className="p-6 max-w-2xl w-full">
        <h2 className="text-2xl font-semibold text-white mb-4">
          US-003: Local Data Persistence
        </h2>

        <div className="space-y-4 text-white/80">
          <div className="p-3 rounded-lg bg-white/5">
            <h3 className="text-sm font-semibold text-white/70 mb-1">Data Path</h3>
            <p className="text-sm font-mono break-all">{dataPath || 'Loading...'}</p>
          </div>

          <div className="p-3 rounded-lg bg-white/5">
            <h3 className="text-sm font-semibold text-white/70 mb-2">
              Aquariums ({aquariums.length})
            </h3>
            {aquariums.length === 0 ? (
              <p className="text-sm text-white/60">No aquariums created yet</p>
            ) : (
              <div className="space-y-2">
                {aquariums.map((aquarium) => (
                  <div
                    key={aquarium.id}
                    className="flex items-center justify-between p-2 rounded bg-white/5"
                  >
                    <div className="flex-1">
                      <p className="font-semibold text-white">{aquarium.name}</p>
                      <p className="text-xs text-white/60">
                        {aquarium.type} • {aquarium.volume.value} {aquarium.volume.unit}
                      </p>
                    </div>
                    <button
                      onClick={() => deleteAquarium(aquarium.id)}
                      className="px-2 py-1 text-xs rounded bg-red-500/20 hover:bg-red-500/30 text-red-300"
                    >
                      Delete
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className="flex gap-3">
            <GlassButton
              variant="primary"
              onClick={testDataPersistence}
              disabled={isTestingData}
            >
              {isTestingData ? 'Creating...' : 'Create Test Aquarium'}
            </GlassButton>
            <GlassButton variant="secondary" onClick={loadAquariums}>
              Refresh Data
            </GlassButton>
          </div>

          <div className="text-xs text-white/60">
            <p>
              💡 Tip: Click "Create Test Aquarium" to test the persistence layer.
              Data is stored in JSON files in the AppData folder.
            </p>
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
