from fastapi import APIRouter, UploadFile, File, Form, HTTPException
from typing import Optional, List
import os
from app.services.ocr_service import extract_text_from_pdf, extract_text_from_image
from app.services.summarizer_service import summarize_text

router = APIRouter(prefix="/summarize", tags=["summarize"])

@router.post("/")
async def summarize_content(
    file: Optional[UploadFile] = File(None),
    text: Optional[str] = Form(None)
):
    """
    Summarize text content from either an uploaded file or direct text input.
    """
    if not file and not text:
        raise HTTPException(
            status_code=400, 
            detail="Either file or text must be provided"
        )
    
    content_text = ""
    
    # Extract text from file if provided
    if file:
        file_ext = os.path.splitext(file.filename)[1].lower()
        file_content = await file.read()
        
        try:
            if file_ext in ['.pdf']:
                content_text = extract_text_from_pdf(file_content)
            elif file_ext in ['.png', '.jpg', '.jpeg']:
                content_text = extract_text_from_image(file_content)
            else:
                # For text files, assume UTF-8 encoding
                content_text = file_content.decode('utf-8')
        except Exception as e:
            raise HTTPException(
                status_code=500,
                detail=f"Failed to process file: {str(e)}"
            )
    else:
        content_text = text
    
    # Summarize the extracted or provided text
    try:
        summary, bullet_points = summarize_text(content_text)
        return {
            "summary": summary,
            "bulletPoints": bullet_points
        }
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Summarization failed: {str(e)}"
        ) 