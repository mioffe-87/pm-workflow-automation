#!/bin/bash

# Load environment variables
set -a
source .env
set +a

# Check if token is set
if [ -z "$GITHUB_TOKEN" ]; then
    echo "❌ Error: GITHUB_TOKEN not set in .env file"
    exit 1
fi

# Check if repo URL is set
if [ -z "$GITHUB_REPO_URL" ]; then
    echo "❌ Error: GITHUB_REPO_URL not set in .env file"
    exit 1
fi

# Clone the repository with token authentication
echo "🔄 Cloning CSP repository..."
git clone --branch master "https://mioffe:${GITHUB_TOKEN}@panwgithub.paloaltonetworks.local/IAD/CSP.git" CSP

if [ $? -eq 0 ]; then
    echo "✅ Repository cloned successfully!"
    echo "📁 Repository location: $(pwd)/CSP"
else
    echo "❌ Failed to clone repository"
    exit 1
fi
