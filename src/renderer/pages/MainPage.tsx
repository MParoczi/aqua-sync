import React, { useState } from 'react';
import { GlassCard, GlassButton } from '../components/common';
import { useAquarium } from '../contexts/AquariumContext';

// Section types for navigation
type Section = 'dashboard' | 'filters' | 'lamps' | 'tests';

// Navigation menu items configuration
const navigationItems: { id: Section; label: string; icon: string }[] = [
  { id: 'dashboard', label: 'Dashboard', icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6' },
  { id: 'filters', label: 'Filters', icon: 'M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z' },
  { id: 'lamps', label: 'Lamps', icon: 'M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z' },
  { id: 'tests', label: 'Tests', icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01' },
];

// Placeholder content components
const DashboardContent: React.FC = () => (
  <div className="space-y-6">
    <div>
      <h2 className="text-3xl font-bold text-white mb-2">Dashboard</h2>
      <p className="text-white/60">Overview of your aquarium</p>
    </div>

    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <GlassCard className="p-6">
        <h3 className="text-xl font-bold text-white mb-4">Quick Stats</h3>
        <p className="text-white/70">
          Statistics and quick actions will be implemented in US-012 to US-015
        </p>
      </GlassCard>

      <GlassCard className="p-6">
        <h3 className="text-xl font-bold text-white mb-4">Recent Activity</h3>
        <p className="text-white/70">
          Activity feed will be implemented in future user stories
        </p>
      </GlassCard>

      <GlassCard className="p-6">
        <h3 className="text-xl font-bold text-white mb-4">Alerts</h3>
        <p className="text-white/70">
          Maintenance alerts and notifications will be implemented in Epic 7
        </p>
      </GlassCard>

      <GlassCard className="p-6">
        <h3 className="text-xl font-bold text-white mb-4">Quick Actions</h3>
        <p className="text-white/70">
          Quick action buttons will be added in future implementations
        </p>
      </GlassCard>
    </div>
  </div>
);

const FiltersContent: React.FC = () => (
  <div className="space-y-6">
    <div>
      <h2 className="text-3xl font-bold text-white mb-2">Filters</h2>
      <p className="text-white/60">Manage your filtration systems</p>
    </div>

    <GlassCard className="p-6">
      <p className="text-white/70">
        Filter management will be implemented in Epic 5 (US-019 to US-028)
      </p>
    </GlassCard>
  </div>
);

const LampsContent: React.FC = () => (
  <div className="space-y-6">
    <div>
      <h2 className="text-3xl font-bold text-white mb-2">Lamps</h2>
      <p className="text-white/60">Control your lighting systems</p>
    </div>

    <GlassCard className="p-6">
      <p className="text-white/70">
        Lamp management will be implemented in Epic 6 (US-029 to US-037)
      </p>
    </GlassCard>
  </div>
);

const TestsContent: React.FC = () => (
  <div className="space-y-6">
    <div>
      <h2 className="text-3xl font-bold text-white mb-2">Water Tests</h2>
      <p className="text-white/60">Track your water parameters</p>
    </div>

    <GlassCard className="p-6">
      <p className="text-white/70">
        Water test tracking will be implemented in Epic 4 (US-016 to US-018)
      </p>
    </GlassCard>
  </div>
);

export const MainPage: React.FC = () => {
  const { selectedAquarium, selectAquarium } = useAquarium();
  const [activeSection, setActiveSection] = useState<Section>('dashboard');

  // If no aquarium is selected, show error state
  if (!selectedAquarium) {
    return (
      <div className="min-h-screen flex items-center justify-center p-8">
        <GlassCard className="p-12 max-w-2xl w-full text-center">
          <h1 className="text-3xl font-bold text-white mb-4">
            No Aquarium Selected
          </h1>
          <p className="text-white/80 text-lg mb-6">
            Please select an aquarium from the landing page.
          </p>
          <GlassButton
            variant="primary"
            onClick={() => selectAquarium(null)}
            className="text-lg px-6 py-3"
          >
            Back to Aquariums
          </GlassButton>
        </GlassCard>
      </div>
    );
  }

  // Render content based on active section
  const renderContent = () => {
    switch (activeSection) {
      case 'dashboard':
        return <DashboardContent />;
      case 'filters':
        return <FiltersContent />;
      case 'lamps':
        return <LampsContent />;
      case 'tests':
        return <TestsContent />;
      default:
        return <DashboardContent />;
    }
  };

  // Main page with sidebar layout
  return (
    <div className="min-h-screen flex">
      {/* Left Sidebar - 28% width, fixed */}
      <aside className="w-[28%] min-h-screen p-6 flex flex-col border-r border-white/10">
        {/* Header with aquarium info */}
        <div className="mb-8">
          <GlassButton
            variant="secondary"
            onClick={() => selectAquarium(null)}
            className="mb-6 w-full justify-center"
          >
            <svg className="w-5 h-5 mr-2 inline-block" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
            Back to Aquariums
          </GlassButton>

          <h1 className="text-2xl font-bold text-white mb-2 break-words">
            {selectedAquarium.name}
          </h1>
          <div className="flex flex-col gap-1 text-white/60 text-sm">
            <span className="capitalize">{selectedAquarium.type}</span>
            <span>
              {selectedAquarium.volume.value} {selectedAquarium.volume.unit}
            </span>
            <span>
              {selectedAquarium.dimensions.width} × {selectedAquarium.dimensions.length} × {selectedAquarium.dimensions.height} {selectedAquarium.dimensions.unit}
            </span>
          </div>
        </div>

        {/* Navigation Menu */}
        <nav className="flex-1">
          <div className="space-y-2">
            {navigationItems.map((item) => (
              <button
                key={item.id}
                onClick={() => setActiveSection(item.id)}
                className={`
                  w-full flex items-center gap-3 px-4 py-3 rounded-lg
                  transition-all duration-200
                  ${
                    activeSection === item.id
                      ? 'bg-white/20 border border-white/30 shadow-lg'
                      : 'bg-white/5 border border-white/10 hover:bg-white/10 hover:border-white/20'
                  }
                `}
              >
                <svg
                  className="w-5 h-5 text-white/90"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d={item.icon}
                  />
                </svg>
                <span
                  className={`
                    text-base font-medium
                    ${activeSection === item.id ? 'text-white' : 'text-white/70'}
                  `}
                >
                  {item.label}
                </span>
              </button>
            ))}
          </div>
        </nav>
      </aside>

      {/* Right Content Area - 72% width, scrollable */}
      <main className="flex-1 w-[72%] min-h-screen overflow-y-auto p-8">
        {renderContent()}
      </main>
    </div>
  );
};
