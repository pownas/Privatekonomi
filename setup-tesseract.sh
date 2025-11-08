#!/bin/bash
# Script to download Tesseract language data files for OCR

TESSDATA_DIR="./src/Privatekonomi.Web/tessdata"
TESSDATA_URL="https://github.com/tesseract-ocr/tessdata_fast/raw/main"

echo "Setting up Tesseract OCR language data..."

# Create tessdata directory if it doesn't exist
mkdir -p "$TESSDATA_DIR"

# Download English language data (required)
echo "Downloading English language data..."
curl -L -o "$TESSDATA_DIR/eng.traineddata" "$TESSDATA_URL/eng.traineddata"

# Download Swedish language data (for better Swedish receipt recognition)
echo "Downloading Swedish language data..."
curl -L -o "$TESSDATA_DIR/swe.traineddata" "$TESSDATA_URL/swe.traineddata"

echo "Tesseract language data installed successfully!"
echo "Files installed in: $TESSDATA_DIR"
