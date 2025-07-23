import { OcrSettings as ApiOcrSettings } from '../types';

export interface OcrSettings {
  removeHeaders: boolean;
  removeFooters: boolean;
  removeDuplicateLines: boolean;
  removePageNumbers: boolean;
}

export const defaultOcrSettings: OcrSettings = {
  removeHeaders: true,
  removeFooters: true,
  removeDuplicateLines: false,
  removePageNumbers: true,
};

// Convert frontend model to API model
export const toApiSettings = (settings: OcrSettings): ApiOcrSettings => ({
  remove_headers: settings.removeHeaders,
  remove_footers: settings.removeFooters,
  remove_duplicate_lines: settings.removeDuplicateLines,
  remove_page_numbers: settings.removePageNumbers,
}); 