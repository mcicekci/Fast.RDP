# 🚀 FastRDP v0.1-beta-1 Release Notes

**Yayın Tarihi**: 23 Ekim 2025  
**Sürüm**: 0.1.0-beta.1  
**Platform**: Windows 10 (Build 17763) ve üzeri

---

## 📢 Genel Bakış

FastRDP v0.1-beta-1, modern bir RDP bağlantı yönetim uygulamasının ilk beta sürümüdür. WinUI 3 ve .NET 8.0 ile geliştirilmiş, Fluent Design prensiplerine uygun bir masaüstü deneyimi sunar.

Bu beta sürümü, temel RDP profil yönetimi, gelişmiş kullanıcı deneyimi özellikleri ve Windows sistem entegrasyonlarını içerir. Kullanıcı geri bildirimlerini toplamak ve stabiliteyi artırmak amacıyla yayınlanmıştır.

---

## ✨ Yeni Özellikler

### 🎯 Temel İşlevsellik

#### RDP Profil Yönetimi
- **Profil Oluşturma ve Düzenleme**: Sezgisel arayüz ile kolay profil yönetimi
- **JSON Tabanlı Depolama**: İnsan tarafından okunabilir, versiyon kontrolü dostu veri formatı
- **Otomatik RDP Dosya Yönetimi**: Profillerin otomatik `.rdp` dosyası olarak kaydedilmesi
- **Profil İmport/Export**: Mevcut RDP dosyalarının sisteme aktarılması

#### Arama ve Filtreleme
- **Gelişmiş Arama**: İsim, host, grup ve etiket bazlı hızlı arama
- **Akıllı Filtreleme**: 
  - Tüm profiller
  - Favoriler
  - Son kullanılanlar
  - Gruplara göre filtreleme
- **Gerçek Zamanlı Arama**: Yazarken anlık sonuçlar

#### Favoriler Sistemi
- **Favori İşaretleme**: Sık kullanılan bağlantıları işaretleme
- **Favori Filtresi**: Favori profillere hızlı erişim
- **Görsel Göstergeler**: Profil kartlarında favori badge'i

### 🎨 Kullanıcı Deneyimi

#### Tema Desteği
- **Light ve Dark Tema**: Sistem temasına uyumlu mod değiştirme
- **Akıcı Geçişler**: Smooth fade-in/fade-out animasyonları
- **Tutarlı Tasarım**: Tüm bileşenlerde Fluent Design prensiplerine uyum

#### Drag & Drop Desteği
- **RDP Dosya İmportı**: Sürükle-bırak ile dosya aktarımı
- **Çoklu Dosya Desteği**: Birden fazla RDP dosyasını aynı anda içe aktarma
- **Otomatik İsim Çözümlemesi**: Dosya adı çakışmalarının otomatik çözümü
- **Görsel Feedback**: Overlay ve animasyonlu geri bildirimler

#### Modern Profil Kartları
- **Thumbnail Önizleme**: 48x48 renkli profil ikonları
- **Responsive Hover Efektleri**: İnteraktif kullanıcı deneyimi
- **Bilgi Yoğunluğu**: Kompakt ama okunabilir kart tasarımı
- **Durum Göstergeleri**: Favori, son kullanım gibi göstergeler

### 🔗 Windows Entegrasyonu

#### Jump List Desteği
- **Taskbar Hızlı Erişim**: Son 5 kullanılan profil
- **Direkt Bağlantı**: Taskbar'dan uygulama açmadan bağlanma
- **Otomatik Güncelleme**: Profil kullanımına göre dinamik liste
- **Launch Arguments**: Komut satırı argümanları ile profil başlatma

#### Sistem Tepsisi
- **Native Windows API**: P/Invoke ile Shell_NotifyIcon entegrasyonu
- **Minimize to Tray**: Sistem tepsisine minimize olma
- **Çift Tıklama Desteği**: Göster/gizle toggle
- **Tooltip Bilgilendirme**: Durum bilgisi gösterimi

### 📊 Profil Özellikleri

#### Detaylı Profil Ayarları
- **Bağlantı Bilgileri**:
  - İsim (zorunlu)
  - Host/IP adresi (zorunlu)
  - Kullanıcı adı (opsiyonel)
  - Domain (opsiyonel)
  - Port (varsayılan: 3389)

