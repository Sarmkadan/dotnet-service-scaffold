#!/bin/bash
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Systemd Deployment Script for dotnet-service-scaffold
#
# This script automates the deployment of dotnet-service-scaffold to a
# Linux system using systemd for service management.
#
# Usage: sudo bash systemd-deployment.sh
#

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
APP_USER="scaffold"
APP_GROUP="scaffold"
APP_DIR="/opt/dotnet-scaffold"
DATA_DIR="/var/lib/dotnet-scaffold"
LOG_DIR="/var/log/dotnet-scaffold"
SERVICE_NAME="dotnet-scaffold"

echo -e "${GREEN}=== DotNet Service Scaffold - Systemd Deployment ===${NC}\n"

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    echo -e "${RED}This script must be run as root (use sudo)${NC}"
    exit 1
fi

# Check if .NET is installed
echo "Checking .NET installation..."
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}.NET SDK not found. Please install .NET 10.0${NC}"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}Found .NET ${DOTNET_VERSION}${NC}\n"

# Create application user
echo "Creating application user..."
if ! id "$APP_USER" &>/dev/null; then
    useradd -m -s /bin/bash "$APP_USER"
    echo -e "${GREEN}User $APP_USER created${NC}"
else
    echo -e "${YELLOW}User $APP_USER already exists${NC}"
fi

# Create directories
echo "Creating directories..."
mkdir -p "$APP_DIR"
mkdir -p "$DATA_DIR"
mkdir -p "$LOG_DIR"

# Set ownership
chown -R "$APP_USER:$APP_GROUP" "$APP_DIR"
chown -R "$APP_USER:$APP_GROUP" "$DATA_DIR"
chown -R "$APP_USER:$APP_GROUP" "$LOG_DIR"

# Set permissions
chmod 755 "$APP_DIR"
chmod 700 "$DATA_DIR"
chmod 755 "$LOG_DIR"

echo -e "${GREEN}Directories created and configured${NC}\n"

# Build the application
echo "Building application (Release configuration)..."
if [ -f "dotnet-service-scaffold.csproj" ]; then
    dotnet publish -c Release -o /tmp/scaffold-publish
    echo -e "${GREEN}Build completed${NC}\n"
else
    echo -e "${RED}dotnet-service-scaffold.csproj not found${NC}"
    exit 1
fi

# Copy application files
echo "Deploying application files..."
cp -r /tmp/scaffold-publish/* "$APP_DIR/"
chown -R "$APP_USER:$APP_GROUP" "$APP_DIR"
rm -rf /tmp/scaffold-publish

echo -e "${GREEN}Application deployed${NC}\n"

# Create appsettings.Production.json if it doesn't exist
if [ ! -f "$APP_DIR/appsettings.Production.json" ]; then
    echo "Creating production configuration..."
    cat > "$APP_DIR/appsettings.Production.json" <<'EOF'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/var/lib/dotnet-scaffold/scaffold.db"
  },
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/dotnet-scaffold/scaffold-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  },
  "ApplicationSettings": {
    "HealthCheckInterval": 60,
    "HealthCheckTimeout": 15,
    "MaxConcurrentHealthChecks": 10,
    "AuditLogRetentionDays": 365,
    "HealthCheckResultRetentionDays": 90
  }
}
EOF
    chown "$APP_USER:$APP_GROUP" "$APP_DIR/appsettings.Production.json"
    chmod 600 "$APP_DIR/appsettings.Production.json"
    echo -e "${GREEN}Production configuration created${NC}\n"
fi

# Create systemd service file
echo "Creating systemd service file..."
cat > "/etc/systemd/system/${SERVICE_NAME}.service" <<EOF
[Unit]
Description=DotNet Service Scaffold
After=network.target
Wants=network-online.target

[Service]
Type=notify
User=$APP_USER
Group=$APP_GROUP
WorkingDirectory=$APP_DIR
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="ASPNETCORE_URLS=http://localhost:5000"
ExecStart=/usr/bin/dotnet $APP_DIR/dotnet-service-scaffold.dll
Restart=on-failure
RestartSec=10
StandardOutput=journal
StandardError=journal
TimeoutStopSec=30

# Security hardening
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=$DATA_DIR $LOG_DIR

[Install]
WantedBy=multi-user.target
EOF

chmod 644 "/etc/systemd/system/${SERVICE_NAME}.service"
echo -e "${GREEN}Systemd service file created${NC}\n"

# Reload systemd
echo "Reloading systemd..."
systemctl daemon-reload

# Enable service
echo "Enabling service..."
systemctl enable "$SERVICE_NAME"

# Start service
echo "Starting service..."
systemctl start "$SERVICE_NAME"

# Wait for service to be ready
echo "Waiting for service to be ready..."
sleep 3

# Check service status
if systemctl is-active --quiet "$SERVICE_NAME"; then
    echo -e "${GREEN}Service started successfully${NC}\n"
else
    echo -e "${RED}Service failed to start. Check logs:${NC}"
    journalctl -u "$SERVICE_NAME" -n 50
    exit 1
fi

# Verify service is responding
echo "Verifying service..."
if curl -s http://localhost:5000/health > /dev/null; then
    echo -e "${GREEN}Service is responding${NC}\n"
else
    echo -e "${YELLOW}Service not responding yet. Check logs:${NC}"
    journalctl -u "$SERVICE_NAME" -n 20
fi

# Display post-deployment information
echo -e "${GREEN}=== Deployment Complete ===${NC}\n"
echo "Service name: $SERVICE_NAME"
echo "Application directory: $APP_DIR"
echo "Data directory: $DATA_DIR"
echo "Log directory: $LOG_DIR"
echo "Application user: $APP_USER"
echo ""
echo "Useful commands:"
echo "  sudo systemctl status $SERVICE_NAME              # Check status"
echo "  sudo systemctl restart $SERVICE_NAME            # Restart service"
echo "  sudo systemctl stop $SERVICE_NAME               # Stop service"
echo "  sudo journalctl -u $SERVICE_NAME -f             # View logs (follow)"
echo "  sudo journalctl -u $SERVICE_NAME -n 100         # View last 100 logs"
echo "  curl http://localhost:5000/health               # Health check"
echo "  curl http://localhost:5000/status               # Service status"
echo ""
echo "Next steps:"
echo "  1. Configure Caddy reverse proxy (optional)"
echo "  2. Create API keys for clients"
echo "  3. Register services to monitor"
echo "  4. Configure health check intervals"
echo ""
echo -e "${GREEN}Deployment successful!${NC}"
