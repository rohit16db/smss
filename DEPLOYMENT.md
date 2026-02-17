# Production Deployment Guide

## Overview

This guide covers deploying School Management Software (SMS) to production environments using Docker and various hosting providers.

## Prerequisites

### Required
- Docker Engine 20.10+
- Docker Compose 1.29+
- PostgreSQL 13+ (or managed PostgreSQL service)
- Domain name with DNS control
- SSL certificate (Let's Encrypt recommended)

### Optional (for cloud deployment)
- AWS account (for Elastic Container Service, RDS, ALB)
- DigitalOcean account (for App Platform, Managed Database)
- Render account (for integrated hosting)

## Local Production Testing

### 1. Build Production Images

```bash
# Build backend image
cd backend
docker build -t sms-backend:1.0.0 -f Dockerfile .

# Build frontend image
cd frontend
docker build -t sms-frontend:1.0.0 -f Dockerfile .
```

### 2. Environment Configuration

Create `.env.production`:

```bash
# Database
DB_HOST=postgres
DB_PORT=5432
DB_NAME=sms_db
DB_USER=sms_user
DB_PASSWORD=your_secure_password_here

# API
API_URL=https://api.yourdomain.com
API_PORT=5000

# Frontend
FRONTEND_URL=https://yourdomain.com
FRONTEND_PORT=3000

# JWT
JWT_SECRET=your_super_secret_jwt_key_min_32_chars

# CORS Origins
CORS_ORIGINS=https://yourdomain.com,https://www.yourdomain.com
```

### 3. Run Production Stack Locally

```bash
docker-compose -f docker-compose.yml -f docker-compose.production.yml up -d
```

### 4. Verify Services

```bash
# Check all services running
docker-compose ps

# Check backend health
curl http://localhost:5000/api/v1/health

# Check frontend
curl http://localhost:3000
```

## Cloud Deployment Options

### Option A: DigitalOcean App Platform (Recommended for Beginners)

#### Setup

1. **Create App**
   - Connect GitHub repository
   - Configure buildpack deployment

2. **Environment Variables**
   ```
   DB_HOST=your-managed-db
   DB_PASSWORD=secure_password
   JWT_SECRET=your_secret_key
   ```

3. **Database**
   - Create Managed PostgreSQL cluster
   - Configure database user and password
   - Enable backups

#### Deployment

```bash
# Push to GitHub
git push origin main

# App Platform auto-deploys on push
# Monitor deployments at https://cloud.digitalocean.com
```

#### Cost Estimation
- App Platform: $6-12/month
- Managed Database: $15-30/month
- Total: ~$21-42/month

---

### Option B: AWS ECS + RDS (Best for Scale)

#### Setup

1. **RDS PostgreSQL**
   ```bash
   # Create RDS instance via AWS Console
   # Engine: PostgreSQL 15
   # Storage: 20GB gp3
   # Backup retention: 30 days
   ```

2. **ECR (Elastic Container Registry)**
   ```bash
   # Create repository
   aws ecr create-repository --repository-name sms-backend
   aws ecr create-repository --repository-name sms-frontend

   # Build and push
   docker build -t sms-backend:1.0.0 .
   aws ecr get-login-password | docker login --username AWS --password-stdin <account>.dkr.ecr.<region>.amazonaws.com
   docker tag sms-backend:1.0.0 <account>.dkr.ecr.<region>.amazonaws.com/sms-backend:1.0.0
   docker push <account>.dkr.ecr.<region>.amazonaws.com/sms-backend:1.0.0
   ```

3. **ECS Cluster & Services**
   - Create ECS cluster
   - Create task definitions for backend and frontend
   - Create services pointing to ALB

4. **Load Balancer & Route 53**
   - Create Application Load Balancer
   - Configure health checks
   - Set up Route 53 DNS

#### Cost Estimation
- ECS tasks: $5-15/month
- RDS: $20-50/month
- Load Balancer: $16/month
- Total: ~$41-81/month

---

### Option C: Render (Simplest Deployment)

#### Setup

1. **Connect GitHub**
   - Authorization at https://dashboard.render.com

2. **Create PostgreSQL Service**
   - New → PostgreSQL
   - Keep default settings
   - Note connection string

3. **Create Web Services**
   ```
   Service Type: Web Service
   GitHub Repo: your-repo
   Build Command: dotnet build -c Release
   Start Command: dotnet SMS.API.dll
   Environment Variables:
     - ConnectionStrings__DefaultConnection: <PostgreSQL connection string>
     - JWT_SECRET: your_secret_key
   ```

#### Deployment
- Push to GitHub
- Render auto-deploys

#### Cost Estimation
- PostgreSQL: $7/month
- Backend Web Service: $7/month
- Frontend Web Service: $7/month
- Total: ~$21/month

---

## SSL Certificate Setup

### Using Let's Encrypt with Certbot

```bash
# Install certbot
sudo apt-get install certbot python3-certbot-nginx

# Get certificate
sudo certbot certonly --standalone -d yourdomain.com -d www.yourdomain.com

# Certificate locations
# Private key: /etc/letsencrypt/live/yourdomain.com/privkey.pem
# Certificate: /etc/letsencrypt/live/yourdomain.com/fullchain.pem

# Auto-renewal
sudo certbot renew --dry-run
```

## Database Backup Strategy

### Automated Backups

```bash
#!/bin/bash
# backup.sh - Daily database backup

BACKUP_DIR="/backups/sms"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/sms_db_$DATE.sql"

# Create backup
pg_dump -h $DB_HOST -U $DB_USER -d sms_db > "$BACKUP_FILE"

# Compress
gzip "$BACKUP_FILE"

# Keep only 30 days of backups
find $BACKUP_DIR -name "*.sql.gz" -mtime +30 -delete

# Upload to S3 (optional)
aws s3 cp "$BACKUP_FILE.gz" s3://your-backup-bucket/
```

Schedule with crontab:
```bash
# Daily backup at 2 AM
0 2 * * * /backup.sh
```

## Monitoring & Logging

### Application Logs

```bash
# Docker logs
docker-compose logs -f backend

# Persist logs
docker logs backend --since 24h > /var/log/sms/backend.log

# Rotate logs
sudo nano /etc/logrotate.d/sms
```

### Health Monitoring

```bash
# Setup monitoring alert
curl -f https://api.yourdomain.com/api/v1/health || alert "SMS API down"

# Add to crontab
*/5 * * * * curl -f https://api.yourdomain.com/api/v1/health || /send-alert.sh
```

### Application Insights (Optional)

Add to `appsettings.Production.json`:

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here"
  }
}
```

## Performance Optimization

### Database Tuning

```sql
-- Analyze query performance
EXPLAIN ANALYZE SELECT * FROM students WHERE status = 'Active';

