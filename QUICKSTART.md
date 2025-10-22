# 🚀 FastRDP Hızlı Başlangıç Kılavuzu

Bu kılavuz, FastRDP'yi hızlıca çalıştırmanız için gerekli adımları içerir.

## 📦 Gereksinimler

- Windows 10 version 1809 (Build 17763) veya üzeri
- .NET 8.0 Runtime
- Visual Studio 2022 (geliştirme için)

## 🏃‍♂️ Hızlı Başlangıç

### 1. Projeyi Klonlayın

```bash
git clone https://github.com/mcicekci/Fast.RDP.git
cd Fast.RDP
```

### 2. Visual Studio ile Açın

```bash
start FastRDP.sln
```

> **Not**: Bu bir WinUI 3 projesidir. `dotnet build` yerine Visual Studio kullanmanız önerilir.

### 3. Çalıştırın

Visual Studio'da `F5` tuşuna basın veya "Hata Ayıklama Başlat" butonuna tıklayın.

## 🎯 Temel Kullanım

### Yeni Profil Ekleme

1. Sol panelde **"Yeni Profil"** butonuna tıklayın
2. Gerekli bilgileri doldurun
3. **"Kaydet"** butonuna tıklayın

### RDP Dosyasını İçe Aktarma

**Yöntem 1: Drag & Drop** 🆕
- Mevcut .rdp dosyalarınızı ana pencereye sürükleyip bırakın
- Dosyalar otomatik olarak içe aktarılır

**Yöntem 2: Manuel Kopyalama**
- RDP dosyalarınızı `Data/profiles` klasörüne kopyalayın
- "Yenile" butonuna tıklayın

### Bağlantı Kurma

**Çift Tıklama**: Profilin üzerine çift tıklayın

**Sağ Panel**: Profili seçin → "Bağlan" butonuna tıklayın

**Jump List** 🆕: Windows taskbar'da FastRDP ikonuna sağ tıklayın → Son kullanılan profillerden birini seçin

### Tema Değiştirme 🆕

Üst bardaki **"Tema Değiştir"** butonuna (🌙) tıklayın. Smooth animasyonla tema değişecektir!

### Sistem Tepsisi 🆕

Pencereyi kapatın ve sistem tepsisinden (notification area) erişmeye devam edin:
- Çift tıklayarak pencereyi açın/kapatın
- Uygulama arka planda çalışmaya devam eder

## 🎨 Yeni Özellikler

### Gelişmiş Profil Kartları

Profiller artık modern kartlarla gösteriliyor:
- 🎨 Renkli ikonlar
- ⭐ Favori badge'leri
- 📊 Hover efektleri
- 📝 Daha iyi bilgi görünümü

### Jump List Entegrasyonu

Windows taskbar'dan hızlı erişim:
1. Taskbar'da FastRDP ikonuna **sağ tıklayın**
2. Son kullanılan 5 profil görünür
3. Doğrudan bağlanmak için profili seçin

### Drag & Drop Import

RDP dosyalarını kolayca içe aktarın:
1. Windows Explorer'dan .rdp dosyalarını seçin
2. FastRDP penceresine sürükleyin
3. Dosyalar otomatik olarak eklenir

## ⌨️ Klavye Kısayolları

| Kısayol | Aksiyon |
|---------|---------|
| `Enter` | Seçili profile bağlan |
| `Ctrl+F` | Arama kutusuna odaklan |
| `Del` | Seçili profili sil |
| `F5` | Profilleri yenile |

## 🔧 İpuçları

### Profil Organize Etme

- **Etiketler** kullanarak profilleri kategorize edin
- **Favoriler** özelliği ile sık kullanılanları işaretleyin
- **Arama** ile hızlıca bulun (isim, host veya etiket)

### Yedekleme

Sol paneldeki **"Yedekle"** butonu ile profillerinizi yedekleyin. Yedek dosyası `Data/` klasörüne kaydedilir.

### Ayarlar 🚧

Üst bardaki **"Ayarlar"** butonuna tıklayarak:
- Tema seçenekleri
- Otomatik yedekleme
- Başlangıçta çalıştırma
- Ve daha fazlası... (yakında gelecek özellikler)

## 🐛 Sorun Giderme

### Uygulama Başlamıyor

1. .NET 8.0 Runtime'ın yüklü olduğundan emin olun
2. Windows sürümünüzü kontrol edin (minimum 1809)
3. Visual Studio'dan çalıştırmayı deneyin

### RDP Dosyaları Görünmüyor

1. **"Yenile"** butonuna tıklayın
2. `Data/profiles` klasörünün var olduğundan emin olun
3. RDP dosyalarının `.rdp` uzantılı olduğunu kontrol edin

### Tema Değişmiyor

1. Uygulamayı yeniden başlatın
2. `Data/settings.json` dosyasını kontrol edin
3. Gerekirse dosyayı silin (varsayılan ayarlar oluşturulur)

## 📚 Daha Fazla Bilgi

- [Mimari Dokümantasyonu](ARCHITECTURE.md)
- [Katkıda Bulunma Rehberi](CONTRIBUTING.md)
- [Değişiklik Geçmişi](CHANGELOG.md)

## 💡 İleri Düzey Kullanım

### Profil Metadata Düzenleme

`Data/profiles.json` dosyasını doğrudan düzenleyerek:
- Toplu değişiklikler yapın
- Profilleri başka bir bilgisayara taşıyın
- Yedekten geri yükleyin

### Jump List Özelleştirme

MainViewModel'de `UpdateJumpListAsync()` metodunu düzenleyerek:
- Gösterilen profil sayısını değiştirin
- Favori profilleri vurgulayın
- Özel kategor iler ekleyin

### Tema Özelleştirme

`App.xaml` içinde tema renklerini özelleştirin:
```xml
<SolidColorBrush x:Key="AccentFillColorDefaultBrush" Color="#0078D4"/>
```

---

**🎉 Artık FastRDP'yi kullanmaya hazırsınız!**

Sorularınız için: [GitHub Issues](https://github.com/mcicekci/Fast.RDP/issues)
