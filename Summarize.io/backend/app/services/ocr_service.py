import os
import io
import tempfile
import re
from typing import List, Dict, Set, Optional
from difflib import SequenceMatcher
import pytesseract
from PIL import Image
from fastapi import UploadFile
from pypdf import PdfReader
from pdf2image import convert_from_bytes

from app.models.ocr import FileType, OcrSettings
from app.core.config import settings

# Standalone functions for reuse in other services
def extract_text_from_image(image_bytes: bytes) -> str:
    """Extract text from an image using OCR"""
    image = Image.open(io.BytesIO(image_bytes))
    
    # Extract text using pytesseract with Hungarian language model
    extracted_text = pytesseract.image_to_string(
        image, 
        lang=settings.TESSERACT_LANGUAGE
    )
    
    return extracted_text

def extract_text_from_pdf(pdf_bytes: bytes) -> str:
    """Extract text from a PDF file"""
    # First try to extract embedded text
    embedded_text = _extract_embedded_text_from_pdf(pdf_bytes)
    
    # If no text found, use OCR
    if not embedded_text or len(embedded_text.strip()) < 100:
        text = _extract_pdf_with_ocr(pdf_bytes)
        return text
    
    return embedded_text

def _extract_embedded_text_from_pdf(pdf_bytes: bytes) -> str:
    """Extract embedded text from PDF"""
    pdf_file = io.BytesIO(pdf_bytes)
    pdf_reader = PdfReader(pdf_file)
    
    text = ""
    for page in pdf_reader.pages:
        page_text = page.extract_text()
        if page_text:
            text += page_text + "\n\n"
    
    return text

def _extract_pdf_with_ocr(pdf_bytes: bytes) -> str:
    """Extract text from PDF using OCR"""
    # Convert PDF to images
    images = convert_from_bytes(pdf_bytes)
    
    # Extract text from each image
    texts = []
    for image in images:
        text = pytesseract.image_to_string(
            image, 
            lang=settings.TESSERACT_LANGUAGE
        )
        texts.append(text)
    
    return "\n\n".join(texts)

# Regular expression for matching page numbers
PAGE_NUMBER_PATTERNS = [
    r'\b\d+\s*$',                # Numbers at end of line: "42"
    r'^\s*\d+\s*$',              # Line with just a number: "  42  "
    r'^\s*Page\s+\d+\s*$',       # "Page 42"
    r'^\s*\d+\s+of\s+\d+\s*$',   # "42 of 100"
    r'^\s*-\s*\d+\s*-\s*$',      # "- 42 -"
    r'^\s*–\s*\d+\s*–\s*$',      # "– 42 –" (with en dash)
    r'^\s*—\s*\d+\s*—\s*$',      # "— 42 —" (with em dash)
    r'\b\d+\s*/\s*\d+\s*$',      # "42/100"
    r'\(\s*\d+\s*\)\s*$',        # "(42)"
    r'\[\s*\d+\s*\]\s*$',        # "[42]"
]

