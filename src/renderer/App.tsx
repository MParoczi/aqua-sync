import React from 'react';
import { LandingPage } from './pages/LandingPage';
import { MainPage } from './pages/MainPage';
import { useAquarium } from './contexts/AquariumContext';
import './App.css';

function App() {
  const { selectedAquarium } = useAquarium();

  // Simple navigation: if an aquarium is selected, show MainPage, otherwise show LandingPage
  return selectedAquarium ? <MainPage /> : <LandingPage />;
}

export default App;
