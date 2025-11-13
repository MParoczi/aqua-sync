import React from 'react';
import ReactDOM from 'react-dom/client';
import { ThemeProvider } from './contexts/ThemeContext';
import { ToastProvider } from './contexts/ToastContext';
import { AquariumProvider } from './contexts/AquariumContext';
import App from './App';
import './index.css';

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);

root.render(
  <React.StrictMode>
    <ThemeProvider>
      <ToastProvider>
        <AquariumProvider>
          <App />
        </AquariumProvider>
      </ToastProvider>
    </ThemeProvider>
  </React.StrictMode>
);
