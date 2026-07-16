-include .env
HTTP_PORT ?= 8080
HTTPS_PORT ?= 8443

.PHONY: help nuke full-nuke nuke-extra monitoring-up monitoring-down monitoring-logs elk-up elk-down elk-logs full-up full-down

help:
	@echo "🐳 Transcendence Docker Commands"
	@echo "================================"
	@echo ""
	@echo "☢️  Nuke Commands:"
	@echo "  make nuke              - ☢️  Core reset (core containers, volumes, cache, uploads - NO images deleted)"
	@echo "  make full-nuke         - ☢️  Full reset including ELK & Monitoring (NO images deleted)"
	@echo "  make nuke-extra        - ☢️  Ultimate reset: Full stack + DELETES ALL IMAGES"
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

# ========================
# Nuke Commands
# ========================
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
