# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================
# Dockerfile for dotnet-service-scaffold
#
# Multi-stage build for optimal image size and security.
# Usage:
#   docker build -t dotnet-scaffold:latest .
#   docker run -p 5000:5000 dotnet-scaffold:latest
#

# ============================================================================
# Stage 1: Build
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files
COPY dotnet-service-scaffold.csproj .

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build and publish
RUN dotnet publish -c Release -o /app/publish \
    --no-restore

# ============================================================================
# Stage 2: Runtime
# ============================================================================
FROM mcr.microsoft.com/dotnet/runtime:10.0

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN useradd -m -u 1000 scaffold && \
    mkdir -p /app/data /app/logs && \
    chown -R scaffold:scaffold /app

WORKDIR /app

# Copy built application from build stage
COPY --from=build --chown=scaffold:scaffold /app/publish .

# Set user to non-root
USER scaffold

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:5000 \
    ConnectionStrings__DefaultConnection=Data Source=/app/data/scaffold.db

# Set volumes for persistence
VOLUME ["/app/data", "/app/logs"]

# Run the application
ENTRYPOINT ["dotnet", "dotnet-service-scaffold.dll"]

# ============================================================================
# Build arguments and labels
# ============================================================================
# ARG VERSION=1.0.0
# LABEL org.opencontainers.image.title="DotNet Service Scaffold" \
#       org.opencontainers.image.version="${VERSION}" \
#       org.opencontainers.image.authors="Vladyslav Zaiets" \
#       org.opencontainers.image.url="https://github.com/sarmkadan/dotnet-service-scaffold" \
#       org.opencontainers.image.source="https://github.com/sarmkadan/dotnet-service-scaffold" \
#       org.opencontainers.image.licenses="MIT"

# ============================================================================
# Multi-arch build example (uncomment to use):
# ============================================================================
# FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# ARG TARGETPLATFORM
# RUN dotnet publish -c Release -o /app/publish \
#     --runtime linux-x64 \  # Change to match TARGETPLATFORM
#     --self-contained=false
