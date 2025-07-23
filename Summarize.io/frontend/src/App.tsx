import React, { useState } from 'react';
import Navbar from './components/Navigation';
import ExtractPage from './pages/Extract';
import SummarizePage from './pages/Summarize';
import './styles/index.css';

function App() {
  const [activePage, setActivePage] = useState<string>('extract');

  const handleNavigate = (page: string) => {
    setActivePage(page);
  };

  return (
    <div className="app-container">
      <Navbar 
        activePage={activePage} 
        onNavigate={handleNavigate} 
      />
      
      {activePage === 'extract' && <ExtractPage />}
      {activePage === 'summarize' && <SummarizePage />}
    </div>
  );
}

export default App;
