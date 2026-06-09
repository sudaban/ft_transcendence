@echo off
echo 🚀 Transendence Docker Setup
echo ============================

REM Check if Docker is installed
docker --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker is not installed
    exit /b 1
)

REM Check if Docker Compose is installed
docker-compose --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker Compose is not installed
    exit /b 1
)

REM Create .env if it doesn't exist
if not exist .env (
    echo 📝 Creating .env from .env.example...
    copy .env.example .env
)

REM Start services
echo 📦 Starting services...
docker-compose up -d

REM Wait for services to start
echo ⏳ Waiting for services to start...
timeout /t 10 /nobreak

REM Check health
echo.
echo 🏥 Services available at:
echo   Nginx/Frontend: http://localhost:8080
echo   Backend API: http://localhost:8080/api
echo   Database: localhost:5432
echo.

echo ✅ Setup complete!
echo.
echo 📌 Quick commands:
echo   View logs: docker-compose logs -f
echo   View backend logs: docker-compose logs -f backend
echo   Stop: docker-compose down
echo   Shell: docker-compose exec backend sh
echo   Database: docker-compose exec database psql -U postgres -d transendence

