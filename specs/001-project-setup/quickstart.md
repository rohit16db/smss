# Quickstart Guide: Initial Project Setup

**Last Updated**: 2026-01-12  
**Estimated Setup Time**: 15 minutes  
**Prerequisites**: Basic command line knowledge

## Overview

This guide walks you through setting up the School Management Software development environment from scratch. By the end, you'll have:
- ✅ Backend API running on http://localhost:5000
- ✅ Frontend React app running on http://localhost:5173
- ✅ PostgreSQL database running in Docker
- ✅ All services communicating with each other

## Prerequisites

### Required Software

| Software | Version | Download Link | Verification Command |
|----------|---------|---------------|---------------------|
| .NET SDK | 8.0 or later | https://dotnet.microsoft.com/download | `dotnet --version` |
| Node.js | 20 LTS or later | https://nodejs.org/ | `node --version` |
| Docker Desktop | Latest | https://www.docker.com/products/docker-desktop | `docker --version` |
| Git | Latest | https://git-scm.com/ | `git --version` |

### Recommended Software (Optional)
- **IDE**: Visual Studio 2022, VS Code, or JetBrains Rider
- **Database Client**: pgAdmin, DBeaver, or Azure Data Studio
- **API Client**: Postman, Insomnia, or VS Code REST Client extension

### System Requirements
- **OS**: Windows 10/11, macOS 10.15+, or Linux
- **RAM**: 8 GB minimum (16 GB recommended)
- **Disk Space**: 10 GB free space
- **Internet**: Required for initial package downloads

---

## Quick Start (Docker Compose - Recommended)

**Use this if you want to get running quickly without individual setup steps.**

### Step 1: Clone Repository
```bash
git clone <repository-url>
cd SMS
```

### Step 2: Configure Environment
```bash
# Copy environment template
cp .env.example .env

# Edit .env file and set secure passwords
# On Windows PowerShell:
notepad .env

# On macOS/Linux:
nano .env
```

**Required changes in `.env`**:
```bash
DATABASE_PASSWORD=<choose-secure-password>
JWT_SECRET=<generate-min-32-char-secret>
```

**Generate secure JWT secret** (PowerShell):
```powershell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```

### Step 3: Start All Services
```bash
# Start all services (PostgreSQL, Backend, Frontend)
docker-compose up -d

# View logs
docker-compose logs -f
```

### Step 4: Verify Setup
```bash
# Check all containers are running
docker-compose ps

# Test backend health endpoint
curl http://localhost:5000/health

# Open frontend in browser
start http://localhost:5173  # Windows
open http://localhost:5173   # macOS
```

**Expected Results**:
- Backend health check returns: `{"status":"Healthy",...}`
- Frontend shows: "Welcome to SMS Admin"
- All 3 containers show "Up" status

### Step 5: Stop Services
```bash
# Stop all services
docker-compose down

# Stop and remove volumes (fresh start)
docker-compose down -v
```

---

## Manual Setup (Alternative)

**Use this if you want more control or can't use Docker.**

### Part 1: Database Setup

#### Option A: Docker (Recommended)
```bash
# Start PostgreSQL container
docker run -d \
  --name sms-postgres \
  -e POSTGRES_USER=sms_user \
  -e POSTGRES_PASSWORD=your-secure-password \
  -e POSTGRES_DB=sms_db \
  -p 5432:5432 \
  -v sms-postgres-data:/var/lib/postgresql/data \
  postgres:15-alpine
```

#### Option B: Local PostgreSQL Installation
1. Download and install PostgreSQL 15 from https://www.postgresql.org/download/
2. During installation, remember the postgres user password
3. Create database and user:
```sql
-- Connect to PostgreSQL (use psql or pgAdmin)
CREATE DATABASE sms_db;
CREATE USER sms_user WITH ENCRYPTED PASSWORD 'your-secure-password';
GRANT ALL PRIVILEGES ON DATABASE sms_db TO sms_user;
```

#### Verify Database Connection
```bash
# Using psql
psql -h localhost -U sms_user -d sms_db

# Should show: sms_db=>
# Type \q to exit
```

