# CocoQR.BE

## Overview

CocoQR.BE is a .NET 8 backend for wallet and payment-related
workflows. The solution exposes REST APIs for authentication, user and
role management, bank and provider configuration, account management,
QR generation, and seed-data synchronization.

The codebase is organized as a multi-project solution with clear
separation between API, application logic, domain models, and
infrastructure concerns. It is designed for production-oriented
deployment with JWT authentication, Google OAuth sign-in, SQL Server
persistence, and Redis integration.

## Key Features

- Generate payment QR codes (VietQR available, MoMo / ZaloPay / VNPay integration in progress)
- Manage multiple bank/payment accounts
- Save and reuse QR configurations
- Save Custom QR styling (colors, logo, layout)
- Role-based admin dashboard
- Google Sign-In authentication

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server
- Dapper
- Redis via `StackExchange.Redis`
- JWT Bearer Authentication
- Google OAuth Authentication
- Swagger / OpenAPI
- QRCoder

## Project Structure

```text
MyWallet.BE/
|-- MyWallet.API/             # HTTP API, controllers, middleware, config
|-- MyWallet.Application/     # Use cases, DTOs, service contracts, services
|-- MyWallet.Domain/          # Domain entities, constants, shared rules
|-- MyWallet.Infrastructure/  # EF Core, repositories, seeders, security
|-- MyWallet.QR_Generator/    # QR code generation module
|-- MyWallet.QR_Decoder/      # QR decoder project referenced by the solution
|-- MyWallet_BE.sln           # Solution entry point
```

Main responsibilities:

- `MyWallet.API`: request pipeline, authentication setup, CORS,
  middleware, controllers, Swagger.
- `MyWallet.Application`: business services, DTOs, contracts, mapping,
  request-oriented logic.
- `MyWallet.Domain`: core entities, constants, and domain-level types.
- `MyWallet.Infrastructure`: database access, repositories, Redis,
  token services, file storage, and seeders.

## Getting Started

### Prerequisites

- .NET SDK 8.0+
- SQL Server
- Redis
- Google OAuth credentials for sign-in flow

### Installation

1. Clone the repository.
2. Update configuration values in
   `MyWallet.API/appsettings.json` or environment-specific settings.
3. Restore dependencies:

```bash
dotnet restore MyWallet_BE.sln
```

### Run the API

```bash
dotnet run --project MyWallet.API
```

Default local profiles are configured in
`MyWallet.API/Properties/launchSettings.json`:

- HTTP: `http://localhost:5092`
- HTTPS: `https://localhost:7234`

Swagger is enabled in `Development` and `Staging`.

### Apply database migrations

```bash
dotnet run --project MyWallet.API -- --migrate
```

In `Development`, the API also attempts to apply pending migrations on
startup.

## Scripts

Common commands for local development:

```bash
dotnet restore MyWallet_BE.sln
dotnet build MyWallet_BE.sln
dotnet run --project MyWallet.API
dotnet run --project MyWallet.API -- --migrate
```

There is no dedicated script runner in this repository; standard
`dotnet` commands are the main workflow.

## Environment

The API reads its primary configuration from
`MyWallet.API/appsettings.json`.

Important settings:

| Key | Purpose |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `JWT:Issuer` | JWT issuer |
| `JWT:Audience` | JWT audience |
| `JWT:SecretKey` | JWT signing key |
| `JWT:AccessTokenExpirationMinutes` | Access token lifetime |
| `JWT:RefreshTokenExpirationDays` | Refresh token lifetime |
| `Redis:ConnectionString` | Redis connection |
| `Redis:AbortOnConnectFail` | Redis startup behavior |
| `Redis:ConnectRetry` | Redis retry count |
| `Redis:ConnectTimeoutMs` | Redis connect timeout |
| `Redis:SyncTimeoutMs` | Redis sync timeout |
| `Redis:ReconnectRetryIntervalMs` | Redis reconnect interval |
| `Oauth:ClientId` | Google OAuth client ID |
| `Oauth:ClientSecret` | Google OAuth client secret |
| `Auth:AllowedOrigins` | Allowed CORS origins |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment |

Notes:

- JWT settings are validated at startup and must be populated.
- Redis is required by the current infrastructure registration.
- `Auth:AllowedOrigins` should contain the frontend origins allowed to
  call the API with credentials.

## Architecture

The solution follows a layered architecture:

1. `MyWallet.API` handles HTTP concerns and middleware.
2. `MyWallet.Application` contains business logic and contracts.
3. `MyWallet.Domain` defines core models and shared constants.
4. `MyWallet.Infrastructure` implements persistence and external
   integrations.

Important scenarios currently supported:

- Google OAuth sign-in and callback handling
- JWT-based authenticated API access
- User and role queries and role switching
- Account CRUD and status updates
- Provider and bank information management
- QR generation and QR style library management
- Admin-only seed synchronization for roles, providers, and banks

Coding conventions used in this repository:

- Keep responsibilities separated by project layer.
- Put HTTP-specific concerns in `MyWallet.API`.
- Put business rules and service contracts in `MyWallet.Application`.
- Put persistence and external service implementations in
  `MyWallet.Infrastructure`.
- Use dependency injection through dedicated registration extensions.
- Keep API routes lowercase and use DTOs for request and response models.
- Return consistent response envelopes from controllers.

Instruction-file note:

- No `.ai/.github/docs` instruction directory was present in this
  repository during analysis.
- UI, layout, CSS, Tailwind, and component-style rules do not appear to
  apply to this backend-focused solution.

## Contributing

Contributions should preserve the existing project layering and keep new
features aligned with the current separation of concerns. When adding
new endpoints, prefer:

- controller changes in `MyWallet.API`
- business logic in `MyWallet.Application`
- repository or integration work in `MyWallet.Infrastructure`
- shared domain changes in `MyWallet.Domain`

Before opening a change, verify that configuration, authentication, and
database impacts are documented clearly.
