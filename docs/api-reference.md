# API Reference

Complete reference for all REST API endpoints in dotnet-service-scaffold.

## Overview

- **Base URL**: `http://localhost:5000`
- **API Version**: v1
- **Format**: JSON
- **Authentication**: API Key or Bearer Token

## Authentication

### API Key Authentication

For service-to-service communication:

```
X-API-Key: sk_live_abc123xyz789
```

Example:
```bash
curl -H "X-API-Key: sk_live_abc123xyz789" \
     http://localhost:5000/api/service
```

### Bearer Token Authentication

For user authentication after login:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Example:
```bash
curl -H "Authorization: Bearer eyJhbGc..." \
     http://localhost:5000/api/user/me
```

## Response Format

All responses follow a standard format:

```json
{
  "success": true,
  "data": {},
  "error": null
}
```

Error response:
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "SERVICE_NOT_FOUND",
    "message": "Service with ID 'svc-123' not found"
  }
}
```

## Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created |
| 204 | No Content - Success with no body |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Missing/invalid auth |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource doesn't exist |
| 500 | Internal Server Error |
| 503 | Service Unavailable |

## Endpoints

### System Endpoints

#### Health Check

```
GET /health
```

Returns service health status.

**Response**:
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy"
  }
}
```

#### Status

```
GET /status
```

Returns service status with version.

**Response**:
```json
{
  "status": "healthy",
  "timestamp": "2026-05-04T10:00:00Z",
  "version": "1.0.0"
}
```

### User Endpoints

#### Register User

```
POST /api/user/register
```

Create new user account.

**Parameters**:
```json
{
  "username": "string",
  "email": "string",
  "password": "string"
}
```

**Validation**:
- Username: 3-50 characters, alphanumeric + underscore
- Email: valid email format
- Password: minimum 8 characters

**Response** (201):
```json
{
  "success": true,
  "data": {
    "userId": "user-12345678",
    "username": "admin",
    "email": "admin@example.com",
    "createdAt": "2026-05-04T10:00:00Z"
  }
}
```

**Errors**:
- `DUPLICATE_USERNAME` - Username already exists
- `DUPLICATE_EMAIL` - Email already exists
- `INVALID_PASSWORD` - Password doesn't meet requirements

#### Login

```
POST /api/user/login
```

Authenticate user and get JWT token.

**Parameters**:
```json
{
  "username": "string",
  "password": "string"
}
```

**Response** (200):
```json
{
  "success": true,
  "data": {
    "userId": "user-12345678",
    "username": "admin",
    "email": "admin@example.com",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 86400
  }
}
```

**Errors**:
- `INVALID_CREDENTIALS` - Username or password incorrect
- `ACCOUNT_LOCKED` - Too many failed attempts
- `USER_NOT_FOUND` - User doesn't exist

#### Get User Info

```
GET /api/user/{userId}
Authorization: Bearer {token}
```

Get user profile information.

**Path Parameters**:
- `userId` - User UUID

**Response** (200):
```json
{
  "success": true,
  "data": {
    "userId": "user-12345678",
    "username": "admin",
    "email": "admin@example.com",
    "createdAt": "2026-05-04T10:00:00Z",
    "lastLoginAt": "2026-05-04T10:15:00Z"
  }
}
```

#### Change Password

```
POST /api/user/{userId}/change-password
Authorization: Bearer {token}
```

Change user password.

**Path Parameters**:
- `userId` - User UUID

**Parameters**:
```json
{
  "oldPassword": "string",
  "newPassword": "string"
}
```

**Response** (200):
```json
{
  "success": true,
  "data": {
    "message": "Password changed successfully"
  }
}
```

**Errors**:
- `INVALID_OLD_PASSWORD` - Current password incorrect
- `PASSWORD_TOO_WEAK` - New password doesn't meet requirements

#### Unlock Account

```
POST /api/user/{userId}/unlock
X-API-Key: {adminKey}
```

Unlock user account after lockout.

**Path Parameters**:
- `userId` - User UUID

**Response** (200):
```json
{
  "success": true,
  "data": {
    "message": "Account unlocked"
  }
}
```

### Service Endpoints

#### Register Service

