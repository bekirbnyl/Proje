# 🚀 Quick Start Guide

Get the Cinema Management System up and running in 5 minutes!

## Prerequisites Checklist

- [ ] .NET 9.0 SDK installed
- [ ] Node.js 18+ installed
- [ ] SQL Server running
- [ ] Git installed

## 1️⃣ Clone & Navigate

```bash
git clone <repository-url>
cd SinemaProject
```

## 2️⃣ Database Setup (2 minutes)

**Open SQL Server Management Studio and run:**
```sql
CREATE DATABASE SinemaDb;
```

**Update connection string in `backend/src/Sinema.Api/appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SinemaDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Apply migrations:**
```bash
cd backend
dotnet ef database update --project src/Sinema.Api
```

## 3️⃣ Start Backend (1 minute)

```bash
cd backend/src/Sinema.Api
dotnet run
```

✅ Backend running at: **http://localhost:5238**

## 4️⃣ Start Frontend (1 minute)

**Open a new terminal:**
```bash
cd frontend
npm install
npm run dev
```

✅ Frontend running at: **http://localhost:5173**

## 5️⃣ Login & Test

Open browser: **http://localhost:5173**

**Admin Login:**
- Email: `admin@cinema.local`
- Password: `Admin*123`

**Box Office Login:**
- Email: `gise@cinema.local`
- Password: `Gise*123`

---

## 🎯 Quick Test Checklist

- [ ] Admin dashboard loads
- [ ] Can view movies
- [ ] Can create a screening
- [ ] Box office can sell tickets
- [ ] User can register and book tickets

---

## 🆘 Common Issues

### Backend won't start?
```bash
# Check .NET version
dotnet --version  # Should be 9.0.x

# Rebuild
cd backend
dotnet clean
dotnet build
```

### Frontend won't start?
```bash
# Clear cache
cd frontend
rm -rf node_modules
npm install
```

### Database connection error?
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Try: `dotnet ef database update --project src/Sinema.Api`

---

## 📚 Next Steps

For detailed documentation, see:
- **[SETUP.md](SETUP.md)** - Complete setup guide (English)
- **[KURULUM.md](KURULUM.md)** - Complete setup guide (Türkçe)
- **[README.md](README.md)** - Project overview

---

**Need help?** Check the [Troubleshooting section](SETUP.md#troubleshooting) in SETUP.md

