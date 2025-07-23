import React, { useState } from 'react';
import Header from '../../components/Header';
import ImageUploadForm from '../../components/ImageUpload';
import ResultsDisplay from '../../components/Results';
import LoadingSpinner from '../../components/LoadingSpinner';
import OcrSettings from '../../components/Settings';
import { useOcr } from '../../hooks/useOcr';

const ExtractPage: React.FC = () => {
  const { 
    extractedText, 
    message, 
    error, 
    isLoading, 
    settings,
    setSettings,
    processImage 
  } = useOcr();
  
  const [showSettings, setShowSettings] = useState(false);

  return (
    <div className="page-container">
      <Header 
        title="Extract Text" 
        subtitle="Upload an image or PDF to extract Hungarian text" 
      />
      
      <div className="settings-toggle">
        <button 
          className="settings-toggle-button"
          onClick={() => setShowSettings(!showSettings)}
        >
          {showSettings ? 'Hide Settings' : 'Show PDF Settings'}
        </button>
      </div>
      
      {showSettings && (
        <OcrSettings 
          settings={settings}
          onChange={setSettings}
        />
      )}
      
      <main>
        <ImageUploadForm 
          onUpload={processImage} 
          error={error} 
          isLoading={isLoading} 
        />
        
        {isLoading && <LoadingSpinner />}
        
        <ResultsDisplay 
          extractedText={extractedText} 
          message={message} 
        />
      </main>
    </div>
  );
};

export default ExtractPage; 