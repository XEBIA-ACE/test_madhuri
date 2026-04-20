# Authentication Service Specification

## Purpose

The Authentication Service provides secure user identity management, authentication, and JWT-based session handling for the Online Food Ordering System. It serves as the central authority for user credentials and token validation across all platform services.

## Technologies and Runtime Stack

- **Runtime**: Node.js 18+ or Java 17+ (Spring Boot 3.x)
- **Database**: PostgreSQL 14+ with AES-256 encryption at rest
- **Cache**: Redis 7+ for session data and rate limiting
- **Security**: bcrypt for password hashing, JWT for tokens, OAuth 2.0 for federated auth
- **Communication**: REST APIs over HTTPS, OpenAPI 3.0 specification
- **Monitoring**: Integration with ELK stack and Prometheus

## Service Components

### Presentation Layer
- **AuthRestController**: Handles HTTP REST requests for authentication operations
- **GraphQLAuthResolver**: Optional GraphQL interface for authentication mutations/queries
- **AuthRequestDTOs**: Request/response data structures with validation
- **AuthInputValidator**: Input validation and sanitization

### Business Layer
- **AuthService**: Core business logic for authentication workflows
- **UserSessionManager**: Stateless session management using JWT tokens
- **JWTManager**: Token creation, validation, and expiration handling
- **PasswordEncoder**: Secure password hashing and verification

### Data Access Layer
- **UserRepository**: Abstract interface for user data operations
- **UserRepositoryImpl**: PostgreSQL implementation of user repository
- **UserDataMapper**: Maps between domain objects and database records
- **TransactionManager**: Ensures ACID compliance for data operations

### Integration Layer
- **OAuth2Adapter**: Integration with external identity providers
- **LoggingAdapter**: Centralized logging for audit trails
- **MonitoringAgent**: Health and performance metrics emission

## API Endpoints

### User Registration
**POST /api/v1/auth/register**
- **Purpose**: Register new user with email and mobile verification
- **Input**: UserRegistrationRequest (email, mobile, password)
- **Output**: UserRegistrationResponse (userId, confirmationRequired, message)
- **Flow**: Validate input → Check duplicates → Hash password → Store user → Send verification

### User Authentication
**POST /api/v1/auth/login**
- **Purpose**: Authenticate user and issue JWT tokens
- **Input**: LoginRequest (emailOrMobile, password)
- **Output**: LoginResponse (accessToken, refreshToken, expiresIn, tokenType)
- **Flow**: Validate credentials → Verify password → Generate tokens → Return response

### Token Management
**POST /api/v1/auth/refresh**
- **Purpose**: Refresh access token using valid refresh token
- **Input**: TokenRefreshRequest (refreshToken)
- **Output**: LoginResponse (new accessToken, refreshToken, expiresIn)
- **Flow**: Validate refresh token → Generate new tokens → Invalidate old refresh token

**POST /api/v1/auth/logout**
- **Purpose**: Invalidate user session and refresh token
- **Input**: None (uses Authorization header)
- **Output**: OperationResult (success, message)
- **Flow**: Extract refresh token → Invalidate token → Clear session data

### Password Management
**POST /api/v1/auth/password/forgot**
- **Purpose**: Initiate password reset workflow
- **Input**: PasswordForgotRequest (emailOrMobile)
- **Output**: OperationResult (success, message)
- **Flow**: Find user → Generate reset token → Send notification → Store token

**POST /api/v1/auth/password/reset**
- **Purpose**: Reset password using secure token
- **Input**: PasswordResetRequest (resetToken, newPassword)
- **Output**: OperationResult (success, message)
- **Flow**: Validate reset token → Hash new password → Update user → Invalidate token

### Verification
**POST /api/v1/auth/verify-email**
- **Purpose**: Verify user email address
- **Input**: EmailVerificationRequest (verificationToken)
- **Output**: OperationResult (success, message)
- **Flow**: Validate verification token → Mark email verified → Update user status

### Health & Info
**GET /health**
- **Purpose**: Service health check for monitoring
- **Output**: HealthCheckResponse (status, timestamp)

**GET /api/v1/auth/info**
- **Purpose**: Service version and uptime information
- **Output**: ServiceInfoResponse (service, version, uptimeSeconds)

## Data Models

### Core Entities
**User**
- userId: UUID (primary key)
- email: Email (value object with validation)
- mobile: String (E.164 format)
- passwordHash: String (bcrypt hashed)
- roles: List<Role> (user permissions)
- createdAt: DateTime
- isActive: Boolean
- emailVerified: Boolean
- mobileVerified: Boolean

**Email** (Value Object)
- value: String
- Methods: isValid(), normalize()

**Role** (Value Object)
- name: String (e.g., "USER", "ADMIN")

### Request/Response DTOs
**UserRegistrationRequest**
- email: String (required, email format, max 256 chars)
- mobile: String (required, E.164 format)
- password: String (required, min 8 chars, max 128 chars)

**LoginRequest**
- emailOrMobile: String (required)
- password: String (required)