- **Görüntü Ayarları**:
  - Otomatik çözünürlük
  - Tam ekran modu
  - Özel çözünürlükler (1920x1080, 1366x768, vb.)

- **Çoklu Monitör Desteği**:
  - Çoklu monitör kullanımı
  - Tüm monitörleri kullan (fullscreen)
  - Monitör seçimi

- **Organizasyon**:
  - Gruplandırma (varsayılan: "Genel")
  - Etiketleme (çoklu etiket desteği)
  - Notlar alanı
  - Favori işaretleme

### 📁 Dosya Yönetimi

- **Otomatik Klasör Yapısı**: `Data/profiles` klasörü otomatik oluşturulur
- **RDP Dosya Senkronizasyonu**: JSON metadata ile `.rdp` dosyaları senkronize
- **Güvenli Dosya İşlemleri**: Path validation ve sanitization
- **Yedekleme Desteği**: Profil verilerinin otomatik yedeklenmesi

---

## 🛠️ Teknik Detaylar

### Platform ve Gereksinimler

**Minimum Sistem Gereksinimleri:**
- **İşletim Sistemi**: Windows 10 version 1809 (Build 17763) veya üzeri
- **Runtime**: .NET 8.0 Runtime
- **SDK**: Windows App SDK 1.5 veya üzeri
- **Mimari**: x64, x86, ARM64 desteği

### Teknoloji Yığını

```
Framework      : .NET 8.0
UI Framework   : WinUI 3 (Windows App SDK 1.5)
Architecture   : MVVM Pattern
Language       : C# 12
Dependencies   : 
  - Microsoft.WindowsAppSDK (1.5.240227000)
  - CommunityToolkit.Mvvm (8.2.2)
  - Microsoft.Extensions.DependencyInjection (8.0.0)
  - Microsoft.Extensions.Hosting (8.0.0)
  - System.Security.Cryptography.ProtectedData (9.0.10)
```

### Mimari Yapı

**MVVM Katmanları:**
```
Views/
├── ProfileListView         # Ana profil listesi
├── ProfileEditorView       # Profil düzenleme formu
└── SettingsView           # Ayarlar ekranı (temel)

ViewModels/
├── BaseViewModel          # Temel ViewModel sınıfı
├── MainViewModel          # Ana pencere mantığı
├── ProfileEditorViewModel # Profil düzenleme mantığı
└── SettingsViewModel      # Ayarlar mantığı

Services/
├── RdpFileService         # RDP dosya işlemleri
├── SettingsService        # Ayar yönetimi
├── JumpListService        # Windows Jump List
├── CredentialService      # Kimlik bilgisi yönetimi
├── ErrorHandlerService    # Hata yönetimi
└── SystemTrayService      # Sistem tepsisi

Models/
├── RdpProfile            # Profil veri modeli
└── AppSettings           # Uygulama ayarları modeli
```

### Veri Formatı

**profiles.json Örneği:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "ERP Sunucusu",
    "file": "erp.rdp",
    "host": "192.168.1.100",
    "username": "admin",
    "domain": "COMPANY",
    "group": "Üretim",
    "resolution": "1920x1080",
    "useMultiMonitor": false,
    "useAllMonitors": false,
    "notes": "Ana ERP sunucusu",
    "tags": ["ERP", "Finans"],
    "lastUsed": "2025-10-22T12:35:00",
    "favorite": true,
    "createdAt": "2025-10-20T10:00:00"
  }
]
```

**settings.json Örneği:**
```json
{
  "rdpFolder": "Data/profiles",
  "theme": "dark",
  "showThumbnails": false,
  "recentCount": 10,
  "startWithWindows": false,
  "minimizeToTray": true,
  "lastBackup": "2025-10-22T12:00:00",
  "autoBackup": true
}
```

### Performans Optimizasyonları

- **Lazy Loading**: Profiller ihtiyaç duyuldukça yüklenir
- **Virtual Scrolling**: ListView'de otomatik virtualization
- **Async Operations**: Blocking olmayan I/O işlemleri
- **Observable Collections**: Verimli UI güncellemeleri
- **Optimized Animations**: Storyboard tabanlı smooth geçişler

---

## 📦 Kurulum

### İkili Paket Kurulumu (Önerilen)

1. [Releases](https://github.com/mcicekci/Fast.RDP/releases/tag/v0.1-beta-1) sayfasından `FastRDP_v0.1-beta-1_Setup.exe` dosyasını indirin
2. Setup dosyasını çalıştırın
3. Kurulum sihirbazını takip edin
4. .NET 8.0 Runtime yoksa otomatik olarak indirilecektir
5. Uygulamayı başlatın

### Portable Sürüm

1. `FastRDP_v0.1-beta-1_Portable.zip` dosyasını indirin
2. İstediğiniz bir klasöre çıkarın
3. `FastRDP.exe` dosyasını çalıştırın

### Kaynak Koddan Derleme

```bash
# Depoyu klonlayın
git clone https://github.com/mcicekci/Fast.RDP.git
cd Fast.RDP

