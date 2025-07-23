import os
import subprocess
from typing import List, Optional

def get_tesseract_languages() -> List[str]:
    """Get a list of installed Tesseract language packs"""
    try:
        import pytesseract
        return pytesseract.get_languages()
    except Exception:
        return []

def check_command_exists(command: str) -> bool:
    """Check if a command exists on the system"""
    try:
        subprocess.run(
            ["which", command], 
            stdout=subprocess.PIPE, 
            stderr=subprocess.PIPE, 
            check=False
        )
        return True
    except Exception:
        return False 