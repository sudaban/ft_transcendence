.PHONY: help up down build rebuild nuke logs clean frontend-only frontend-up backend-only backend-up database-up frontend-build backend-build frontend-rebuild backend-rebuild frontend-logs backend-logs database-logs frontend-shell backend-shell database-shell frontend-down backend-down database-down nginx-up nginx-logs nginx-down logs-backend logs-frontend logs-nginx logs-db shell-backend shell-frontend shell-nginx shell-db test-health monitoring-up monitoring-down monitoring-logs

help:
	@echo "🐳 Transendence Docker Commands"
	@echo "================================"
	@echo ""
	@echo "📦 All Services:"
	@echo "  make up                - Start all services"
	@echo "  make down              - Stop all services"
	@echo "  make build             - Build all images"
	@echo "  make rebuild           - Rebuild all and clean volumes"
	@echo "  make nuke              - ☢️  Full reset (containers, volumes, images, cache, uploads)"
	@echo "  make logs              - Show all logs"
	@echo "  make clean             - Stop and remove volumes"
	@echo ""
	@echo "🎨 Frontend Only (SvelteKit) - No Dependencies:"
	@echo "  make frontend-only     - Start ONLY frontend (no db/backend needed)"
	@echo "  make frontend-dev      - Alias: frontend-only"
	@echo ""
	@echo "🎨 Frontend + Database:"
	@echo "  make frontend-up       - Start frontend + database"
	@echo "  make frontend-build    - Build frontend image"
	@echo "  make frontend-rebuild  - Rebuild frontend"
	@echo "  make frontend-logs     - View frontend logs (-f)"
	@echo "  make frontend-shell    - Open frontend shell"
	@echo "  make frontend-down     - Stop frontend"
	@echo ""
	@echo "⚙️  Backend Only (.NET) - No Dependencies:"
	@echo "  make backend-only      - Start ONLY backend (no db needed)"
	@echo "  make backend-dev       - Alias: backend-only"
	@echo ""
	@echo "⚙️  Backend + Database:"
	@echo "  make backend-up        - Start backend + database"
	@echo "  make backend-build     - Build backend image"
	@echo "  make backend-rebuild   - Rebuild backend"
	@echo "  make backend-logs      - View backend logs (-f)"
	@echo "  make backend-shell     - Open backend shell"
	@echo "  make backend-down      - Stop backend"
	@echo ""
	@echo "🗄️  Database Only:"
	@echo "  make database-up       - Start database"
	@echo "  make database-logs     - View database logs (-f)"
	@echo "  make database-shell    - Open database psql"
	@echo "  make database-down     - Stop database"
	@echo ""
	@echo "🔀 Nginx Reverse Proxy:"
	@echo "  make nginx-up          - Start nginx + all services"
	@echo "  make nginx-logs        - View nginx logs (-f)"
	@echo "  make nginx-down        - Stop nginx"
	@echo ""
	@echo "📈 Monitoring (Prometheus & Grafana):"
	@echo "  make monitoring-up     - Start monitoring services"
	@echo "  make monitoring-down   - Stop monitoring services"
	@echo "  make monitoring-logs   - View monitoring logs"
	@echo ""
	@echo "📊 Logs & Debug:"
	@echo "  make logs-frontend     - Frontend logs"
	@echo "  make logs-backend      - Backend logs"
	@echo "  make logs-nginx        - Nginx logs"
	@echo "  make logs-db           - Database logs"
	@echo "  make shell-frontend    - Frontend shell"
	@echo "  make shell-backend     - Backend shell"
	@echo "  make shell-db          - Database shell"
	@echo "  make test-health       - Health check all services"
	@echo ""

# ========================
# All Services Commands
# ========================
up:
	docker compose up -d

down:
	docker compose down

build:
	docker compose build

rebuild:
	docker compose down -v
	docker compose up --build -d

