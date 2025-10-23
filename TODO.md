# 📋 FastRDP - Yapılacaklar Listesi

## 🔴 Öncelikli (Acil Düzeltmeler)

- [ ] **Arama fonksiyonu düzeltmesi** - Arama çalışmıyor, acil düzeltme gerekiyor
  - Dosya: ProfileListView / MainViewModel
  - Sorun: Arama kutusu yanıt vermiyor

## 🟡 Yüksek Öncelik (Kullanıcı Deneyimi)

- [ ] **Profile ismi değiştirme** - Mevcut profillerin ismini düzenleme özelliği
  - UI'da rename butonı ekle
  - Profil dosyasını da yeniden adlandır
  
- [ ] **Favorileri ayrı bir alanda gösterme** - Favoriler için özel bölüm
  - Sol panelde favoriler için ayrı liste
  - Hızlı erişim için öne çıkar

- [ ] **Profile alanında gruplar arası dropdown** - Grup değiştirme kolaylığı
  - Profil düzenlerken grup seçimi için dropdown
  - Yeni grup oluşturma özelliği

## 🟢 Orta Öncelik (Yeni Özellikler)

- [ ] **Yedekleme sistemi iyileştirmesi** - Profilleri ZIP olarak yedekleme
  - Otomatik yedekleme
  - Manuel yedekleme ve geri yükleme
  - Dosya: SettingsService.cs

- [ ] **VNC protokol desteği** - VNC bağlantılarını da yönetme
  - VNC profil modeli
  - VNC bağlantı servisi
  - UI güncellemeleri

## 📊 Sprint Bazlı Planlama

### Sprint 5: İyileştirmeler (Mevcut Sprint)
- [ ] Arama fonksiyonu düzeltmesi
- [ ] Profile ismi değiştirme
- [ ] Favoriler ayrı alan

### Sprint 6: Gelişmiş Özellikler
- [ ] Gruplar arası dropdown
- [ ] Yedekleme ZIP desteği
- [ ] VNC protokol desteği

## 💡 Gelecek Fikirler (Backlog)

- [ ] Şifre yöneticisi entegrasyonu (Credential Manager)
- [ ] Çoklu dil desteği (i18n)
- [ ] Profil şablonları
- [ ] Toplu profil içe aktarma
- [ ] Profil dışa aktarma (JSON/XML)
- [ ] Bağlantı geçmişi ve istatistikler
- [ ] Widget görünümü
- [ ] Bulut senkronizasyonu

## 🐛 Bilinen Hatalar

- [ ] Arama fonksiyonu çalışmıyor → Düzeltilecek
