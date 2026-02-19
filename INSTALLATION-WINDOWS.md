# Windows Local IIS Installation Guide

This guide explains how to run the SMS backend and frontend on a new Windows machine using IIS (client-style install, no Docker).

## Prerequisites

Install these tools first:

- Git (latest)
  - Verify: `git --version`
  - Download: https://git-scm.com/
- .NET SDK 10.0+ (used only to publish builds)
  - Verify: `dotnet --version`
  - Download: https://dotnet.microsoft.com/download
- ASP.NET Core Hosting Bundle 10.x (required for IIS to run the API)
  - Download: https://dotnet.microsoft.com/download/dotnet/10.0
  - Install and then restart IIS: `iisreset`
- Node.js 20 LTS+
  - Verify: `node --version` and `npm --version`
  - Download: https://nodejs.org/
- PostgreSQL 15+ (only needed for Manual setup)
  - Verify: `psql --version`
  - Download: https://www.postgresql.org/download/windows/
- EF Core CLI (only needed for Manual setup)
  - Install: `dotnet tool install --global dotnet-ef`
  - Verify: `dotnet ef --version`
- IIS with required features
  - Windows Features: Web Server (IIS), Static Content, ASP.NET Core Module, URL Rewrite

## Setup Overview

You will publish the backend API and build the frontend, then host both with IIS. The frontend calls the API at `http://api.schoolapi.local`.

If you plan to send the published artifacts to a client machine, create the packages on your build machine and copy them to the client before IIS setup.

## Package Checklist (What to Send)

Send these folders to the client machine:

- `publish\api` (backend publish output)
- `frontend\dist` (frontend build output)

Optional but recommended:

- `INSTALLATION-WINDOWS.md` (this guide)
- `backend\src\SMS.API\appsettings.Production.json` (if you provide environment defaults)

1. Clone the repository

```powershell
git clone <repository-url>
cd SMS
```

2. Configure backend secrets

```powershell
cd backend

dotnet user-secrets init --project src/SMS.API

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=school_management_db;Username=postgres;Password=YOUR_PASSWORD" --project src/SMS.API

dotnet user-secrets set "JwtSettings:SecretKey" "your-super-secret-jwt-key-min-32-chars-long" --project src/SMS.API
```

3. Start PostgreSQL

- If installed locally, create a database named `school_management_db`.
- Ensure PostgreSQL is running on `localhost:5432`.

4. Apply migrations

```powershell
cd backend

dotnet ef database update --project src/SMS.Infrastructure --startup-project src/SMS.API
```

5. Publish backend API

```powershell
cd backend

dotnet publish src/SMS.API -c Release -o ..\publish\api
```

Copy the output folder `publish\api` to the client machine, for example:

- `C:\SMS\publish\api`

6. Configure frontend env

```powershell
cd frontend

Copy-Item .env.development.example .env.development
```

Edit `.env.development` and set:

- `VITE_API_URL=http://api.schoolapi.local`

7. Build frontend

```powershell
cd frontend

npm install
npm run build
```

The build output is in `frontend\dist`.

Copy the build output to the client machine, for example:

- `C:\SMS\frontend\dist`

## IIS Hosting - Backend API

1. Create an app pool
  - Name: `sms-api`
  - .NET CLR Version: No Managed Code
  - Start mode: AlwaysRunning (optional)

2. Create a site
  - Site name: `sms-api`
  - Physical path: `C:\SMS\publish\api`
  - Binding: `http` on port `5208` or a host name like `api.schoolapi.local`
  - Assign the `sms-api` app pool

3. Set environment variables for the app pool
  - `ASPNETCORE_ENVIRONMENT=Production`
  - `ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=school_management_db;Username=postgres;Password=YOUR_PASSWORD`
  - `JwtSettings__SecretKey=your-super-secret-jwt-key-min-32-chars-long`

4. Ensure database is reachable
  - PostgreSQL is running locally on `localhost:5432`
  - Run migrations if needed (see Step 4)

5. Verify API
  - http://api.schoolapi.local/health
  - http://api.schoolapi.local/swagger

## IIS Hosting - Frontend

1. Create a site
  - Site name: `sms-frontend`
  - Physical path: `C:\SMS\frontend\dist`
  - Binding: `http` on port `80` or a host name like `schoolapp.local`

2. Add SPA rewrite (so deep routes load)
  - Create a `web.config` in `frontend\dist` with this content:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
   <rewrite>
    <rules>
      <rule name="SpaFallback" stopProcessing="true">
       <match url=".*" />
       <conditions logicalGrouping="MatchAll">
        <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
        <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
       </conditions>
       <action type="Rewrite" url="/index.html" />
      </rule>
    </rules>
   </rewrite>
  </system.webServer>
</configuration>
```

3. Verify frontend
  - http://schoolapp.local

## Common Ports

- Frontend (IIS): 80 or 443
- Backend API (IIS): 5208 or host-based binding
- PostgreSQL: 5432

## Troubleshooting

- Backend cannot connect to database:
  - Verify PostgreSQL is running and connection string is correct.
  - Check port 5432 is not blocked.
- Frontend shows API errors:
  - Confirm backend health endpoint works: http://localhost:5208/health
  - Confirm `VITE_API_URL` is set correctly.
- IIS API shows 502.5:
  - Ensure ASP.NET Core Hosting Bundle is installed and IIS restarted.
  - Check Event Viewer for ASP.NET Core Module errors.

## Related Docs

- Docker guide: DOCKER.md
- Backend guide: backend/README.md
- Frontend guide: frontend/README.md
- Production deployment: DEPLOYMENT.md
