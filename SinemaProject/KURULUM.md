# Sinema Yönetim Sistemi - Kurulum Kılavuzu

Bu kılavuz, Sinema Yönetim Sistemi'ni yeni bir bilgisayarda kurmanıza ve çalıştırmanıza yardımcı olacaktır.

## İçindekiler
- [Gereksinimler](#gereksinimler)
- [Veritabanı Kurulumu](#veritabanı-kurulumu)
- [Backend Kurulumu](#backend-kurulumu)
- [Frontend Kurulumu](#frontend-kurulumu)
- [Uygulamayı Çalıştırma](#uygulamayı-çalıştırma)
- [Varsayılan Kullanıcı Bilgileri](#varsayılan-kullanıcı-bilgileri)
- [Sorun Giderme](#sorun-giderme)

---

## Gereksinimler

Başlamadan önce, aşağıdaki yazılımların kurulu olduğundan emin olun:

### Gerekli Yazılımlar

1. **.NET 9.0 SDK**
   - İndirme linki: https://dotnet.microsoft.com/download/dotnet/9.0
   - Kurulumu doğrulama: `dotnet --version`
   - 9.0.x veya üzeri versiyon göstermeli

2. **Node.js (v18 veya üzeri)**
   - İndirme linki: https://nodejs.org/
   - Kurulumu doğrulama: `node --version` ve `npm --version`
   - Önerilen: v18.x veya v20.x

3. **SQL Server (MSSQL)**
   - SQL Server 2019 veya üzeri
   - SQL Server Express yeterlidir
   - İndirme: https://www.microsoft.com/sql-server/sql-server-downloads
   - VEYA SQL Server Management Studio (SSMS): https://aka.ms/ssmsfullsetup

4. **Git**
   - İndirme: https://git-scm.com/downloads
   - Projeyi klonlamak için gerekli

---

## Veritabanı Kurulumu

### Adım 1: Veritabanı Oluşturma

1. **SQL Server Management Studio (SSMS)** veya herhangi bir SQL istemcisi açın
2. SQL Server örneğinize bağlanın
3. Yeni bir veritabanı oluşturun:

```sql
CREATE DATABASE SinemaDb;
```

### Adım 2: Bağlantı Dizesini Yapılandırma

1. `backend/src/Sinema.Api/appsettings.json` dosyasına gidin
2. Bağlantı dizesini SQL Server bilgilerinizle güncelleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SUNUCU_ADI;Database=SinemaDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Yaygın Sunucu Adları:**
- Yerel SQL Server: `localhost` veya `.` veya `(localdb)\\mssqllocaldb`
- SQL Server Express: `localhost\\SQLEXPRESS` veya `.\\SQLEXPRESS`
- Uzak Sunucu: `sunucu_ip,port` (örn: `192.168.1.100,1433`)

**Kimlik Doğrulama Seçenekleri:**
- **Windows Kimlik Doğrulama (Önerilen):** `Trusted_Connection=True;`
- **SQL Server Kimlik Doğrulama:** `User Id=sa;Password=Sifreniz;`

### Adım 3: Veritabanı Migration'larını Uygulama

1. Terminal/komut istemi açın
2. Backend dizinine gidin:

```bash
cd backend
```

3. Veritabanı şemasını oluşturmak için migration'ları uygulayın:

```bash
dotnet ef database update --project src/Sinema.Api
```

Bu komut tüm gerekli tabloları, ilişkileri oluşturacak ve başlangıç verilerini ekleyecektir.

---

## Backend Kurulumu

### Adım 1: Projeyi Klonlama

```bash
git clone <repository-url>
cd SinemaProject
```

### Adım 2: NuGet Paketlerini Geri Yükleme

Backend dizinine gidin ve bağımlılıkları geri yükleyin:

```bash
cd backend
dotnet restore
```

### Adım 3: Backend'i Derleme

```bash
dotnet build
```

### Adım 4: Ayarları Yapılandırma (İsteğe Bağlı)

`backend/src/Sinema.Api/appsettings.json` dosyasını düzenleyerek özelleştirebilirsiniz:

- **JWT Ayarları**: Token süresi, gizli anahtarlar
- **Loglama**: Log seviyeleri ve dosya yolları
- **CORS**: Frontend için izin verilen kaynaklar

```json
{
  "Jwt": {
    "Secret": "en-az-32-karakter-uzunlugunda-super-gizli-anahtar",
    "Issuer": "cinema.local",
    "Audience": "cinema.local",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

---

## Frontend Kurulumu

### Adım 1: Frontend Dizinine Gitme

```bash
cd frontend
```

### Adım 2: Bağımlılıkları Yükleme

```bash
npm install
```

Bu komut `package.json` dosyasındaki tüm gerekli paketleri yükleyecektir.

### Adım 3: API URL'sini Yapılandırma

Frontend varsayılan olarak `http://localhost:5238` adresine bağlanacak şekilde yapılandırılmıştır.

Eğer backend'iniz farklı bir portta çalışıyorsa, `frontend/src/services/api.ts` dosyasını güncelleyin:

```typescript
const API_BASE_URL = 'http://localhost:SIZIN_PORT/api/v1';
```

---

## Uygulamayı Çalıştırma

### Adım 1: Backend'i Başlatma

1. `backend` dizininde bir terminal açın
2. API projesine gidin:

```bash
cd src/Sinema.Api
```

3. Backend'i çalıştırın:

```bash
dotnet run
```

Veya geliştirme sırasında otomatik yenileme için:

```bash
dotnet watch run
```

Backend **http://localhost:5238** adresinde başlayacaktır (veya https://localhost:7238)

**Beklenen Çıktı:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5238
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Adım 2: Frontend'i Başlatma

1. `frontend` dizininde **yeni bir terminal** açın
2. Geliştirme sunucusunu başlatın:

```bash
npm run dev
```

Frontend **http://localhost:5173** adresinde başlayacaktır

**Beklenen Çıktı:**
```
VITE v5.x.x  ready in xxx ms

  ➜  Local:   http://localhost:5173/
  ➜  Network: use --host to expose
```

### Adım 3: Uygulamaya Erişim

Tarayıcınızı açın ve şu adrese gidin: **http://localhost:5173**

---

## Varsayılan Kullanıcı Bilgileri

Sistem, test için varsayılan kullanıcılarla birlikte gelir:

### Admin Hesabı
- **E-posta:** `admin@cinema.local`
- **Şifre:** `Admin*123`
- **Rol:** Admin (tam erişim)

### Gişe Görevlisi
- **E-posta:** `gise@cinema.local`
- **Şifre:** `Gise*123`
- **Rol:** Gişe Görevlisi

### Normal Kullanıcı
Kayıt sayfasından yeni bir hesap oluşturabilirsiniz.
- Yeni üyeler için admin onayı gereklidir
- Yeni kayıtları onaylamak için admin hesabını kullanın

---

## Proje Yapısı

```
SinemaProject/
├── backend/
│   ├── src/
│   │   ├── Sinema.Api/              # Web API (Controller'lar, Middleware)
│   │   ├── Sinema.Application/      # İş Mantığı (Feature'lar, DTO'lar)
│   │   ├── Sinema.Domain/           # Domain Modelleri (Entity'ler, Interface'ler)
│   │   ├── Sinema.Infrastructure/   # Veri Erişimi (Repository'ler, DbContext)
│   │   └── Sinema.Background/       # Arka Plan İşleri
│   └── tests/                       # Birim & Entegrasyon Testleri
└── frontend/
    ├── src/
    │   ├── components/              # React Bileşenleri
    │   ├── store/                   # Redux Store & Slice'lar
    │   ├── services/                # API Servisleri
    │   └── types/                   # TypeScript Tipleri
    └── public/                      # Statik Dosyalar
```

---

## Özellikler

### Adminler İçin
- ✅ Kullanıcı yönetimi ve onaylama
- ✅ Film yönetimi (CRUD işlemleri)
- ✅ Salon ve koltuk düzeni yapılandırması
- ✅ Seans programı yönetimi
- ✅ Fiyatlandırma politikası yapılandırması
- ✅ Satış raporları ve analizler
- ✅ Sistem ayarları

### Gişe Görevlileri İçin
- ✅ Bilet satış arayüzü
- ✅ Koltuk seçimi ve rezervasyon
- ✅ Çoklu ödeme yöntemleri
- ✅ Hızlı üye arama

### Normal Kullanıcılar İçin (Web Kullanıcıları)
- ✅ Film ve seansları görüntüleme
- ✅ Online bilet rezervasyonu
- ✅ Koltuk seçimi
- ✅ Online ödeme
- ✅ Satın alma geçmişi
- ✅ Profil yönetimi

---

## Sorun Giderme

### Backend Sorunları

#### 1. Veritabanı Bağlantı Hatası

**Hata:** `A connection was successfully established with the server, but then an error occurred`

**Çözümler:**
- SQL Server'ın çalıştığını doğrulayın
- `appsettings.json` içindeki bağlantı dizesini kontrol edin
- `SinemaDb` veritabanının mevcut olduğundan emin olun
- Şunu deneyin: `dotnet ef database update --project src/Sinema.Api`

#### 2. Port Zaten Kullanımda

**Hata:** `Failed to bind to address http://localhost:5238`

**Çözümler:**
- `backend/src/Sinema.Api/Properties/launchSettings.json` içinde portu değiştirin
- Portu kullanan işlemi sonlandırın: `netstat -ano | findstr :5238` sonra `taskkill /PID <PID> /F`

#### 3. Migration Hataları

**Hata:** `Unable to create migration` veya `No migrations found`

**Çözümler:**
```bash
# Mevcut migration'ları kaldır (gerekirse)
cd backend
dotnet ef migrations remove --project src/Sinema.Api

# Yeni migration oluştur
dotnet ef migrations add InitialCreate --project src/Sinema.Api

# Migration'ı uygula
dotnet ef database update --project src/Sinema.Api
```

#### 4. JWT Token Hataları

**Hata:** `401 Unauthorized` veya `Token validation failed`

**Çözümler:**
- Tarayıcı localStorage'ını temizleyin
- Çıkış yapıp tekrar giriş yapın
- `appsettings.json` içindeki JWT ayarlarını kontrol edin
- Gizli anahtarın en az 32 karakter olduğundan emin olun

### Frontend Sorunları

#### 1. Backend'e Bağlanılamıyor

**Hata:** `Network Error` veya `CORS policy blocked`

**Çözümler:**
- Backend'in `http://localhost:5238` üzerinde çalıştığından emin olun
- `frontend/src/services/api.ts` içindeki API URL'sini kontrol edin
- Backend'deki `Program.cs` dosyasında CORS yapılandırmasını doğrulayın

#### 2. Bağımlılık Yükleme Hatası

**Hata:** `npm install` hataları

**Çözümler:**
```bash
# Cache'i temizle ve yeniden yükle
rm -rf node_modules package-lock.json
npm cache clean --force
npm install
```

#### 3. Derleme Hataları

**Hata:** TypeScript veya ESLint hataları

**Çözümler:**
```bash
# Tip hatalarını kontrol et
npm run type-check

# Lint hatalarını düzelt
npm run lint -- --fix
```

#### 4. Hot Reload Çalışmıyor

**Çözümler:**
- Dev sunucuyu durdurun (Ctrl+C)
- `.vite` cache klasörünü silin
- Yeniden başlatın: `npm run dev`

### Veritabanı Sorunları

#### 1. Veritabanı Oluşturulamıyor

**Hata:** İzin reddedildi veya giriş başarısız

**Çözümler:**
- SQL Server Management Studio'yu Yönetici olarak çalıştırın
- Windows kullanıcınızın SQL Server'da `sysadmin` rolüne sahip olduğundan emin olun
- Veya `sa` hesabı ile SQL kimlik doğrulaması kullanın

#### 2. Migration'lar Uygulanmadı

**Çözümler:**
```bash
# Migration durumunu kontrol et
dotnet ef migrations list --project src/Sinema.Api

# Tüm migration'ları zorla uygula
dotnet ef database update --project src/Sinema.Api --verbose
```

#### 3. Veri Ekleme (Seeding) Başarısız

**Çözümler:**
- `backend/src/Sinema.Api/logs/` klasöründeki logları kontrol edin
- Gerekirse seed script'i manuel olarak çalıştırın
- Benzersizlik kısıtlamalarının ihlal edilmediğinden emin olun

---

## Geliştirme İpuçları

### Otomatik Yenileme (Hot Reload)

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

### Logları Görüntüleme

**Backend logları:**
- Konum: `backend/src/Sinema.Api/logs/`
- Format: `sinema-YYYYMMDD.log`

**Tarayıcı konsolu:**
- Tarayıcıda F12'ye basarak DevTools'u açın
- Frontend hataları için Console sekmesini kontrol edin

### Veritabanı İnceleme

SQL Server Management Studio kullanın veya:
```bash
dotnet ef dbcontext scaffold "BaglantiDizesi" Microsoft.EntityFrameworkCore.SqlServer --project src/Sinema.Api
```

### API Testi

Dahil edilen HTTP dosyalarını kullanın:
- `backend/TestScript.http`
- `backend/Phase3TestScript.http`
- `backend/src/Sinema.Api/Sinema.Api.http`

Veya şu araçları kullanın:
- Postman
- Insomnia
- REST Client (VS Code eklentisi)

---

## Canlı Ortama Alma (Production)

### Backend

1. **Canlı ortam için derleme:**
```bash
cd backend
dotnet publish -c Release -o ./publish
```

2. **Canlı ortam ayarlarını yapılandırma:**
   - `appsettings.Production.json` dosyasını güncelleyin
   - Güvenli JWT secret'ları ayarlayın
   - Canlı veritabanını yapılandırın
   - HTTPS'i etkinleştirin

3. **IIS veya Azure'a dağıtma:**
   - `publish` klasörünü sunucuya kopyalayın
   - IIS application pool'u yapılandırın (.NET CLR Version: No Managed Code)
   - SSL sertifikası kurun

### Frontend

1. **Canlı ortam için derleme:**
```bash
cd frontend
npm run build
```

2. **Statik dosyaları dağıtma:**
   - Çıktı klasörü: `frontend/dist/`
   - Dağıtım: IIS, Nginx, Apache veya CDN
   - API URL için ortam değişkenlerini yapılandırın

3. **API URL'sini güncelleme:**
   - `frontend/.env.production` oluşturun
   - Ayarlayın: `VITE_API_URL=https://api-domain.com/api/v1`

---

## Ek Kaynaklar

- **.NET Dokümantasyonu:** https://docs.microsoft.com/dotnet/
- **React Dokümantasyonu:** https://react.dev/
- **Redux Toolkit:** https://redux-toolkit.js.org/
- **Vite Dokümantasyonu:** https://vitejs.dev/
- **Entity Framework Core:** https://docs.microsoft.com/ef/core/

---

## Destek

Sorunlar veya sorular için:
1. [Sorun Giderme](#sorun-giderme) bölümünü kontrol edin
2. Uygulama loglarını inceleyin
3. Frontend hataları için tarayıcı konsolunu kontrol edin
4. Backend loglarını `logs/` dizininde gözden geçirin

---

## Lisans

[Lisans Bilgisi Buraya]

---

**Son Güncelleme:** 19 Ekim 2025
**Versiyon:** 1.0.0