```
POST /api/service/register
X-API-Key: {apiKey}
```

Register new service for monitoring.

**Parameters**:
```json
{
  "name": "string",
  "description": "string",
  "healthCheckUrl": "string",
  "ownerId": "string",
  "isEnabled": "boolean"
}
```

**Validation**:
- `name`: 1-100 characters
- `healthCheckUrl`: valid HTTP/HTTPS URL
- `ownerId`: valid user UUID

**Response** (201):
```json
{
  "success": true,
  "data": {
    "id": "svc-abc123def456",
    "name": "UserService",
    "description": "User authentication service",
    "healthCheckUrl": "https://users.internal/health",
    "status": "Pending",
    "isEnabled": true,
    "createdAt": "2026-05-04T10:00:00Z"
  }
}
```

#### List Services

```
GET /api/service
X-API-Key: {apiKey}
```

List all registered services with optional filtering.

**Query Parameters**:
- `name` (optional) - Filter by name (substring match)
- `status` (optional) - Filter by status (Healthy/Unhealthy/Pending)
- `ownerId` (optional) - Filter by owner
- `limit` (default: 50) - Results per page
- `offset` (default: 0) - Pagination offset

**Response** (200):
```json
{
  "success": true,
  "data": [
    {
      "id": "svc-abc123",
      "name": "UserService",
      "description": "User authentication service",
      "status": "Healthy",
      "healthCheckUrl": "https://users.internal/health",
      "successRate": 99.8,
      "lastChecked": "2026-05-04T10:00:30Z",
      "isEnabled": true
    }
  ],
  "pagination": {
    "total": 42,
    "limit": 50,
    "offset": 0
  }
}
```

#### Get Service Details

```
GET /api/service/{serviceId}
X-API-Key: {apiKey}
```

Get detailed information about specific service.

**Path Parameters**:
- `serviceId` - Service UUID

**Response** (200):
```json
{
  "success": true,
  "data": {
    "id": "svc-abc123",
    "name": "UserService",
    "description": "User authentication service",
    "healthCheckUrl": "https://users.internal/health",
    "status": "Healthy",
    "isEnabled": true,
    "successRate": 99.8,
    "lastCheckedAt": "2026-05-04T10:00:30Z",
    "ownerId": "user-12345678",
    "createdAt": "2026-05-04T09:00:00Z",
    "updatedAt": "2026-05-04T10:00:30Z"
  }
}
```

#### Get Services by Owner

```
GET /api/service/owner/{ownerId}
X-API-Key: {apiKey}
```

List services owned by specific user.

**Path Parameters**:
- `ownerId` - Owner user UUID

**Response** (200):
```json
{
  "success": true,
  "data": [
    {
      "id": "svc-abc123",
      "name": "UserService",
      "status": "Healthy"
    },
    {
      "id": "svc-def456",
      "name": "PaymentService",
      "status": "Unhealthy"
    }
  ]
}
```

#### Enable Service

```
POST /api/service/{serviceId}/enable
X-API-Key: {apiKey}
```

Enable health checks for service.

**Path Parameters**:
- `serviceId` - Service UUID

**Response** (200):
```json
{
  "success": true,
  "data": {
    "message": "Service enabled"
  }
}
```

#### Disable Service

```
POST /api/service/{serviceId}/disable
X-API-Key: {apiKey}
```

Disable health checks for service.

**Path Parameters**:
- `serviceId` - Service UUID

**Response** (200):
```json
{
  "success": true,
  "data": {
    "message": "Service disabled"
  }
}
```

#### Get Unhealthy Services

```
GET /api/service/health/unhealthy
X-API-Key: {apiKey}
```

List all services currently unhealthy.

**Query Parameters**:
- `limit` (default: 50) - Results per page

**Response** (200):
```json
{
  "success": true,
  "data": [
    {
      "id": "svc-xyz789",
      "name": "CacheService",
      "status": "Unhealthy",
      "lastError": "Connection refused",
      "failureCount": 3,
      "lastCheckedAt": "2026-05-04T10:00:20Z"
    }
  ]
}
```

### Health Check Endpoints

#### Check Service Health

```
POST /api/healthcheck/{serviceId}/check
X-API-Key: {apiKey}
```

