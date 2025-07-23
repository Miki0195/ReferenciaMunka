from enum import Enum
from typing import Optional, List
from pydantic import BaseModel, Field

class FileType(str, Enum):
    IMAGE = "image"
    PDF = "pdf"
    UNSUPPORTED = "unsupported"

class OcrSettings(BaseModel):
    """Settings for OCR processing"""
    remove_headers: bool = Field(default=True, description="Remove repetitive headers from PDF pages")
    remove_footers: bool = Field(default=True, description="Remove repetitive footers from PDF pages")
    remove_duplicate_lines: bool = Field(default=False, description="Remove duplicate lines across pages")
    remove_page_numbers: bool = Field(default=True, description="Remove page numbers from the text")

class OcrRequest(BaseModel):
    """Request model for OCR processing with settings"""
    settings: Optional[OcrSettings] = Field(default=None, description="OCR processing settings")

class OCRResponse(BaseModel):
    """Response model for OCR extraction"""
    text: str
    message: Optional[str] = None 