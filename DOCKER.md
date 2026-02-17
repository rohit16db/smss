# Docker Compose Guide - School Management System

Complete guide for running the SMS application using Docker Compose with hot reload support for development.

## 📋 Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Services](#services)
- [Development Workflow](#development-workflow)
- [Production Deployment](#production-deployment)
- [Troubleshooting](#troubleshooting)

## 🎯 Overview

The SMS application uses Docker Compose to orchestrate three services:
- **PostgreSQL 15**: Database
- **ASP.NET Core 10 API**: Backend service
- **React 19 + Vite**: Frontend application

All services are configured with hot reload for seamless development experience.

## 📦 Prerequisites

### Required Software

- **Docker Desktop 20.10+** or **Docker Engine + Docker Compose**
- **Windows/Mac/Linux** with at least 8GB RAM and 20GB free disk space

### Verify Installation

```powershell
# Check Docker version
docker --version
# Output: Docker version 28.0.0 or higher

# Check Docker Compose version
docker-compose --version
# Output: Docker Compose version 2.x.x or higher

# Verify Docker is running
docker ps
# Should show no errors
```

## 🚀 Quick Start

### 1. Clone and Navigate

```powershell
git clone <repository-url>
cd SMS
```

### 2. Create Environment File

```powershell
# Copy the example file
Copy-Item .env.example .env

# Edit .env with your preferred editor (optional - defaults work for development)
notepad .env
```

### 3. Build Images

```powershell
# Build all services
docker-compose build

# Or build with no cache (fresh build)
docker-compose build --no-cache
```

**Build Time:** ~5-10 minutes on first build (downloads base images, installs dependencies)

### 4. Start All Services

```powershell
# Start all services in detached mode
docker-compose up -d

# Or start with logs visible
docker-compose up
```

**Startup Time:** ~30-60 seconds

### 5. Verify Services

```powershell
# Check container status
docker-compose ps

# Expected output:
# NAME           STATUS    PORTS
# sms-postgres   Up        0.0.0.0:5432->5432/tcp
# sms-backend    Up        0.0.0.0:5208->8080/tcp
# sms-frontend   Up        0.0.0.0:5173->5173/tcp
```

### 6. Access Applications

- **Frontend:** http://localhost:5173
- **Backend API:** http://localhost:5208
- **Swagger UI:** http://localhost:5208/swagger
- **Database:** localhost:5432

### 7. View Logs

```powershell
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f postgres

# Last 100 lines
docker-compose logs --tail=100 backend
```

### 8. Stop Services

```powershell
# Stop all services (keeps volumes/data)
docker-compose down

# Stop and remove volumes (deletes database data)
docker-compose down -v

# Stop and remove everything including images
docker-compose down -v --rmi all
```

## 🏗 Architecture

### Service Dependency Flow

```
frontend (React)
    ↓ depends_on
backend (ASP.NET Core)
    ↓ depends_on (health check)
postgres (PostgreSQL)
```

### Network Configuration

All services communicate through the `sms-network` bridge network:

- **Internal DNS:** Services can reach each other by name (e.g., `http://backend:8080`)
- **External Access:** Exposed ports allow host machine access
- **Isolation:** Network is isolated from other Docker networks

### Volume Mounts

#### Development (Hot Reload)

```yaml
backend:
  volumes:
    - ./backend/src:/src:cached           # Source code sync
    - backend-nuget:/root/.nuget/packages # Cached packages

frontend:
  volumes:
    - ./frontend:/app:cached              # Source code sync
    - /app/node_modules                   # Preserve node_modules
    - /app/.vite                          # Preserve Vite cache

postgres:
  volumes:
    - postgres-data:/var/lib/postgresql/data  # Data persistence
```

**Note:** `:cached` flag optimizes file sync performance on macOS/Windows

## 🔧 Services

### PostgreSQL Service

**Image:** `postgres:15-alpine`  
**Port:** 5432  
**Container Name:** `sms-postgres`

**Configuration:**
- **User:** sms_user (configurable via `.env`)
- **Password:** dev_password_123 (change in production!)
- **Database:** sms_db
- **Encoding:** UTF8
- **Locale:** en_US.utf8

**Health Check:**
```yaml
test: pg_isready -U sms_user -d sms_db
interval: 10s
timeout: 5s
retries: 5
```

**Development Features:**
- Query logging enabled
- Statement duration logging
- All queries logged to container logs

**Connect from Host:**
```powershell
# Using psql
psql -h localhost -p 5432 -U sms_user -d sms_db

# Using connection string
Host=localhost;Port=5432;Database=sms_db;Username=sms_user;Password=dev_password_123
```

### Backend Service

**Base Image:** `mcr.microsoft.com/dotnet/sdk:10.0`  
**Port:** 5208 → 8080 (container)  
**Container Name:** `sms-backend`

**Build Stages:**
1. **base:** Runtime image (aspnet:10.0)
2. **build:** Compile application
3. **publish:** Create production artifacts
4. **final:** Production image
5. **development:** Development with hot reload

**Development Mode:**
- Uses `dotnet watch run` for hot reload
- Monitors `/src` for file changes
- Automatic recompilation on save
- EF Core tools pre-installed

**Environment Variables:**
```yaml
ASPNETCORE_ENVIRONMENT: Development
ASPNETCORE_URLS: http://+:8080
ConnectionStrings__DefaultConnection: Host=postgres;Port=5432;...
ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT: Debug  # In override file
```

**Health Check:**
```yaml
test: curl -f http://localhost:8080/health
interval: 30s
timeout: 10s
retries: 3
start_period: 40s  # Allows time for .NET startup
```

**Endpoints:**
- `/health` - Health check
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe
- `/swagger` - API documentation

### Frontend Service

**Base Image:** `node:20-alpine`  
**Port:** 5173 → 5173 (container)  
**Container Name:** `sms-frontend`

**Build Stages:**
1. **development:** Vite dev server with HMR
2. **build:** Production build artifacts
3. **production:** Nginx serving static files

**Development Mode:**
- Vite dev server with Hot Module Replacement (HMR)
- Monitors `/app` for file changes
- Instant browser updates on save
- React Fast Refresh enabled

**Environment Variables:**
```yaml
VITE_API_URL: http://localhost:5208
VITE_APP_TITLE: School Management System
NODE_ENV: development  # In override file
```

**Health Check:**
```yaml
test: wget --quiet --tries=1 --spider http://localhost:5173
interval: 30s
timeout: 10s
retries: 3
start_period: 60s  # Allows time for npm install + build
```

## 💻 Development Workflow

### Hot Reload - Backend

1. **Edit any C# file** in `backend/src/`
2. **Save the file**
3. **Watch logs:** `docker-compose logs -f backend`
4. **See output:**
   ```
   watch : File changed: /src/SMS.API/Controllers/HealthController.cs
   watch : Building...
   watch : Succeeded
   ```
5. **Test changes** immediately at http://localhost:5208

**Hot Reload Time:** ~2-5 seconds

### Hot Reload - Frontend

1. **Edit any file** in `frontend/src/`
2. **Save the file**
3. **Browser updates automatically** (no manual refresh needed)
4. **See HMR log** in browser console:
   ```
   [vite] hmr update /src/pages/HomePage.tsx
   ```

**Hot Reload Time:** <1 second (instant)

### Database Migrations

#### Apply Migrations

```powershell
# From host machine
cd backend
dotnet ef database update --project src/SMS.Infrastructure --startup-project src/SMS.API

# Or from backend container
docker-compose exec backend dotnet ef database update --project SMS.Infrastructure --startup-project SMS.API
```

#### Create New Migration

```powershell
# From host machine
cd backend
dotnet ef migrations add MigrationName --project src/SMS.Infrastructure --startup-project src/SMS.API

# Or from backend container
docker-compose exec backend dotnet ef migrations add MigrationName --project SMS.Infrastructure --startup-project SMS.API
```

### Install New Packages

#### Backend - NuGet Package

```powershell
# Stop backend service
docker-compose stop backend

# Add package from host
cd backend/src/SMS.API
dotnet add package PackageName

# Rebuild backend image
docker-compose build backend

# Start backend service
docker-compose up -d backend
```

#### Frontend - NPM Package

```powershell
# Stop frontend service
docker-compose stop frontend

# Add package from host
cd frontend
npm install package-name

# Restart frontend (no rebuild needed - mounts volume)
docker-compose up -d frontend
```

### Debugging

#### Backend Debugging

**Attach debugger to running container:**
1. Stop current backend: `docker-compose stop backend`
2. Run with debug port exposed:
   ```powershell
   docker-compose run --rm --service-ports -e ASPNETCORE_ENVIRONMENT=Development backend
   ```
3. Attach VS Code or Visual Studio debugger to localhost:8080

**View detailed logs:**
```powershell
# Set debug logging
docker-compose exec backend \
  dotnet run --project SMS.API --environment Development -- --Logging:LogLevel:Default=Debug
```

#### Frontend Debugging

**Browser DevTools:**
- Open http://localhost:5173
- Press F12 for DevTools
- Source maps enabled - debug original TypeScript

**Network Inspection:**
- Network tab shows all API calls
- Verify CORS headers
- Check request/response payloads

### Environment Variables

**Change variables in `.env` file, then restart containers:**

```powershell
# Edit .env file
notepad .env

# Restart all services to apply changes
docker-compose down
docker-compose up -d
```

**Important:** Environment variables are read at container startup, not dynamically.

## 🚢 Production Deployment

### Build Production Images

```powershell
# Build production targets
docker-compose -f docker-compose.yml -f docker-compose.prod.yml build

# Or manually specify production target
docker build --target production -t sms-backend:prod ./backend
docker build --target production -t sms-frontend:prod ./frontend
```

### Production Considerations

1. **Use secrets management** (Azure Key Vault, AWS Secrets Manager)
2. **Set secure database password** (minimum 32 characters, complex)
3. **Use HTTPS** with valid SSL certificates
4. **Enable HSTS** and security headers
5. **Set `ASPNETCORE_ENVIRONMENT=Production`**
6. **Disable Swagger** in production
7. **Use read-only volumes** where possible
8. **Implement proper backup strategy**
9. **Set resource limits** (CPU, memory)
10. **Use health checks** for orchestration

### Example Production Override

Create `docker-compose.prod.yml`:

```yaml
version: '3.8'

services:
  postgres:
    restart: always
    environment:
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD_SECRET}  # From secret store
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G

  backend:
    build:
      target: final  # Production stage
    restart: always
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: https://+:443;http://+:80
      ENABLE_SWAGGER: "false"
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 1G

  frontend:
    build:
      target: production  # Nginx stage
    restart: always
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 512M
```

## 🐛 Troubleshooting

### Port Already in Use

**Symptom:** "Error starting userland proxy: listen tcp 0.0.0.0:5432: bind: address already in use"

**Solutions:**
```powershell
# Option 1: Stop conflicting service
# For PostgreSQL on Windows:
Stop-Service postgresql-x64-15

# Option 2: Change port in .env
# Edit .env and change POSTGRES_PORT=5433 (or any free port)
```

### Container Won't Start

**Check logs:**
```powershell
docker-compose logs backend
docker-compose logs frontend
docker-compose logs postgres
```

**Common causes:**
- **Backend:** Database connection failed (wait for PostgreSQL health check)
- **Frontend:** npm install failed (check internet connection)
- **PostgreSQL:** Data directory permission error (delete volume and recreate)

### Hot Reload Not Working

**Backend:**
```powershell
# Verify volume mount
docker-compose exec backend ls -la /src

# Check dotnet watch process
docker-compose exec backend ps aux | grep "dotnet watch"

# Restart with fresh build
docker-compose stop backend
docker-compose build --no-cache backend
docker-compose up -d backend
```

**Frontend:**
```powershell
# Verify volume mount
docker-compose exec frontend ls -la /app

# Check Vite process
docker-compose logs frontend | grep "ready in"

# Restart frontend
docker-compose restart frontend
```

### Database Connection Error

**Error:** "Npgsql.NpgsqlException: could not translate host name 'postgres' to address"

**Solution:** Backend started before PostgreSQL was ready
```powershell
# Wait for health check
docker-compose up -d postgres
docker-compose logs -f postgres
# Wait for "database system is ready to accept connections"

# Then start backend
docker-compose up -d backend
```

### Out of Disk Space

**Check Docker disk usage:**
```powershell
docker system df
```

**Clean up unused resources:**
```powershell
# Remove unused containers, networks, images
docker system prune

# Remove unused volumes (WARNING: deletes data!)
docker volume prune

# Nuclear option - remove everything
docker system prune -a --volumes
```

### Performance Issues

**Slow file sync on Windows/Mac:**
```yaml
# Use :cached flag in docker-compose.yml (already configured)
volumes:
  - ./backend/src:/src:cached
```

**High CPU usage:**
```powershell
# Limit resources in docker-compose.yml
deploy:
  resources:
    limits:
      cpus: '2'
      memory: 2G
```

### Network Connectivity Issues

**Services can't reach each other:**
```powershell
# Verify network exists
docker network ls | grep sms-network

# Recreate network
docker-compose down
docker network rm sms-network
docker-compose up -d
```

**Can't access from host:**
```powershell
# Verify ports are published
docker-compose ps

# Check firewall (Windows)
New-NetFirewallRule -DisplayName "Docker SMS" -Direction Inbound -LocalPort 5173,5208,5432 -Protocol TCP -Action Allow
```

## 📚 Useful Commands

### Container Management

```powershell
# Start services
docker-compose up -d

# Stop services
docker-compose down

# Restart specific service
docker-compose restart backend

# View logs
docker-compose logs -f --tail=100

# Execute command in container
docker-compose exec backend bash
docker-compose exec frontend sh
docker-compose exec postgres psql -U sms_user -d sms_db

# View resource usage
docker stats

# Inspect service
docker-compose exec backend env
```

### Image Management

```powershell
# List images
docker images | grep sms

# Remove image
docker rmi sms-backend
docker rmi sms-frontend

# Rebuild specific service
docker-compose build backend

# Rebuild without cache
docker-compose build --no-cache --pull
```

### Volume Management

```powershell
# List volumes
docker volume ls | grep sms

# Inspect volume
docker volume inspect sms-postgres-data

# Remove volume (WARNING: deletes data!)
docker volume rm sms-postgres-data

# Backup database volume
docker run --rm -v sms-postgres-data:/data -v ${PWD}:/backup alpine tar czf /backup/db-backup.tar.gz -C /data .
```

### Health Checks

```powershell
# Check health status
docker-compose ps
docker inspect --format='{{.State.Health.Status}}' sms-backend

# Manual health check
curl http://localhost:5208/health
curl http://localhost:5173
docker-compose exec postgres pg_isready -U sms_user
```

## 📝 Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [ASP.NET Core Docker Guide](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)
- [Vite Docker Guide](https://vite.dev/guide/docker.html)
- [PostgreSQL Docker Guide](https://hub.docker.com/_/postgres)

---

**Need Help?** Check the [main project README](README.md) or [GitHub Issues](issues).
