import React from 'react';
import { GlassButton } from './common';
import { AquariumCard } from './AquariumCard';
import type { Aquarium } from '../../shared/types';

interface AquariumGridProps {
  aquariums: Aquarium[];
  onAddNew: () => void;
  onEdit?: (aquarium: Aquarium) => void;
  onDelete?: (aquarium: Aquarium) => void;
  onClick?: (aquarium: Aquarium) => void;
}

export const AquariumGrid: React.FC<AquariumGridProps> = ({
  aquariums,
  onAddNew,
  onEdit,
  onDelete,
  onClick
}) => {
  // Sort aquariums by createdAt (oldest first)
  const sortedAquariums = React.useMemo(() => {
    return [...aquariums].sort((a, b) => {
      const dateA = new Date(a.createdAt).getTime();
      const dateB = new Date(b.createdAt).getTime();
      return dateA - dateB; // Oldest first
    });
  }, [aquariums]);

  return (
    <div className="min-h-screen p-8">
      {/* Header with Add button */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-4xl font-bold text-white mb-2">
            Your Aquariums
          </h1>
          <p className="text-white/70 text-lg">
            {aquariums.length} {aquariums.length === 1 ? 'aquarium' : 'aquariums'}
          </p>
        </div>
        <GlassButton
          variant="primary"
          onClick={onAddNew}
          className="text-lg px-6 py-3"
        >
          <svg className="w-5 h-5 mr-2 inline-block" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          Add new aquarium
        </GlassButton>
      </div>

      {/* Grid of aquarium cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {sortedAquariums.map((aquarium) => (
          <AquariumCard
            key={aquarium.id}
            aquarium={aquarium}
            onEdit={onEdit}
            onDelete={onDelete}
            onClick={onClick}
          />
        ))}
      </div>
    </div>
  );
};