---

### Part 2: Backend Setup

#### Step 1: Navigate to Backend Directory
```bash
cd backend
```

#### Step 2: Restore Dependencies
```bash
dotnet restore
```

#### Step 3: Configure Connection String

**Option A: User Secrets (Recommended for Development)**
```bash
cd src/SMS.API

# Initialize user secrets
dotnet user-secrets init

# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=sms_db;Username=sms_user;Password=your-secure-password"

cd ../..
```

**Option B: appsettings.Development.json (Less Secure)**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=sms_db;Username=sms_user;Password=your-secure-password"
  }
}
```

#### Step 4: Run Database Migrations
```bash
# Create initial migration
dotnet ef migrations add InitialCreate \
  --project src/SMS.Infrastructure \
  --startup-project src/SMS.API

# Apply migration to database
dotnet ef database update \
  --project src/SMS.Infrastructure \
  --startup-project src/SMS.API
```

#### Step 5: Run Backend API
```bash
# Development mode with hot reload
dotnet watch run --project src/SMS.API

# Or without hot reload
dotnet run --project src/SMS.API
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

#### Step 6: Test Backend
Open browser: http://localhost:5000/swagger

Try health endpoint: http://localhost:5000/health

**Expected Response**:
```json
{
  "status": "Healthy",
  "timestamp": "2026-01-12T10:30:00Z",
  "checks": {
    "database": "Healthy",
    "api": "Healthy"
  },
  "duration": "00:00:00.0234567"
}
```

---

### Part 3: Frontend Setup

#### Step 1: Navigate to Frontend Directory
```bash
# Open new terminal window
cd frontend
```

#### Step 2: Install Dependencies
```bash
npm install
```

**Expected Output**: Should complete without errors in 1-2 minutes

#### Step 3: Configure API URL
Create `.env.development` file:
```bash
# On Windows:
echo VITE_API_BASE_URL=http://localhost:5000 > .env.development

# On macOS/Linux:
echo "VITE_API_BASE_URL=http://localhost:5000" > .env.development
```

#### Step 4: Start Development Server
```bash
npm run dev
```

**Expected Output**:
```
  VITE v5.x.x  ready in XXX ms

  ➜  Local:   http://localhost:5173/
  ➜  Network: use --host to expose
```

#### Step 5: Test Frontend
1. Open browser: http://localhost:5173
2. Should see: "Welcome to SMS Admin"
3. Check browser console for any errors (should be none)

---

## Verification Checklist

After setup, verify everything works:

- [ ] **Database**: PostgreSQL container/service running on port 5432
- [ ] **Backend**: API responding at http://localhost:5000/health
- [ ] **Swagger**: API documentation at http://localhost:5000/swagger
- [ ] **Frontend**: React app loaded at http://localhost:5173
- [ ] **Integration**: Frontend can call backend health endpoint
- [ ] **Hot Reload**: Change a backend/frontend file and see automatic refresh
- [ ] **No Errors**: No error messages in any terminal or browser console

---

## Troubleshooting

### Backend Issues

#### Issue: "Cannot connect to database"
**Solutions**:
1. Verify PostgreSQL is running: `docker ps` or check services
2. Check connection string has correct password
3. Test database connection: `psql -h localhost -U sms_user -d sms_db`
4. Check firewall isn't blocking port 5432

#### Issue: "Port 5000 already in use"
**Solutions**:
1. Find process using port: `netstat -ano | findstr :5000` (Windows)
2. Kill process or change port in launchSettings.json
3. Use different port: `dotnet run --urls "http://localhost:5050"`

#### Issue: "EF Core migrations fail"
**Solutions**:
1. Verify EF Core tools installed: `dotnet tool install --global dotnet-ef`
2. Ensure correct paths in migration commands
3. Check database user has schema creation permissions
4. Delete Migrations folder and try again

### Frontend Issues

#### Issue: "npm install fails"
**Solutions**:
1. Verify Node.js version: `node --version` (should be 20+)
2. Clear npm cache: `npm cache clean --force`
3. Delete node_modules and package-lock.json, try again
4. Check internet connection for package downloads

