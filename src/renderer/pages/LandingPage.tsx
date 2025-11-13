import React, { useState, useEffect } from 'react';
import { GlassCard, GlassButton } from '../components/common';
import type { Aquarium } from '../../shared/types';
import { AquariumModal } from '../components/AquariumModal';
import { AquariumGrid } from '../components/AquariumGrid';

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

  // If aquariums exist, show grid
  return (
    <>
      <AquariumGrid
        aquariums={aquariums}
        onAddNew={() => setIsModalOpen(true)}
      />

      <AquariumModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onAquariumCreated={handleAquariumCreated}
      />
    </>
  );
};
