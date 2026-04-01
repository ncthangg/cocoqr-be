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
- AWS S3 SDK (`AWSSDK.S3`) for DigitalOcean Spaces
- Cloudinary (`CloudinaryDotNet`)
- Swagger / OpenAPI
- QRCoder

## Project Structure

```text
CocoQR.BE/
|-- CocoQR.API/             # HTTP API, controllers, middleware, config
|-- CocoQR.Application/     # Use cases, DTOs, service contracts, services
|-- CocoQR.Domain/          # Domain entities, constants, shared rules
|-- CocoQR.Infrastructure/  # EF Core, repositories, seeders, security
|-- CocoQR.QR_Generator/    # QR code generation module
|-- CocoQR.QR_Decoder/      # QR decoder project referenced by the solution
|-- CocoQR.sln              # Solution entry point
```

Main responsibilities:

- `CocoQR.API`: request pipeline, authentication setup, CORS,
  middleware, controllers, Swagger.
- `CocoQR.Application`: business services, DTOs, contracts, mapping,
  request-oriented logic.
- `CocoQR.Domain`: core entities, constants, and domain-level types.
- `CocoQR.Infrastructure`: database access, repositories, Redis,
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
   `CocoQR.API/appsettings.json` or environment-specific settings.
3. Restore dependencies:

```bash
dotnet restore CocoQR.sln
```

### Run the API

```bash
dotnet run --project CocoQR.API
```

Default local profiles are configured in
`CocoQR.API/Properties/launchSettings.json`:

- HTTP: `http://localhost:5092`
- HTTPS: `https://localhost:7234`

Swagger is enabled in `Development` and `Staging`.

### Apply database migrations

```bash
dotnet run --project CocoQR.API -- --migrate
```

In `Development`, the API also attempts to apply pending migrations on
startup.

## Scripts

Common commands for local development:

```bash
dotnet restore CocoQR.sln
dotnet build CocoQR.sln
dotnet run --project CocoQR.API
dotnet run --project CocoQR.API -- --migrate
```

There is no dedicated script runner in this repository; standard
`dotnet` commands are the main workflow.

## Environment

The API reads its primary configuration from
`CocoQR.API/appsettings.json`.

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

Cloud storage settings:

| Key | Purpose |
| --- | --- |
| `DigitalOcean:AccessKey` | Spaces access key (S3 credential) |
| `DigitalOcean:SecretKey` | Spaces secret key |
| `DigitalOcean:Bucket` | Spaces bucket name |
| `DigitalOcean:Region` | Spaces region (for example `sgp1`) |
| `DigitalOcean:Endpoint` | Public bucket endpoint, for example `https://{bucket}.{region}.digitaloceanspaces.com` |
| `Cloudinary:ApiKey` | Cloudinary API key |
| `Cloudinary:ApiSecret` | Cloudinary API secret |
| `Cloudinary:CloudName` | Cloudinary cloud name |
| `Cloudinary:ProjectName` | Root virtual folder prefix in Cloudinary |
| `Cloudinary:BaseUrl` | Cloudinary delivery base URL (for example `https://res.cloudinary.com`) |
| `FileUrl:BaseUrl` (optional) | Base URL used in Development when serving local files |

Notes:

- JWT settings are validated at startup and must be populated.
- Redis is required by the current infrastructure registration.
- `Auth:AllowedOrigins` should contain the frontend origins allowed to
  call the API with credentials.

## File Storage Usage

The project uses `IFileStorageService` as the application-level API and
`ICloudStorage` provider adapters in infrastructure.

Provider registration is configured in
`CocoQR.Infrastructure/DependencyInjection/DependencyInjection.cs`.
Current default:

```csharp
services.AddScoped<ICloudStorage, DigitalOceanStorage>();
// services.AddScoped<ICloudStorage, CloudinaryStorage>();
```

To switch provider:

1. Fill the corresponding section in `CocoQR.API/appsettings.json`.
2. Update the `ICloudStorage` registration in infrastructure DI.
3. Restart the API.

Runtime behavior by environment:

| Environment | Upload behavior | URL returned by API |
| --- | --- | --- |
| `Development` | Save local file only (`wwwroot/...`) | Relative path, or absolute URL if `FileUrl:BaseUrl` is set |
| `Staging` / `Production` | Upload cloud first, then save local mirror | Public cloud URL |

## Storage Path Convention

### 1) Business file upload path

For uploaded business files (bank/provider logos), the service builds:

- Folder from use case, for example:
  - `assets/banks`
  - `assets/providers`
- Filename as GUID with original extension:
  - `{guidN}{ext}`

Example relative path:

```text
assets/providers/2f6e9e6f3e9a4c35b9d5c31f2a4a3d7c.png
```

Cloud object key always includes environment prefix:

```text
{environment}/{relativePath}
```

Example (`Staging`):

```text
staging/assets/providers/2f6e9e6f3e9a4c35b9d5c31f2a4a3d7c.png
```

### 2) DigitalOcean Spaces URL pattern

`DigitalOceanStorage` keeps the key as-is and returns:

```text
{DigitalOcean:Endpoint}/{environment}/{relativePath}
```

Example:

```text
https://cocoqr.sgp1.digitaloceanspaces.com/staging/assets/providers/2f6e9e6f3e9a4c35b9d5c31f2a4a3d7c.png
```

### 3) Cloudinary URL pattern

`CloudinaryStorage` prepends `ProjectName` before the environment key.
Final virtual storage path:

```text
{ProjectName}/{environment}/{relativePath}
```

Delivery URL pattern depends on resource type:

- Image: `/image/upload/...`
- Video: `/video/upload/...`
- Other files (documents/logs): `/raw/upload/...`

Image example:

```text
https://res.cloudinary.com/{CloudName}/image/upload/{ProjectName}/staging/assets/providers/2f6e9e6f3e9a4c35b9d5c31f2a4a3d7c.png
```

### 4) Log upload path to cloud

In `Staging`/`Production`, `LogUploadService` uploads old `.txt` files
from log level folders:

- `logs/information`
- `logs/warning`
- `logs/error`

Cloud key format:

```text
{environment}/logs/{level}/{filename}.txt
```

Examples:

- DigitalOcean key: `production/logs/error/2026-04-01.txt`
- Cloudinary virtual path: `{ProjectName}/production/logs/error/2026-04-01.txt`

### 5) File validation limits

- Max upload size: `10 MB`
- Allowed image extensions: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.svg`
- Allowed document extensions: `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.txt`, `.csv`

## Architecture

The solution follows a layered architecture:

1. `CocoQR.API` handles HTTP concerns and middleware.
2. `CocoQR.Application` contains business logic and contracts.
3. `CocoQR.Domain` defines core models and shared constants.
4. `CocoQR.Infrastructure` implements persistence and external
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
- Put HTTP-specific concerns in `CocoQR.API`.
- Put business rules and service contracts in `CocoQR.Application`.
- Put persistence and external service implementations in
  `CocoQR.Infrastructure`.
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

- controller changes in `CocoQR.API`
- business logic in `CocoQR.Application`
- repository or integration work in `CocoQR.Infrastructure`
- shared domain changes in `CocoQR.Domain`

Before opening a change, verify that configuration, authentication, and
database impacts are documented clearly.
