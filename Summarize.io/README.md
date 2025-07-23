# Hungarian OCR Web Application

This is a full-stack web application that extracts Hungarian text from uploaded images and PDFs using OCR (Optical Character Recognition).

## Features

- **Frontend**: React application with a clean UI for uploading files and displaying results
- **Backend**: FastAPI application that processes images/PDFs and extracts Hungarian text
- **OCR**: Uses Tesseract OCR with Hungarian language support
- **File Support**: 
  - Images: JPG, JPEG, PNG
  - Documents: PDF

## Project Structure

### Backend
```
backend/
├── app/                    # Main application package
│   ├── api/                # API endpoints
│   │   └── routes/         # Route definitions
│   ├── core/               # Core application code
│   ├── models/             # Pydantic models
│   ├── services/           # Business logic 
│   └── utils/              # Utility functions
├── Dockerfile              # Docker configuration for backend
└── requirements.txt        # Python dependencies
```

### Frontend
```
frontend/
├── src/
│   ├── components/         # React components
│   ├── hooks/              # Custom React hooks
│   ├── services/           # API services
│   ├── styles/             # CSS styles
│   └── types/              # TypeScript type definitions
└── Dockerfile              # Docker configuration for frontend
```

## Prerequisites

- Docker and Docker Compose
- Alternatively:
  - Node.js (v14+) for the frontend
  - Python (v3.8+) for the backend
  - Tesseract OCR with Hungarian language support
  - Poppler (for PDF processing)

## Running with Docker (Recommended)

1. Clone this repository
2. Run the application with Docker Compose:

```bash
docker-compose up
```

3. Access the application at http://localhost:3000

## Running Manually

### Backend

1. Navigate to the backend directory:

```bash
cd backend
```

2. Install Tesseract OCR with Hungarian language support and Poppler:

```bash
# For Ubuntu/Debian
sudo apt-get update
sudo apt-get install -y tesseract-ocr tesseract-ocr-hun poppler-utils

# For macOS
brew install tesseract
brew install tesseract-lang  # Includes Hungarian support
brew install poppler         # Required for PDF processing
```

3. Install Python dependencies:

```bash
pip install -r requirements.txt
```

4. Run the backend server:

```bash
python run.py
```

### Frontend

1. Navigate to the frontend directory:

```bash
cd frontend
```

2. Install dependencies:

```bash
npm install
```

3. Run the frontend application:

```bash
npm start
```

4. Access the application at http://localhost:3000

## API Endpoints

- `GET /`: Check if the API is running
- `POST /api/ocr/extract-text`: Upload a file and extract Hungarian text
  - Request: Multipart form with a file field
  - Response: JSON with the extracted text

## Notes

- The application supports .jpg, .jpeg, .png, and .pdf file formats
- For PDFs, the application first tries to extract embedded text; if none is found, it uses OCR
- For best results, ensure the images/PDFs have clear, high-resolution text
- If you encounter issues with Tesseract OCR language support, make sure the Hungarian language pack (hun) is properly installed

## License

MIT 