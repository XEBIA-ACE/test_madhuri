# Authentication Service

**Service ID:** AUTH-1  
**Service Name:** Authentication Service  
**Organization ID:** 95bd4e80-e002-4fe5-ab71-fa85aad9fec8  
**Project ID:** 6ac6b43c-29aa-4b94-abde-18897563e8e1

## Overview

The Authentication Service provides secure user authentication, token issuance, and session management for the platform. It ensures that only authorized users can access protected APIs and resources.

## Features

- User login with credential validation
- Issuance of authentication tokens
- Token validation for protected resource access
- User logout and session/token invalidation
- Request logging with timing metrics
- HTTPS enforcement (configurable)
- Basic resiliency with retry policies

## API Endpoints

### POST /login

Authenticates a user and issues an authentication token.

**Request:**
```json
{
  "username": "string",
  "password": "string"
}
```

**Response (200 OK):**
```json
{
  "token": "string",
  "expiresAt": "2024-01-01T12:00:00Z"
}
```

**Response (401 Unauthorized):**
```json
{
  "errorCode": "AUTH_INVALID_CREDENTIALS",
  "message": "Invalid username or password",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### POST /validate

Validates an authentication token.

**Request:**
```json
{
  "token": "string"
}
```

**Response (200 OK):**
```json
{
  "valid": true,
  "expiresAt": "2024-01-01T12:00:00Z"
}
```

### POST /logout

Invalidates an authentication token.

**Request:**
```json
{
  "token": "string"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Logout successful"
}
```

### GET /health

Health check endpoint.

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### GET /metrics

Returns authentication metrics.

**Response (200 OK):**
```json
{
  "totalLoginAttempts": 100,
  "successfulLogins": 95,
  "failedLogins": 5,
  "tokenValidations": 500,
  "validTokens": 480,
  "invalidTokens": 20,
  "logouts": 50,
  "snapshotTime": "2024-01-01T12:00:00Z"
}
```

## Configuration

### appsettings.json

```json
{
  "TokenSettings": {
    "TokenExpirationMinutes": 60,
    "AllowRefresh": false,
    "MaxActiveTokensPerUser": 5
  },
  "Security": {
    "EnforceHttps": false
  }
}
```

## Development

### Prerequisites

- .NET 8.0 SDK

### Build

```bash
cd src
dotnet build AuthenticationService.sln
```

### Run

```bash
cd src/AuthenticationService
dotnet run
```

The service will start on `http://localhost:5000` (or configured port).

### Test

```bash
cd src
dotnet test AuthenticationService.Tests
```

### Swagger UI

In development mode, Swagger UI is available at `/swagger`.

## Test Users

For development/testing, the following users are seeded:

| Username | Password |
|----------|----------|
| testuser | testpassword |
| admin | adminpassword |

## Architecture

```
AuthenticationService/
├── Controllers/           # API controllers
├── Configuration/         # Settings classes
├── Middleware/           # HTTP middleware
├── Models/
│   ├── Entities/         # Domain entities
│   ├── Requests/         # API request DTOs
│   └── Responses/        # API response DTOs
├── Resiliency/           # Polly policies
├── Services/             # Business logic
└── Stores/               # Data access
```

## TODO

- [ ] Replace in-memory stores with persistent storage (SQL Server, PostgreSQL)
- [ ] Replace in-memory token store with Redis for distributed scenarios
- [ ] Implement proper password hashing (bcrypt, Argon2)
- [ ] Consider JWT format for stateless token validation
- [ ] Add rate limiting for login attempts
- [ ] Integrate with proper metrics system (Prometheus, Application Insights)
- [ ] Add multi-factor authentication support
