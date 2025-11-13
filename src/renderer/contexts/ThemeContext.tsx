import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';

type Theme = 'light' | 'dark';

interface ThemeContextType {
  theme: Theme;
  toggleTheme: () => void;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export const useTheme = () => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
};

interface ThemeProviderProps {
  children: ReactNode;
}

export const ThemeProvider: React.FC<ThemeProviderProps> = ({ children }) => {
  const [theme, setTheme] = useState<Theme>('light');

  useEffect(() => {
    // Detect system theme preference
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    const systemTheme: Theme = mediaQuery.matches ? 'dark' : 'light';
    setTheme(systemTheme);

    // Apply theme class to body
    document.body.classList.remove('light', 'dark');
    document.body.classList.add(systemTheme);

    // Listen for system theme changes
    const handleThemeChange = (e: MediaQueryListEvent) => {
      const newTheme: Theme = e.matches ? 'dark' : 'light';
      setTheme(newTheme);
      document.body.classList.remove('light', 'dark');
      document.body.classList.add(newTheme);
    };

    mediaQuery.addEventListener('change', handleThemeChange);

    return () => {
      mediaQuery.removeEventListener('change', handleThemeChange);
    };
  }, []);

  useEffect(() => {
    // Apply theme class when theme changes
    document.body.classList.remove('light', 'dark');
    document.body.classList.add(theme);
  }, [theme]);

  const toggleTheme = () => {
    setTheme(prev => prev === 'light' ? 'dark' : 'light');
  };

  return (
    <ThemeContext.Provider value={{ theme, toggleTheme }}>
      {children}
    </ThemeContext.Provider>
  );
};
