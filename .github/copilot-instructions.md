# Copilot Instructions - CocoQR Backend

## Project Summary
CocoQR is a .NET 8 backend for payment QR workflows. The system provides APIs for authentication, user and role management, account and bank/provider configuration, QR generation, and QR style library management.

## Solution Structure
- `CocoQR.API`: HTTP API entry point (controllers, auth, middleware, CORS, Swagger).
- `CocoQR.Application`: service contracts, DTOs, business logic, and mappers.
- `CocoQR.Domain`: entities, constants, enums, and domain exceptions.
- `CocoQR.Infrastructure`: EF Core persistence, repositories, UnitOfWork, Redis, security/token services, seeders, background services.
- `CocoQR.QR_Generator`: QR payload builders and image rendering for supported providers.
- `CocoQR.QR_Decoder`: standalone QR decoding service.

## Runtime and Security
- Authentication uses JWT Bearer and Google OAuth sign-in flow.
- API pipeline includes forwarded headers, HTTPS redirection, static files, CORS, authentication, authorization, global exception handling, and security stamp validation.
- `SecurityStampMiddleware` rejects requests when token/user state is no longer valid.
- `GlobalExceptionHandlerMiddleware` standardizes error responses and maps business/application errors to HTTP status codes.

## Data and Integrations
- SQL Server with EF Core (`CocoQRDbContext`) and startup migrations in Development.
- Repository + UnitOfWork pattern for data access.
- Redis via StackExchange.Redis for infrastructure-level caching/session needs.
- File cleanup uses queue + background retry in non-Development environments.

## Core API Domains
- Auth and Google sign-in callback flow.
- Users, Roles, and UserRoles.
- Accounts, BankInfos, and Providers.
- QR generation/history and QR style library.
- Seed data synchronization.

## Development Conventions
- Keep architecture boundaries strict:
	- API: transport and HTTP concerns only.
	- Application/Domain: business rules and use-case logic.
	- Infrastructure: database and external integrations.
- Use DTO request/response models and keep response envelopes consistent.
- Keep routes lowercase and preserve current error code conventions.
- Register dependencies through each project's `DependencyInjection` extension methods.
- Preserve exception model (`DomainException`, `ApplicationException`) and status mapping behavior.
- For new payment providers, add a new `IQrPayloadBuilder` in `CocoQR.QR_Generator` and register it in DI.

## Local Run Notes
- API default URLs: `http://localhost:5092` and `https://localhost:7234`.
- Run migrations: `dotnet run --project CocoQR.API -- --migrate`.

