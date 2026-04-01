# Deployment Guide

Complete guide for deploying dotnet-service-scaffold to production environments.

## Fresh Ubuntu 24.04 VPS — Step-by-Step Walkthrough

This section covers a complete, first-time deployment on a clean Ubuntu 24.04 server.
Follow these steps in order; later sections contain more detailed reference material.

### Prerequisites

- Ubuntu 24.04 LTS VPS with sudo access
- A domain name with its A record pointing to the server's IP (needed for HTTPS)
- Ports 22, 80, and 443 open in your firewall / hosting panel

### Step 1 — Install .NET 10

```bash
# Add the Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install the SDK (needed to publish) or just the runtime on production
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
```

Verify:

```bash
dotnet --version
```

### Step 2 — Create a dedicated service account

Running the application as a non-root user limits the blast radius of any security issue.

```bash
sudo useradd -r -s /bin/false scaffold
sudo mkdir -p /opt/dotnet-scaffold /var/lib/dotnet-scaffold /var/log/dotnet-scaffold
sudo chown scaffold:scaffold /opt/dotnet-scaffold /var/lib/dotnet-scaffold /var/log/dotnet-scaffold
sudo chmod 750 /var/lib/dotnet-scaffold   # DB directory — restricted to service account
```

### Step 3 — Publish and copy the application

Run these commands from your development machine (or a CI runner) and then copy the output to the server, **or** clone the repository directly on the server and publish there.

```bash
# On your build machine (or on the server after cloning)
dotnet publish -c Release -o ./publish

# Copy to the server (replace user@your-server-ip)
rsync -avz ./publish/ user@your-server-ip:/opt/dotnet-scaffold/
```

### Step 4 — Configure the connection string for the production database path

Create `/opt/dotnet-scaffold/appsettings.Production.json` with the production database
location and WAL-enabled connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/var/lib/dotnet-scaffold/scaffold.db;Mode=ReadWriteCreate;Cache=Shared"
  },
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/dotnet-scaffold/scaffold-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

### Step 5 — Install the systemd unit

```bash
sudo tee /etc/systemd/system/dotnet-scaffold.service > /dev/null <<'EOF'
[Unit]
Description=DotNet Service Scaffold
After=network-online.target
Wants=network-online.target

[Service]
Type=notify
User=scaffold
Group=scaffold
WorkingDirectory=/opt/dotnet-scaffold
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="ASPNETCORE_URLS=http://localhost:5000"
ExecStart=/usr/bin/dotnet /opt/dotnet-scaffold/dotnet-service-scaffold.dll
ExecStartPost=/bin/sh -c 'until curl -sf http://localhost:5000/health; do sleep 1; done'
Restart=on-failure
RestartSec=10
StandardOutput=journal
StandardError=journal
TimeoutStopSec=30
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ReadWritePaths=/var/lib/dotnet-scaffold /var/log/dotnet-scaffold
ProtectHome=true

[Install]
WantedBy=multi-user.target
EOF
```

Enable and start the service:

```bash
sudo systemctl daemon-reload
sudo systemctl enable dotnet-scaffold
sudo systemctl start dotnet-scaffold
```

Check that it came up correctly:

```bash
sudo systemctl status dotnet-scaffold
curl http://localhost:5000/health
```

### Step 6 — Install Caddy (automatic HTTPS)

Caddy obtains a Let's Encrypt certificate automatically the first time it receives a
request for your domain — no manual certificate management needed.

```bash
sudo apt-get install -y caddy
```

Create `/etc/caddy/Caddyfile` (replace `your.domain.com` and the email address):

```caddy
{
    email you@example.com
}

your.domain.com {
    reverse_proxy localhost:5000 {
        header_up X-Forwarded-For {http.request.remote.host}
        header_up X-Forwarded-Proto {http.request.proto}
        health_uri /health
        health_interval 10s
    }

    encode gzip

    header {
        Strict-Transport-Security "max-age=31536000; includeSubDomains"
        X-Content-Type-Options "nosniff"
        X-Frame-Options "DENY"
    }

    log {
        output file /var/log/caddy/scaffold.log
        level info
    }
}
```