#### Issue: "CORS errors in browser"
**Solutions**:
1. Verify backend CORS policy includes http://localhost:5173
2. Check backend is running before frontend
3. Clear browser cache and reload
4. Check .env.development has correct VITE_API_BASE_URL

#### Issue: "Frontend shows blank page"
**Solutions**:
1. Check browser console for JavaScript errors
2. Verify all npm dependencies installed successfully
3. Try different browser (Chrome, Firefox, Edge)
4. Check Vite config and index.html are correct

### Docker Issues

#### Issue: "Docker daemon not running"
**Solutions**:
1. Start Docker Desktop application
2. Wait for Docker to fully start (icon in system tray)
3. Restart Docker Desktop if stuck

#### Issue: "docker-compose up fails"
**Solutions**:
1. Check .env file exists and has all required variables
2. Verify no port conflicts: `docker ps` and check ports 5000, 5173, 5432
3. Try `docker-compose down -v` and start fresh
4. Check Docker has enough resources (Settings → Resources)

#### Issue: "Containers start but frontend can't reach backend"
**Solutions**:
1. Verify all containers are on same network: `docker network inspect sms_network`
2. Check backend is actually listening: `docker logs sms-backend`
3. Verify environment variables are set correctly
4. Try accessing backend from host: http://localhost:5000/health

---

## Development Workflow

### Daily Workflow
```bash
# 1. Start services
docker-compose up -d

# 2. View logs (optional)
docker-compose logs -f backend frontend

# 3. Make code changes (hot reload enabled)

# 4. Run migrations if database changes
docker-compose exec backend dotnet ef database update

# 5. Stop services when done
docker-compose down
```

### After Package Changes
```bash
# Backend: Restore NuGet packages
cd backend && dotnet restore

# Frontend: Install npm packages
cd frontend && npm install

# Rebuild Docker images
docker-compose up -d --build
```

### Database Migrations
```bash
# Create migration
cd backend
dotnet ef migrations add MigrationName \
  --project src/SMS.Infrastructure \
  --startup-project src/SMS.API

# Apply migration
dotnet ef database update \
  --project src/SMS.Infrastructure \
  --startup-project src/SMS.API

# Or via Docker
docker-compose exec backend dotnet ef database update
```

### Viewing Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f postgres

# Last 100 lines
docker-compose logs --tail=100 backend
```

### Accessing Containers
```bash
# Backend shell
docker-compose exec backend /bin/bash

# PostgreSQL CLI
docker-compose exec postgres psql -U sms_user -d sms_db

# View environment variables
docker-compose exec backend env
```

---

## Next Steps

After successful setup:

1. **Explore the API**: Open http://localhost:5000/swagger and test the health endpoint
2. **Review Architecture**: Read `/specs/001-project-setup/data-model.md` to understand the structure
3. **Start Developing**: Proceed to implement authentication feature (next feature)
4. **Run Tests**: Execute `dotnet test` in backend directory (when tests are added)
5. **Read Constitution**: Familiarize yourself with `.specify/memory/constitution.md` for development principles

---

## Additional Resources

### Documentation
- **Backend README**: `backend/README.md`
- **Frontend README**: `frontend/README.md`
- **Data Model**: `specs/001-project-setup/data-model.md`
- **API Contract**: `specs/001-project-setup/contracts/health-api.yaml`
- **Research**: `specs/001-project-setup/research.md`

### External Links
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core)
- [React Documentation](https://react.dev)
- [Material UI Documentation](https://mui.com)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core)
- [PostgreSQL Documentation](https://www.postgresql.org/docs)
- [Docker Documentation](https://docs.docker.com)

### Support
- Check `CONTRIBUTING.md` for contribution guidelines
- Open GitHub issue for bugs or feature requests
- Consult `specs/` directory for feature specifications

---

**Success!** Your School Management Software development environment is ready! 🎉

**Estimated time to first API call**: Under 15 minutes if following this guide

**Remember**: Keep your `.env` file secure and never commit it to version control!