**LoginResponse**
- accessToken: String (JWT)
- refreshToken: String (secure random)
- expiresIn: Integer (seconds)
- tokenType: String ("Bearer")

## External Dependencies

### User Database Integration
- **Connection**: PostgreSQL via JDBC/ORM (Hibernate/TypeORM)
- **Operations**: User CRUD, credential lookup, transaction management
- **Security**: Encrypted connections, prepared statements, connection pooling
- **Error Handling**: Database connection failures, constraint violations, timeouts

### OAuth2 Provider Integration
- **Providers**: Google, Facebook, Apple OAuth 2.0/OpenID Connect
- **Flow**: Authorization code flow with PKCE
- **Operations**: Initiate OAuth flow, handle callbacks, exchange tokens, fetch user profile
- **Error Handling**: Invalid authorization codes, provider downtime, token validation failures

### Email/SMS Provider Integration
- **Providers**: Twilio (SMS), SendGrid (Email)
- **Operations**: Send verification emails, password reset notifications
- **Retry Logic**: Exponential backoff for failed deliveries
- **Error Handling**: Provider API failures, invalid recipient addresses

### Distributed Cache Integration
- **Technology**: Redis cluster
- **Operations**: Store/retrieve session data, rate limiting counters, temporary tokens
- **TTL Management**: Automatic expiration for tokens and rate limit windows
- **Error Handling**: Cache unavailable (fallback to database), connection timeouts

## Key Flows

### User Registration Flow
1. Receive registration request with email, mobile, password
2. Validate input format and business rules
3. Check for existing users with same email/mobile
4. Hash password using bcrypt with salt
5. Generate email verification token
6. Store user record in database (transaction)
7. Send verification email via external provider
8. Return registration response with confirmation requirement

### Login Authentication Flow
1. Receive login request with emailOrMobile and password
2. Validate input and apply rate limiting
3. Lookup user by email or mobile in database
4. Verify password hash using bcrypt
5. Check user account status (active, verified)
6. Generate JWT access token (15min expiry) and refresh token (7 days)
7. Store refresh token in cache with TTL
8. Return tokens and expiration information

### Token Refresh Flow
1. Receive refresh token from client
2. Validate refresh token format and signature
3. Lookup token in cache/database
4. Verify token hasn't expired or been revoked
5. Generate new access token and refresh token
6. Invalidate old refresh token
7. Store new refresh token with TTL
8. Return new token pair

### Password Reset Flow
1. Receive forgot password request with email/mobile
2. Lookup user in database
3. Generate secure reset token (UUID) with 1-hour expiry
4. Store reset token in cache with TTL
5. Send reset instructions via email/SMS
6. User clicks reset link with token
7. Validate reset token and expiry
8. Hash new password and update user record
9. Invalidate reset token
10. Send confirmation notification

### OAuth Integration Flow
1. Client initiates OAuth flow via API Gateway
2. Redirect to OAuth provider with state parameter
3. User authenticates with provider
4. Provider redirects back with authorization code
5. Exchange code for access token with provider
6. Fetch user profile from provider
7. Create or lookup user in local database
8. Generate local JWT tokens
9. Return tokens to client

## Requirements

### Functional Requirements

#### Requirement: User Registration
Users SHALL be able to create accounts using email and mobile number.

**Scenario: Successful Registration**
- Given a user provides valid email, mobile, and password
- When they submit registration request
- Then system SHALL create user account
- And system SHALL send email verification
- And system SHALL return success response with userId

**Scenario: Duplicate Email Registration**
- Given a user tries to register with existing email
- When they submit registration request  
- Then system SHALL reject registration
- And system SHALL return 409 Conflict error

#### Requirement: User Authentication
Users SHALL be able to authenticate using email or mobile with password.

**Scenario: Successful Login**
- Given a user has valid credentials
- When they submit login request
- Then system SHALL validate credentials
- And system SHALL issue JWT tokens
- And system SHALL return tokens with expiration

**Scenario: Invalid Credentials**
- Given a user provides incorrect password
- When they submit login request
- Then system SHALL reject authentication
- And system SHALL return 401 Unauthorized
- And system SHALL log failed attempt

#### Requirement: Token Management
System SHALL provide stateless JWT-based session management.

**Scenario: Token Validation**
- Given a valid JWT access token
- When system validates token
- Then system SHALL extract user context
- And system SHALL allow access to protected resources

**Scenario: Token Refresh**
- Given a valid refresh token
- When user requests token refresh
- Then system SHALL issue new access token
- And system SHALL invalidate old refresh token

#### Requirement: Password Security
System SHALL securely handle password storage and recovery.

**Scenario: Password Hashing**
- Given a user password during registration/reset
- When system processes password
- Then system SHALL hash password using bcrypt
- And system SHALL never store plaintext passwords

**Scenario: Password Reset**
- Given a user requests password reset
- When system processes request
- Then system SHALL generate secure reset token
- And system SHALL send reset instructions
- And token SHALL expire within 1 hour