Validate and reload:

```bash
sudo caddy validate --config /etc/caddy/Caddyfile
sudo systemctl enable caddy
sudo systemctl restart caddy
```

### Step 7 — Open the firewall

```bash
sudo ufw allow 22/tcp    # SSH — keep this open!
sudo ufw allow 80/tcp    # HTTP (Caddy redirects to HTTPS)
sudo ufw allow 443/tcp   # HTTPS
sudo ufw enable
```

### Step 8 — Verify end-to-end

```bash
# Health check over HTTPS (certificate is obtained on first request — may take a few seconds)
curl https://your.domain.com/health
```

A successful response looks like:

```json
{
  "status": "Healthy",
  "checks": [
    { "name": "database", "status": "Healthy" },
    { "name": "sqlite-file", "status": "Healthy", "data": { "diskAvailableMB": 12345 } }
  ]
}
```

### Ongoing maintenance commands

```bash
# View application logs
sudo journalctl -u dotnet-scaffold -f

# Restart after updating binaries
sudo systemctl restart dotnet-scaffold

# Update Caddy configuration without downtime
sudo caddy validate --config /etc/caddy/Caddyfile && sudo systemctl reload caddy

# Backup the database
sudo -u scaffold sqlite3 /var/lib/dotnet-scaffold/scaffold.db ".backup /tmp/scaffold-$(date +%Y%m%d).db"
```

---



- [ ] Application builds without errors (`dotnet build -c Release`)
- [ ] All tests pass (`dotnet test`)
- [ ] Configuration reviewed and updated for production
- [ ] SSL certificates obtained and configured
- [ ] Database backup strategy established
- [ ] Logging and monitoring configured
- [ ] API keys generated and secured
- [ ] Systemd/Docker setup tested

## Deployment Options

### 1. Linux Systemd Deployment

Recommended for single-server setups on Linux/Unix systems.

#### Prerequisites

- Linux system (Ubuntu 20.04+ recommended)
- .NET 10.0 SDK installed
- Sudo access
- Systemd support

#### Installation Steps

**1. Create application user**

```bash
sudo useradd -m -s /bin/bash scaffold
sudo mkdir -p /opt/dotnet-scaffold
sudo chown -R scaffold:scaffold /opt/dotnet-scaffold
```

**2. Deploy application files**

```bash
# As root or with sudo
cd /path/to/dotnet-service-scaffold
dotnet publish -c Release -o /tmp/publish
sudo cp -r /tmp/publish/* /opt/dotnet-scaffold/
sudo chown -R scaffold:scaffold /opt/dotnet-scaffold
sudo chmod 755 /opt/dotnet-scaffold
```

**3. Create systemd service file**

```bash
sudo tee /etc/systemd/system/dotnet-scaffold.service > /dev/null <<EOF
[Unit]
Description=DotNet Service Scaffold
After=network.target
Wants=network-online.target

[Service]
Type=notify
User=scaffold
Group=scaffold
WorkingDirectory=/opt/dotnet-scaffold
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="ASPNETCORE_URLS=http://localhost:5000"
ExecStart=/usr/bin/dotnet /opt/dotnet-scaffold/dotnet-service-scaffold.dll
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

[Install]
WantedBy=multi-user.target
EOF
```

**4. Configure logging**

```bash
sudo mkdir -p /var/log/dotnet-scaffold
sudo chown scaffold:scaffold /var/log/dotnet-scaffold
sudo chmod 755 /var/log/dotnet-scaffold
```

Update `appsettings.json`:
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/dotnet-scaffold/scaffold-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

**5. Enable and start service**

```bash
sudo systemctl daemon-reload
sudo systemctl enable dotnet-scaffold
sudo systemctl start dotnet-scaffold
```

**6. Verify service**

```bash
sudo systemctl status dotnet-scaffold
sudo journalctl -u dotnet-scaffold -n 50 -f
```

#### Database Location

For systemd deployments, store database in:

```bash
/var/lib/dotnet-scaffold/scaffold.db
```