nuke:
	@echo "☢️  Full system reset starting..."
	@echo "⛔ Stopping all containers..."
	docker compose down -v --remove-orphans
	@echo "🗑️  Removing project images..."
	-docker rmi $$(docker images 'transendence-*' -q) 2>/dev/null || true
	@echo "📁 Cleaning uploads folder..."
	rm -rf uploads/*
	@echo "🔨 Rebuilding everything from scratch..."
	docker compose build --no-cache
	docker compose up -d
	@echo "✅ Full reset complete! All services are starting fresh."

logs:
	docker compose logs -f

clean:
	docker compose down -v

# ========================
# Frontend Commands - ONLY (No Dependencies)
# ========================
frontend-only:
	@echo "🚀 Starting ONLY Frontend (no database/backend needed)..."
	docker compose up -d frontend

frontend-dev: frontend-only

# ========================
# Frontend Commands - WITH Database
# ========================
frontend-up:
	@echo "🚀 Starting Frontend + Database..."
	docker compose up -d database frontend

frontend-build:
	@echo "🔨 Building Frontend..."
	docker compose build frontend

frontend-rebuild:
	@echo "🔄 Rebuilding Frontend..."
	docker compose down frontend
	docker compose up --build -d database frontend

frontend-logs:
	docker compose logs -f frontend

frontend-shell:
	docker compose exec frontend sh

frontend-down:
	@echo "⛔ Stopping Frontend..."
	docker compose down frontend

# ========================
# Backend Commands - ONLY (No Dependencies)
# ========================
backend-only:
	@echo "🚀 Starting ONLY Backend (no database needed)..."
	docker compose up -d backend

backend-dev: backend-only

# ========================
# Backend Commands - WITH Database
# ========================
backend-up:
	@echo "🚀 Starting Backend + Database..."
	docker compose up -d database backend

backend-build:
	@echo "🔨 Building Backend..."
	docker compose build backend

backend-rebuild:
	@echo "🔄 Rebuilding Backend..."
	docker compose down backend
	docker compose up --build -d database backend

backend-logs:
	docker compose logs -f backend

backend-shell:
	docker compose exec backend sh

backend-down:
	@echo "⛔ Stopping Backend..."
	docker compose down backend

# ========================
# Database Commands
# ========================
database-up:
	@echo "🚀 Starting Database..."
	docker compose up -d database

database-logs:
	docker compose logs -f database

database-shell:
	docker compose exec database psql -U postgres -d transendence

database-down:
	@echo "⛔ Stopping Database..."
	docker compose down database

# ========================
# Nginx Commands
# ========================
nginx-up:
	@echo "🚀 Starting All Services (via Nginx)..."
	docker compose up -d

nginx-logs:
	docker compose logs -f nginx

nginx-down:
	@echo "⛔ Stopping Nginx..."
	docker compose down nginx

# ========================
# Logs (Aliases)
# ========================
logs-backend:
	docker compose logs -f backend

logs-frontend:
	docker compose logs -f frontend

logs-nginx:
	docker compose logs -f nginx

logs-db:
	docker compose logs -f database

# ========================
# Shells (Aliases)
# ========================
shell-backend:
	docker compose exec backend sh

shell-frontend:
	docker compose exec frontend sh

shell-nginx:
	docker compose exec nginx sh

shell-db:
	docker compose exec database psql -U postgres -d transendence

# ========================
# Monitoring Commands
# ========================
monitoring-up:
	@echo "📈 Starting Monitoring services..."
	docker compose up -d prometheus grafana node-exporter cadvisor postgres-exporter nginx-exporter

monitoring-down:
	@echo "⛔ Stopping Monitoring services..."
	docker compose stop prometheus grafana node-exporter cadvisor postgres-exporter nginx-exporter

monitoring-logs:
	docker compose logs -f prometheus grafana node-exporter cadvisor postgres-exporter nginx-exporter

# ========================
# Health Check
# ========================
test-health:
	@echo "🏥 Testing services health..."
	@echo "Frontend (via Nginx): https://localhost"
	@echo "Backend API (via Nginx): https://localhost/api"
	@echo "Grafana: https://localhost/grafana/"
	@echo "Prometheus: http://localhost:9090"
	@echo ""
	@curl -sk https://localhost/ | head -20 || echo "❌ Frontend: FAILED"
	@echo ""
	@curl -sk https://localhost/api || echo "❌ Backend: FAILED"
	@echo ""
	@curl -s http://localhost:9090/-/healthy || echo "❌ Prometheus: FAILED"
	@echo ""
	@curl -sk https://localhost/grafana/api/health || echo "❌ Grafana: FAILED"
	@echo ""
	@echo "✅ Health check complete"