-- Create indexes for common queries
CREATE INDEX idx_students_status ON students(status);
CREATE INDEX idx_fee_payments_teacher ON fee_payments(teacher_id);
CREATE INDEX idx_salaries_period ON salary_payments(period_start_date, period_end_date);

-- Update statistics
VACUUM ANALYZE;
```

### Application Caching

Add Redis to `docker-compose.yml`:

```yaml
redis:
  image: redis:7-alpine
  command: redis-server --appendonly yes
  volumes:
    - redis_data:/data
```

Configure in `appsettings.Production.json`:

```json
{
  "Redis": {
    "Connection": "redis:6379"
  }
}
```

### Frontend Optimization

```bash
# Build optimization
npm run build -- --analyze

# Gzip compression in nginx
gzip on;
gzip_types text/css application/javascript;
gzip_min_length 1000;
```

## Security Checklist

- [ ] Change all default passwords
- [ ] Enable HTTPS/SSL
- [ ] Configure firewall rules
- [ ] Set up regular backups
- [ ] Enable database encryption
- [ ] Use strong JWT secret (min 32 chars, random)
- [ ] Keep dependencies updated
- [ ] Enable API rate limiting
- [ ] Monitor error logs
- [ ] Configure CORS for specific domains only
- [ ] Setup DDoS protection (Cloudflare optional)

## Troubleshooting

### Application Won't Start

```bash
# Check logs
docker-compose logs backend

# Verify database connection
docker-compose exec backend dotnet ef database update --dry-run

# Check environment variables
docker-compose config | grep JWT_SECRET
```

### High Database CPU Usage

```sql
-- Find slow queries
SELECT query, mean_time, calls FROM pg_stat_statements 
ORDER BY mean_time DESC LIMIT 10;

-- Analyze execution plans
EXPLAIN ANALYZE SELECT ...;
```

### Out of Memory

```bash
# Check resource limits
docker stats

# Increase memory limit in docker-compose.yml
services:
  backend:
    mem_limit: 1g
```

## Support & Updates

### Check for Updates

```bash
# Backend dependencies
dotnet package list | grep outdated

# Frontend dependencies
npm outdated
```

### Upgrade Process

```bash
# Plan upgrade in staging first
1. Test in staging environment
2. Create database backup
3. Update docker images
4. Run migrations: dotnet ef database update
5. Verify health checks
6. Monitor error logs
```

---

**Last Updated**: January 13, 2026
**Supported Version**: 1.0.0+
