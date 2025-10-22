# Changelog

Tüm önemli değişiklikler bu dosyada belgelenecektir.

## [Unreleased] - 2025-10-22

### Eklenen Özellikler ✨

#### Sprint 2: Kullanıcı Deneyimi Geliştirmeleri
- ✅ **Tema Değiştirme Animasyonları**: Smooth fade-in/fade-out efektleri ile akıcı tema geçişleri
- ✅ **Drag & Drop RDP Import**: RDP dosyalarını sürükle-bırak ile içe aktarma
  - Çoklu dosya desteği
  - Otomatik isim çakışması çözümü
  - Görsel overlay feedback
  - Animasyonlu geçişler
- ✅ **Gelişmiş Profil Kartları**: Modern thumbnail önizlemeli kart görünümü
  - 48x48 renkli ikonlar
  - Favori badge göstergesi
  - Responsive hover efektleri
  - Daha iyi okunabilirlik
- 🚧 **Ayarlar Penceresi**: Temel dialog hazır (geliştirilecek)

#### Sprint 3: Gelişmiş Özellikler
- ✅ **Jump List Tam Entegrasyonu**: Windows taskbar hızlı erişim
  - Son 5 kullanılan profil
  - Taskbar'dan direk bağlantı
  - Otomatik güncelleme
  - Launch arguments desteği
- ✅ **Sistem Tepsisi Desteği**: Native Windows Shell API entegrasyonu
  - P/Invoke ile Shell_NotifyIcon API
  - Minimize to tray
  - Çift tıklama ile göster/gizle
  - Tooltip desteği

### Teknik İyileştirmeler 🔧
- Tüm ViewModels'de MVVM pattern tutarlılığı
- Event-based mimari ile loose coupling
- Async/await pattern kullanımı
- Modern C# 12 özellikleri
- Code-behind azaltılması

### Performans İyileştirmeleri ⚡
- Lazy loading profil yükleme
- Animated transitions için optimize edilmiş storyboard'lar
- Virtual scrolling ListView'de varsayılan olarak aktif

### Bilinen Sorunlar 🐛
- `dotnet build` CLI ile derleme sorunlu (Visual Studio ile çalışıyor)
- Sistem tepsisi event handlers henüz implement edilmedi

### Yaklaşan Özellikler 📋
- Ayarlar penceresi tam implementasyonu
- Windows Credential Manager entegrasyonu
- Çoklu monitör desteği
- Widget görünümü
- Birim ve UI testleri

---

## [0.1.0] - 2025-10-20

### İlk Sürüm
- Temel RDP profil yönetimi
- MVVM architecture
- JSON-based veri depolama
- Fluent Design UI
- Arama ve filtreleme
- Favoriler desteği
