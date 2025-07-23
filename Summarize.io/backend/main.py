"""
This file is kept for backward compatibility.
The application has been restructured for better organization.
Please use run.py to start the application.
"""

from app.main import app

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True) 