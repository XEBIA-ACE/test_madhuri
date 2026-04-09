# Authentication Service (AUTH-1)

A secure, standards-compliant authentication service for user registration, login, logout, and token management.

## Service Information

- **Service ID**: AUTH-1
- **Service Name**: Authentication Service
- **Organization ID**: 95bd4e80-e002-4fe5-ab71-fa85aad9fec8
- **Project ID**: 6ac6b43c-29aa-4b94-abde-18897563e8e1

## Features

- User registration with email and password
- Secure login with JWT token issuance
- Token validation for service-to-service authentication
- Session management and logout
- Password hashing with BCrypt
- Rate limiting for brute-force protection
- Health check endpoints

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Authenticate and receive tokens |
| POST | `/api/auth/logout` | Invalidate session token |
| POST | `/api/auth/token/validate` | Validate a token (for internal services) |
| GET | `/api/health` | Health check |
| GET | `/api/health/ready` | Readiness probe |
| GET | `/api/health/live` | Liveness probe |

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server (optional, uses in-memory database by default)

### Running Locally

```bash
cd src/AuthenticationService.Api
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`.

### Running Tests

```bash
cd src
dotnet test
```

### Docker

```bash
cd src
docker build -t authentication-service -f AuthenticationService.Api/Dockerfile .
docker run -p 8080:80 authentication-service
```

## Configuration

Configure the service using `appsettings.json` or environment variables:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSecretKey",
    "Issuer": "AuthenticationService",
    "Audience": "AuthenticationServiceClients",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=AuthDb;..."
  }
}
```

## Security

- Passwords are hashed using BCrypt with work factor 12
- JWT tokens are signed with HMAC-SHA256
- Rate limiting prevents brute-force attacks
- Input validation on all endpoints
- HTTPS recommended for production

## Project Structure

```
src/
├── AuthenticationService.Api/          # Web API layer
│   ├── Controllers/                    # API controllers
│   ├── Middleware/                     # Custom middleware
│   └── Program.cs                      # Application entry point
├── AuthenticationService.Core/         # Domain layer
│   ├── Configuration/                  # Settings classes
│   ├── DTOs/                          # Data transfer objects
│   ├── Entities/                      # Domain entities
│   ├── Exceptions/                    # Custom exceptions
│   └── Interfaces/                    # Service interfaces
├── AuthenticationService.Infrastructure/ # Infrastructure layer
│   ├── Data/                          # Database context
│   ├── Repositories/                  # Data access
│   └── Services/                      # Service implementations
└── AuthenticationService.Tests/        # Unit and integration tests
```
