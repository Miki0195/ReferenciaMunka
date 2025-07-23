import os
from typing import List
from pydantic_settings import BaseSettings
from dotenv import load_dotenv

# Load .env file if it exists
load_dotenv()

class Settings(BaseSettings):
    # Project info
    PROJECT_NAME: str = "Hungarian OCR API"
    PROJECT_DESCRIPTION: str = "API for extracting Hungarian text from images and PDFs"
    PROJECT_VERSION: str = "1.0.0"
    
    # CORS
    CORS_ORIGINS: List[str] = ["*"]  # In production, replace with specific origins
    
    # Tesseract
    TESSERACT_LANGUAGE: str = "hun"
    
    class Config:
        env_file = ".env"
        case_sensitive = True

# Create settings instance
settings = Settings()

# Check if Hungarian language pack is installed
def check_hun_language_pack():
    import pytesseract
    available_langs = pytesseract.get_languages()
    if settings.TESSERACT_LANGUAGE not in available_langs:
        print(f"WARNING: {settings.TESSERACT_LANGUAGE} language pack not installed!")
        print(f"Please install it with 'apt-get install tesseract-ocr-{settings.TESSERACT_LANGUAGE}'")
        print("Continuing startup, but OCR functionality may be limited.")

# Run check
try:
    check_hun_language_pack()
except Exception as e:
    print(f"Warning: Error checking tesseract language packs: {str(e)}")
    print("Continuing startup, but OCR functionality may be limited.") 