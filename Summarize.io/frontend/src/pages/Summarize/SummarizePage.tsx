import React, { useState } from 'react';
import Header from '../../components/Header';
import LoadingSpinner from '../../components/LoadingSpinner';
import { summarizeText } from '../../services/api';
import '../../styles/Summarize.css';

const SummarizePage: React.FC = () => {
  const [inputText, setInputText] = useState('');
  const [uploadedFile, setUploadedFile] = useState<File | null>(null);
  const [summary, setSummary] = useState('');
  const [bulletPoints, setBulletPoints] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [loadingState, setLoadingState] = useState('');
  const [error, setError] = useState('');

  const handleTextChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setInputText(e.target.value);
    setUploadedFile(null);
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setUploadedFile(e.target.files[0]);
      setInputText('');
    }
  };

  const generateSummary = async () => {
    setIsLoading(true);
    setError('');
    setSummary('');
    setBulletPoints([]);
    
    // Show progress states
    const updateLoadingState = (state: string) => {
      setLoadingState(state);
    };

    try {
      if (!uploadedFile && !inputText) {
        throw new Error('Please provide text or upload a file');
      }

      updateLoadingState('Preparing input...');
      
      const input = uploadedFile 
        ? { file: uploadedFile } 
        : { text: inputText };
      
      // Small delay to show loading states
      await new Promise(resolve => setTimeout(resolve, 500));
      updateLoadingState('Processing text with advanced NLP...');
      
      const response = await summarizeText(input);
      
      updateLoadingState('Finalizing results...');
      await new Promise(resolve => setTimeout(resolve, 500));
      
      setSummary(response.summary);
      setBulletPoints(response.bulletPoints);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An unknown error occurred');
    } finally {
      setIsLoading(false);
      setLoadingState('');
    }
  };

  return (
    <div className="page-container">
      <Header 
        title="Summarize Hungarian Text" 
        subtitle="Generate concise summaries and bullet points from Hungarian text" 
      />
      
      <main className="summarize-container">
        <div className="input-section">
          <div className="input-methods">
            <div className="text-input-container">
              <h3>Input Text</h3>
              <textarea 
                value={inputText}
                onChange={handleTextChange}
                placeholder="Paste Hungarian text here to summarize..."
                disabled={isLoading}
                className="text-input"
              />
            </div>
            
            <div className="file-upload-container">
              <h3>Or Upload a File</h3>
              <div className="file-input-wrapper">
                <input
                  type="file"
                  onChange={handleFileChange}
                  accept=".txt,.pdf,.doc,.docx,.png,.jpg,.jpeg"
                  disabled={isLoading}
                  id="file-upload"
                  className="file-input"
                />
                <label htmlFor="file-upload" className="file-label">
                  Choose File
                </label>
                {uploadedFile && (
                  <div className="file-name">{uploadedFile.name}</div>
                )}
              </div>
            </div>
          </div>
          
          {error && <div className="error-message">{error}</div>}
          
          <button 
            onClick={generateSummary}
            disabled={isLoading || (!inputText && !uploadedFile)}
            className="summarize-button"
          >
            Generate Summary
          </button>
        </div>
        
        {isLoading && (
          <div className="loading-container">
            <LoadingSpinner />
            <div className="loading-state">{loadingState}</div>
            <div className="loading-description">
              Using advanced NLP to analyze and summarize Hungarian text...
            </div>
          </div>
        )}
        
        {summary && (
          <div className="results-section">
            <div className="summary-container">
              <h3>Summary</h3>
              <div className="summary-text">{summary}</div>
            </div>
            
            {bulletPoints.length > 0 && (
              <div className="bullet-points-container">
                <h3>Key Points</h3>
                <ul className="bullet-points-list">
                  {bulletPoints.map((point, index) => (
                    <li key={index}>{point}</li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        )}
      </main>
      
      <div className="development-note">
        <p>This feature is using advanced NLP models optimized for Hungarian text to extract the most important information from your documents.</p>
      </div>
    </div>
  );
};

export default SummarizePage; 