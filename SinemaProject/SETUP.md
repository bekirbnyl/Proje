# Cinema Management System - Setup Guide

This guide will help you set up and run the Cinema Management System on a new computer.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Database Setup](#database-setup)
- [Backend Setup](#backend-setup)
- [Frontend Setup](#frontend-setup)
- [Running the Application](#running-the-application)
- [Default Credentials](#default-credentials)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

Before starting, ensure you have the following installed:

### Required Software

1. **.NET 9.0 SDK**
   - Download: https://dotnet.microsoft.com/download/dotnet/9.0
   - Verify installation: `dotnet --version`
   - Should show version 9.0.x or higher

2. **Node.js (v18 or higher)**
   - Download: https://nodejs.org/
   - Verify installation: `node --version` and `npm --version`
   - Recommended: v18.x or v20.x

3. **SQL Server (MSSQL)**
   - SQL Server 2019 or higher
   - SQL Server Express is sufficient
   - Download: https://www.microsoft.com/sql-server/sql-server-downloads
   - OR use SQL Server Management Studio (SSMS): https://aka.ms/ssmsfullsetup

4. **Git**
   - Download: https://git-scm.com/downloads
   - For cloning the repository

---

## Database Setup

### Step 1: Create Database

1. Open **SQL Server Management Studio (SSMS)** or any SQL client
2. Connect to your SQL Server instance
3. Create a new database:

```sql
CREATE DATABASE SinemaDb;
```

### Step 2: Configure Connection String

1. Navigate to `backend/src/Sinema.Api/appsettings.json`
2. Update the connection string with your SQL Server details:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=SinemaDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Common Server Names:**
- Local SQL Server: `localhost` or `.` or `(localdb)\\mssqllocaldb`
- SQL Server Express: `localhost\\SQLEXPRESS` or `.\\SQLEXPRESS`
- Remote Server: `server_ip,port` (e.g., `192.168.1.100,1433`)

**Authentication Options:**
- **Windows Authentication (Recommended):** `Trusted_Connection=True;`
- **SQL Server Authentication:** `User Id=sa;Password=YourPassword;`

### Step 3: Apply Database Migrations

1. Open terminal/command prompt
2. Navigate to the backend directory:

```bash
cd backend
```

3. Apply migrations to create database schema:

```bash
dotnet ef database update --project src/Sinema.Api
```

This will create all necessary tables, relationships, and seed initial data.

---

## Backend Setup

### Step 1: Clone Repository

```bash
git clone <repository-url>
cd SinemaProject
```

### Step 2: Restore NuGet Packages

Navigate to the backend directory and restore dependencies:

```bash
cd backend
dotnet restore
```

### Step 3: Build the Backend

```bash
dotnet build
```

### Step 4: Configure Settings (Optional)

Edit `backend/src/Sinema.Api/appsettings.json` to customize:

- **JWT Settings**: Token expiration, secret keys
- **Logging**: Log levels and file paths
- **CORS**: Allowed origins for frontend

```json
{
  "Jwt": {
    "Secret": "your-super-secret-key-at-least-32-characters-long",
    "Issuer": "cinema.local",
    "Audience": "cinema.local",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

---

## Frontend Setup

### Step 1: Navigate to Frontend Directory

```bash
cd frontend
```

### Step 2: Install Dependencies

```bash
npm install
```

This will install all required packages from `package.json`.

### Step 3: Configure API URL

The frontend is configured to connect to `http://localhost:5238` by default.

If your backend runs on a different port, update `frontend/src/services/api.ts`:

```typescript
const API_BASE_URL = 'http://localhost:YOUR_PORT/api/v1';
```

---

## Running the Application

### Step 1: Start the Backend

1. Open a terminal in the `backend` directory
2. Navigate to the API project:

```bash
cd src/Sinema.Api
```

3. Run the backend:

```bash
dotnet run
```

Or for hot reload during development:

```bash
dotnet watch run
```

The backend will start on **http://localhost:5238** (or https://localhost:7238)

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5238
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Step 2: Start the Frontend

1. Open a **new terminal** in the `frontend` directory
2. Start the development server:

```bash
npm run dev
```

The frontend will start on **http://localhost:5173**

**Expected Output:**
```
VITE v5.x.x  ready in xxx ms

  ➜  Local:   http://localhost:5173/
  ➜  Network: use --host to expose
```

### Step 3: Access the Application

Open your browser and navigate to: **http://localhost:5173**

---

## Default Credentials

The system is seeded with default users for testing:

### Admin Account
- **Email:** `admin@cinema.local`
- **Password:** `Admin*123`
- **Role:** Admin (full access)

### Box Office Staff
- **Email:** `gise@cinema.local`
- **Password:** `Gise*123`
- **Role:** Gişe Görevlisi (Box Office)

### Regular User
You can register a new account via the registration page.
- Admin approval is required for new members
- Use the admin account to approve new registrations

---

## Project Structure

```
SinemaProject/
├── backend/
│   ├── src/
│   │   ├── Sinema.Api/              # Web API (Controllers, Middleware)
│   │   ├── Sinema.Application/      # Business Logic (Features, DTOs)
│   │   ├── Sinema.Domain/           # Domain Models (Entities, Interfaces)
│   │   ├── Sinema.Infrastructure/   # Data Access (Repositories, DbContext)
│   │   └── Sinema.Background/       # Background Jobs
│   └── tests/                       # Unit & Integration Tests
└── frontend/
    ├── src/
    │   ├── components/              # React Components
    │   ├── store/                   # Redux Store & Slices
    │   ├── services/                # API Services
    │   └── types/                   # TypeScript Types
    └── public/                      # Static Assets
```

---

## Features Overview

### For Admins
- ✅ User management and approval
- ✅ Movie management (CRUD operations)
- ✅ Hall and seating layout configuration
- ✅ Screening schedule management
- ✅ Pricing policy configuration
- ✅ Sales reports and analytics
- ✅ System settings

### For Box Office Staff
- ✅ Ticket sales interface
- ✅ Seat selection and reservation
- ✅ Multiple payment methods
- ✅ Quick member search

### For Regular Users (Web Users)
- ✅ Browse movies and screenings
- ✅ Online ticket booking
- ✅ Seat selection
- ✅ Online payment
- ✅ View purchase history
- ✅ Profile management

---

## Troubleshooting

### Backend Issues

#### 1. Database Connection Failed

**Error:** `A connection was successfully established with the server, but then an error occurred`

**Solutions:**
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure database `SinemaDb` exists
- Try: `dotnet ef database update --project src/Sinema.Api`

#### 2. Port Already in Use

**Error:** `Failed to bind to address http://localhost:5238`

**Solutions:**
- Change port in `backend/src/Sinema.Api/Properties/launchSettings.json`
- Kill process using the port: `netstat -ano | findstr :5238` then `taskkill /PID <PID> /F`

#### 3. Migration Errors

**Error:** `Unable to create migration` or `No migrations found`

**Solutions:**
```bash
# Remove existing migrations (if needed)
cd backend
dotnet ef migrations remove --project src/Sinema.Api

# Create new migration
dotnet ef migrations add InitialCreate --project src/Sinema.Api

# Apply migration
dotnet ef database update --project src/Sinema.Api
```

#### 4. JWT Token Errors

**Error:** `401 Unauthorized` or `Token validation failed`

**Solutions:**
- Clear browser localStorage
- Log out and log in again
- Verify JWT settings in `appsettings.json`
- Ensure secret key is at least 32 characters

### Frontend Issues

#### 1. Cannot Connect to Backend

**Error:** `Network Error` or `CORS policy blocked`

**Solutions:**
- Ensure backend is running on `http://localhost:5238`
- Check `frontend/src/services/api.ts` for correct API URL
- Verify CORS is configured in backend `Program.cs`

#### 2. Dependencies Installation Failed

**Error:** `npm install` errors

**Solutions:**
```bash
# Clear cache and reinstall
rm -rf node_modules package-lock.json
npm cache clean --force
npm install
```

#### 3. Build Errors

**Error:** TypeScript or ESLint errors

**Solutions:**
```bash
# Check for type errors
npm run type-check

# Fix linting issues
npm run lint -- --fix
```

#### 4. Hot Reload Not Working

**Solutions:**
- Stop dev server (Ctrl+C)
- Delete `.vite` cache folder
- Restart: `npm run dev`

### Database Issues

#### 1. Cannot Create Database

**Error:** Permission denied or login failed

**Solutions:**
- Run SQL Server Management Studio as Administrator
- Ensure your Windows user has `sysadmin` role in SQL Server
- Or use SQL authentication with `sa` account

#### 2. Migrations Not Applied

**Solutions:**
```bash
# Check migration status
dotnet ef migrations list --project src/Sinema.Api

# Force apply all migrations
dotnet ef database update --project src/Sinema.Api --verbose
```

#### 3. Data Seeding Failed

**Solutions:**
- Check logs in `backend/src/Sinema.Api/logs/`
- Manually run seed script if needed
- Ensure unique constraints are not violated

---

## Development Tips

### Hot Reload

**Backend:**
```bash
cd backend/src/Sinema.Api
dotnet watch run
```

**Frontend:**
```bash
cd frontend
npm run dev
```

### Viewing Logs

**Backend logs:**
- Location: `backend/src/Sinema.Api/logs/`
- Format: `sinema-YYYYMMDD.log`

**Browser console:**
- Press F12 in browser to open DevTools
- Check Console tab for frontend errors

### Database Inspection

Use SQL Server Management Studio or:
```bash
dotnet ef dbcontext scaffold "YourConnectionString" Microsoft.EntityFrameworkCore.SqlServer --project src/Sinema.Api
```

### API Testing

Use the included HTTP files:
- `backend/TestScript.http`
- `backend/Phase3TestScript.http`
- `backend/src/Sinema.Api/Sinema.Api.http`

Or use tools like:
- Postman
- Insomnia
- REST Client (VS Code extension)

---

## Production Deployment

### Backend

1. **Build for production:**
```bash
cd backend
dotnet publish -c Release -o ./publish
```

2. **Configure production settings:**
   - Update `appsettings.Production.json`
   - Set secure JWT secrets
   - Configure production database
   - Enable HTTPS

3. **Deploy to IIS or Azure:**
   - Copy `publish` folder to server
   - Configure IIS application pool (.NET CLR Version: No Managed Code)
   - Set up SSL certificate

### Frontend

1. **Build for production:**
```bash
cd frontend
npm run build
```

2. **Deploy static files:**
   - Output folder: `frontend/dist/`
   - Deploy to: IIS, Nginx, Apache, or CDN
   - Configure environment variables for API URL

3. **Update API URL:**
   - Create `frontend/.env.production`
   - Set: `VITE_API_URL=https://your-api-domain.com/api/v1`

---

## Additional Resources

- **.NET Documentation:** https://docs.microsoft.com/dotnet/
- **React Documentation:** https://react.dev/
- **Redux Toolkit:** https://redux-toolkit.js.org/
- **Vite Documentation:** https://vitejs.dev/
- **Entity Framework Core:** https://docs.microsoft.com/ef/core/

---

## Support

For issues or questions:
1. Check the [Troubleshooting](#troubleshooting) section
2. Review application logs
3. Check browser console for frontend errors
4. Review backend logs in `logs/` directory

---

## License

[Your License Here]

---

**Last Updated:** October 19, 2025
**Version:** 1.0.0

