import React from 'react';
import '../../styles/Navigation.css';

interface NavbarProps {
  activePage: string;
  onNavigate: (page: string) => void;
}

const Navbar: React.FC<NavbarProps> = ({ activePage, onNavigate }) => {
  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <h1>Hungarian OCR Tool</h1>
      </div>
      <ul className="navbar-menu">
        <li 
          className={`navbar-item ${activePage === 'extract' ? 'active' : ''}`}
          onClick={() => onNavigate('extract')}
        >
          Extract Text
        </li>
        <li 
          className={`navbar-item ${activePage === 'summarize' ? 'active' : ''}`}
          onClick={() => onNavigate('summarize')}
        >
          Summarize Text
        </li>
      </ul>
    </nav>
  );
};

export default Navbar; 