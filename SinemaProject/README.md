# 🎬 Cinema Management System

A comprehensive cinema management system built with .NET 9 and React, featuring ticket sales, seat reservations, member management, and reporting capabilities.

## 🚀 Quick Start

Choose your documentation language:
- 🇬🇧 **[QUICKSTART.md](QUICKSTART.md)** - 5-minute quick start guide
- 🇬🇧 **[SETUP.md](SETUP.md)** - Detailed setup instructions (English)
- 🇹🇷 **[KURULUM.md](KURULUM.md)** - Detaylı kurulum talimatları (Türkçe)

### Quick Setup (5 minutes)

1. **Prerequisites:** .NET 9, Node.js 18+, SQL Server
2. **Database:** Create `SinemaDb` and update connection string in `appsettings.json`
3. **Backend:**
   ```bash
   cd backend
   dotnet restore
   dotnet ef database update --project src/Sinema.Api
   cd src/Sinema.Api
   dotnet run
   ```
4. **Frontend:**
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
5. **Access:** http://localhost:5173

## 🔑 Default Credentials

- **Admin:** `admin@cinema.local` / `Admin*123`
- **Box Office:** `gise@cinema.local` / `Gise*123`

## ✨ Features

### 🎫 For Users
- Browse movies and screenings
- Online ticket booking with seat selection
- Multiple payment methods
- Purchase history and profile management

### 🏢 For Box Office Staff
- Quick ticket sales interface
- Real-time seat availability
- Member search and management
- Multiple payment processing

### 👨‍💼 For Administrators
- Complete movie and screening management
- Hall layout configuration
- Dynamic pricing policies
- Member approval system
- Comprehensive sales reports
- System settings and configuration

## 🏗️ Architecture

### Backend (.NET 9)
- **Clean Architecture** with CQRS pattern
- **Entity Framework Core** for data access
- **JWT Authentication** with refresh tokens
- **FluentValidation** for input validation
- **Serilog** for logging
- **Background Jobs** with Quartz.NET

### Frontend (React + TypeScript)
- **React 18** with TypeScript
- **Redux Toolkit** for state management
- **React Router** for navigation
- **Tailwind CSS** for styling
- **Vite** for fast development

### Database
- **SQL Server** (MSSQL 2019+)
- Entity Framework Core migrations
- Optimized indexes and relationships

## 📁 Project Structure

```
SinemaProject/
├── backend/
│   ├── src/
│   │   ├── Sinema.Api/              # REST API
│   │   ├── Sinema.Application/      # Business Logic
│   │   ├── Sinema.Domain/           # Domain Models
│   │   ├── Sinema.Infrastructure/   # Data & Services
│   │   └── Sinema.Background/       # Background Jobs
│   └── tests/                       # Tests
└── frontend/
    └── src/
        ├── components/              # React Components
        ├── store/                   # Redux Store
        ├── services/                # API Services
        └── types/                   # TypeScript Types
```

## 🛠️ Tech Stack

### Backend
- .NET 9.0
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- Serilog
- FluentValidation
- MediatR (CQRS)
- Quartz.NET

### Frontend
- React 18
- TypeScript
- Redux Toolkit
- React Router 6
- Tailwind CSS
- Vite
- Axios

## 📖 Documentation

> 📚 **[View Complete Documentation Index](DOCUMENTATION-INDEX.md)** - Find all documentation organized by role and task

### Setup & Installation
- **[QUICKSTART.md](QUICKSTART.md)** - 5-minute quick start guide 🇬🇧
- **[SETUP.md](SETUP.md)** - Detailed setup and installation guide 🇬🇧
- **[KURULUM.md](KURULUM.md)** - Detaylı kurulum ve yükleme kılavuzu 🇹🇷
- **[README-CONNECTION.md](README-CONNECTION.md)** - Database connection information

### Deployment
- **[DEPLOYMENT.md](DEPLOYMENT.md)** - Production deployment guide (IIS, Nginx, Azure)

### Presentation
- **[PRESENTATION-GUIDE.md](PRESENTATION-GUIDE.md)** - Complete presentation guide with Q&A 🇬🇧
- **[SUNUM-REHBERI.md](SUNUM-REHBERI.md)** - Kapsamlı sunum rehberi ve S&C 🇹🇷

## 🐛 Troubleshooting

See the [Troubleshooting section in SETUP.md](SETUP.md#troubleshooting) for common issues and solutions.

## 🔐 Security Features

- JWT token-based authentication
- Refresh token mechanism
- Role-based access control (RBAC)
- Password hashing with ASP.NET Core Identity
- CORS protection
- SQL injection prevention via parameterized queries

## 🎯 Key Business Rules

- **T-60 Rule:** No reservations within 60 minutes of screening start (can be disabled for testing)
- **Seat Hold System:** Seats are held temporarily before purchase
- **Dynamic Pricing:** Prices calculated based on day, time, and member type
- **Member Approval:** New members require admin approval
- **VIP Benefits:** Special pricing for VIP members

## 📊 Reports

The system includes comprehensive reporting:
- Daily/Weekly/Monthly sales reports
- Movie performance analytics
- Hall utilization statistics
- Revenue breakdown by payment method
- Member activity reports

## 🔄 API Endpoints

Key endpoints include:
- `/api/v1/auth` - Authentication
- `/api/v1/movies` - Movie management
- `/api/v1/screenings` - Screening schedules
- `/api/v1/tickets` - Ticket sales
- `/api/v1/halls` - Hall management
- `/api/v1/members` - Member management
- `/api/v1/reports` - Reporting

API documentation available via Swagger at: `http://localhost:5238/swagger`

## 🧪 Testing

### Backend Tests
```bash
cd backend/tests/Sinema.UnitTests
dotnet test

cd ../Sinema.IntegrationTests
dotnet test
```

### Frontend Tests
```bash
cd frontend
npm run test
```

## 📝 License

[Your License Here]

---

**Version:** 1.0.0  
**Last Updated:** October 19, 2025

For detailed setup instructions, see **[SETUP.md](SETUP.md)**