Perform immediate health check on service.

**Path Parameters**:
- `serviceId` - Service UUID

**Response** (200):
```json
{
  "success": true,
  "data": {
    "id": "hc-uuid123",
    "serviceId": "svc-abc123",
    "status": "Healthy",
    "responseTime": 125,
    "statusCode": 200,
    "message": "OK",
    "checkedAt": "2026-05-04T10:00:45Z"
  }
}
```

**Status Values**:
- `Healthy` - Service responding normally
- `Degraded` - Service responding slowly
- `Unhealthy` - Service not responding or errors

#### Get Health Status

```
GET /api/healthcheck/{serviceId}/status
X-API-Key: {apiKey}
```

Get current health status without new check.

**Response** (200):
```json
{
  "success": true,
  "data": {
    "serviceId": "svc-abc123",
    "status": "Healthy",
    "lastCheckedAt": "2026-05-04T10:00:30Z",
    "successRate": 99.8,
    "failureCount": 2
  }
}
```

#### Get Health History

```
GET /api/healthcheck/{serviceId}/history
X-API-Key: {apiKey}
```

Get historical health check results.

**Path Parameters**:
- `serviceId` - Service UUID

**Query Parameters**:
- `days` (default: 7) - Number of days to retrieve
- `limit` (default: 100) - Results per page

**Response** (200):
```json
{
  "success": true,
  "data": [
    {
      "id": "hc-uuid1",
      "status": "Healthy",
      "responseTime": 123,
      "statusCode": 200,
      "checkedAt": "2026-05-04T10:00:45Z"
    },
    {
      "id": "hc-uuid2",
      "status": "Healthy",
      "responseTime": 145,
      "statusCode": 200,
      "checkedAt": "2026-05-04T09:59:45Z"
    }
  ]
}
```

#### Get Failed Checks

```
GET /api/healthcheck/{serviceId}/failures
X-API-Key: {apiKey}
```

Get failed health check attempts.

**Path Parameters**:
- `serviceId` - Service UUID

**Query Parameters**:
- `limit` (default: 50) - Number of failures
- `days` (default: 30) - Days to look back

**Response** (200):
```json
{
  "success": true,
  "data": [
    {
      "id": "hc-fail1",
      "status": "Unhealthy",
      "responseTime": 0,
      "statusCode": 500,
      "message": "Internal Server Error",
      "checkedAt": "2026-05-03T14:22:15Z"
    }
  ]
}
```

### Metrics Endpoints

#### Get Service Metrics

```
GET /api/metrics/service/{serviceId}
X-API-Key: {apiKey}
```

Get performance metrics for service.

**Path Parameters**:
- `serviceId` - Service UUID

**Response** (200):
```json
{
  "success": true,
  "data": {
    "serviceId": "svc-abc123",
    "cpuUsage": 45.2,
    "memoryUsage": 512,
    "diskUsage": 2048,
    "averageResponseTime": 125,
    "requestsPerMinute": 450,
    "errorRate": 0.2,
    "lastUpdated": "2026-05-04T10:00:00Z"
  }
}
```

#### Get All Metrics

```
GET /api/metrics
X-API-Key: {apiKey}
```

Get aggregated system metrics.

**Query Parameters**:
- `limit` (default: 50) - Services to include

**Response** (200):
```json
{
  "success": true,
  "data": [
    {
      "serviceId": "svc-abc123",
      "cpuUsage": 45.2,
      "errorRate": 0.2,
      "lastUpdated": "2026-05-04T10:00:00Z"
    }
  ]
}
```

### Audit Log Endpoints

#### Get Audit Logs

```
GET /api/auditlog
X-API-Key: {apiKey}
```

Retrieve audit logs for compliance and analysis.

**Query Parameters**:
- `userId` (optional) - Filter by user
- `action` (optional) - Filter by action type
- `days` (default: 30) - Days to retrieve
- `limit` (default: 100) - Results per page
- `offset` (default: 0) - Pagination offset

