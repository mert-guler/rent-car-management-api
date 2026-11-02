# Rent Car Management API

A .NET 10 car-rental API and web interface backed by SQL Server. It includes JWT authentication, vehicle and rental management, profile updates, support requests, password reset, and Serilog logging.

## Run locally (Windows / SQL Server Express)

1. Install .NET 10 SDK and SQL Server. The default connection string targets `localhost\SQLEXPRESS` with Windows authentication; adjust `ConnectionStrings:DefaultConnection` in `appsettings.json` for your SQL Server instance if needed.
2. Set a local JWT signing key using .NET User Secrets (never commit this key):

   ```powershell
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:Key" "<random secret of at least 32 bytes>"
   ```

   Generate a strong value, for example with a password manager. In production, provide the configuration as the `Jwt__Key` environment variable or through a secret manager. The application refuses to start if this secret is missing or shorter than 32 bytes.
3. Apply migrations and start the app:

   ```powershell
   dotnet ef database update
   dotnet run
   ```

The web app is served at `http://localhost:5165` when using the included launch profile. On startup it applies pending migrations and seeds three sample cars if the vehicle table is empty. OpenAPI is available at `/openapi/v1.json` in Development.

## Make your account an admin

Register through the web app first. In SQL Server Management Studio (SSMS), connect to `localhost\SQLEXPRESS` using Windows Authentication, open a New Query window, and run the following after replacing the email with the exact address you registered:

```sql
USE RentCarManagementDb;
GO

SELECT Id, Email, Role
FROM dbo.Users
WHERE Email = 'your-registered-email@example.com';
GO

UPDATE dbo.Users
SET Role = 'Admin'
WHERE Email = 'your-registered-email@example.com';
GO

SELECT Id, Email, Role
FROM dbo.Users
WHERE Email = 'your-registered-email@example.com';
GO
```

Confirm that exactly your account shows `Admin`. Then sign out and sign in again: the role is embedded in the JWT at login, so an already-issued token keeps its previous role. Admin-only vehicle management endpoints and the admin damage-review panel will then be available. Keep admin access limited to trusted accounts.

## Main API routes

- `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/forgot-password`, `POST /api/auth/reset-password`
- `GET/PUT /api/profile`
- `GET /api/vehicles`; admin-only `POST/PUT/DELETE /api/vehicles` and `POST /api/vehicles/{id}/maintenance`
- `POST /api/rentals`, `GET /api/rentals`, `PUT /api/rentals/{id}/extend`, `POST /api/rentals/{id}/return`; admin-only `GET /api/rentals/admin` and `POST /api/rentals/admin/{id}/return`
- `POST /api/damages/rentals/{rentalId}`, `GET /api/damages`, `GET /api/damages/admin`, `PUT /api/damages/{id}/assess`, `POST /api/damages/{id}/pay`
- `POST /api/contact`

## Demo behavior and data note

Damage payments are simulated only: the customer reports a description, an admin reviews it and sets an amount, and the customer can mark it as paid in the demo. No payment provider is connected and no real money is charged. Rental pricing includes the daily rate, 200 km/day allowance, extra-kilometer charges, and late-return fees. Returning a vehicle beyond its maintenance-mileage threshold automatically marks it for maintenance.

This is an educational/local demo, not a production service. National ID numbers and other profile data are stored in the local database; do not use real sensitive personal information or expose the app/database publicly. Configure SMTP and `App__PublicBaseUrl` before deploying password reset outside Development. Serilog writes to the console and the ignored `logs/` folder.
