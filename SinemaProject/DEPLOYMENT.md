# 🚀 Deployment Guide

This guide covers deploying the Cinema Management System to production environments.

## Table of Contents
- [Pre-Deployment Checklist](#pre-deployment-checklist)
- [Backend Deployment](#backend-deployment)
- [Frontend Deployment](#frontend-deployment)
- [Database Migration](#database-migration)
- [Security Configuration](#security-configuration)
- [Monitoring & Logging](#monitoring--logging)

---

## Pre-Deployment Checklist

### Environment Setup
- [ ] Production server ready (Windows Server or Linux)
- [ ] SQL Server installed and configured
- [ ] SSL/TLS certificates obtained
- [ ] Domain names configured (DNS)
- [ ] Firewall rules configured

### Code Preparation
- [ ] All tests passing
- [ ] Code reviewed and approved
- [ ] Environment-specific configurations prepared
- [ ] Secrets and keys generated
- [ ] Database backup created

---

## Backend Deployment

### 1. Build for Production

```bash
cd backend
dotnet publish -c Release -o ./publish
```

### 2. Configure Production Settings

Create `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=production-server;Database=SinemaDb_Prod;User Id=sa;Password=StrongPassword123!;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Secret": "PRODUCTION-SECRET-KEY-AT-LEAST-64-CHARACTERS-LONG-AND-RANDOM",
    "Issuer": "cinema.yourdomain.com",
    "Audience": "cinema.yourdomain.com",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "cinema.yourdomain.com",
  "Cors": {
    "AllowedOrigins": ["https://cinema.yourdomain.com"]
  }
}
```

### 3. Deploy to IIS (Windows)

**Install IIS Components:**
```powershell
# Run as Administrator
Install-WindowsFeature -name Web-Server -IncludeManagementTools
Install-WindowsFeature Web-Asp-Net45
```

**Install .NET Hosting Bundle:**
- Download from: https://dotnet.microsoft.com/download/dotnet/9.0
- Install: `dotnet-hosting-9.0.x-win.exe`
- Restart IIS: `net stop was /y` then `net start w3svc`

**Configure IIS:**
1. Open IIS Manager
2. Create new Application Pool:
   - Name: `SinemaAppPool`
   - .NET CLR Version: `No Managed Code`
   - Managed Pipeline Mode: `Integrated`
3. Create new Website:
   - Site name: `SinemaAPI`
   - Application Pool: `SinemaAppPool`
   - Physical path: `C:\inetpub\wwwroot\SinemaAPI` (copy publish folder here)
   - Binding: `https`, Port: `443`, Host name: `api.cinema.yourdomain.com`
4. Configure SSL certificate

**Set Permissions:**
```powershell
icacls "C:\inetpub\wwwroot\SinemaAPI" /grant "IIS AppPool\SinemaAppPool:(OI)(CI)F" /T
```

### 4. Deploy to Linux (Ubuntu/Nginx)

**Install .NET Runtime:**
```bash
wget https://dot.net/v1/dotnet-install.sh
sudo chmod +x dotnet-install.sh
sudo ./dotnet-install.sh --runtime aspnetcore --channel 9.0
```

**Create Service:**
```bash
sudo nano /etc/systemd/system/sinema-api.service
```

```ini
[Unit]
Description=Sinema API

[Service]
WorkingDirectory=/var/www/sinema-api
ExecStart=/usr/bin/dotnet /var/www/sinema-api/Sinema.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=sinema-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

**Configure Nginx:**
```bash
sudo nano /etc/nginx/sites-available/sinema-api
```

```nginx
server {
    listen 80;
    server_name api.cinema.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.cinema.yourdomain.com;

    ssl_certificate /etc/ssl/certs/cinema.crt;
    ssl_certificate_key /etc/ssl/private/cinema.key;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

**Start Service:**
```bash
sudo systemctl enable sinema-api
sudo systemctl start sinema-api
sudo systemctl status sinema-api

sudo ln -s /etc/nginx/sites-available/sinema-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## Frontend Deployment

### 1. Configure Environment

Create `frontend/.env.production`:

```env
VITE_API_URL=https://api.cinema.yourdomain.com/api/v1
VITE_APP_NAME=Cinema Management System
VITE_APP_ENV=production
```

### 2. Build for Production

```bash
cd frontend
npm install
npm run build
```

Output will be in `frontend/dist/`

### 3. Deploy to IIS (Windows)

1. Copy `dist` folder contents to `C:\inetpub\wwwroot\SinemaWeb`
2. Create new Website in IIS:
   - Site name: `SinemaWeb`
   - Physical path: `C:\inetpub\wwwroot\SinemaWeb`
   - Binding: `https`, Port: `443`, Host name: `cinema.yourdomain.com`
3. Configure SSL certificate
4. Add `web.config` for SPA routing:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="React Routes" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/" />
        </rule>
      </rules>
    </rewrite>
    <staticContent>
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <mimeMap fileExtension=".woff" mimeType="font/woff" />
      <mimeMap fileExtension=".woff2" mimeType="font/woff2" />
    </staticContent>
  </system.webServer>
</configuration>
```

### 4. Deploy to Nginx (Linux)

```bash
sudo cp -r dist/* /var/www/sinema-web/
sudo chown -R www-data:www-data /var/www/sinema-web/
```

**Configure Nginx:**
```nginx
server {
    listen 80;
    server_name cinema.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name cinema.yourdomain.com;
    root /var/www/sinema-web;
    index index.html;

    ssl_certificate /etc/ssl/certs/cinema.crt;
    ssl_certificate_key /etc/ssl/private/cinema.key;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /assets {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    gzip on;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml application/xml+rss text/javascript;
}
```

---

## Database Migration

### 1. Backup Current Database

```sql
BACKUP DATABASE SinemaDb
TO DISK = 'C:\Backups\SinemaDb_Backup.bak'
WITH FORMAT, INIT, SKIP, NOREWIND, NOUNLOAD, STATS = 10;
```

### 2. Apply Migrations

**Option A: Using CLI**
```bash
dotnet ef database update --project src/Sinema.Api --connection "ProductionConnectionString"
```

**Option B: Using SQL Script**
```bash
# Generate SQL script
dotnet ef migrations script --project src/Sinema.Api --output migration.sql --idempotent

# Apply using SSMS or sqlcmd
sqlcmd -S production-server -d SinemaDb_Prod -U sa -P password -i migration.sql
```

### 3. Verify Migration

```sql
-- Check applied migrations
SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId;

-- Verify tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
```

---

## Security Configuration

### 1. Secure Connection Strings

Use Azure Key Vault, AWS Secrets Manager, or Windows DPAPI:

```csharp
// In Program.cs
builder.Configuration.AddAzureKeyVault(/* configuration */);
```

### 2. Enable HTTPS Redirect

```csharp
app.UseHttpsRedirection();
app.UseHsts();
```

### 3. Configure CORS Properly

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", builder =>
    {
        builder.WithOrigins("https://cinema.yourdomain.com")
               .AllowCredentials()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
```

### 4. Secure JWT

- Use strong secret keys (64+ characters)
- Store secrets in environment variables or key vault
- Use short expiration times (15 minutes)
- Implement refresh token rotation

### 5. SQL Injection Prevention

✅ Already implemented via Entity Framework parameterized queries

### 6. Rate Limiting

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromMinutes(1);
    });
});
```

---

## Monitoring & Logging

### 1. Application Insights (Azure)

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### 2. Serilog File Logging

Already configured in `Program.cs`
- Logs location: `logs/sinema-YYYYMMDD.log`
- Configure log retention and rotation

### 3. Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SinemaDbContext>()
    .AddSqlServer(connectionString);

app.MapHealthChecks("/health");
```

### 4. Performance Monitoring

```bash
# Windows Performance Monitor
perfmon

# Linux top/htop
htop

# Application metrics
curl https://api.cinema.yourdomain.com/health
```

---

## Post-Deployment Checklist

- [ ] Backend API accessible and responding
- [ ] Frontend loads correctly
- [ ] Database connections working
- [ ] SSL certificates valid
- [ ] Admin login working
- [ ] Box office functionality tested
- [ ] User registration and booking tested
- [ ] Logs being written correctly
- [ ] Health check endpoint responding
- [ ] Backup strategy implemented
- [ ] Monitoring alerts configured

---

## Rollback Plan

### Backend Rollback

```bash
# Stop service
sudo systemctl stop sinema-api

# Restore previous version
sudo cp -r /var/www/sinema-api-backup/* /var/www/sinema-api/

# Start service
sudo systemctl start sinema-api
```

### Database Rollback

```sql
RESTORE DATABASE SinemaDb
FROM DISK = 'C:\Backups\SinemaDb_Backup.bak'
WITH REPLACE;
```

### Frontend Rollback

```bash
sudo cp -r /var/www/sinema-web-backup/* /var/www/sinema-web/
sudo systemctl reload nginx
```

---

## Maintenance

### Regular Tasks

**Daily:**
- Monitor error logs
- Check system health
- Verify backup completion

**Weekly:**
- Review performance metrics
- Update dependencies (security patches)
- Clean old log files

**Monthly:**
- Database maintenance (index rebuild, stats update)
- Review and optimize slow queries
- Security audit

---

## Useful Commands

### Check Application Status
```bash
# Windows
Get-Service -Name "SinemaAPI"

# Linux
sudo systemctl status sinema-api
```

### View Logs
```bash
# Backend logs
tail -f /var/www/sinema-api/logs/sinema-$(date +%Y%m%d).log

# Nginx logs
tail -f /var/log/nginx/error.log
```

### Restart Services
```bash
# Linux
sudo systemctl restart sinema-api
sudo systemctl reload nginx

# Windows
Restart-Service SinemaAPI
iisreset
```

---

**Last Updated:** October 19, 2025
**Version:** 1.0.0

