import { useState } from 'react';
import { extractText } from '../services/api';
import { OcrResponse, ApiError } from '../types';
import { OcrSettings, defaultOcrSettings } from '../models/ocr';
import axios from 'axios';

interface OcrState {
  extractedText: string;
  message: string;
  error: string;
  isLoading: boolean;
}

export const useOcr = () => {
  const [state, setState] = useState<OcrState>({
    extractedText: '',
    message: '',
    error: '',
    isLoading: false,
  });
  
  const [settings, setSettings] = useState<OcrSettings>(defaultOcrSettings);

  const resetState = () => {
    setState({
      extractedText: '',
      message: '',
      error: '',
      isLoading: false,
    });
  };

  const processImage = async (file: File) => {
    setState(prev => ({ ...prev, isLoading: true, extractedText: '', message: '', error: '' }));
    
    try {
      const response = await extractText(file, settings);
      
      setState(prev => ({
        ...prev,
        extractedText: response.text || '',
        message: response.message || '',
        isLoading: false,
      }));
    } catch (err) {
      console.error('Error:', err);
      
      let errorMessage = 'Error connecting to the server. Please try again.';
      
      if (axios.isAxiosError(err) && err.response) {
        const apiError = err.response.data as ApiError;
        errorMessage = apiError.detail || errorMessage;
      }
      
      setState(prev => ({
        ...prev,
        error: errorMessage,
        isLoading: false,
      }));
    }
  };

  return {
    ...state,
    settings,
    setSettings,
    processImage,
    resetState,
  };
}; 