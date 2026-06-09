#!/bin/bash

echo "🚀 Transendence Docker Setup"
echo "============================"

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed"
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose is not installed"
    exit 1
fi

# Create .env if it doesn't exist
if [ ! -f .env ]; then
    echo "📝 Creating .env from .env.example..."
    cp .env.example .env
fi

# Start services
echo "📦 Starting services..."
docker-compose up -d

# Wait for services to start
echo "⏳ Waiting for services to start..."
sleep 10

# Check health
echo ""
echo "🏥 Checking service health..."
echo ""
echo "📌 Services available at:"
echo "  Nginx/Frontend: http://localhost:8080"
echo "  Backend API: http://localhost:8080/api"
echo "  Database: localhost:5432"
echo ""

echo "✅ Setup complete!"
echo ""
echo "📌 Quick commands:"
echo "  View logs: docker-compose logs -f"
echo "  View backend logs: docker-compose logs -f backend"
echo "  Stop: docker-compose down"
echo "  Shell: docker-compose exec backend sh"
echo "  Database: docker-compose exec database psql -U postgres -d transendence"

