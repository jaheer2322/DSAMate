# DSAMate Backend (ASP.NET Core Web API)

A production-style API for DSA question management, user authentication, and progress tracking.

## What this API demonstrates

- Clean controller + repository architecture
- JWT authentication with ASP.NET Identity
- Role-based authorization (`Admin`, `User`)
- Entity Framework Core with SQL Server
- Topic progress aggregation and solved-state tracking
- Swagger for discoverability and testing
- Centralized exception handling middleware + Serilog logging

## Tech Stack

- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core + SQL Server
- ASP.NET Core Identity
- JWT Bearer authentication
- AutoMapper
- Serilog
- MSTest + Moq + EF InMemory (unit tests)

## Core Domains

- **Question**: title, description, topic, difficulty, hint
- **UserQuestionStatus**: per-user solved status and solved timestamp
- **Auth user**: Identity-managed users + roles

## Main API capabilities

### Auth (`/api/auth`)
- `POST /register` → create account (with role validation)
- `POST /login` → authenticate and return JWT
- `POST /unregister` → remove user account after credential check

### Questions (`/api/questions`)
- `GET /` → filtered, sorted, paginated list
- `GET /{id}` → single question
- `GET /random` → random unsolved question for current user
- `GET /solved` → all solved questions for current user
- `GET /progress` → topic-wise solved vs total summary
- `POST /{questionId}/mark-solved` → toggle solved state
- `POST /reset-progress` → clear user solved history
- `POST /` (Admin) → create question
- `POST /bulk` (Admin) → bulk create questions with duplicate validation

## Security model

- JWT validation (issuer, audience, signing key, lifetime)
- Role-based policies via `[Authorize(Roles = ...)]`
- User context extraction for per-user data operations

## Local Setup

### Prerequisites
- .NET SDK 8+
- SQL Server (local or remote)

### 1) Configure appsettings
Update `appsettings.json` connection strings and JWT settings:
- `ConnectionStrings:DbConnectionString`
- `ConnectionStrings:AuthDbConnectionString`
- `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`

### 2) Restore and run
```bash
dotnet restore
dotnet run --project DSAMate.API
```

### 3) Open Swagger
- Development Swagger UI is enabled at runtime (default local URL).

## Test

```bash
dotnet test DSAMate.API.Tests/DSAMate.API.UnitTests.csproj
```