Update connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/var/lib/dotnet-scaffold/scaffold.db"
  }
}
```

Create directory:
```bash
sudo mkdir -p /var/lib/dotnet-scaffold
sudo chown scaffold:scaffold /var/lib/dotnet-scaffold
sudo chmod 700 /var/lib/dotnet-scaffold
```

### 2. Docker Deployment

Recommended for containerized environments, Kubernetes, or Docker Swarm.

#### Build Docker Image

Use included `Dockerfile`:

```bash
docker build -t dotnet-scaffold:latest .
```

Or with build arguments:

```bash
docker build -t dotnet-scaffold:1.0.0 \
  --build-arg VERSION=1.0.0 \
  .
```

#### Run Container

**Development**:
```bash
docker run -it \
  -p 5000:5000 \
  -e "ASPNETCORE_ENVIRONMENT=Development" \
  dotnet-scaffold:latest
```

**Production**:
```bash
docker run -d \
  --name dotnet-scaffold \
  -p 5000:5000 \
  -v scaffold-data:/app/data \
  -e "ASPNETCORE_ENVIRONMENT=Production" \
  --restart unless-stopped \
  --health-cmd="curl -f http://localhost:5000/health || exit 1" \
  --health-interval=30s \
  --health-timeout=3s \
  --health-retries=3 \
  dotnet-scaffold:latest
```

#### Docker Compose Deployment

Use included `docker-compose.yml`:

```bash
docker-compose up -d
```

View logs:
```bash
docker-compose logs -f
```

Stop service:
```bash
docker-compose down
```

### 3. Kubernetes Deployment

For cloud-native and scalable deployments.

#### Namespace Setup

```bash
kubectl create namespace dotnet-scaffold
kubectl config set-context --current --namespace=dotnet-scaffold
```

#### Persistent Volume

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: scaffold-data
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 10Gi
```

Apply:
```bash
kubectl apply -f pvc.yaml
```

#### Deployment Manifest

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: dotnet-scaffold
spec:
  replicas: 3
  selector:
    matchLabels:
      app: dotnet-scaffold
  template:
    metadata:
      labels:
        app: dotnet-scaffold
    spec:
      containers:
      - name: scaffold
        image: dotnet-scaffold:latest
        imagePullPolicy: Always
        ports:
        - containerPort: 5000
          name: http
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ASPNETCORE_URLS
          value: "http://+:5000"
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /status
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 5
        volumeMounts:
        - name: data
          mountPath: /app/data
      volumes:
      - name: data
        persistentVolumeClaim:
          claimName: scaffold-data
```

#### Service Manifest

```yaml
apiVersion: v1
kind: Service
metadata:
  name: dotnet-scaffold
spec:
  type: LoadBalancer
  selector:
    app: dotnet-scaffold
  ports:
  - protocol: TCP
    port: 80
    targetPort: 5000
```

Deploy:
```bash
kubectl apply -f deployment.yaml
kubectl apply -f service.yaml
```

Monitor:
```bash
kubectl get pods
kubectl logs -f deployment/dotnet-scaffold
```

### 4. Caddy Reverse Proxy

Recommended for all deployments. Provides HTTPS termination, compression, and request routing.

#### Install Caddy

```bash
sudo apt-get update
sudo apt-get install -y caddy
```

#### Configure Caddy

Create `/etc/caddy/Caddyfile`:

```caddy
scaffold.example.com {
    reverse_proxy localhost:5000 {
        header_up X-Forwarded-For {http.request.remote.host}
        header_up X-Forwarded-Proto {http.request.proto}
        header_up X-Forwarded-Host {http.request.host}
        health_uri /health
        health_interval 10s
        health_timeout 5s
        health_status 200
    }
    
    # Compression
    encode gzip
    
    # Logging
    log {
        output file /var/log/caddy/scaffold.log
        level info
        format json
    }
    
    # Security headers
    header {
        Strict-Transport-Security "max-age=31536000; includeSubDomains"
        X-Content-Type-Options "nosniff"
        X-Frame-Options "DENY"
        X-XSS-Protection "1; mode=block"
    }
}
```

#### Enable Caddy

```bash
sudo systemctl enable caddy
sudo systemctl restart caddy
sudo systemctl status caddy
```

#### Verify

```bash
curl https://scaffold.example.com/health
```

### 5. Cloud Hosting

#### Azure

```bash
az appservice plan create \
  --name scaffold-plan \
  --resource-group mygroup \
  --sku B2