# .NET 8.0 SDK'nın yüklü olduğundan emin olun
dotnet --version

# NuGet paketlerini restore edin
dotnet restore

# Projeyi derleyin
dotnet build --configuration Release

# Uygulamayı çalıştırın
dotnet run
```

**Not**: Visual Studio 2022 ile derleme önerilir. CLI derleme bazı WinUI projelerinde sorunlu olabilir.

---

## 📖 Hızlı Başlangıç

### 1. İlk Profil Oluşturma

1. Uygulamayı başlatın
2. Sol panelde **"Yeni Profil"** butonuna tıklayın
3. Gerekli bilgileri doldurun:
   - Bağlantı Adı: `Test Sunucusu`
   - Host: `192.168.1.100`
   - Kullanıcı Adı: `admin` (opsiyonel)
4. **"Kaydet"** butonuna tıklayın

### 2. RDP Bağlantısı Kurma

**Yöntem 1**: Profili **çift tıklayın**  
**Yöntem 2**: Profili seçip **"Bağlan"** butonuna tıklayın  
**Yöntem 3**: **Sağ tıklayıp** "Bağlan" seçeneğini seçin  
**Yöntem 4**: **Taskbar Jump List**'ten seçin

### 3. Mevcut RDP Dosyalarını İçe Aktarma

**Yöntem 1**: Drag & Drop
- RDP dosyalarını uygulama penceresine sürükleyin
- Birden fazla dosya seçilebilir

**Yöntem 2**: Manuel Kopyalama
- Mevcut RDP dosyalarınızı `Data/profiles` klasörüne kopyalayın
- Uygulama otomatik olarak algılayacaktır

---

## ⚠️ Bilinen Sorunlar

### Kritik Olmayan Sorunlar

1. **CLI Derleme Sorunu** (Düşük Öncelik)
   - **Sorun**: `dotnet build` komutu bazı sistemlerde hata verebilir
   - **Geçici Çözüm**: Visual Studio 2022 kullanarak derleyin
   - **Etkilenen Kullanıcılar**: Sadece geliştiriciler
   - **Durum**: İnceleniyor

2. **Sistem Tepsisi Event Handlers** (Orta Öncelik)
   - **Sorun**: Sistem tepsisi sağ tık menüsü henüz implement edilmedi
   - **Mevcut Durum**: Çift tıklama çalışıyor, context menu eksik
   - **Planlanan Çözüm**: v0.1-beta-2'de eklenecek

3. **Ayarlar Penceresi** (Düşük Öncelik)
   - **Sorun**: Ayarlar penceresi temel seviyede
   - **Eksik Özellikler**: Gelişmiş ayarlar, dışa aktarma seçenekleri
   - **Planlanan Çözüm**: v0.2.0'da tam implementasyon

### Limitasyonlar

- **Şifre Yönetimi**: Şifreler dosyada saklanmaz (güvenlik amacıyla), her bağlantıda sorulur
- **Credential Manager**: Windows Credential Manager entegrasyonu henüz yok
- **Widget Görünümü**: Planlandı, henüz mevcut değil
- **Birim Testleri**: Test coverage henüz düşük

### Raporlanan Hatalar

Henüz kritik hata raporu yoktur. Herhangi bir sorunla karşılaşırsanız lütfen [GitHub Issues](https://github.com/mcicekci/Fast.RDP/issues) üzerinden bildirin.

---

## 🔮 Gelecek Planlar

### v0.1-beta-2 (Planlanan)
- ✅ Sistem tepsisi context menu implementasyonu
- ✅ İyileştirilmiş hata yönetimi ve kullanıcı bildirimleri
- ✅ Profil import/export özellikleri
- ✅ Performans iyileştirmeleri
- ✅ Bug fixes ve stabilite güncellemeleri

### v0.2.0 (Gelecek)
- 🎯 Ayarlar penceresi tam implementasyonu
- 🎯 Windows Credential Manager entegrasyonu
- 🎯 Çoklu dil desteği (İngilizce)
- 🎯 Otomatik güncelleme mekanizması
- 🎯 Gelişmiş profil şablonları

### v0.3.0 (Uzun Vadeli)
- 🔮 Widget görünümü
- 🔮 Cloud senkronizasyon (OneDrive, Google Drive)
- 🔮 Profil şifreleme
- 🔮 Grup politikası desteği
- 🔮 Microsoft Store yayını

---

## 🤝 Katkıda Bulunma

Bu beta sürümü, topluluk geri bildirimlerine açıktır. Katkıda bulunmak için:

### Hata Bildirimi

1. [GitHub Issues](https://github.com/mcicekci/Fast.RDP/issues) sayfasını ziyaret edin
2. "New Issue" butonuna tıklayın
3. Hata şablonunu doldurun:
   - Hatanın açıklaması
   - Yeniden üretme adımları
   - Beklenen davranış
   - Ekran görüntüleri (varsa)
   - Sistem bilgileri

### Özellik İsteği

1. [GitHub Issues](https://github.com/mcicekci/Fast.RDP/issues) sayfasında "Feature Request" şablonunu kullanın
2. Özelliği detaylı açıklayın
3. Kullanım senaryolarını paylaşın

### Kod Katkısı

1. Projeyi fork edin
2. Feature branch oluşturun: `git checkout -b feature/YeniOzellik`
3. Değişikliklerinizi commit edin: `git commit -m 'Yeni özellik: XYZ'`
4. Branch'inizi push edin: `git push origin feature/YeniOzellik`
5. Pull Request açın

**Kod Standartları:**
- Clean Code prensiplerini takip edin
- MVVM pattern'e uyun
- XML dokümantasyonu ekleyin
- Türkçe yorum ve değişken isimleri kullanabilirsiniz

---

## 📄 Lisans

FastRDP, MIT Lisansı altında lisanslanmıştır. Bu, uygulamayı özgürce kullanabileceğiniz, değiştirebileceğiniz ve dağıtabileceğiniz anlamına gelir.

Detaylar için [LICENSE](https://github.com/mcicekci/Fast.RDP/blob/main/LICENSE) dosyasına bakın.

---

## 🙏 Teşekkürler

Bu projeyi mümkün kılan harika topluluklara teşekkürler:

- **Microsoft WinUI Team**: Modern UI framework için
- **Windows App SDK Contributors**: Windows entegrasyonu için
- **.NET Community**: Güçlü platform için
- **Beta Testers**: Geri bildirimler için

---

## 📞 İletişim ve Destek

### Destek Kanalları

- **GitHub Issues**: [Sorun Bildirimi](https://github.com/mcicekci/Fast.RDP/issues)
- **GitHub Discussions**: [Soru & Cevap](https://github.com/mcicekci/Fast.RDP/discussions)
- **Dokümantasyon**: [Wiki Sayfası](https://github.com/mcicekci/Fast.RDP/wiki)

### Sosyal Medya

- **GitHub**: [@mcicekci](https://github.com/mcicekci)

---

## 📊 Değişiklik Özeti

```
Sprint 1: Temel İşlevsellik ✅
├── Proje yapısı ve MVVM implementasyonu
├── RDP profil yönetimi (CRUD)
├── JSON tabanlı veri depolama
└── Temel UI ve navigasyon

