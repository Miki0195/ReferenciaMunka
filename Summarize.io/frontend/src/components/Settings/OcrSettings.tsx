import React from 'react';
import { OcrSettings as OcrSettingsType } from '../../models/ocr';
import '../../styles/Settings.css';

interface OcrSettingsProps {
  settings: OcrSettingsType;
  onChange: (settings: OcrSettingsType) => void;
}

const OcrSettings: React.FC<OcrSettingsProps> = ({ settings, onChange }) => {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, checked } = e.target;
    onChange({
      ...settings,
      [name]: checked,
    });
  };

  return (
    <div className="settings-panel">
      <h3 className="settings-title">PDF Processing Options</h3>
      <div className="settings-description">
        Configure how PDFs are processed to improve text extraction quality
      </div>
      
      <div className="settings-group">
        <label className="settings-option">
          <input
            type="checkbox"
            name="removeHeaders"
            checked={settings.removeHeaders}
            onChange={handleChange}
          />
          <div className="settings-option-text">
            <span className="settings-label">Remove Headers</span>
            <span className="settings-help">Remove repetitive text from the top of each page</span>
          </div>
        </label>
        
        <label className="settings-option">
          <input
            type="checkbox"
            name="removeFooters"
            checked={settings.removeFooters}
            onChange={handleChange}
          />
          <div className="settings-option-text">
            <span className="settings-label">Remove Footers</span>
            <span className="settings-help">Remove repetitive text from the bottom of each page</span>
          </div>
        </label>
        
        <label className="settings-option">
          <input
            type="checkbox"
            name="removePageNumbers"
            checked={settings.removePageNumbers}
            onChange={handleChange}
          />
          <div className="settings-option-text">
            <span className="settings-label">Remove Page Numbers</span>
            <span className="settings-help">Remove page numbers and pagination marks (Under developement)</span>
          </div>
        </label>
        
        <label className="settings-option">
          <input
            type="checkbox"
            name="removeDuplicateLines"
            checked={settings.removeDuplicateLines}
            onChange={handleChange}
          />
          <div className="settings-option-text">
            <span className="settings-label">Remove Duplicate Lines</span>
            <span className="settings-help">Remove lines that appear on multiple pages</span>
          </div>
        </label>
      </div>
    </div>
  );
};

export default OcrSettings; 