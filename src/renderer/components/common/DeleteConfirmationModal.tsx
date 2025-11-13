import React from 'react';
import { GlassModal } from './GlassModal';
import { GlassButton } from './GlassButton';

interface DeleteConfirmationModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  itemName: string;
  isDeleting?: boolean;
}

export const DeleteConfirmationModal: React.FC<DeleteConfirmationModalProps> = ({
  isOpen,
  onClose,
  onConfirm,
  itemName,
  isDeleting = false,
}) => {
  return (
    <GlassModal isOpen={isOpen} onClose={onClose} title="Delete Aquarium">
      <div className="space-y-6">
        <p className="text-white/90 text-lg">
          Are you sure you want to delete <span className="font-bold">{itemName}</span>?
        </p>
        <p className="text-white/70 text-sm">
          This action cannot be undone. All associated devices and water test data will also be deleted.
        </p>

        <div className="flex gap-3 pt-4">
          <GlassButton
            type="button"
            variant="secondary"
            onClick={onClose}
            disabled={isDeleting}
            className="flex-1"
          >
            Cancel
          </GlassButton>
          <GlassButton
            type="button"
            variant="primary"
            onClick={onConfirm}
            disabled={isDeleting}
            className="flex-1 bg-red-500/30 border-red-400/50 hover:bg-red-500/40 text-red-100"
          >
            {isDeleting ? 'Deleting...' : 'Delete'}
          </GlassButton>
        </div>
      </div>
    </GlassModal>
  );
};
