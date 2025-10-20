# Windows Backend'e Bağlanma Talimatları

## 🎯 Frontend Hazır Durumda!

Frontend'iniz **%100 hazır** ve Windows backend'inizle çalışmaya uygun. İşte bağlantı seçenekleri:

## 🔗 Bağlantı Seçenekleri

### Seçenek 1: Aynı Ağdaki Windows Makine

Eğer Windows ve Mac aynı Wi-Fi/ağdaysa:

1. **Windows'ta IP adresini öğrenin:**
   ```cmd
   ipconfig
   ```
   
2. **Frontend .env dosyasını güncelleyin:**
   ```bash
   cd frontend
   echo "VITE_API_BASE_URL=http://192.168.1.100:5238" > .env
   echo "VITE_MOCK_MODE=false" >> .env
   ```
   
   > `192.168.1.100` yerine gerçek Windows IP'nizi yazın

### Seçenek 2: Farklı Port

Windows backend'iniz farklı portta çalışıyorsa:

```bash
echo "VITE_API_BASE_URL=http://localhost:5000" > frontend/.env
echo "VITE_MOCK_MODE=false" >> frontend/.env
```

### Seçenek 3: Cloud/Remote Backend

Windows backend'i cloud'da ise:

```bash
echo "VITE_API_BASE_URL=https://your-domain.com" > frontend/.env
echo "VITE_MOCK_MODE=false" >> frontend/.env
```

## 🧪 Mock Mode ile Test

Windows backend hazır değilken test etmek için:

```bash
echo "VITE_MOCK_MODE=true" >> frontend/.env
```

**Mock Test Hesapları:**
- `test@example.com` / `test123` (Normal user)
- `vip@example.com` / `vip123` (VIP user)

## 🚀 Çalıştırma

```bash
# Frontend başlat
cd frontend
npm run dev
# http://localhost:5173

# Windows backend'iniz zaten çalışıyor olmalı
# http://localhost:5238 (veya belirlediğiniz port)
```

## ✅ Test Edebileceğiniz Özellikler

### Public (Giriş yapmadan):
- Ana sayfa film listesi
- Film detayları ve seanslar
- "Bilet Al" → Login'e yönlendirme

### Authenticated (Login sonrası):
- Koltuk seçimi ve rezervasyon
- VIP indirimleri ve özellikler
- Profile sayfası

### Admin/Desk:
- Yönetim panelleri (placeholder hazır)
- Box office işlemleri (placeholder hazır)

## 🔧 CORS Ayarları

Eğer CORS hatası alırsanız, Windows backend'inizde CORS ayarlarını kontrol edin:

```csharp
// Program.cs veya Startup.cs
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
```

## 📋 Kontrol Listesi

- [ ] Windows backend çalışıyor (`http://localhost:5238/health`)
- [ ] Frontend .env dosyası doğru IP/port'a ayarlı
- [ ] CORS ayarları yapıldı
- [ ] Test hesapları oluşturuldu
- [ ] Database connection çalışıyor

## 🎊 Hazır Özellikler

✅ **Tam Fonksiyonel:**
- JWT authentication & refresh
- Role-based access control
- Public movie browsing
- Seat selection & pricing
- VIP benefits display
- Responsive design
- Error handling

✅ **API Entegrasyonu:**
- TypeScript DTOs backend'le uyumlu
- Axios interceptors
- Redux state management
- Auto-logout on 401

Frontend **backend değişikliği gerektirmeden** çalışmaya hazır! 🚀
