# ⚡ FastRDP - Hızlı Başlangıç Rehberi

Bu rehber, FastRDP'yi 5 dakikada kullanmaya başlamanız için hazırlanmıştır.

## 🎯 3 Adımda Başlayın

### 1️⃣ Projeyi Açın

```bash
# Repository'i klonlayın
git clone https://github.com/mcicekci/Fast.RDP.git
cd Fast.RDP

# Visual Studio ile açın
start FastRDP.sln
```

### 2️⃣ Derleyin ve Çalıştırın

Visual Studio'da:
- `F5` tuşuna basın veya
- Üstteki yeşil ▶️ butonuna tıklayın

Komut satırından:
```bash
dotnet build
dotnet run
```

### 3️⃣ İlk Profilinizi Oluşturun

1. Sol panelde **"Yeni Profil"** butonuna tıklayın
2. Formu doldurun:
   - **Bağlantı Adı**: Test Sunucusu
   - **Host**: 192.168.1.100
3. **"Kaydet"** butonuna tıklayın
4. Profili çift tıklayarak bağlanın! 🎉

## 📋 Temel Kullanım

### Profil Yönetimi

```
➕ Yeni Profil Oluştur
├─ Sol panelde "Yeni Profil" butonuna tıkla
└─ Formu doldur ve kaydet

✏️ Profil Düzenle
├─ Profili seç
├─ Sağ tıkla → "Düzenle"
└─ Değişiklikleri kaydet

❌ Profil Sil
├─ Profili seç
├─ Sağ tıkla → "Sil"
└─ Onayla

⭐ Favorilere Ekle
├─ Profili seç
└─ Sağ tıkla → "Favorilere Ekle"
```

### Hızlı Bağlantı

**Yöntem 1**: Çift tık
```
Liste → Profili çift tıkla → Bağlan
```

**Yöntem 2**: Sağ panel
```
Profil seç → Sağ panel → "Bağlan" butonu
```

**Yöntem 3**: Context menu
```
Sağ tık → "Bağlan"
```

### Arama ve Filtreleme

**Arama:**
```
Üst bar → Arama kutusu → İsim/host/etiket yaz
```

**Filtreleme:**
```
Sol panel → Filtre seç:
├─ Tümü
├─ Favoriler
└─ Son Kullanılanlar
```

## 🎨 Tema Değiştirme

```
Üst sağ → 🌙 ikonu → Tıkla
```

## 🔑 Klavye Kısayolları

| Kısayol | Açıklama |
|---------|----------|
| `Ctrl + N` | Yeni profil |
| `Ctrl + F` | Arama |
| `Enter` | Bağlan |
| `Delete` | Sil |
| `F5` | Yenile |

## 📂 Dosya Konumları

```
FastRDP/
└── Data/
    ├── profiles/           ← RDP dosyaları burada
    ├── settings.json       ← Ayarlar
    └── profiles.json       ← Profil metadata
```

## 🚀 Gelişmiş Özellikler

### RDP Dosyalarını Import Etme

**Yöntem 1**: Manuel kopyalama
```bash
# Mevcut RDP dosyalarınızı kopyalayın
copy *.rdp FastRDP\Data\profiles\

# Uygulamada "Yenile" butonuna basın
```

**Yöntem 2**: Drag & Drop (Gelecek sürümde)
```
RDP dosyasını uygulamaya sürükle bırak
```

### Yedekleme

```
Sol panel → "Yedekle" butonu → Otomatik yedek oluşturulur
```

Yedek konumu:
```
Data/profiles_backup_YYYYMMDD_HHMMSS.json
```

## 🐛 Sorun Giderme

### Sorun: Uygulama açılmıyor

**Çözüm:**
```bash
# .NET 8.0 yüklü mü kontrol edin
dotnet --version

# NuGet paketlerini restore edin
dotnet restore

# Temiz build
dotnet clean
dotnet build
```

### Sorun: RDP bağlantısı açılmıyor

**Çözüm:**
1. Windows'ta mstsc.exe çalışıyor mu kontrol edin
2. Host adresinin doğru olduğunu kontrol edin
3. Firewall ayarlarını kontrol edin

### Sorun: Profiller görünmüyor

**Çözüm:**
```bash
# Data klasörünü kontrol edin
dir Data\profiles

# Varsa profiles.json'ı silin
del Data\profiles.json

# Yeniden tarayın
# Uygulamada F5 (Yenile)
```

## 💡 İpuçları

### 1. Etiketleme Stratejisi

```
🏢 İş
  ├─ ERP
  ├─ Veritabanı
  └─ Web Server

🏠 Ev
  ├─ NAS
  └─ Medya

☁️ Bulut
  ├─ Azure
  └─ AWS
```

### 2. İsimlendirme Kuralları

```
✅ İYİ:
- "ERP Sunucusu - Üretim"
- "Web01 - Test Ortamı"
- "Database - Yedek"

❌ KÖTÜ:
- "server1"
- "test"
- "192.168.1.100"
```

### 3. Çözünürlük Ayarları

```
Fullscreen    → Tam ekran çalışma
1920x1080     → Pencere modunda
Auto          → Otomatik boyutlandırma (önerilen)
```

## 📊 Örnek Profil

```json
{
  "name": "ERP Ana Sunucu",
  "host": "erp.company.local",
  "username": "admin",
  "domain": "COMPANY",
  "resolution": "1920x1080",
  "tags": ["ERP", "Üretim", "Kritik"],
  "notes": "7/24 çalışır durumda olmalı",
  "favorite": true
}
```

## 🎓 Video Eğitimler

*(Gelecekte eklenecek)*

- [ ] Temel kullanım
- [ ] Profil yönetimi
- [ ] İleri özellikler
- [ ] Sorun giderme

## 📞 Yardım

Sorunlarla karşılaşırsanız:

1. **Dokümantasyon**: [README.md](README.md)
2. **Mimari**: [ARCHITECTURE.md](ARCHITECTURE.md)
3. **Derleme**: [BUILDING.md](BUILDING.md)
4. **Issues**: [GitHub Issues](https://github.com/mcicekci/Fast.RDP/issues)

## 🎉 Tebrikler!

FastRDP'yi kullanmaya başladınız! Daha fazla özellik için tam dokümantasyonu okuyun.

---

**İyi çalışmalar! 🚀**

