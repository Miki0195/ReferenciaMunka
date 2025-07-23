import os
from fastapi import APIRouter, File, UploadFile, HTTPException, Depends, Form
from fastapi.responses import JSONResponse
import json
from typing import Optional

from app.services.ocr_service import OCRService
from app.models.ocr import OCRResponse, FileType, OcrSettings

router = APIRouter(prefix="/ocr", tags=["OCR"])

@router.post("/extract-text", response_model=OCRResponse)
async def extract_text(
    file: UploadFile = File(...),
    settings_json: Optional[str] = Form(None),
    ocr_service: OCRService = Depends()
):
    """
    Extract Hungarian text from an uploaded image or PDF file.
    
    Supported formats:
    - Images: JPG, JPEG, PNG
    - Documents: PDF
    
    You can optionally provide OCR settings as a JSON string to control the text extraction:
    - remove_headers: Remove repetitive headers from PDF pages (default: true)
    - remove_footers: Remove repetitive footers from PDF pages (default: true)
    - remove_duplicate_lines: Remove duplicate lines across pages (default: false)
    
    Returns the extracted text or an error message.
    """
    # Parse OCR settings if provided
    ocr_settings = None
    if settings_json:
        try:
            settings_dict = json.loads(settings_json)
            ocr_settings = OcrSettings(**settings_dict)
        except Exception as e:
            raise HTTPException(status_code=400, detail=f"Invalid OCR settings format: {str(e)}")
    
    # Validate file type
    file_type = ocr_service.get_file_type(file)
    
    try:
        # Process the file based on its type
        if file_type == FileType.PDF:
            extracted_text = await ocr_service.process_pdf(file, ocr_settings)
        elif file_type == FileType.IMAGE:
            extracted_text = await ocr_service.process_image(file)
        else:
            raise HTTPException(
                status_code=400, 
                detail=f"Unsupported file format. Supported formats: JPG, JPEG, PNG, PDF"
            )
        
        if not extracted_text.strip():
            return OCRResponse(text="", message="No text detected in the file")
        
        return OCRResponse(text=extracted_text)
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error processing file: {str(e)}")
    finally:
        await file.close() 