az webapp create \
  --resource-group mygroup \
  --plan scaffold-plan \
  --name dotnet-scaffold \
  --runtime "DOTNET|10.0"

# Deploy
dotnet publish -c Release -o ./publish
az webapp deployment source config-zip \
  --resource-group mygroup \
  --name dotnet-scaffold \
  --src-path ./publish.zip
```

#### AWS

Use Elastic Beanstalk:

```bash
eb create dotnet-scaffold \
  --instance-type t3.medium \
  --envvars ASPNETCORE_ENVIRONMENT=Production
eb deploy
```

Or App Runner:

```bash
aws apprunner create-service \
  --service-name dotnet-scaffold \
  --source-configuration ImageRepository={ImageRepositoryType=ECR, ImageIdentifier=xxx}
```

#### Google Cloud

```bash
gcloud run deploy dotnet-scaffold \
  --image gcr.io/project/dotnet-scaffold:latest \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated
```

## Configuration for Production

### Environment Variables

```bash
# Hosting
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://localhost:5000

# Database
ConnectionStrings__DefaultConnection="Data Source=/var/lib/dotnet-scaffold/scaffold.db"

# Application settings
ApplicationSettings__HealthCheckInterval=60
ApplicationSettings__HealthCheckTimeout=15
ApplicationSettings__MaxConcurrentHealthChecks=10
ApplicationSettings__AuditLogRetentionDays=365
ApplicationSettings__HealthCheckResultRetentionDays=90
```

### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/dotnet-scaffold/scaffold-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 365,
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
```

## Backup & Recovery

### Database Backup

**Automated daily backup**:

```bash
#!/bin/bash
# /opt/scripts/backup-scaffold.sh

BACKUP_DIR="/backups/dotnet-scaffold"
DB_PATH="/var/lib/dotnet-scaffold/scaffold.db"
DATE=$(date +%Y%m%d_%H%M%S)

mkdir -p $BACKUP_DIR
cp $DB_PATH $BACKUP_DIR/scaffold_$DATE.db.bak

# Keep only 30 days of backups
find $BACKUP_DIR -type f -mtime +30 -delete
```

Schedule with cron:

```bash
sudo crontab -e
# Add line:
0 2 * * * /opt/scripts/backup-scaffold.sh
```

### Backup to S3

```bash
#!/bin/bash
# /opt/scripts/backup-s3.sh

aws s3 cp /var/lib/dotnet-scaffold/scaffold.db \
  s3://backups/dotnet-scaffold/$(date +%Y%m%d).db
```

### Restore from Backup

```bash
# Stop service
sudo systemctl stop dotnet-scaffold

# Restore database
sudo cp /backups/dotnet-scaffold/scaffold_20260501_020000.db.bak \
  /var/lib/dotnet-scaffold/scaffold.db
sudo chown scaffold:scaffold /var/lib/dotnet-scaffold/scaffold.db

# Start service
sudo systemctl start dotnet-scaffold
```

## Monitoring & Alerting

### Health Check Monitoring

```bash
#!/bin/bash
# Monitor health every minute
while true; do
  response=$(curl -s http://localhost:5000/health)
  status=$(echo $response | jq -r '.status')
  
  if [ "$status" != "Healthy" ]; then
    # Send alert
    echo "Service unhealthy: $response" | mail -s "Alert: Scaffold Down" admin@example.com
  fi
  
  sleep 60
done
```

### Prometheus Metrics

Add to `Program.cs`:

```csharp
builder.Services.AddPrometheusMetrics();

app.UsePrometheusMetrics();
```

Scrape config:
```yaml
scrape_configs:
  - job_name: 'dotnet-scaffold'
    static_configs:
      - targets: ['localhost:5000']
    metrics_path: '/metrics'
```

