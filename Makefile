# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Makefile for dotnet-service-scaffold
#
# Common development and deployment tasks
# Usage: make help
#

.PHONY: help build run test clean publish docker deploy stop restart logs

# Variables
APP_NAME := dotnet-service-scaffold
VERSION := 1.0.0
DOCKER_IMAGE := $(APP_NAME):$(VERSION)
DOCKER_LATEST := $(APP_NAME):latest
PUBLISH_DIR := ./publish
BUILD_CONFIG := Release

# Help command
help:
	@echo "================================"
	@echo "$(APP_NAME) - Makefile Commands"
	@echo "================================"
	@echo ""
	@echo "Build & Development:"
	@echo "  make build          - Build project (Debug)"
	@echo "  make build-release  - Build project (Release)"
	@echo "  make clean          - Clean build artifacts"
	@echo "  make run            - Run application locally"
	@echo "  make watch          - Run with file watching (auto-reload)"
	@echo ""
	@echo "Testing & Quality:"
	@echo "  make test           - Run unit tests"
	@echo "  make format         - Format code with dotnet format"
	@echo "  make analyze        - Run code analysis"
	@echo ""
	@echo "Publishing & Deployment:"
	@echo "  make publish        - Publish release build"
	@echo "  make docker-build   - Build Docker image"
	@echo "  make docker-run     - Run Docker container"
	@echo "  make docker-push    - Push Docker image"
	@echo ""
	@echo "Docker Compose:"
	@echo "  make docker-up      - Start Docker Compose services"
	@echo "  make docker-down    - Stop Docker Compose services"
	@echo "  make docker-logs    - View Docker Compose logs"
	@echo ""
	@echo "Systemd Deployment:"
	@echo "  make deploy         - Deploy to systemd service"
	@echo "  make restart        - Restart systemd service"
	@echo "  make stop           - Stop systemd service"
	@echo "  make logs           - View systemd service logs"
	@echo ""
	@echo "Database:"
	@echo "  make db-migrate     - Run database migrations"
	@echo "  make db-backup      - Create database backup"
	@echo ""
	@echo "Documentation:"
	@echo "  make docs           - Generate documentation"
	@echo ""

# Build targets
build:
	@echo "Building project (Debug)..."
	dotnet build

build-release:
	@echo "Building project (Release)..."
	dotnet build -c Release

clean:
	@echo "Cleaning build artifacts..."
	dotnet clean
	rm -rf $(PUBLISH_DIR)
	rm -rf bin obj
	@echo "Clean complete"

run:
	@echo "Running application..."
	dotnet run

watch:
	@echo "Running with file watching..."
	dotnet watch run

# Testing & Code Quality
test:
	@echo "Running tests..."
	dotnet test
	@echo "Tests complete"

format:
	@echo "Formatting code..."
	dotnet format
	@echo "Formatting complete"

analyze:
	@echo "Running code analysis..."
	dotnet build /p:EnforceCodeStyleInBuild=true
	@echo "Analysis complete"

# Publishing
publish: clean build-release
	@echo "Publishing application..."
	dotnet publish -c Release -o $(PUBLISH_DIR)
	@echo "Published to $(PUBLISH_DIR)"

# Docker targets
docker-build: publish
	@echo "Building Docker image: $(DOCKER_IMAGE)"
	docker build -t $(DOCKER_IMAGE) .
	docker tag $(DOCKER_IMAGE) $(DOCKER_LATEST)
	@echo "Docker image built successfully"

docker-run: docker-build
	@echo "Running Docker container..."
	docker run -d \
		--name $(APP_NAME) \
		-p 5000:5000 \
		-v scaffold-data:/app/data \
		-e "ASPNETCORE_ENVIRONMENT=Production" \
		--restart unless-stopped \
		$(DOCKER_IMAGE)
	@echo "Container running. Access at http://localhost:5000"

docker-stop:
	@echo "Stopping Docker container..."
	docker stop $(APP_NAME) || true
	docker rm $(APP_NAME) || true
	@echo "Container stopped"

docker-push:
	@echo "Pushing Docker image..."
	docker push $(DOCKER_IMAGE)
	docker push $(DOCKER_LATEST)
	@echo "Image pushed"

docker-clean:
	@echo "Cleaning Docker images..."
	docker rmi $(DOCKER_IMAGE) $(DOCKER_LATEST) || true
	@echo "Docker images cleaned"

# Docker Compose targets
docker-up:
	@echo "Starting Docker Compose services..."
	docker-compose up -d
	@echo "Services started. Access at http://localhost:5000"

docker-down:
	@echo "Stopping Docker Compose services..."
	docker-compose down
	@echo "Services stopped"

docker-logs:
	@echo "Showing Docker Compose logs..."
	docker-compose logs -f

docker-ps:
	@echo "Docker Compose services:"
	docker-compose ps

# Systemd deployment targets
deploy: publish
	@echo "Deploying to systemd service..."
	sudo bash examples/systemd-deployment.sh
	@echo "Deployment complete"

restart:
	@echo "Restarting systemd service..."
	sudo systemctl restart dotnet-scaffold
	@echo "Service restarted"

stop:
	@echo "Stopping systemd service..."
	sudo systemctl stop dotnet-scaffold
	@echo "Service stopped"

start:
	@echo "Starting systemd service..."
	sudo systemctl start dotnet-scaffold
	@echo "Service started"

logs:
	@echo "Showing systemd service logs..."
	sudo journalctl -u dotnet-scaffold -f

status:
	@echo "Service status:"
	sudo systemctl status dotnet-scaffold

# Database targets
db-migrate:
	@echo "Running database migrations..."
	dotnet ef database update
	@echo "Migrations complete"

db-backup:
	@echo "Creating database backup..."
	@mkdir -p backups
	sqlite3 scaffold.db ".backup backups/scaffold_$(shell date +%Y%m%d_%H%M%S).db"
	@echo "Backup created"

# Documentation
docs:
	@echo "Documentation is in docs/ directory"
	@echo "Available:"
	@ls -la docs/

# Integration targets
health-check:
	@echo "Checking service health..."
	@curl -s http://localhost:5000/health | jq '.'

status-check:
	@echo "Checking service status..."
	@curl -s http://localhost:5000/status | jq '.'

# Useful shortcuts
all: clean build test
	@echo "All tasks completed"

dev: build run
	@echo "Development build complete"

prod: clean build-release publish docker-build
	@echo "Production build complete"

info:
	@echo "Project Information:"
	@echo "  Name: $(APP_NAME)"
	@echo "  Version: $(VERSION)"
	@echo "  Docker Image: $(DOCKER_IMAGE)"
	@echo ""
	@dotnet --version
	@echo ""
	@echo "Directories:"
	@ls -la | grep "^d" | awk '{print "  " $$NF}'
