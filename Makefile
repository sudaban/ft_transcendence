-include .env
HTTP_PORT ?= 8080
HTTPS_PORT ?= 8443

.PHONY: help up down build rebuild nuke logs clean frontend-only frontend-up backend-only backend-up database-up frontend-build backend-build frontend-rebuild backend-rebuild frontend-logs backend-logs database-logs frontend-shell backend-shell database-shell frontend-down backend-down database-down nginx-up nginx-logs nginx-down logs-backend logs-frontend logs-nginx logs-db shell-backend shell-frontend shell-nginx shell-db test-health monitoring-up monitoring-down monitoring-logs elk-up elk-down elk-logs full-up full-down db-migration-add db-migration-remove

help:
	@echo "🐳 Transcendence Docker Commands"
	@echo "================================"
	@echo ""
	@echo "📦 All Services:"
	@echo "  make up                - Start all services"
	@echo "  make down              - Stop all services"
	@echo "  make build             - Build all images"
	@echo "  make rebuild           - Rebuild all and clean volumes"
	@echo "  make nuke              - ☢️  Core reset (core containers, volumes, cache, uploads - NO images deleted)"
	@echo "  make full-nuke         - ☢️  Full reset including ELK & Monitoring (NO images deleted)"
	@echo "  make nuke-extra        - ☢️  Ultimate reset: Full stack + DELETES ALL IMAGES"
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
	@echo "📈 Monitoring (Prometheus & Grafana) [Profile]:"
	@echo "  make monitoring-up     - Start monitoring services"
	@echo "  make monitoring-down   - Stop monitoring services"
	@echo "  make monitoring-logs   - View monitoring logs"
	@echo ""
	@echo "📋 ELK Stack (Elasticsearch, Logstash, Kibana) [Profile]:"
	@echo "  make elk-up            - Start ELK stack"
	@echo "  make elk-down          - Stop ELK stack"
	@echo "  make elk-logs          - View ELK logs"
	@echo ""
	@echo "🚀 Full Stack (Core + Monitoring + ELK):"
	@echo "  make full-up           - Start ALL services (including monitoring & ELK)"
	@echo "  make full-down         - Stop ALL services"
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
	@echo "☢️  Core system reset starting..."
	@echo "⛔ Stopping core containers..."
	docker compose down -v --remove-orphans
	@echo "📁 Cleaning uploads folder..."
	rm -rf uploads/*
	@echo "🔨 Rebuilding core services from scratch..."
	docker compose build --no-cache
	docker compose up -d
	@echo "✅ Core reset complete! Core services are starting fresh."

full-nuke:
	@echo "☢️  Full system reset starting (including Monitoring & ELK)..."
	@echo "⛔ Stopping ALL containers..."
	docker compose --profile monitoring --profile elk down -v --remove-orphans
	@echo "📁 Cleaning uploads folder..."
	rm -rf uploads/*
	@echo "🔨 Rebuilding ALL services from scratch..."
	docker compose --profile monitoring --profile elk build --no-cache
	docker compose --profile monitoring --profile elk up -d
	@echo "✅ Full reset complete! All services are starting fresh."

nuke-extra:
	@echo "☢️  Ultimate system reset starting (including images)..."
	@echo "⛔ Stopping ALL containers..."
	docker compose --profile monitoring --profile elk down -v --remove-orphans
	@echo "🗑️  Removing ALL project images..."
	-docker rmi $$(docker images 'transcendence-*' -q) 2>/dev/null || true
	@echo "📁 Cleaning uploads folder..."
	rm -rf uploads/*
	@echo "🔨 Rebuilding ALL services from scratch..."
	docker compose --profile monitoring --profile elk build --no-cache
	docker compose --profile monitoring --profile elk up -d
	@echo "✅ Ultimate reset complete! All services and images are starting fresh."

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
	docker compose exec database psql -U postgres -d transcendence

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
	docker compose exec database psql -U postgres -d transcendence

# ========================
# Monitoring Commands
# ========================
monitoring-up:
	@echo "📈 Starting Monitoring services..."
	docker compose --profile monitoring up -d

monitoring-down:
	@echo "⛔ Stopping Monitoring services..."
	docker compose --profile monitoring stop

monitoring-logs:
	docker compose --profile monitoring logs -f

# ========================
# ELK Stack Commands
# ========================
elk-up:
	@echo "📋 Starting ELK Stack..."
	docker compose --profile elk up -d

elk-down:
	@echo "⛔ Stopping ELK Stack..."
	docker compose --profile elk stop

elk-logs:
	docker compose --profile elk logs -f

# ========================
# Full Stack (Core + Monitoring + ELK)
# ========================
full-up:
	@echo "🚀 Starting ALL services (core + monitoring + ELK)..."
	docker compose --profile monitoring --profile elk up -d

full-down:
	@echo "⛔ Stopping ALL services..."
	docker compose --profile monitoring --profile elk down

# ========================
# Health Check
# ========================
test-health:
	@echo "🏥 Testing services health..."
	@echo "Frontend (via Nginx): https://localhost:$(HTTPS_PORT)"
	@echo "Backend API (via Nginx): https://localhost:$(HTTPS_PORT)/api"
	@echo "Grafana: https://localhost:$(HTTPS_PORT)/grafana/"
	@echo "Prometheus: http://localhost:9090"
	@echo ""
	@curl -sk https://localhost:$(HTTPS_PORT)/ | head -20 || echo "❌ Frontend: FAILED"
	@echo ""
	@curl -sk https://localhost:$(HTTPS_PORT)/api || echo "❌ Backend: FAILED"
	@echo ""
	@curl -s http://localhost:9090/-/healthy || echo "❌ Prometheus: FAILED"
	@echo ""
	@curl -sk https://localhost:$(HTTPS_PORT)/grafana/api/health || echo "❌ Grafana: FAILED"
	@echo ""
	@echo "✅ Health check complete"

# ========================
# Database Migrations
# ========================
db-migration-add:
ifndef name
	$(error Error: 'name' parameter is required. Example: make db-migration-add name=AddSomeField)
endif
	docker run --rm -v "$(shell pwd)/Backend:/src" -w /src mcr.microsoft.com/dotnet/sdk:10.0-alpine sh -c "dotnet restore && dotnet tool install --global dotnet-ef && /root/.dotnet/tools/dotnet-ef migrations add $(name) --project Backend.Persistence --startup-project Backend.API"

db-migration-remove:
	docker run --rm -v "$(shell pwd)/Backend:/src" -w /src mcr.microsoft.com/dotnet/sdk:10.0-alpine sh -c "dotnet restore && dotnet tool install --global dotnet-ef && /root/.dotnet/tools/dotnet-ef migrations remove --project Backend.Persistence --startup-project Backend.API"