class OCRService:
    """Service for extracting text from images and PDFs using OCR"""
    
    # Regular expression for matching page numbers
    PAGE_NUMBER_PATTERNS = PAGE_NUMBER_PATTERNS
    
    def get_file_type(self, file: UploadFile) -> FileType:
        """Determine the file type based on file extension"""
        file_ext = os.path.splitext(file.filename)[1].lower()
        
        if file_ext in ['.jpg', '.jpeg', '.png']:
            return FileType.IMAGE
        elif file_ext == '.pdf':
            return FileType.PDF
        else:
            return FileType.UNSUPPORTED
    
    async def process_image(self, file: UploadFile) -> str:
        """Process an image file to extract text"""
        contents = await file.read()
        if not contents:
            return ""
        
        return extract_text_from_image(contents)
    
    async def process_pdf(self, file: UploadFile, ocr_settings: Optional[OcrSettings] = None) -> str:
        """Process a PDF file to extract text"""
        contents = await file.read()
        if not contents:
            return ""
        
        # Use default settings if none provided
        if ocr_settings is None:
            ocr_settings = OcrSettings()
        
        # First try to extract text directly from PDF
        page_texts = self._extract_text_from_pdf_pages(contents)
        
        # If no text found, use OCR on the PDF pages
        if not page_texts or all(len(text.strip()) < 50 for text in page_texts):
            page_texts = await self._extract_text_from_pdf_with_ocr(contents)
        
        # Clean the extracted text by removing headers and footers
        cleaned_text = self._clean_pdf_text(page_texts, ocr_settings)
        
        return cleaned_text
    
    def _extract_text_from_pdf_pages(self, pdf_bytes: bytes) -> List[str]:
        """Extract text from each PDF page separately"""
        pdf_file = io.BytesIO(pdf_bytes)
        pdf_reader = PdfReader(pdf_file)
        
        page_texts = []
        for page in pdf_reader.pages:
            page_text = page.extract_text()
            if page_text:
                page_texts.append(page_text)
        
        return page_texts
    
    async def _extract_text_from_pdf_with_ocr(self, pdf_bytes: bytes) -> List[str]:
        """Convert PDF to images and perform OCR on each page"""
        # Convert PDF to images
        images = convert_from_bytes(pdf_bytes)
        
        # Extract text from each image
        page_texts = []
        for image in images:
            text = pytesseract.image_to_string(
                image, 
                lang=settings.TESSERACT_LANGUAGE
            )
            page_texts.append(text)
        
        return page_texts
    
    def _clean_pdf_text(self, page_texts: List[str], ocr_settings: OcrSettings) -> str:
        """Clean PDF text by removing headers and footers based on settings"""
        if not page_texts:
            return ""
        
        # If there's only one page, just remove page numbers if requested
        if len(page_texts) <= 1:
            text = page_texts[0] if page_texts else ""
            if ocr_settings.remove_page_numbers:
                text = self._remove_page_numbers(text)
            return text
        
        # Find potential headers and footers
        headers = self._find_headers(page_texts) if ocr_settings.remove_headers else set()
        footers = self._find_footers(page_texts) if ocr_settings.remove_footers else set()
        
        # Clean each page
        cleaned_pages = []
        for text in page_texts:
            # Remove headers and footers
            cleaned_text = self._remove_headers_footers(text, headers, footers)
            
            # Remove page numbers if requested
            if ocr_settings.remove_page_numbers:
                cleaned_text = self._remove_page_numbers(cleaned_text)
                
            if cleaned_text.strip():
                cleaned_pages.append(cleaned_text)
        
        # Remove duplicate lines if requested
        if ocr_settings.remove_duplicate_lines:
            cleaned_pages = self._remove_duplicate_lines(cleaned_pages)
        
        # Join the cleaned pages
        return "\n\n".join(cleaned_pages)
    
    def _remove_page_numbers(self, text: str) -> str:
        """Remove page numbers from the text"""
        if not text:
            return text
            
        lines = text.split('\n')
        filtered_lines = []
        
        for line in lines:
            # Skip lines that match page number patterns
            is_page_number = False
            trimmed_line = line.strip()
            
            # Skip very short lines that are just numbers
            if len(trimmed_line) <= 3 and trimmed_line.isdigit():
                continue
                
            # Check against common page number patterns
            for pattern in self.PAGE_NUMBER_PATTERNS:
                if re.search(pattern, line):
                    is_page_number = True
                    break
            
            if not is_page_number:
                filtered_lines.append(line)
        
        return '\n'.join(filtered_lines)
    
    def _find_headers(self, page_texts: List[str], max_lines: int = 3) -> Set[str]:
        """
        Find potential headers by identifying similar text at the beginning of pages
        
        Args:
            page_texts: List of text extracted from each page
            max_lines: Maximum number of lines to consider as potential header
            
        Returns:
            Set of identified header strings
        """
        potential_headers = set()
        
        # Extract the first few lines from each page
        page_starts = []
        for text in page_texts:
            lines = text.strip().split('\n')
            start = '\n'.join(lines[:min(max_lines, len(lines))])
            page_starts.append(start)
        
        # Compare the starts of pages to find similar text
        for i in range(len(page_starts)):
            for j in range(i + 1, len(page_starts)):
                common_text = self._find_common_text(page_starts[i], page_starts[j])
                if common_text and len(common_text) > 10:  # Minimum length to be considered a header
                    potential_headers.add(common_text)
        
        return potential_headers
    
    def _find_footers(self, page_texts: List[str], max_lines: int = 3) -> Set[str]:
        """
        Find potential footers by identifying similar text at the end of pages
        
        Args:
            page_texts: List of text extracted from each page
            max_lines: Maximum number of lines to consider as potential footer
            
        Returns:
            Set of identified footer strings
        """
        potential_footers = set()
        
        # Extract the last few lines from each page
        page_ends = []
        for text in page_texts:
            lines = text.strip().split('\n')
            end = '\n'.join(lines[-min(max_lines, len(lines)):])
            page_ends.append(end)
        
        # Compare the ends of pages to find similar text
        for i in range(len(page_ends)):
            for j in range(i + 1, len(page_ends)):
                common_text = self._find_common_text(page_ends[i], page_ends[j])
                if common_text and len(common_text) > 10:  # Minimum length to be considered a footer
                    potential_footers.add(common_text)
        
        return potential_footers
    
    def _find_common_text(self, text1: str, text2: str) -> str:
        """Find the longest common substring between two strings"""
        matcher = SequenceMatcher(None, text1, text2)
        match = matcher.find_longest_match(0, len(text1), 0, len(text2))
        
        if match.size > 0:
            return text1[match.a:match.a + match.size]
        return ""
    
    def _remove_headers_footers(self, text: str, headers: Set[str], footers: Set[str]) -> str:
        """Remove identified headers and footers from text"""
        cleaned_text = text
        
        # Remove headers
        for header in headers:
            cleaned_text = cleaned_text.replace(header, "", 1)  # Replace only the first occurrence
        
        # Remove footers
        for footer in footers:
            if footer in cleaned_text:
                last_idx = cleaned_text.rindex(footer)
                cleaned_text = cleaned_text[:last_idx] + cleaned_text[last_idx + len(footer):]
        
        # Clean up excessive whitespace
        cleaned_text = re.sub(r'\n{3,}', '\n\n', cleaned_text)
        
        return cleaned_text.strip()
    
    def _remove_duplicate_lines(self, page_texts: List[str]) -> List[str]:
        """Remove lines that are duplicated across multiple pages"""
        if not page_texts:
            return []
            
        # Count line occurrences across all pages
        line_counts = {}
        for text in page_texts:
            lines = text.strip().split('\n')
            for line in lines:
                line = line.strip()
                if line:
                    line_counts[line] = line_counts.get(line, 0) + 1
        
        # Identify lines that appear in multiple pages (potential headers/footers)
        duplicate_lines = {line for line, count in line_counts.items() 
                          if count > 1 and len(line) > 10}
        
        # Remove duplicate lines from each page
        cleaned_pages = []
        for text in page_texts:
            lines = text.strip().split('\n')
            cleaned_lines = [line for line in lines if line.strip() not in duplicate_lines]
            cleaned_pages.append('\n'.join(cleaned_lines))
            
        return cleaned_pages 