import React, { useState } from 'react';
import '../../styles/Results.css';
import { ClipboardIcon, CheckIcon } from '../icons';

interface ResultsDisplayProps {
  extractedText: string;
  message: string;
}

const ResultsDisplay: React.FC<ResultsDisplayProps> = ({ extractedText, message }) => {
  const [copySuccess, setCopySuccess] = useState<boolean>(false);

  const copyToClipboard = async () => {
    try {
      await navigator.clipboard.writeText(extractedText);
      setCopySuccess(true);
      
      // Reset the success state after 2 seconds
      setTimeout(() => {
        setCopySuccess(false);
      }, 2000);
    } catch (err) {
      console.error('Failed to copy text: ', err);
    }
  };
  
  const hasText = extractedText.trim().length > 0;

  return (
    <section className="results-section">
      <div className="results-header-container">
        <h2 className="results-header">Extracted Text:</h2>
        {hasText && (
          <button 
            className={`copy-button ${copySuccess ? 'copy-success' : ''}`}
            onClick={copyToClipboard}
            disabled={!hasText}
            aria-label="Copy to clipboard"
          >
            {copySuccess ? (
              <>
                <CheckIcon /> Copied!
              </>
            ) : (
              <>
                <ClipboardIcon /> Copy to Clipboard
              </>
            )}
          </button>
        )}
      </div>
      
      {message && <div className="info-message">{message}</div>}
      
      <div className="results-text">
        {extractedText}
      </div>
    </section>
  );
};

export default ResultsDisplay; 