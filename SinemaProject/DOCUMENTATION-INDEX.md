# 📚 Documentation Index

Complete guide to all documentation files in the Cinema Management System project.

---

## 🎯 Getting Started

### For First-Time Users

1. **[QUICKSTART.md](QUICKSTART.md)** ⚡
   - **Time:** 5 minutes
   - **Language:** English
   - **Purpose:** Get the app running quickly
   - **Best for:** Quick demo or evaluation

2. **[SETUP.md](SETUP.md)** 📋
   - **Time:** 15-20 minutes
   - **Language:** English
   - **Purpose:** Complete setup with explanations
   - **Best for:** Development setup with understanding

3. **[KURULUM.md](KURULUM.md)** 🇹🇷
   - **Time:** 15-20 dakika
   - **Language:** Türkçe
   - **Purpose:** Eksiksiz kurulum talimatları
   - **Best for:** Türkçe konuşan geliştiriciler

---

## 📂 Documentation Files Overview

### Main Documentation

| File | Purpose | Audience | Language |
|------|---------|----------|----------|
| **[README.md](README.md)** | Project overview, features, tech stack | Everyone | English |
| **[QUICKSTART.md](QUICKSTART.md)** | 5-minute setup guide | New users | English |
| **[SETUP.md](SETUP.md)** | Detailed installation & configuration | Developers | English |
| **[KURULUM.md](KURULUM.md)** | Detaylı kurulum kılavuzu | Geliştiriciler | Türkçe |
| **[DEPLOYMENT.md](DEPLOYMENT.md)** | Production deployment guide | DevOps/Admins | English |
| **[PRESENTATION-GUIDE.md](PRESENTATION-GUIDE.md)** | Presentation roadmap, demo scenarios, Q&A | Presenters | English |
| **[SUNUM-REHBERI.md](SUNUM-REHBERI.md)** | Sunum yol haritası, demo senaryoları, S&C | Sunumcular | Türkçe |
| **[README-CONNECTION.md](README-CONNECTION.md)** | Database connection info | Developers | English |

---

## 🗺️ Documentation by Task

### I want to...

#### 🚀 **Run the application locally**
→ Start with **[QUICKSTART.md](QUICKSTART.md)** or **[KURULUM.md](KURULUM.md)** (Türkçe)

#### 🔧 **Set up a development environment**
→ Follow **[SETUP.md](SETUP.md)** for detailed instructions

#### 🌐 **Deploy to production**
→ Follow **[DEPLOYMENT.md](DEPLOYMENT.md)**

#### 🎤 **Prepare for presentation**
→ Read **[PRESENTATION-GUIDE.md](PRESENTATION-GUIDE.md)** or **[SUNUM-REHBERI.md](SUNUM-REHBERI.md)** (Türkçe)

