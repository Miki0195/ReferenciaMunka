export interface OcrResponse {
  text: string;
  message?: string;
}

export interface ApiError {
  detail: string;
}

export interface OcrSettings {
  remove_headers: boolean;
  remove_footers: boolean;
  remove_duplicate_lines: boolean;
  remove_page_numbers: boolean;
}

export interface SummarizeResponse {
  summary: string;
  bulletPoints: string[];
} 