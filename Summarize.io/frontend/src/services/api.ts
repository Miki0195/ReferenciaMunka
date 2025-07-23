import axios from 'axios';
import { OcrResponse, OcrSettings as ApiOcrSettings, SummarizeResponse } from '../types';
import { OcrSettings, toApiSettings } from '../models/ocr';

const API_URL = 'http://localhost:8000';

const api = axios.create({
  baseURL: API_URL,
});

export const extractText = async (file: File, settings?: OcrSettings): Promise<OcrResponse> => {
  const formData = new FormData();
  formData.append('file', file);
  
  // Add settings if provided
  if (settings) {
    const apiSettings = toApiSettings(settings);
    formData.append('settings_json', JSON.stringify(apiSettings));
  }
  
  const response = await api.post<OcrResponse>('/api/ocr/extract-text', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });
  
  return response.data;
};

export const summarizeText = async (
  input: { file?: File; text?: string }
): Promise<SummarizeResponse> => {
  const formData = new FormData();
  
  if (input.file) {
    formData.append('file', input.file);
  } else if (input.text) {
    formData.append('text', input.text);
  } else {
    throw new Error('Either file or text must be provided');
  }
  
  const response = await api.post<SummarizeResponse>('/api/summarize', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });
  
  return response.data;
};

export default api; 