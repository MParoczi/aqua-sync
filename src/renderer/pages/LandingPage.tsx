import React, { useState, useEffect } from 'react';
import { GlassCard, GlassButton, DeleteConfirmationModal } from '../components/common';
import type { Aquarium } from '../../shared/types';
import { AquariumModal } from '../components/AquariumModal';
import { AquariumGrid } from '../components/AquariumGrid';
import { useToast } from '../contexts/ToastContext';
import { useAquarium } from '../contexts/AquariumContext';

export const LandingPage: React.FC = () => {
  const { showSuccess, showError } = useToast();
  const { selectAquarium } = useAquarium();
  const [aquariums, setAquariums] = useState<Aquarium[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingAquarium, setEditingAquarium] = useState<Aquarium | undefined>(undefined);
  const [deletingAquarium, setDeletingAquarium] = useState<Aquarium | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

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
    setEditingAquarium(undefined);
  };

  const handleAquariumUpdated = () => {
    // Reload aquariums after update
    loadAquariums();
    setIsModalOpen(false);
    setEditingAquarium(undefined);
  };

  const handleEdit = (aquarium: Aquarium) => {
    setEditingAquarium(aquarium);
    setIsModalOpen(true);
  };

  const handleDelete = (aquarium: Aquarium) => {
    setDeletingAquarium(aquarium);
  };

  const handleConfirmDelete = async () => {
    if (!deletingAquarium) return;

    setIsDeleting(true);
    try {
      const result = await window.electron.data.deleteAquarium(deletingAquarium.id);
      if (result.success) {
        showSuccess(`Aquarium "${deletingAquarium.name}" deleted successfully!`);
        loadAquariums();
        setDeletingAquarium(null);
      } else {
        showError(result.error || 'Failed to delete aquarium');
      }
    } catch (error) {
      console.error('Error deleting aquarium:', error);
      showError('An unexpected error occurred');
    } finally {
      setIsDeleting(false);
    }
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingAquarium(undefined);
  };

  const handleCardClick = (aquarium: Aquarium) => {
    selectAquarium(aquarium);
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
          onClose={handleCloseModal}
          onAquariumCreated={handleAquariumCreated}
        />

        <DeleteConfirmationModal
          isOpen={!!deletingAquarium}
          onClose={() => setDeletingAquarium(null)}
          onConfirm={handleConfirmDelete}
          itemName={deletingAquarium?.name || ''}
          isDeleting={isDeleting}
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
        onEdit={handleEdit}
        onDelete={handleDelete}
        onClick={handleCardClick}
      />

      <AquariumModal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        onAquariumCreated={handleAquariumCreated}
        onAquariumUpdated={handleAquariumUpdated}
        aquarium={editingAquarium}
      />

      <DeleteConfirmationModal
        isOpen={!!deletingAquarium}
        onClose={() => setDeletingAquarium(null)}
        onConfirm={handleConfirmDelete}
        itemName={deletingAquarium?.name || ''}
        isDeleting={isDeleting}
      />
    </>
  );
};
