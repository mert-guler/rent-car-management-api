# Rent Car Management

A .NET 10 layered-monolith car rental application with a separate static web frontend and SQL Server persistence.

## Structure

```text
frontend/                          React + TypeScript client (Vite)
backend/
  RentCarManagement.Api/           HTTP API, controllers, host configuration
  RentCarManagement.Application/    Use cases, business rules, DTOs and repository/service interfaces
  RentCarManagement.Domain/         Entities, enums and stable domain error codes
  RentCarManagement.Infrastructure/ EF Core repositories, SQL Server, migrations and adapters
  RentCarManagement.Api.Tests/      HTTP integration tests / test client
RentCarManagement.sln
```

All backend projects run as one deployable application (a monolith). Controllers translate HTTP requests and claims into calls to Application services; Application owns workflows and business rules through repository interfaces; Infrastructure implements those interfaces with EF Core and SQL Server.

## Run locally (Windows / SQL Server Express)

1. Install the .NET 10 SDK and SQL Server Express.
2. Install Node.js LTS. From the repository root, install and build the frontend:

   ```powershell
   cd frontend
   npm install
   npm run build
   cd ..
   ```
3. Set the local JWT signing key using User Secrets from the API project directory:

   ```powershell
   cd backend/RentCarManagement.Api
   dotnet user-secrets set "Jwt:Key" "<random secret of at least 32 bytes>"
   ```

   The existing User Secrets ID is retained. Never commit a real signing key.
4. From the repository root, start the app:

   ```powershell
   dotnet run --project backend/RentCarManagement.Api
   ```

The API serves the built React UI at `http://localhost:5165`. Rebuild the frontend with `npm run build` after UI changes. In Development, Swagger UI is at `http://localhost:5165/swagger` and the OpenAPI document is at `/openapi/v1.json`.

## Database migration note

The latest migration changes entity primary and foreign keys from integers to GUIDs. It creates new GUIDs and remaps existing rental, damage-report, and password-reset relationships, preserving existing rows. **Back up `RentCarManagementDb` before starting the updated application for the first time.** The migration is intentionally not reversible because old integer IDs are not retained; restore the backup to roll back. The app applies pending migrations at startup.

## Build and test

From the repository root:

```powershell
dotnet build RentCarManagement.sln
dotnet test RentCarManagement.sln
```

The test project uses `WebApplicationFactory` as an in-process HTTP test client and an in-memory database. It checks public vehicle listing, authentication on rental history, frontend delivery, and Swagger UI availability. Build the frontend before running API tests so the host has the generated static assets.

## Main API routes

- `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/forgot-password`, `POST /api/auth/reset-password`
- `GET/PUT /api/profile`
- `GET /api/vehicles`; admin-only `POST/PUT/DELETE /api/vehicles` and `POST /api/vehicles/{guid}/maintenance`
- `POST /api/rentals`, `GET /api/rentals`, `PUT /api/rentals/{guid}/extend`, `POST /api/rentals/{guid}/return`; admin-only `GET /api/rentals/admin` and `POST /api/rentals/admin/{guid}/return`
- `POST /api/damages/rentals/{guid}`, `GET /api/damages`, `GET /api/damages/admin`, `PUT /api/damages/{guid}/assess`, `POST /api/damages/{guid}/pay`
- `POST /api/contact`

Domain error codes are defined in `RentCarManagement.Domain/Errors/DomainError.cs`; API errors return a stable `code` alongside a human-readable message.

## Demo behavior and data note

Damage payments are simulated only. Rental pricing includes the daily rate, a 200 km/day allowance, excess-kilometer charges, and late-return fees. Returning a vehicle over its maintenance threshold marks it for maintenance.

This is an educational/local demo, not a production service. National ID numbers are stored without field-level encryption and browser JWTs use local storage. Do not enter real sensitive data or expose the app/database publicly. Production use requires a security review, HTTPS, managed secrets, protection for personal data and tokens, SMTP, backups, and controlled database deployments.
