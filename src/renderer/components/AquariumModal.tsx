import React, { useState, useEffect } from 'react';
import { GlassModal, GlassButton } from './common';
import { useToast } from '../contexts/ToastContext';
import type { Aquarium } from '../../shared/types';

interface AquariumModalProps {
  isOpen: boolean;
  onClose: () => void;
  onAquariumCreated?: () => void;
  onAquariumUpdated?: () => void;
  aquarium?: Aquarium; // For editing (US-008)
}

type DimensionUnit = 'cm' | 'inch';
type VolumeUnit = 'liter' | 'gallon';

interface FormData {
  name: string;
  type: 'freshwater' | 'marine';
  width: string;
  length: string;
  height: string;
  dimensionUnit: DimensionUnit;
  volume: string;
  volumeUnit: VolumeUnit;
  isCustomVolume: boolean;
  startDate: string;
  thumbnail: File | null;
}

interface FormErrors {
  name?: string;
  width?: string;
  length?: string;
  height?: string;
  volume?: string;
  startDate?: string;
}

/**
 * Aquarium Creation/Edit Modal - US-006
 */
export const AquariumModal: React.FC<AquariumModalProps> = ({
  isOpen,
  onClose,
  onAquariumCreated,
  onAquariumUpdated,
  aquarium,
}) => {
  const isEditMode = !!aquarium;
  const { showSuccess, showError } = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [thumbnailPreview, setThumbnailPreview] = useState<string | null>(null);
  const [formData, setFormData] = useState<FormData>({
    name: '',
    type: 'freshwater',
    width: '',
    length: '',
    height: '',
    dimensionUnit: 'cm',
    volume: '',
    volumeUnit: 'liter',
    isCustomVolume: false,
    startDate: new Date().toISOString().split('T')[0],
    thumbnail: null,
  });
  const [errors, setErrors] = useState<FormErrors>({});

  // Reset form when modal opens/closes
  useEffect(() => {
    if (isOpen) {
      if (aquarium) {
        // Pre-fill form for editing (US-008)
        setFormData({
          name: aquarium.name,
          type: aquarium.type,
          width: aquarium.dimensions.width.toString(),
          length: aquarium.dimensions.length.toString(),
          height: aquarium.dimensions.height.toString(),
          dimensionUnit: aquarium.dimensions.unit,
          volume: aquarium.volume.value.toString(),
          volumeUnit: aquarium.volume.unit,
          isCustomVolume: aquarium.volume.isCustom,
          startDate: aquarium.startDate.split('T')[0],
          thumbnail: null,
        });
        if (aquarium.thumbnailPath) {
          // TODO: Load existing thumbnail preview
        }
      } else {
        // Reset form for new aquarium
        setFormData({
          name: '',
          type: 'freshwater',
          width: '',
          length: '',
          height: '',
          dimensionUnit: 'cm',
          volume: '',
          volumeUnit: 'liter',
          isCustomVolume: false,
          startDate: new Date().toISOString().split('T')[0],
          thumbnail: null,
        });
        setThumbnailPreview(null);
      }
      setErrors({});
    }
  }, [isOpen, aquarium]);

  // Auto-calculate volume when dimensions change
  useEffect(() => {
    if (!formData.isCustomVolume && formData.width && formData.length && formData.height) {
      const width = parseFloat(formData.width);
      const length = parseFloat(formData.length);
      const height = parseFloat(formData.height);

      if (!isNaN(width) && !isNaN(height) && !isNaN(length) && width > 0 && length > 0 && height > 0) {
        const calculatedVolume = calculateVolume(
          width,
          length,
          height,
          formData.dimensionUnit,
          formData.volumeUnit
        );
        setFormData((prev) => ({
          ...prev,
          volume: calculatedVolume.toFixed(2),
        }));
      }
    }
  }, [
    formData.width,
    formData.length,
    formData.height,
    formData.dimensionUnit,
    formData.volumeUnit,
    formData.isCustomVolume,
  ]);

  const calculateVolume = (
    width: number,
    length: number,
    height: number,
    dimensionUnit: DimensionUnit,
    volumeUnit: VolumeUnit
  ): number => {
    const volumeInCubicUnits = width * length * height;

    // Convert based on units
    if (dimensionUnit === 'cm') {
      // Volume in cubic cm, convert to liters or gallons
      const volumeInLiters = volumeInCubicUnits / 1000;
      return volumeUnit === 'liter' ? volumeInLiters : volumeInLiters * 0.264172; // 1 liter = 0.264172 gallons
    } else {
      // Dimensions in inches, volume in cubic inches
      const volumeInGallons = volumeInCubicUnits / 231; // 231 cubic inches = 1 gallon
      return volumeUnit === 'gallon' ? volumeInGallons : volumeInGallons * 3.78541; // 1 gallon = 3.78541 liters
    }
  };

  const handleInputChange = (field: keyof FormData, value: string | boolean | File | null) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }));

    // Clear error for this field
    if (errors[field as keyof FormErrors]) {
      setErrors((prev) => {
        const newErrors = { ...prev };
        delete newErrors[field as keyof FormErrors];
        return newErrors;
      });
    }
  };

  const handleDimensionUnitChange = (unit: DimensionUnit) => {
    setFormData((prev) => ({
      ...prev,
      dimensionUnit: unit,
    }));
  };

  const handleVolumeUnitChange = (unit: VolumeUnit) => {
    setFormData((prev) => ({
      ...prev,
      volumeUnit: unit,
    }));
  };

  const handleVolumeChange = (value: string) => {
    setFormData((prev) => ({
      ...prev,
      volume: value,
      isCustomVolume: true, // Mark as custom when user manually edits
    }));

    // Clear volume error
    if (errors.volume) {
      setErrors((prev) => {
        const newErrors = { ...prev };
        delete newErrors.volume;
        return newErrors;
      });
    }
  };

  const handleThumbnailChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // Validate file type
      if (!file.type.startsWith('image/')) {
        showError('Please select a valid image file');
        return;
      }

      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        showError('Image size must be less than 5MB');
        return;
      }

      setFormData((prev) => ({ ...prev, thumbnail: file }));

      // Create preview
      const reader = new FileReader();
      reader.onloadend = () => {
        setThumbnailPreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    // Name validation
    if (!formData.name.trim()) {
      newErrors.name = 'Name is required';
    } else if (formData.name.trim().length < 2) {
      newErrors.name = 'Name must be at least 2 characters';
    }

    // Dimensions validation
    const width = parseFloat(formData.width);
    const length = parseFloat(formData.length);
    const height = parseFloat(formData.height);

    if (!formData.width || isNaN(width) || width <= 0) {
      newErrors.width = 'Valid width is required';
    }

    if (!formData.length || isNaN(length) || length <= 0) {
      newErrors.length = 'Valid length is required';
    }

    if (!formData.height || isNaN(height) || height <= 0) {
      newErrors.height = 'Valid height is required';
    }

    // Volume validation
    const volume = parseFloat(formData.volume);
    if (!formData.volume || isNaN(volume) || volume <= 0) {
      newErrors.volume = 'Valid volume is required';
    }

    // Start date validation
    if (!formData.startDate) {
      newErrors.startDate = 'Start date is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      showError('Please fix the errors in the form');
      return;
    }

    setIsSubmitting(true);

    try {
      let thumbnailPath: string | undefined;

      // Upload thumbnail if present
      if (formData.thumbnail) {
        const result = await window.electron.files.copyThumbnail(formData.thumbnail);
        if (result.success && result.data) {
          thumbnailPath = result.data;
        } else {
          showError('Failed to upload thumbnail');
          setIsSubmitting(false);
          return;
        }
      }

      // Prepare aquarium data
      const aquariumData = {
        name: formData.name.trim(),
        type: formData.type,
        dimensions: {
          width: parseFloat(formData.width),
          length: parseFloat(formData.length),
          height: parseFloat(formData.height),
          unit: formData.dimensionUnit,
        },
        volume: {
          value: parseFloat(formData.volume),
          unit: formData.volumeUnit,
          isCustom: formData.isCustomVolume,
        },
        startDate: new Date(formData.startDate).toISOString(),
        thumbnailPath: thumbnailPath || aquarium?.thumbnailPath,
      };

      let result;
      if (isEditMode && aquarium) {
        // Update existing aquarium
        result = await window.electron.data.updateAquarium(aquarium.id, aquariumData);
        if (result.success) {
          showSuccess('Aquarium updated successfully!');
          onAquariumUpdated?.();
          onClose();
        } else {
          showError(result.error || 'Failed to update aquarium');
        }
      } else {
        // Create new aquarium
        result = await window.electron.data.createAquarium(aquariumData);
        if (result.success) {
          showSuccess('Aquarium created successfully!');
          onAquariumCreated?.();
          onClose();
        } else {
          showError(result.error || 'Failed to create aquarium');
        }
      }
    } catch (error) {
      console.error('Error creating aquarium:', error);
      showError('An unexpected error occurred');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    if (isSubmitting) return;
    onClose();
  };

  return (
    <GlassModal
      isOpen={isOpen}
      onClose={handleCancel}
      title={isEditMode ? 'Edit Aquarium' : 'Create Aquarium'}
    >
      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Name Field */}
        <div>
          <label className="block text-white/90 text-sm font-medium mb-2">
            Name <span className="text-red-400">*</span>
          </label>
          <input
            type="text"
            value={formData.name}
            onChange={(e) => handleInputChange('name', e.target.value)}
            placeholder="My Aquarium"
            className={`w-full px-4 py-2 rounded-lg bg-white/10 border ${
              errors.name ? 'border-red-400' : 'border-white/20'
            } text-white placeholder-white/40 focus:outline-none focus:border-white/40 transition-colors`}
          />
          {errors.name && <p className="text-red-400 text-xs mt-1">{errors.name}</p>}
        </div>

        {/* Type Field */}
        <div>
          <label className="block text-white/90 text-sm font-medium mb-2">
            Type <span className="text-red-400">*</span>
          </label>
          <div className="flex gap-4">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="radio"
                name="type"
                value="freshwater"
                checked={formData.type === 'freshwater'}
                onChange={(e) => handleInputChange('type', e.target.value)}
                className="w-4 h-4 text-blue-500"
              />
              <span className="text-white/90">Freshwater</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="radio"
                name="type"
                value="marine"
                checked={formData.type === 'marine'}
                onChange={(e) => handleInputChange('type', e.target.value)}
                className="w-4 h-4 text-blue-500"
              />
              <span className="text-white/90">Marine</span>
            </label>
          </div>
        </div>

        {/* Dimensions Section */}
        <div>
          <label className="block text-white/90 text-sm font-medium mb-2">
            Dimensions <span className="text-red-400">*</span>
          </label>

          {/* Unit Selector */}
          <div className="flex gap-4 mb-3">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="radio"
                name="dimensionUnit"
                value="cm"
                checked={formData.dimensionUnit === 'cm'}
                onChange={() => handleDimensionUnitChange('cm')}
                className="w-4 h-4"
              />
              <span className="text-white/90 text-sm">Centimetre</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="radio"
                name="dimensionUnit"
                value="inch"
                checked={formData.dimensionUnit === 'inch'}
                onChange={() => handleDimensionUnitChange('inch')}
                className="w-4 h-4"
              />
              <span className="text-white/90 text-sm">Inch</span>
            </label>
          </div>

          {/* Dimension Inputs */}
          <div className="grid grid-cols-3 gap-3">
            <div>
              <input
                type="number"
                step="0.01"
                min="0"
                value={formData.width}
                onChange={(e) => handleInputChange('width', e.target.value)}
                placeholder="Width"
                className={`w-full px-3 py-2 rounded-lg bg-white/10 border ${
                  errors.width ? 'border-red-400' : 'border-white/20'
                } text-white placeholder-white/40 focus:outline-none focus:border-white/40 transition-colors`}
              />
              {errors.width && <p className="text-red-400 text-xs mt-1">{errors.width}</p>}
            </div>
            <div>
              <input
                type="number"
                step="0.01"
                min="0"
                value={formData.length}
                onChange={(e) => handleInputChange('length', e.target.value)}
                placeholder="Length"
                className={`w-full px-3 py-2 rounded-lg bg-white/10 border ${
                  errors.length ? 'border-red-400' : 'border-white/20'
                } text-white placeholder-white/40 focus:outline-none focus:border-white/40 transition-colors`}
              />
              {errors.length && <p className="text-red-400 text-xs mt-1">{errors.length}</p>}
            </div>
            <div>
              <input
                type="number"
                step="0.01"
                min="0"
                value={formData.height}
                onChange={(e) => handleInputChange('height', e.target.value)}
                placeholder="Height"
                className={`w-full px-3 py-2 rounded-lg bg-white/10 border ${
                  errors.height ? 'border-red-400' : 'border-white/20'
                } text-white placeholder-white/40 focus:outline-none focus:border-white/40 transition-colors`}
              />
              {errors.height && <p className="text-red-400 text-xs mt-1">{errors.height}</p>}
            </div>
          </div>
        </div>

        {/* Volume Section */}
        <div>
          <label className="block text-white/90 text-sm font-medium mb-2">
            Volume <span className="text-red-400">*</span>
            {!formData.isCustomVolume && (
              <span className="text-white/60 text-xs ml-2">(auto-calculated)</span>
            )}
          </label>

          {/* Volume Unit Selector */}
          <div className="flex gap-4 mb-3">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="radio"
                name="volumeUnit"
                value="liter"
                checked={formData.volumeUnit === 'liter'}
                onChange={() => handleVolumeUnitChange('liter')}
                className="w-4 h-4"
              />
              <span className="text-white/90 text-sm">Litre</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="radio"
                name="volumeUnit"
                value="gallon"
                checked={formData.volumeUnit === 'gallon'}
                onChange={() => handleVolumeUnitChange('gallon')}
                className="w-4 h-4"
              />
              <span className="text-white/90 text-sm">Gallon</span>
            </label>
          </div>

          {/* Volume Input */}
          <input
            type="number"
            step="0.01"
            min="0"
            value={formData.volume}
            onChange={(e) => handleVolumeChange(e.target.value)}
            placeholder="Volume"
            className={`w-full px-4 py-2 rounded-lg bg-white/10 border ${
              errors.volume ? 'border-red-400' : 'border-white/20'
            } text-white placeholder-white/40 focus:outline-none focus:border-white/40 transition-colors`}
          />
          {errors.volume && <p className="text-red-400 text-xs mt-1">{errors.volume}</p>}
          {formData.isCustomVolume && (
            <p className="text-white/60 text-xs mt-1">
              Manual override active. Change dimensions to recalculate.
            </p>
          )}
        </div>

        {/* Start Date Field */}
        <div>
          <label className="block text-white/90 text-sm font-medium mb-2">
            Start Date <span className="text-red-400">*</span>
          </label>
          <input
            type="date"
            value={formData.startDate}
            onChange={(e) => handleInputChange('startDate', e.target.value)}
            className={`w-full px-4 py-2 rounded-lg bg-white/10 border ${
              errors.startDate ? 'border-red-400' : 'border-white/20'
            } text-white focus:outline-none focus:border-white/40 transition-colors`}
          />
          {errors.startDate && <p className="text-red-400 text-xs mt-1">{errors.startDate}</p>}
        </div>

        {/* Thumbnail Upload */}
        <div>
          <label className="block text-white/90 text-sm font-medium mb-2">
            Thumbnail (Optional)
          </label>
          <div className="flex items-start gap-4">
            <div className="flex-1">
              <input
                type="file"
                accept="image/*"
                onChange={handleThumbnailChange}
                className="w-full px-4 py-2 rounded-lg bg-white/10 border border-white/20 text-white file:mr-4 file:py-1 file:px-3 file:rounded file:border-0 file:bg-white/20 file:text-white file:cursor-pointer hover:file:bg-white/30 focus:outline-none"
              />
              <p className="text-white/60 text-xs mt-1">Max size: 5MB. Formats: JPG, PNG, GIF</p>
            </div>
            {thumbnailPreview && (
              <div className="w-20 h-20 rounded-lg overflow-hidden border border-white/20">
                <img
                  src={thumbnailPreview}
                  alt="Thumbnail preview"
                  className="w-full h-full object-cover"
                />
              </div>
            )}
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex gap-3 pt-4">
          <GlassButton
            type="button"
            variant="secondary"
            onClick={handleCancel}
            disabled={isSubmitting}
            className="flex-1"
          >
            Cancel
          </GlassButton>
          <GlassButton
            type="submit"
            variant="primary"
            disabled={isSubmitting}
            className="flex-1"
          >
            {isSubmitting
              ? isEditMode
                ? 'Updating...'
                : 'Creating...'
              : isEditMode
              ? 'Update Aquarium'
              : 'Create Aquarium'}
          </GlassButton>
        </div>
      </form>
    </GlassModal>
  );
};
