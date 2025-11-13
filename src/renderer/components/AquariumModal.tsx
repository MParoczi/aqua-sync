import React from 'react';
import { GlassModal, GlassButton } from './common';

interface AquariumModalProps {
  isOpen: boolean;
  onClose: () => void;
  onAquariumCreated?: () => void;
}

/**
 * Aquarium Creation/Edit Modal
 *
 * US-005: Basic modal structure
 * US-006: Will implement full form with all fields and validation
 */
export const AquariumModal: React.FC<AquariumModalProps> = ({
  isOpen,
  onClose,
  onAquariumCreated,
}) => {
  return (
    <GlassModal
      isOpen={isOpen}
      onClose={onClose}
      title="Create Aquarium"
    >
      <div className="space-y-4 text-white/90">
        <p className="text-center py-8">
          Aquarium creation form will be implemented in US-006.
        </p>
        <p className="text-sm text-white/70 text-center">
          This modal will include fields for:
        </p>
        <ul className="text-sm text-white/60 space-y-1 list-disc list-inside">
          <li>Name (required)</li>
          <li>Type (Freshwater/Marine)</li>
          <li>Dimensions (Width, Length, Height)</li>
          <li>Volume (auto-calculated or custom)</li>
          <li>Start date</li>
          <li>Thumbnail image upload</li>
        </ul>
        <div className="flex gap-3 mt-6 justify-center">
          <GlassButton variant="secondary" onClick={onClose}>
            Close
          </GlassButton>
        </div>
      </div>
    </GlassModal>
  );
};
