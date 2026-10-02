#!/bin/bash

# Download and extract CSP repository as ZIP

REPO_URL="https://panwgithub.paloaltonetworks.local/IAD/CSP/archive/refs/heads/master.zip"
DOWNLOAD_DIR="/Users/mioffe/Documents/pm-workflow-automation"
ZIP_FILE="$DOWNLOAD_DIR/CSP-master.zip"
EXTRACT_DIR="$DOWNLOAD_DIR/CSP"

echo "🔄 Downloading CSP repository..."
curl -L -o "$ZIP_FILE" "$REPO_URL"

if [ $? -eq 0 ]; then
    echo "✅ Download successful!"
    
    # Remove existing CSP directory if it exists
    if [ -d "$EXTRACT_DIR" ]; then
        echo "🗑️  Removing existing CSP directory..."
        rm -rf "$EXTRACT_DIR"
    fi
    
    echo "📦 Extracting ZIP file..."
    unzip -q "$ZIP_FILE" -d "$DOWNLOAD_DIR"
    
    # Rename the extracted folder
    if [ -d "$DOWNLOAD_DIR/CSP-master" ]; then
        mv "$DOWNLOAD_DIR/CSP-master" "$EXTRACT_DIR"
        echo "✅ Repository extracted to: $EXTRACT_DIR"
    fi
    
    # Clean up ZIP file
    rm "$ZIP_FILE"
    echo "🧹 Cleaned up ZIP file"
    
    echo "✅ Done! CSP repository is ready at: $EXTRACT_DIR"
else
    echo "❌ Failed to download repository"
    exit 1
fi
