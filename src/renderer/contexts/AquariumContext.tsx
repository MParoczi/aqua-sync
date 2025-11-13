import React, { createContext, useContext, useState, ReactNode } from 'react';
import type { Aquarium } from '../../shared/types';

interface AquariumContextType {
  selectedAquarium: Aquarium | null;
  selectAquarium: (aquarium: Aquarium | null) => void;
}

const AquariumContext = createContext<AquariumContextType | undefined>(undefined);

export const AquariumProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [selectedAquarium, setSelectedAquarium] = useState<Aquarium | null>(null);

  const selectAquarium = (aquarium: Aquarium | null) => {
    setSelectedAquarium(aquarium);
  };

  return (
    <AquariumContext.Provider value={{ selectedAquarium, selectAquarium }}>
      {children}
    </AquariumContext.Provider>
  );
};

export const useAquarium = () => {
  const context = useContext(AquariumContext);
  if (context === undefined) {
    throw new Error('useAquarium must be used within an AquariumProvider');
  }
  return context;
};
