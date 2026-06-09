.PHONY: help up down build logs logs-backend logs-frontend logs-nginx logs-db clean rebuild shell-backend shell-frontend shell-nginx shell-db test-health

help:
	@echo "Transendence Docker Commands"
	@echo "============================"
	@echo "make up              - Start all services"
	@echo "make down            - Stop all services"
	@echo "make build           - Build images"
	@echo "make rebuild         - Rebuild images"
	@echo "make logs            - Show all logs"
	@echo "make logs-backend    - Show backend logs"
	@echo "make logs-frontend   - Show frontend logs"
	@echo "make logs-nginx      - Show nginx logs"
	@echo "make logs-db         - Show database logs"
	@echo "make clean           - Stop and remove volumes"
	@echo "make shell-backend   - Open backend shell"
	@echo "make shell-frontend  - Open frontend shell"
	@echo "make shell-nginx     - Open nginx shell"
	@echo "make shell-db        - Open database shell"
	@echo "make test-health     - Check service health"

up:
	docker-compose up -d

down:
	docker-compose down

build:
	docker-compose build

rebuild:
	docker-compose down -v
	docker-compose up --build -d

logs:
	docker-compose logs -f

logs-backend:
	docker-compose logs -f backend

logs-frontend:
	docker-compose logs -f frontend

logs-nginx:
	docker-compose logs -f nginx

logs-db:
	docker-compose logs -f database

clean:
	docker-compose down -v

shell-backend:
	docker-compose exec backend sh

shell-frontend:
	docker-compose exec frontend sh

shell-nginx:
	docker-compose exec nginx sh

shell-db:
	docker-compose exec database psql -U postgres -d transendence

test-health:
	@echo "Testing services health..."
	@echo "Frontend (via Nginx): http://localhost:8080"
	@echo "Backend API (via Nginx): http://localhost:8080/api"
	@curl -s http://localhost:8080/ | head -20 || echo "Frontend: TIMEOUT"
	@curl -s http://localhost:8080/api || echo "Backend: TIMEOUT"
	@echo "Done"