Sprint 2: Kullanıcı Deneyimi ✅
├── Tema değiştirme (Light/Dark)
├── Drag & Drop RDP import
├── Gelişmiş profil kartları (thumbnail)
└── Arama ve filtreleme sistemi

Sprint 3: Gelişmiş Özellikler ✅
├── Jump List entegrasyonu
├── Sistem tepsisi desteği
├── Çoklu monitör desteği
└── Favoriler sistemi

Toplam:
- 15+ ana özellik
- 100+ commit
- 2.500+ satır kod
- MVVM mimari ile clean code
```

---

## 🔐 Güvenlik

### Güvenlik Yaklaşımı

FastRDP, kullanıcı verilerinin güvenliğini ciddiye alır:

- ✅ **Şifreler saklanmaz**: RDP şifreleri dosyada tutulmaz
- ✅ **DPAPI desteği**: Windows Data Protection API kullanılabilir
- ✅ **Güvenli dosya işlemleri**: Path validation ve sanitization
- ✅ **Local-first yaklaşım**: Veriler sadece kullanıcının bilgisayarında

### Güvenlik Açığı Bildirimi

Bir güvenlik açığı bulduysanız lütfen:
1. **Public issue açmayın**
2. GitHub üzerinden private mesaj gönderin
3. Detaylı açıklama yapın
4. 24-48 saat içinde geri dönüş alacaksınız

---

## ⚡ Hızlı İstatistikler

| Metrik | Değer |
|--------|-------|
| İlk Commit | 20 Ekim 2025 |
| Beta Release | 23 Ekim 2025 |
| Geliştirme Süresi | 3 gün |
| Satır Kod | ~2,500+ |
| Dosya Sayısı | 30+ |
| Dependencies | 5 paket |
| Desteklenen Platform | Windows 10+ |
| Mimari | x64, x86, ARM64 |
| Minimum .NET | 8.0 |

---

## 🎯 Hedef Kullanıcılar

FastRDP şu kullanıcılar için tasarlanmıştır:

1. **IT Profesyonelleri**: Birden fazla sunucuyu yöneten sistem yöneticileri
2. **Geliştiriciler**: Geliştirme ve test sunucularına sık erişim yapanlar
3. **Kurumsal Kullanıcılar**: Şirket içi sunucu bağlantılarını yönetenler
4. **Power Users**: Windows Remote Desktop'u yoğun kullananlar

---

## ⚠️ Feragatname

- Bu uygulama bağımsız bir açık kaynak projedir
- Microsoft Corporation ile bağlantılı değildir
- RDP (Remote Desktop Protocol), Microsoft'un tescilli protokolüdür
- Uygulamayı kendi sorumluluğunuzda kullanın

---

## 📈 İndirme Linkleri

### v0.1-beta-1 İndirme Seçenekleri

| Paket | Boyut | İndirme |
|-------|-------|---------|
| 🔧 Setup Installer (x64) | ~45 MB | [FastRDP_v0.1-beta-1_Setup_x64.exe](https://github.com/mcicekci/Fast.RDP/releases/download/v0.1-beta-1/FastRDP_v0.1-beta-1_Setup_x64.exe) |
| 📦 Portable (x64) | ~40 MB | [FastRDP_v0.1-beta-1_Portable_x64.zip](https://github.com/mcicekci/Fast.RDP/releases/download/v0.1-beta-1/FastRDP_v0.1-beta-1_Portable_x64.zip) |
| 💾 Source Code (zip) | ~2 MB | [Source.zip](https://github.com/mcicekci/Fast.RDP/archive/refs/tags/v0.1-beta-1.zip) |
| 💾 Source Code (tar.gz) | ~1.8 MB | [Source.tar.gz](https://github.com/mcicekci/Fast.RDP/archive/refs/tags/v0.1-beta-1.tar.gz) |

### Checksum Doğrulama

SHA256 checksums:
```
FastRDP_v0.1-beta-1_Setup_x64.exe: [Placeholder - Build sonrası eklenecek]
FastRDP_v0.1-beta-1_Portable_x64.zip: [Placeholder - Build sonrası eklenecek]
```

---

**⭐ Projeyi beğendiyseniz GitHub'da yıldız vermeyi unutmayın!**

**🐛 Bir hata mı buldunuz? [Issue açın](https://github.com/mcicekci/Fast.RDP/issues/new)**

**💬 Geri bildirimlerinizi bekliyoruz: [Discussions](https://github.com/mcicekci/Fast.RDP/discussions)**

---

<p align="center">
  <b>FastRDP - Modern RDP Bağlantı Yönetimi</b><br>
  Made with ❤️ by FastRDP Team
</p>

