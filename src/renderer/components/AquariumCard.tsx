import React from 'react';
import { GlassCard } from './common';
import type { Aquarium } from '../../shared/types';

interface AquariumCardProps {
  aquarium: Aquarium;
}

export const AquariumCard: React.FC<AquariumCardProps> = ({ aquarium }) => {
  const [thumbnailSrc, setThumbnailSrc] = React.useState<string>('');
  const [imageError, setImageError] = React.useState(false);

  React.useEffect(() => {
    if (aquarium.thumbnailPath) {
      // Get the full path to the thumbnail
      window.electron.files.getThumbnailPath(aquarium.thumbnailPath)
        .then((result) => {
          if (result.success && result.data) {
            setThumbnailSrc(`file://${result.data}`);
          }
        })
        .catch(() => setImageError(true));
    }
  }, [aquarium.thumbnailPath]);

  // Generate default SVG placeholder if no thumbnail
  const defaultThumbnail = `data:image/svg+xml,${encodeURIComponent(`
    <svg width="400" height="300" xmlns="http://www.w3.org/2000/svg">
      <defs>
        <linearGradient id="water-gradient" x1="0%" y1="0%" x2="0%" y2="100%">
          <stop offset="0%" style="stop-color:${aquarium.type === 'marine' ? '#1e40af' : '#059669'};stop-opacity:0.8" />
          <stop offset="100%" style="stop-color:${aquarium.type === 'marine' ? '#1e3a8a' : '#047857'};stop-opacity:1" />
        </linearGradient>
      </defs>
      <rect width="400" height="300" fill="url(#water-gradient)"/>
      <g opacity="0.3">
        <path d="M 50 150 Q 100 130, 150 150 T 250 150 T 350 150" stroke="white" stroke-width="2" fill="none"/>
        <path d="M 30 180 Q 80 160, 130 180 T 230 180 T 330 180" stroke="white" stroke-width="2" fill="none"/>
        <path d="M 70 210 Q 120 190, 170 210 T 270 210 T 370 210" stroke="white" stroke-width="2" fill="none"/>
      </g>
      <circle cx="300" cy="100" r="8" fill="white" opacity="0.6"/>
      <circle cx="320" cy="110" r="6" fill="white" opacity="0.5"/>
      <circle cx="310" cy="130" r="4" fill="white" opacity="0.4"/>
    </svg>
  `)}`;

  const displayThumbnail = !imageError && thumbnailSrc ? thumbnailSrc : defaultThumbnail;

  // Icon for aquarium type
  const TypeIcon = aquarium.type === 'marine' ? (
    // Marine icon (waves)
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8c0 0 3-2 6 0s6 0 6 0 3 2 6 0M3 14c0 0 3-2 6 0s6 0 6 0 3 2 6 0M3 20c0 0 3-2 6 0s6 0 6 0 3 2 6 0" />
    </svg>
  ) : (
    // Freshwater icon (droplet)
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
    </svg>
  );

  return (
    <GlassCard className="overflow-hidden cursor-pointer transition-all duration-300 hover:scale-105 hover:shadow-2xl group">
      {/* Thumbnail */}
      <div className="relative w-full h-48 overflow-hidden">
        <img
          src={displayThumbnail}
          alt={aquarium.name}
          className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-110"
          onError={() => setImageError(true)}
        />
        {/* Type badge */}
        <div className="absolute top-3 right-3 flex items-center gap-2 px-3 py-1.5 rounded-full bg-white/20 backdrop-blur-md border border-white/30">
          {TypeIcon}
          <span className="text-white text-sm font-medium capitalize">
            {aquarium.type}
          </span>
        </div>
      </div>

      {/* Card content */}
      <div className="p-4">
        <h3 className="text-xl font-bold text-white mb-2 truncate">
          {aquarium.name}
        </h3>
        <div className="flex items-center gap-2 text-white/70 text-sm">
          <span>
            {aquarium.volume.value} {aquarium.volume.unit}
          </span>
          <span>•</span>
          <span>
            {aquarium.dimensions.width} × {aquarium.dimensions.length} × {aquarium.dimensions.height} {aquarium.dimensions.unit}
          </span>
        </div>
      </div>
    </GlassCard>
  );
};