**Response** (200):
```json
{
  "success": true,
  "data": [
    {
      "id": "audit-uuid1",
      "userId": "user-12345",
      "action": "ServiceRegistered",
      "entityType": "Service",
      "entityId": "svc-abc123",
      "changes": {
        "name": "UserService",
        "status": "Active"
      },
      "timestamp": "2026-05-04T10:00:00Z",
      "ipAddress": "192.168.1.100"
    }
  ],
  "pagination": {
    "total": 450,
    "limit": 100,
    "offset": 0
  }
}
```

#### Get Audit Log Entry

```
GET /api/auditlog/{logId}
X-API-Key: {apiKey}
```

Get specific audit log entry.

**Path Parameters**:
- `logId` - Audit log UUID

**Response** (200):
```json
{
  "success": true,
  "data": {
    "id": "audit-uuid1",
    "userId": "user-12345",
    "action": "ServiceRegistered",
    "entityType": "Service",
    "entityId": "svc-abc123",
    "changes": {
      "name": "UserService",
      "status": "Active"
    },
    "timestamp": "2026-05-04T10:00:00Z",
    "ipAddress": "192.168.1.100"
  }
}
```

### API Key Endpoints

#### Create API Key

```
POST /api/apikey/create
X-API-Key: {adminKey}
```

Create new API key for service authentication.

**Parameters**:
```json
{
  "name": "string",
  "description": "string",
  "ipWhitelist": ["string"],
  "scopes": ["string"],
  "expiresInDays": "integer"
}
```

**Scopes**:
- `service:read` - Read service data
- `service:write` - Register/modify services
- `healthcheck:read` - Read health checks
- `healthcheck:write` - Execute health checks
- `metrics:read` - Read metrics
- `audit:read` - Read audit logs

**Response** (201):
```json
{
  "success": true,
  "data": {
    "apiKey": "sk_live_abc123xyz789def456",
    "name": "MyService",
    "description": "Production monitoring",
    "scopes": ["service:read", "healthcheck:read"],
    "createdAt": "2026-05-04T10:00:00Z",
    "expiresAt": "2026-08-02T10:00:00Z"
  }
}
```

#### List API Keys

```
GET /api/apikey
X-API-Key: {adminKey}
```

List all active API keys (no key values shown).

**Query Parameters**:
- `limit` (default: 50) - Results per page

**Response** (200):
```json
{
  "success": true,
  "data": [
    {
      "id": "key-uuid1",
      "name": "MyService",
      "keyPrefix": "sk_live_abc123...",
      "scopes": ["service:read"],
      "createdAt": "2026-05-04T10:00:00Z",
      "lastUsedAt": "2026-05-04T10:15:30Z",
      "expiresAt": "2026-08-02T10:00:00Z"
    }
  ]
}
```

#### Revoke API Key

```
POST /api/apikey/{keyId}/revoke
X-API-Key: {adminKey}
```

Revoke and disable API key.

**Path Parameters**:
- `keyId` - API key UUID

**Response** (200):
```json
{
  "success": true,
  "data": {
    "message": "API key revoked"
  }
}
```

## Rate Limiting

API rate limits (per IP):

- **Default**: 60 requests per minute
- **Burst**: 10 requests per second

Response includes rate limit headers:

```
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1704196560
```

When limit exceeded (429):
```json
{
  "success": false,
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many requests. Reset at 2026-05-04T10:03:00Z"
  }
}
```

## Error Codes

| Code | HTTP | Description |
|------|------|-------------|
| `INVALID_REQUEST` | 400 | Malformed request |
| `INVALID_API_KEY` | 401 | Missing or invalid API key |
| `UNAUTHORIZED` | 401 | Not authenticated |
| `FORBIDDEN` | 403 | Insufficient permissions |
| `NOT_FOUND` | 404 | Resource not found |
| `DUPLICATE_RESOURCE` | 409 | Resource already exists |
| `RATE_LIMIT_EXCEEDED` | 429 | Too many requests |
| `VALIDATION_ERROR` | 422 | Invalid input data |
| `INTERNAL_ERROR` | 500 | Server error |

## Pagination

List endpoints support pagination:

```
GET /api/service?limit=25&offset=50
```

Response includes metadata:
```json
{
  "success": true,
  "data": [ ... ],
  "pagination": {
    "total": 150,
    "limit": 25,
    "offset": 50,
    "hasMore": true
  }
}
```