#### 🗄️ **Configure database connection**
→ See **[README-CONNECTION.md](README-CONNECTION.md)** or **[SETUP.md#database-setup](SETUP.md#database-setup)**

#### ❓ **Troubleshoot an issue**
→ Check **[SETUP.md#troubleshooting](SETUP.md#troubleshooting)**

#### 📚 **Understand project architecture**
→ Read **[README.md#architecture](README.md#architecture)**

#### 🎯 **Learn about features**
→ See **[README.md#features](README.md#features)**

---

## 📖 Documentation by Role

### 👨‍💻 Developers

**First Visit:**
1. [README.md](README.md) - Understand what the project does
2. [QUICKSTART.md](QUICKSTART.md) - Get it running
3. [SETUP.md](SETUP.md) - Deep dive into setup

**Daily Work:**
- [SETUP.md#development-tips](SETUP.md#development-tips)
- Backend: `backend/src/Sinema.Api/Sinema.Api.http` (API testing)
- Frontend: Component documentation in `frontend/src/components/`

### 🚀 DevOps Engineers

**Deployment:**
1. [DEPLOYMENT.md](DEPLOYMENT.md) - Complete deployment guide
2. [DEPLOYMENT.md#security-configuration](DEPLOYMENT.md#security-configuration)
3. [DEPLOYMENT.md#monitoring--logging](DEPLOYMENT.md#monitoring--logging)

**Maintenance:**
- [DEPLOYMENT.md#maintenance](DEPLOYMENT.md#maintenance)
- [DEPLOYMENT.md#rollback-plan](DEPLOYMENT.md#rollback-plan)

### 👔 Project Managers

**Overview:**
1. [README.md](README.md) - Project overview
2. [README.md#features](README.md#features) - Feature list
3. [README.md#tech-stack](README.md#tech-stack) - Technologies used

### 🆕 New Team Members

**Onboarding Sequence:**
1. [README.md](README.md) - Learn about the project (10 min)
2. [QUICKSTART.md](QUICKSTART.md) - Get it running (5 min)
3. [SETUP.md](SETUP.md) - Understand the details (20 min)
4. Explore codebase with running application

---

## 🔍 Quick Reference

### Common Tasks

| Task | Documentation | Section |
|------|---------------|---------|
| Install .NET 9 | [SETUP.md](SETUP.md) | Prerequisites |
| Install Node.js | [SETUP.md](SETUP.md) | Prerequisites |
| Create database | [SETUP.md](SETUP.md) | Database Setup |
| Configure connection string | [SETUP.md](SETUP.md) | Database Setup → Step 2 |
| Apply migrations | [SETUP.md](SETUP.md) | Database Setup → Step 3 |
| Start backend | [QUICKSTART.md](QUICKSTART.md) | Step 3 |
| Start frontend | [QUICKSTART.md](QUICKSTART.md) | Step 4 |
| Login as admin | [SETUP.md](SETUP.md) | Default Credentials |
| Deploy to IIS | [DEPLOYMENT.md](DEPLOYMENT.md) | Backend Deployment → Deploy to IIS |
| Deploy to Linux | [DEPLOYMENT.md](DEPLOYMENT.md) | Backend Deployment → Deploy to Linux |
| Configure SSL | [DEPLOYMENT.md](DEPLOYMENT.md) | Security Configuration |
| Database backup | [DEPLOYMENT.md](DEPLOYMENT.md) | Database Migration |

### Common Issues

| Issue | Solution |
|-------|----------|
| Backend won't start | [SETUP.md#backend-issues](SETUP.md#backend-issues) |
| Frontend connection error | [SETUP.md#frontend-issues](SETUP.md#frontend-issues) |
| Database connection failed | [SETUP.md#database-issues](SETUP.md#database-issues) |
| Port already in use | [SETUP.md#backend-issues](SETUP.md#backend-issues) → Port Already in Use |
| Migration errors | [SETUP.md#backend-issues](SETUP.md#backend-issues) → Migration Errors |
| CORS policy blocked | [SETUP.md#frontend-issues](SETUP.md#frontend-issues) → Cannot Connect to Backend |

---

## 📋 Prerequisites Checklist

Before starting, ensure you have:

- [ ] .NET 9.0 SDK installed
- [ ] Node.js 18+ installed
- [ ] SQL Server running
- [ ] Git installed
- [ ] Text editor/IDE (VS Code, Visual Studio, etc.)
- [ ] SQL Server Management Studio (optional but recommended)

See **[SETUP.md#prerequisites](SETUP.md#prerequisites)** for download links.

---

## 🌐 Language Options

### English Documentation
- [README.md](README.md)
- [QUICKSTART.md](QUICKSTART.md)
- [SETUP.md](SETUP.md)
- [DEPLOYMENT.md](DEPLOYMENT.md)

### Türkçe Dokümantasyon
- [KURULUM.md](KURULUM.md)

---

## 📞 Support

Can't find what you're looking for?

1. **Search the docs:** Use Ctrl+F (Cmd+F) to search in files
2. **Check troubleshooting:** [SETUP.md#troubleshooting](SETUP.md#troubleshooting)
3. **Review logs:**
   - Backend: `backend/src/Sinema.Api/logs/`
   - Browser: F12 → Console tab
4. **Check GitHub Issues:** [Project Issues](#)

---

## 🔄 Documentation Updates

**Last Updated:** October 19, 2025  
**Version:** 1.0.0

To contribute to documentation:
1. Edit the relevant `.md` file
2. Follow the existing format
3. Submit a pull request

---

## 📜 License

[Your License Here]

---

**Quick Links:**
- 🚀 [Get Started in 5 Minutes](QUICKSTART.md)
- 📋 [Full Setup Guide](SETUP.md)
- 🇹🇷 [Türkçe Kurulum](KURULUM.md)
- 🌐 [Deploy to Production](DEPLOYMENT.md)

