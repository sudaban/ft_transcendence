-include .env
HTTP_PORT ?= 8080
HTTPS_PORT ?= 8443

.PHONY: all build up elk down clean fclean re database-shell

# Default komut (make): ELK olmadan build alıp ayağa kaldırır
all: build up

build:
	@echo "🔨 Building core services (without ELK)..."
	docker compose build

up:
	@echo "🚀 Starting core services..."
	docker compose up -d

# ELK ile build alıp ayağa kaldırma
elk:
	@echo "🔨 Building ALL services (including ELK & Monitoring)..."
	docker compose --profile elk --profile monitoring build
	@echo "🚀 Starting ALL services (including ELK & Monitoring)..."
	docker compose --profile elk --profile monitoring up -d

# Sadece container'ları indirir
down:
	@echo "⛔ Stopping all services..."
	docker compose --profile elk --profile monitoring down

# Volume içlerini (local uploads vb.) ve containerları temizler, ama sanal diskleri (volumes) silmez
clean: down
	@echo "🧹 Cleaning containers and local files..."
	docker compose --profile elk --profile monitoring rm -f
	rm -rf uploads/*

# Container, Image ve Volume'lerin hepsini tamamen siler
fclean:
	@echo "☢️ Full clean: Removing containers, volumes, and images..."
	docker compose --profile elk --profile monitoring down -v --rmi all --remove-orphans
	rm -rf uploads/*

# Sistemi tamamen sıfırlayıp baştan kurar
re: fclean all

# Veritabanı (PostgreSQL) terminaline bağlanır
database-shell:
	@echo "🐘 Connecting to PostgreSQL shell..."
	docker exec -it transcendence-db sh -c 'psql -U $$POSTGRES_USER -d $$POSTGRES_DB'