### Log Aggregation

**ELK Stack** (Elasticsearch, Logstash, Kibana):

```json
{
  "WriteTo": [
    {
      "Name": "Elasticsearch",
      "Args": {
        "nodeUris": ["http://localhost:9200"],
        "indexFormat": "dotnet-scaffold-{0:yyyy.MM.dd}"
      }
    }
  ]
}
```

## Scaling & Performance

### Load Balancing

Use Caddy or Nginx with multiple instances:

```caddy
scaffold.example.com {
    reverse_proxy localhost:5000 localhost:5001 localhost:5002 {
        policy round_robin
        health_uri /health
    }
}
```

### Database Optimization

For larger deployments, consider migrating to PostgreSQL:

```bash
# Update connection string
ConnectionStrings__DefaultConnection="Server=postgres-host;Database=scaffold;User Id=scaffold;Password=xxx;"

# Change DbContext
options.UseNpgsql(connectionString)
```

## Troubleshooting

### Service won't start

```bash
# Check logs
sudo journalctl -u dotnet-scaffold -n 100

# Verify permissions
ls -la /opt/dotnet-scaffold/
ls -la /var/lib/dotnet-scaffold/

# Test configuration
dotnet /opt/dotnet-scaffold/dotnet-service-scaffold.dll --help
```

### High memory usage

```bash
# Monitor memory
watch -n 1 'ps aux | grep dotnet'

# Check for leaks
curl http://localhost:5000/api/metrics

# Restart service
sudo systemctl restart dotnet-scaffold
```

### Database locked

```bash
# Check lock files
ls -la /var/lib/dotnet-scaffold/scaffold.db*

# Remove lock (after stopping service)
sudo rm /var/lib/dotnet-scaffold/scaffold.db-wal
sudo rm /var/lib/dotnet-scaffold/scaffold.db-shm

# Restart
sudo systemctl restart dotnet-scaffold
```

### SSL certificate issues

```bash
# Verify certificate
curl -v https://scaffold.example.com/health

# Check Caddy status
sudo systemctl status caddy
sudo journalctl -u caddy -n 50

# Renew certificate
sudo caddy reload
```

## Security Hardening

1. **Firewall**:
   ```bash
   sudo ufw default deny incoming
   sudo ufw allow 22/tcp
   sudo ufw allow 80/tcp
   sudo ufw allow 443/tcp
   sudo ufw enable
   ```

2. **SSH Hardening**:
   - Disable password auth
   - Use SSH keys only
   - Change default port

3. **Application Secrets**:
   - Use environment variables
   - Implement secrets management (HashiCorp Vault)
   - Rotate API keys regularly

4. **Database**:
   - Regular backups
   - Access control
   - Encryption at rest

## Maintenance

### Updates

```bash
# Build new version
dotnet build -c Release
dotnet publish -c Release -o /tmp/publish

# Stop service
sudo systemctl stop dotnet-scaffold

# Backup current
sudo cp -r /opt/dotnet-scaffold /opt/dotnet-scaffold.backup

# Deploy new
sudo cp -r /tmp/publish/* /opt/dotnet-scaffold/
sudo chown -R scaffold:scaffold /opt/dotnet-scaffold

# Start service
sudo systemctl start dotnet-scaffold

# Verify
sudo systemctl status dotnet-scaffold
```

### Log Rotation

Configured in systemd with journalctl. Manual rotation:

```bash
sudo journalctl --vacuum=30d  # Keep 30 days
```

### Database Maintenance

```bash
# Analyze database
sqlite3 /var/lib/dotnet-scaffold/scaffold.db "ANALYZE;"

# Vacuum (compress)
sqlite3 /var/lib/dotnet-scaffold/scaffold.db "VACUUM;"
```

## Summary

You now have dotnet-service-scaffold deployed with:
- ✅ Production systemd service
- ✅ Reverse proxy (Caddy)
- ✅ HTTPS/SSL
- ✅ Automated backups
- ✅ Logging and monitoring
- ✅ Security hardening
