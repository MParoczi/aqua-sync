import React from 'react';
import { GlassCard, GlassButton } from '../components/common';
import { useAquarium } from '../contexts/AquariumContext';

export const MainPage: React.FC = () => {
  const { selectedAquarium, selectAquarium } = useAquarium();

  // If no aquarium is selected, show error state
  if (!selectedAquarium) {
    return (
      <div className="min-h-screen flex items-center justify-center p-8">
        <GlassCard className="p-12 max-w-2xl w-full text-center">
          <h1 className="text-3xl font-bold text-white mb-4">
            No Aquarium Selected
          </h1>
          <p className="text-white/80 text-lg mb-6">
            Please select an aquarium from the landing page.
          </p>
          <GlassButton
            variant="primary"
            onClick={() => selectAquarium(null)}
            className="text-lg px-6 py-3"
          >
            Back to Aquariums
          </GlassButton>
        </GlassCard>
      </div>
    );
  }

  // Main page content
  return (
    <div className="min-h-screen p-8">
      {/* Header with back button */}
      <div className="mb-8">
        <GlassButton
          variant="secondary"
          onClick={() => selectAquarium(null)}
          className="mb-4"
        >
          <svg className="w-5 h-5 mr-2 inline-block" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
          Back to Aquariums
        </GlassButton>

        <h1 className="text-4xl font-bold text-white mb-2">
          {selectedAquarium.name}
        </h1>
        <div className="flex items-center gap-4 text-white/70 text-lg">
          <span className="capitalize">{selectedAquarium.type}</span>
          <span>•</span>
          <span>
            {selectedAquarium.volume.value} {selectedAquarium.volume.unit}
          </span>
          <span>•</span>
          <span>
            {selectedAquarium.dimensions.width} × {selectedAquarium.dimensions.length} × {selectedAquarium.dimensions.height} {selectedAquarium.dimensions.unit}
          </span>
        </div>
      </div>

      {/* Main content area - placeholder for future Epic 3 implementation */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <GlassCard className="p-6">
          <h2 className="text-2xl font-bold text-white mb-4">Overview</h2>
          <p className="text-white/70">
            Dashboard overview will be implemented in Epic 3 (US-010 to US-015)
          </p>
        </GlassCard>

        <GlassCard className="p-6">
          <h2 className="text-2xl font-bold text-white mb-4">Quick Stats</h2>
          <p className="text-white/70">
            Statistics and quick actions will be implemented in Epic 3
          </p>
        </GlassCard>

        <GlassCard className="p-6">
          <h2 className="text-2xl font-bold text-white mb-4">Devices</h2>
          <p className="text-white/70">
            Device management will be implemented in Epic 5 and Epic 6
          </p>
        </GlassCard>

        <GlassCard className="p-6">
          <h2 className="text-2xl font-bold text-white mb-4">Water Tests</h2>
          <p className="text-white/70">
            Water test history will be implemented in Epic 4 (US-016 to US-018)
          </p>
        </GlassCard>
      </div>
    </div>
  );
};
