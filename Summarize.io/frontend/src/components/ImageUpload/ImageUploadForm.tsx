import React, { useState } from 'react';
import '../../styles/ImageUpload.css';

interface ImageUploadFormProps {
  onUpload: (file: File) => void;
  error: string;
  isLoading: boolean;
}

const ImageUploadForm: React.FC<ImageUploadFormProps> = ({ onUpload, error, isLoading }) => {
  const [file, setFile] = useState<File | null>(null);
  const [validationError, setValidationError] = useState<string>('');

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.target.files?.[0];
    setValidationError('');
    
    if (!selectedFile) {
      setFile(null);
      return;
    }
    
    // Check file type
    const validTypes = ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf'];
    if (!validTypes.includes(selectedFile.type)) {
      setValidationError('Please select a valid file (JPG, JPEG, PNG, or PDF)');
      setFile(null);
      return;
    }
    
    setFile(selectedFile);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!file) {
      setValidationError('Please select a file first');
      return;
    }
    
    onUpload(file);
  };

  const displayError = validationError || error;

  return (
    <section className="upload-section">
      <form onSubmit={handleSubmit}>
        <div className="file-input-container">
          <label htmlFor="file-upload" className="file-input-label">
            Select an image or PDF file:
          </label>
          <input
            id="file-upload"
            type="file"
            accept=".jpg,.jpeg,.png,.pdf"
            onChange={handleFileChange}
            className="file-input"
          />
          <div className="file-format-note">
            Supported formats: JPG, JPEG, PNG, PDF
          </div>
        </div>
        
        {displayError && <div className="error-message">{displayError}</div>}
        
        <button 
          type="submit" 
          className="submit-button" 
          disabled={!file || isLoading}
        >
          {isLoading ? 'Processing...' : 'Extract Text'}
        </button>
      </form>
    </section>
  );
};

export default ImageUploadForm; 