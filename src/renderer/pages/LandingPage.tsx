import React, { useState, useEffect } from 'react';
import { GlassCard, GlassButton } from '../components/common';
import type { Aquarium } from '../../shared/types';
import { AquariumModal } from '../components/AquariumModal';

export const LandingPage: React.FC = () => {
  const [aquariums, setAquariums] = useState<Aquarium[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);

  useEffect(() => {
    loadAquariums();
  }, []);

  const loadAquariums = async () => {
    setIsLoading(true);
    try {
      const result = await window.electron.data.getAquariums();
      if (result.success && result.data) {
        setAquariums(result.data);
      }
    } catch (error) {
      console.error('Failed to load aquariums:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleAquariumCreated = () => {
    // Reload aquariums after creation
    loadAquariums();
    setIsModalOpen(false);
  };

  // Show loading state
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <GlassCard className="p-8">
          <p className="text-white text-lg">Loading...</p>
        </GlassCard>
      </div>
    );
  }

  // Show empty state if no aquariums exist
  if (aquariums.length === 0) {
    return (
      <>
        <div className="min-h-screen flex items-center justify-center p-8">
          <GlassCard className="p-12 max-w-2xl w-full text-center">
            <h1 className="text-5xl font-bold text-white mb-4">
              Welcome to Aqua Sync
            </h1>
            <p className="text-white/80 text-xl mb-8">
              Create your first aquarium!
            </p>
            <GlassButton
              variant="primary"
              onClick={() => setIsModalOpen(true)}
              className="text-lg px-8 py-4"
            >
              Create new aquarium
            </GlassButton>
          </GlassCard>
        </div>

        <AquariumModal
          isOpen={isModalOpen}
          onClose={() => setIsModalOpen(false)}
          onAquariumCreated={handleAquariumCreated}
        />
      </>
    );
  }

  // If aquariums exist, show grid (will be implemented in US-007)
  return (
    <div className="min-h-screen p-8">
      <GlassCard className="p-8">
        <h1 className="text-3xl font-bold text-white mb-4">
          Your Aquariums ({aquariums.length})
        </h1>
        <p className="text-white/70 mb-4">
          Aquarium grid will be implemented in US-007
        </p>
        <div className="space-y-2">
          {aquariums.map((aquarium) => (
            <div
              key={aquarium.id}
              className="p-4 rounded-lg bg-white/10 border border-white/20"
            >
              <p className="text-white font-semibold">{aquarium.name}</p>
              <p className="text-white/60 text-sm">
                {aquarium.type} • {aquarium.volume.value} {aquarium.volume.unit}
              </p>
            </div>
          ))}
        </div>
        <div className="mt-6">
          <GlassButton variant="primary" onClick={() => setIsModalOpen(true)}>
            Add new aquarium
          </GlassButton>
        </div>
      </GlassCard>

      <AquariumModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onAquariumCreated={handleAquariumCreated}
      />
    </div>
  );
};
