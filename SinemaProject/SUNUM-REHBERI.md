# 🎤 Sinema Yönetim Sistemi - Sunum Rehberi

Sinema Yönetim Sistemi projesini sunmak için kapsamlı rehber: demo senaryoları, soru-cevaplar ve konuşma notları.

## 📋 İçindekiler
- [Sunum Yol Haritası](#sunum-yol-haritası)
- [Demo Senaryoları](#demo-senaryoları)
- [Konuşma Notları](#konuşma-notları)
- [Teknik Sorular & Cevaplar](#teknik-sorular--cevaplar)
- [İş Soruları & Cevaplar](#i̇ş-soruları--cevaplar)
- [Canlı Demo Kontrol Listesi](#canlı-demo-kontrol-listesi)
- [Demo Sırasında Sorun Giderme](#demo-sırasında-sorun-giderme)

---

## 🗺️ Sunum Yol Haritası

### Önerilen Yapı (30-45 dakika)

#### 1. Giriş (3 dakika)
- **Proje Özeti**
  - "Sinema Yönetim Sistemi - Sinema operasyonları için kapsamlı bir çözüm"
  - ".NET 9 ve React ile geliştirildi, gerçek zamanlı koltuk rezervasyonu ve dinamik fiyatlandırma içeriyor"

- **Önemli İstatistikler**
  - Clean Architecture ile full-stack uygulama
  - 3 kullanıcı rolü: Admin, Gişe Görevlisi, Web Kullanıcıları
  - Hold mekanizması ile gerçek zamanlı koltuk rezervasyonu
  - 8+ farklı politikaya dayalı dinamik fiyatlandırma
  - Kapsamlı raporlama sistemi

#### 2. Mimari & Teknoloji (5 dakika)
- **Teknoloji Stack**
  - Backend: .NET 9, Entity Framework Core, SQL Server
  - Frontend: React 18, TypeScript, Redux Toolkit, Tailwind CSS
  - Desenler: Clean Architecture, CQRS (MediatR ile)
  - Güvenlik: JWT (refresh token ile), rol tabanlı yetkilendirme

- **Sistem Mimarisi Diyagramı**
  ```
  Frontend (React) → REST API (.NET 9) → Veritabanı (SQL Server)
         ↓                  ↓
    Redux Store      Arka Plan İşleri
    ```

- **Önemli Mimari Kararlar**
  - Sürdürülebilirlik için Clean Architecture
  - Sorumlulukların ayrılması için CQRS
  - Veri erişimi için Repository pattern
  - Durumsuz kimlik doğrulama için JWT

#### 3. Özellik Gösterimi (20 dakika)

**En iyi etki için özellikleri bu sırayla gösterin:**

##### A. Admin Özellikleri (7 dakika)
1. **Kullanıcı Yönetimi & Onaylama** (2 dk)
   - Onay bekleyen üyeleri göster
   - Yeni bir üyeyi onayla
   - VIP statüsü ver
   - Onay iş akışını açıkla

2. **Film Yönetimi** (2 dk)
   - Yeni film oluştur
   - Film detaylarını göster (süre, tür, yaş sınırı)
   - Film bilgilerini güncelle

3. **Salon & Koltuk Düzeni** (2 dk)
   - Salon düzeni editörünü göster
   - Satır ve sütunlarla koltuk düzeni oluştur
   - Özel koltukları işaretle (VIP, engelli erişimi)
   - Düzeni aktive et

4. **Seans Yönetimi** (1 dk)
   - Yeni seans programı oluştur
   - Fiyatlandırma önizlemesini göster
   - T-60 kuralını açıkla (seans başlangıcından 60 dk önce rezervasyon yapılamaz)

##### B. Gişe Özellikleri (5 dakika)
1. **Hızlı Bilet Satışı** (3 dk)
   - Seans ara
   - Açılır listeden birden fazla koltuk seç
   - Bilet tiplerini seç (Tam, Öğrenci, Çocuk)
   - Dinamik fiyat hesaplamasını göster
   - Birden fazla ödeme yöntemiyle işlem yap
   - Biletleri yazdır

2. **Üye Arama** (2 dk)
   - Hızlı üye sorgulama
   - Üye geçmişini göster
   - VIP indirimleri uygula

##### C. Kullanıcı Özellikleri (Online Rezervasyon) (8 dakika)
1. **Kullanıcı Kayıt & Giriş** (2 dk)
   - Yeni kullanıcı kaydet
   - Onay bekliyor durumunu göster
   - Admin kullanıcıyı onayla
   - Kullanıcı başarıyla giriş yapar

2. **Film Görüntüleme & Bilet Alma** (4 dk)
   - Mevcut filmlere göz at
   - Seans seç
   - Listeden koltuk seç (gişe gibi)
   - Bilet tiplerini seç
   - Ödeme yöntemini seç
   - **Direkt satın alma** - tek tık!
   - Başarı bildirimini göster

3. **Kullanıcı Paneli** (2 dk)
   - Satın alma geçmişini görüntüle
   - Aktif rezervasyonları göster
   - Profil yönetimi

#### 4. Teknik Öne Çıkanlar (5 dakika)
- **Koltuk Hold Sistemi**
  - Geçici tutmalar çift rezervasyonu önler
  - Otomatik süre dolumu
  - İstemci token tabanlı takip

- **Dinamik Fiyatlandırma**
  - 8 farklı fiyatlandırma politikası
  - Hafta sonu/hafta içi oranları
  - Zamana dayalı fiyatlandırma (matine, akşam, gece)
  - Üye tipi indirimleri (VIP, Öğrenci)
  - Tatil günü fiyatlandırması

- **Arka Plan İşleri**
  - Otomatik hold süresi dolumu
  - Zamanlanmış raporlar
  - Veri temizleme

- **Güvenlik Özellikleri**
  - Refresh token ile JWT kimlik doğrulama
  - Rol tabanlı erişim kontrolü
  - Şifre hashleme
  - SQL injection koruması

#### 5. Raporlar & Analizler (3 dakika)
- Günlük/Haftalık/Aylık satış raporları
- Film performans metrikleri
- Salon kullanım istatistikleri
- Ödeme yöntemine göre gelir dağılımı
- Üye aktivite takibi

#### 6. Soru-Cevap (5-10 dakika)
- Aşağıdaki [S&C bölümlerine](#teknik-sorular--cevaplar) bakın

#### 7. Kapanış (2 dakika)
- Önemli başarıların özeti
- Gelecek geliştirmeler
- Dinleyicilere teşekkür

---

## 🎬 Demo Senaryoları

### Senaryo 1: Tam Kullanıcı Yolculuğu ("Wow" Demosu)

**Hikaye:** "Bir müşterinin kayıttan bilet satın almaya kadar olan yolculuğunu takip edelim"

1. **Başlangıç:** Kullanıcı film izlemek istiyor
2. **Kayıt:** Normal kullanıcı olarak hesap oluştur
3. **Onay:** Admin bildirim alır → kullanıcıyı onayla
4. **Giriş:** Kullanıcı başarıyla giriş yapar
5. **Gezinme:** Kullanıcı mevcut filmleri görür
6. **Seçim:** Kullanıcı bir film ve seans seçer
7. **Rezervasyon:** Kullanıcı koltukları, bilet tiplerini, ödeme yöntemini seçer
8. **Satın Alma:** Tek tıkla satın alma - tamam!
9. **Onay:** Kullanıcı biletleri panelinde görür

**Önemli Konuşma Noktaları:**
- "Akıcı onay iş akışına dikkat edin"
- "Dinamik fiyatlandırmanın bilet tipine göre nasıl hesaplandığını görün"
- "Tek tıkla satın alma - karmaşık ödeme akışı yok"
- "Gerçek zamanlı koltuk müsaitliği"

### Senaryo 2: Admin Güç Demosu

**Hikaye:** "Bir adminin sinemayı nasıl yönettiğini görelim"

1. **Film Yönetimi:** "Avatar 3" filmi oluştur
2. **Salon Kurulumu:** 100 koltuklu salon düzeni yapılandır
3. **Programlama:** Yarın için 5 seans planla
4. **Fiyat Önizleme:** Farklı saatler için farklı fiyatları göster
5. **Kullanıcı Yönetimi:** 2 bekleyen üyeyi onayla, birine VIP ver
6. **Raporlar:** Bugünün satış raporunu oluştur

**Önemli Konuşma Noktaları:**
- "Adminler tam kontrole sahip"
- "Görsel salon editörü düzeni kolaylaştırır"
- "Fiyatlandırma otomatik hesaplanır"
- "Gerçek zamanlı analizler"

### Senaryo 3: Gişede Yoğun Saatler

**Hikaye:** "Cumartesi akşamı, müşteriler gişede"

1. **Hızlı Satış:** Müşteri bir sonraki seans için 4 bilet istiyor
2. **Üye Sorgulama:** Müşterinin üyelik kartı var
3. **Koltuk Seçimi:** Hızlı açılır liste seçimi
4. **Karışık Biletler:** 2 Tam, 1 Öğrenci, 1 Çocuk
5. **Ödeme:** Kredi kartı ödemesini işle
6. **Yazdır:** Biletler saniyeler içinde hazır

**Önemli Konuşma Noktaları:**
- "Gişe arayüzü hız için optimize edilmiş"
- "Koltuklara tıklama yok - hızlı liste seçimi"
- "Otomatik VIP indirimi uygulandı"
- "Birden fazla ödeme yöntemi"

### Senaryo 4: Teknik Derinlemesine İnceleme

**Hikaye:** "Kaputun altına bakalım"

1. **Mimariyi Göster:** Clean Architecture katmanlarını açıkla
2. **API Endpoint'leri:** Swagger'ı aç, REST API'yi göster
3. **Veritabanı:** EF Core migration'larını göster
4. **Kimlik Doğrulama:** Refresh token'lı JWT akışını açıkla
5. **State Yönetimi:** Redux DevTools'u göster
6. **Gerçek Zamanlı Güncellemeler:** Koltuk hold süre dolumunu göster

**Önemli Konuşma Noktaları:**
- "Clean Architecture sürdürülebilirliği garanti eder"
- "CQRS pattern okuma ve yazmayı ayırır"
- "Entity Framework tüm veritabanı işlemlerini yönetir"
- "JWT durumsuz kimlik doğrulamayı sağlar"

---

## 💡 Konuşma Notları

### Açılış İfadeleri

**Güçlü Açılış:**
> "Bugün size bilet satışından üye yönetimine kadar her şeyi kapsayan kapsamlı bir Sinema Yönetim Sistemi sunacağım. Bu sadece bir rezervasyon sistemi değil - dinamik fiyatlandırma, gerçek zamanlı koltuk rezervasyonları ve detaylı analizler içeren tam bir sinema operasyon platformu."

**Problem İfadesi:**
> "Sinema operatörleri karmaşık fiyatlandırma politikalarını yönetme, çift rezervasyonları önleme, online ve gişe satışları arasında koordinasyon sağlama ve iş performansını takip etme konularında zorluklar yaşıyor. Bu sistem tüm bu sorunları tek bir entegre çözümde hallediyor."

### Anahtar Değer Önerileri

1. **Sinema Sahipleri İçin:**
   - Dinamik fiyatlandırma ile geliri artırın
   - Otomasyon ile operasyonel maliyetleri azaltın
   - Kapsamlı raporlarla içgörü kazanın
   - Birden fazla salona kolayca ölçeklendirin

2. **Gişe Personeli İçin:**
   - Hızlı bilet satış arayüzü
   - Eğitim gerektirmez - sezgisel tasarım
   - Yoğun saatleri verimli şekilde yönetin
   - Hızlı üye sorgulama

3. **Müşteriler İçin:**
   - İstediğiniz zaman, istediğiniz yerden online rezervasyon
   - İstedikleri koltuğu tam olarak seçme
   - Şeffaf fiyatlandırma
   - Anında onay

---

## ❓ Teknik Sorular & Cevaplar

### Mimari & Tasarım

**S: Neden Clean Architecture seçtiniz?**
> **C:** Clean Architecture net sorumluluk ayrımı sağlar ve sistemi oldukça sürdürülebilir ve test edilebilir yapar. Domain ve Application katmanlarındaki iş mantığı framework'lerden, veritabanlarından ve UI'dan bağımsızdır. Bu şu anlama gelir:
> - İş kurallarını kolayca unit test edebilirsiniz
> - İş mantığını etkilemeden veritabanını değiştirebilirsiniz
> - Backend'e dokunmadan React'i başka bir framework ile değiştirebilirsiniz
> - Yeni ekip üyeleri yapıyı hızlıca anlayabilir

**S: CQRS nedir ve neden kullanıyorsunuz?**
> **C:** CQRS (Command Query Responsibility Segregation) veriyi değiştiren operasyonları (Command'lar) veriyi okuyan operasyonlardan (Query'ler) ayırır. Faydaları:
> - Query'ler command'lardan ayrı optimize edilebilir
> - Command'lar iş kurallarını uygular
> - Ölçeklendirmesi daha kolay - okuma ve yazma bağımsız ölçeklenebilir
> - Net sorumluluk - her handler bir şey yapar
> - Örnek: `CreateScreeningCommand` vs `GetScreeningQuery`

**S: Koltuk rezervasyonunda yarış durumlarını (race conditions) nasıl önlüyorsunuz?**
> **C:** Veritabanı transaction'ları ile koltuk hold sistemi kullanıyoruz:
> 1. Kullanıcı koltuk seçer → İstemci token ile geçici "hold"lar oluşturur
> 2. Hold'lar koltuk başına benzersiz (veritabanı kısıtı)
> 3. Sadece koltuğu hold eden istemci rezervasyon yapabilir
> 4. Hold'lar 10 dakika sonra sona erer (arka plan işi)
> 5. Rezervasyonlar transaction'larda atomik operasyonlardır
> Bu, iki müşterinin aynı koltuğu rezerve edememesini garantiler.

**S: Fiyatlandırma sisteminiz nasıl çalışıyor?**
> **C:** Üst üste gelen 8 dinamik fiyatlandırma politikamız var:
> - **Temel fiyat** bilet tipi başına (Tam/Öğrenci/Çocuk)
> - **Gün politikası**: Hafta sonları daha pahalı
> - **Zaman politikası**: Matine < Akşam < Gece
> - **Üye politikası**: VIP üyeler indirim alır
> - **Tatil politikası**: Tatillerde özel fiyatlar
> - **Film politikası**: Gişe rekorları kıran filmler prim alabilir
> - Strategy pattern kullanılarak gerçek zamanlı hesaplanır
> - Örnek: Hafta sonu + Akşam + VIP = %20 artış - %10 VIP indirimi

### Backend Teknolojisi

**S: Neden .NET 9, .NET 6 veya 8 değil?**
> **C:** .NET 9 şunları sağlar:
> - En son performans iyileştirmeleri (daha hızlı JSON serileştirme)
> - Yeni C# 13 özellikleri
> - Daha iyi minimal API desteği
> - Uzun vadeli destek
> - Kurumsal uygulamalar için endüstri standardı

**S: Kimlik doğrulamayı nasıl yönetiyorsunuz?**
> **C:** Refresh token'lı JWT (JSON Web Token):
> 1. Kullanıcı giriş yapar → Access token (15 dk) + refresh token (7 gün) alır
> 2. Her istekle access token gönderilir
> 3. Access token süresi dolduğunda → Yeni access token almak için refresh token kullanılır
> 4. Refresh token'lar veritabanında saklanır, iptal edilebilir
> 5. Şifreler ASP.NET Core Identity ile hashlenmiş
> 6. JWT payload'ında rol tabanlı claim'ler

**S: Güvenlik konusunda ne düşündünüz?**
> **C:** Çoklu katmanlar:
> - **Kimlik Doğrulama**: Refresh token'lı JWT
> - **Yetkilendirme**: Rol tabanlı erişim kontrolü (RBAC)
> - **Şifre Güvenliği**: Hashleme ile ASP.NET Core Identity
> - **SQL Injection**: Entity Framework parametreli sorgular kullanır
> - **CORS**: Sadece belirli kaynaklara izin verecek şekilde yapılandırılmış
> - **HTTPS**: Production'da zorunlu
> - **Input Validasyonu**: Tüm command'lar için FluentValidation
> - **Rate Limiting**: Kolayca eklenebilir

**S: Veritabanı migration'larını nasıl yönetiyorsunuz?**
> **C:** Entity Framework Core migration'ları:
> - Migration'lar versiyon kontrolünde (git'te)
> - `dotnet ef migrations add` migration oluşturur
> - `dotnet ef database update` veritabanına uygular
> - Production için SQL script'leri oluşturulabilir
> - Gerekirse migration'lar geri alınabilir
> - Veritabanı şeması kod ile birlikte gelişir

### Frontend Teknolojisi

**S: Neden React, Angular veya Vue değil?**
> **C:** React şunları sağlar:
> - Geniş ekosistem ve topluluk
> - Component yeniden kullanılabilirliği
> - Performans için Virtual DOM
> - Temiz state yönetimi için Hook'lar
> - TypeScript desteği
> - Öngörülebilir state için Redux Toolkit
> - Vite ile hızlı geliştirme

**S: Neden Redux Toolkit?**
> **C:** Redux Toolkit state yönetimini basitleştirir:
> - Merkezi uygulama state'i
> - Öngörülebilir state güncellemeleri
> - Time-travel debugging (Redux DevTools)
> - createAsyncThunk ile kolay async operasyonlar
> - Sade Redux'a göre daha az boilerplate
> - Mükemmel TypeScript entegrasyonu

**S: API çağrılarını nasıl yönetiyorsunuz?**
> **C:** Axios ile merkezi API servisi:
> - Tek doğruluk kaynağı: `services/api.ts`
> - Otomatik JWT token enjeksiyonu
> - 401'de otomatik token yenileme
> - Hata yönetimi ve yeniden deneme mantığı
> - Tüm yanıtlar için TypeScript tipleri
> - Test için kolayca mock'lanabilir

### Performans & Ölçeklenebilirlik

**S: Bu nasıl ölçeklenir?**
> **C:** Çoklu stratejiler:
> - **Durumsuz API**: Load balancer arkasına daha fazla API sunucusu eklenebilir
> - **Veritabanı**: SQL Server clustering ve replication destekler
> - **Caching**: Sık erişilen veriler için Redis eklenebilir
> - **CDN**: Statik frontend varlıkları CDN'den servis edilir
> - **Arka Plan İşleri**: Quartz.NET zamanlanmış görevleri yönetir
> - **Async/Await**: Her yerde non-blocking I/O

**S: Performans nasıl?**
> **C:** Uygulanan optimizasyonlar:
> - **Veritabanı**: Sık sorgulanan sütunlarda uygun indexler
> - **EF Core**: N+1 sorgularını önlemek için Include/Projection
> - **Frontend**: React.lazy ile code splitting
> - **API**: Uygun yerlerde response caching
> - **Arka Plan İşleri**: Ağır operasyonlar asenkron çalışır
> - Gerektiğinde Redis caching eklenebilir

**S: Kaç eş zamanlı kullanıcıyı kaldırabilir?**
> **C:** Altyapıya bağlı, ancak mimari şunu destekler:
> - Mütevazı donanımda yüzlerce eş zamanlı kullanıcı
> - Uygun ölçeklendirme ile binlerce (load balancer, çoklu sunucu)
> - Veritabanı connection pooling darboğazları önler
> - Koltuk hold sistemi yük altında bile yarış durumlarını önler
> - JMeter veya k6 gibi araçlarla load test edilebilir

---

## 💼 İş Soruları & Cevaplar

**S: Bu proje ne kadar sürdü?**
> **C:** Proje Clean Architecture ile full-stack yetenekleri gösteriyor. Geliştirme şunları içerdi:
> - Mimari tasarım ve planlama
> - .NET 9 ile backend geliştirme
> - React ile frontend geliştirme
> - Veritabanı tasarımı ve migration'lar
> - Test ve hata düzeltme
> - Dokümantasyon (kapsamlı kurulum kılavuzları)
> Bu kurumsal seviye geliştirme becerilerini sergiliyor.

**S: Bu gerçek sinemalar tarafından kullanılabilir mi?**
> **C:** Kesinlikle! Sistem şunları içeriyor:
> - Tüm temel sinema operasyonları (bilet, seans, salonlar)
> - Gerçek dünya iş kuralları (T-60 kuralı, dinamik fiyatlandırma)
> - Çoklu kullanıcı rolleri ve izinleri
> - Raporlama ve analizler
> - Güvenlik ve kimlik doğrulama
> - Production deployment kılavuzu
> - Spesifik sinema ihtiyaçları için özelleştirilebilir

**S: Bundan sonra ne eklerdiniz?**
> **C:** Potansiyel geliştirmeler:
> - **Ödeme Entegrasyonu**: Gerçek ödemeler için Stripe, PayPal
> - **E-posta Bildirimleri**: Rezervasyon onayları, hatırlatmalar
> - **SMS Entegrasyonu**: SMS ile bilet kodları
> - **Mobil Uygulama**: iOS/Android için React Native
> - **Sadakat Programı**: Sık müşteriler için puan sistemi
> - **Sosyal Özellikler**: Yorumlar, değerlendirmeler, öneriler
> - **Çoklu Sinema**: Sinema zincirleri için destek
> - **Gelişmiş Analizler**: AI tabanlı tahminler, doluluk tahmini
> - **Koltuk Önerileri**: En iyi müsait koltukları önerin
> - **Grup Rezervasyonları**: Büyük gruplar için özel işleme

**S: Birden fazla sinemayı nasıl yönetirsiniz?**
> **C:** Mevcut tasarım genişletilebilir:
> - Konum bilgisi ile `Cinema` entity'si ekle
> - Salonlar sinemalara ait olur
> - Kullanıcılar sinema konumu seçebilir
> - Raporlar sinemaya göre filtrelenebilir
> - Mimari zaten bunu destekliyor - sadece entity'ler ekle

**S: İade ve iptaller konusunda ne düşündünüz?**
> **C:** İmplemente edilebilir:
> - İptal politikası ekle (örn. seansdan 2 saat önce)
> - Orijinal ödeme yöntemini sakla
> - Ödeme gateway'i üzerinden iade işle
> - Bileti iptal edildi olarak işaretle (soft delete)
> - Koltukları envantere geri ver
> - Bu özellik için tüm altyapı yerinde

**S: Yoğun zamanları (gala geceleri) nasıl yönetirsiniz?**
> **C:** Sistem bunun için tasarlandı:
> - Koltuk hold sistemi çakışmaları önler
> - Veritabanı transaction'ları tutarlılığı sağlar
> - Arka plan işleri süresi dolan hold'ları temizler
> - Yatay olarak ölçeklenebilir (daha fazla sunucu ekle)
> - Gerekirse kuyruk sistemi eklenebilir
> - Redis popüler verileri cache'leyebilir (popüler seanslar)

---

## ✅ Canlı Demo Kontrol Listesi

### Sunum Öncesi

**24 Saat Önce:**
- [ ] Tam kullanıcı akışını uçtan uca test et
- [ ] Tüm özelliklerin çalıştığını doğrula
- [ ] Veritabanında örnek veri olduğunu kontrol et
- [ ] İyi çeşitlilikte filmler ve seanslar olsun
- [ ] Test hesapları oluştur (şifreleri bil!)
- [ ] Eski test verilerini temizle
- [ ] Backend ve frontend'in hatasız çalıştığını kontrol et
- [ ] Sunum yapılacak bilgisayar/ağda test et
- [ ] Yedek plan hazırla (ekran görüntüleri, video)

**1 Saat Önce:**
- [ ] Backend'i başlat (çalıştığını doğrula)
- [ ] Frontend'i başlat (erişilebilir olduğunu doğrula)
- [ ] Gereksiz tarayıcı sekmelerini kapat
- [ ] Tarayıcı geçmişini/cache'i temizle
- [ ] Tüm hesaplardan çıkış yap
- [ ] İlk girişi hazırla (admin@cinema.local / Admin*123)
- [ ] Sunum notlarını aç
- [ ] İnternet bağlantısını test et
- [ ] Dikkat dağıtan uygulamaları kapat
- [ ] "Rahatsız Etmeyin" modunu aç

**Hemen Önce:**
- [ ] Backend http://localhost:5238 üzerinde çalışıyor
- [ ] Frontend http://localhost:5173 üzerinde çalışıyor
- [ ] Tarayıcı http://localhost:5173 adresinde
- [ ] Backend logları görünür (isteğe bağlı)
- [ ] Redux DevTools açık (isteğe bağlı)
- [ ] Rahat seviyeye zoom yap (Ctrl+Plus)
- [ ] Yanında su olsun 💧

### Sunum Sırasında

**Açılış (İlk Yapılacaklar):**
1. Giriş sayfasını göster
2. "Bu Sinema Yönetim Sistemi"
3. En etkileyici demo ile başla (tam kullanıcı yolculuğu)

**Demo Akış İpuçları:**
- 🗣️ **Yazarken konuş** - Ne yaptığını açıkla
- ⏸️ **Etki için duraklama** - Sonucu görmelerini sağla
- 👉 **İşaret et ve açıkla** - "Fiyatın nasıl otomatik hesaplandığına dikkat edin"
- 🔄 **Bir şey başarısız olursa** - Sakin kal, yedek plan var
- ❓ **Dinleyicileri dahil et** - "Bu özellik hakkında sorusu olan var mı?"

---

## 🚨 Demo Sırasında Sorun Giderme

### Yaygın Sorunlar & Hızlı Çözümler

**Sorun: Backend yanıt vermiyor**
```
Hızlı Çözüm: Backend'in çalışıp çalışmadığını kontrol et
→ Görev Yöneticisi'ni aç → "Sinema.Api.exe" ara
→ Çalışmıyorsa: Terminal aç, cd backend/src/Sinema.Api, dotnet run
→ Başlaması için 10 saniye bekle
```

**Sorun: Frontend yüklenmiyor**
```
Hızlı Çözüm: Frontend'in çalışıp çalışmadığını kontrol et
→ Terminal'e bak - "Local: http://localhost:5173" yazmalı
→ Yoksa: cd frontend, npm run dev
→ Tarayıcıyı hard refresh: Ctrl+Shift+R
```

**Sorun: Giriş yapamıyorum**
```
Hızlı Çözüm: Kimlik bilgileri yanlış olabilir
→ Admin: admin@cinema.local / Admin*123
→ Gişe: gise@cinema.local / Gise*123
→ Caps Lock kapalı mı kontrol et
```

**Sorun: "Network Error" veya CORS hatası**
```
Hızlı Çözüm: Backend çalışmıyor veya yanlış port
→ Backend'in http://localhost:5238 üzerinde olduğunu kontrol et
→ Frontend api.ts dosyasında doğru URL var mı kontrol et
→ Gerekirse backend'i yeniden başlat
```

**Sorun: Veritabanı hatası**
```
Hızlı Çözüm: Veritabanı kilitli veya bağlantı kesilmiş olabilir
→ SQL Server Management Studio açıksa kapat
→ Backend'i yeniden başlat
→ En kötü senaryo: Yedek ekran görüntüleri/video göster
```

**Sorun: Özellik beklendiği gibi çalışmıyor**
```
Hızlı Çözüm: Sakin ve profesyonel kal
→ "Size bunun yerine şu özelliği göstereyim"
→ "Bunu sunumdan sonra gösterebilirim"
→ Bir sonraki demo maddesine sorunsuz geç
→ Asla panik yapmayın veya aşırı özür dilemeyin
```

### Yedek Plan

**Canlı Demo Tamamen Başarısız Olursa:**
1. 📸 **Ekran Görüntüleri**: Tüm önemli özelliklerin ekran görüntüleri
2. 🎥 **Ekran Kaydı**: Önceden kaydedilmiş demo videosu
3. 📊 **Sunum Slaytları**: Özellik açıklamalı yedek slaytlar
4. 💻 **Kod İncelemesi**: Çalıştırmak yerine kodu göster

---

## 🎯 Başarı Metrikleri

### Harika Bir Sunum Ne Yapar

**Teknik Değerlendirme:**
- ✅ Temiz kod mimarisini gösterir
- ✅ Tasarım desenlerini anlamayı gösterir
- ✅ Teknik kararları iyi açıklar
- ✅ Sorulara kendinden emin cevap verir
- ✅ Canlı demo sorunsuz çalışır

**İş Değerlendirmesi:**
- ✅ Gerçek iş problemlerini çözer
- ✅ Net değer önermesi
- ✅ Profesyonel sunum
- ✅ Soruları öngörür
- ✅ İş anlayışı gösterir

**Sunum Değerlendirmesi:**
- ✅ Açık ve kendinden emin anlatım
- ✅ İyi hızlama
- ✅ Dinleyiciyi dahil eder
- ✅ Sorunları zarif şekilde yönetir
- ✅ Profesyonel tavır

---

## 🎬 Son İpuçları

### Yapılacaklar ✅
- ✅ Demoyu birden fazla kez pratik yap
- ✅ Açık ve kendinden emin konuş
- ✅ Dinleyicilerle göz teması kur
- ✅ SADECE NE değil NEDEN'i açıkla
- ✅ Projen için coşku göster
- ✅ Sınırlamalar hakkında dürüst ol
- ✅ Yedek planlar hazır olsun

### Yapılmayacaklar ❌
- ❌ Demo'da acele etme
- ❌ Küçük sorunlar için özür dileme
- ❌ Çok fazla jargon kullanma
- ❌ Sürekli notlardan oku
- ❌ Dinleyici sorularını yok say
- ❌ Yarım kalmış özellikleri göster
- ❌ Bir şey bozulursa panikle

### Başarının Sırrı
> **Güven + Hazırlık + Coşku = Harika Sunum**

---

**Sunumunuzda başarılar! 🌟**

Unutmayın: Bu sistemi siz yaptınız. Onu salondan daha iyi kimse bilmiyor. Yarattığınızı kendinden emin bir şekilde gösterin!

---

**Son Güncelleme:** 19 Ekim 2025

