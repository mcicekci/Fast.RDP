# 🤝 Katkıda Bulunma Rehberi

FastRDP projesine katkıda bulunmak istediğiniz için teşekkür ederiz! Bu rehber, projeye nasıl katkıda bulunabileceğinizi açıklar.

## 📋 İçindekiler

- [Davranış Kuralları](#davranış-kuralları)
- [Nasıl Katkıda Bulunabilirim?](#nasıl-katkıda-bulunabilirim)
- [Geliştirme Ortamı Kurulumu](#geliştirme-ortamı-kurulumu)
- [Kod Standartları](#kod-standartları)
- [Pull Request Süreci](#pull-request-süreci)
- [Issue Raporlama](#issue-raporlama)

## 📜 Davranış Kuralları

### Bizim Taahhüdümüz

Açık ve misafirperver bir ortam oluşturmak için, katkıda bulunanlar ve yöneticiler olarak, projemize ve topluluğumuza katılımı herkes için taciz içermeyen bir deneyim haline getirmeyi taahhüt ediyoruz.

### Standartlarımız

**Olumlu davranışlar:**
- Karşılıklı saygı ve nezaket
- Farklı bakış açılarına açık olmak
- Yapıcı eleştiri kabul etmek
- Topluluk yararını ön planda tutmak

**Kabul edilemez davranışlar:**
- Taciz edici veya aşağılayıcı dil/yorumlar
- Kişisel veya siyasi saldırılar
- Başkalarının özel bilgilerini izinsiz paylaşmak

## 🚀 Nasıl Katkıda Bulunabilirim?

### Kod Katkısı

1. **Issue Seçin veya Oluşturun**
   - Mevcut issue'lardan birini seçin
   - Yeni bir özellik eklemek istiyorsanız önce issue açın

2. **Fork ve Clone**
   ```bash
   git clone https://github.com/YOURUSERNAME/FastRDP.git
   cd FastRDP
   ```

3. **Branch Oluşturun**
   ```bash
   git checkout -b feature/amazing-feature
   ```

4. **Geliştirme Yapın**
   - Kod standartlarını takip edin
   - Testler yazın
   - Dokümantasyonu güncelleyin

5. **Commit ve Push**
   ```bash
   git add .
   git commit -m "feat: amazing feature eklendi"
   git push origin feature/amazing-feature
   ```

6. **Pull Request Açın**

### Dokümantasyon Katkısı

- README.md'yi geliştirin
- Kod örnekleri ekleyin
- Yazım hatalarını düzeltin
- Türkçe/İngilizce çeviri yapın

### Bug Raporlama

Issue açarken şu bilgileri ekleyin:
- Bug'ın açıklaması
- Adım adım tekrarlama yöntemi
- Beklenen davranış
- Gerçekleşen davranış
- Ekran görüntüleri (varsa)
- Ortam bilgileri (Windows sürümü, .NET sürümü)

## 🛠️ Geliştirme Ortamı Kurulumu

### Gereksinimler

```
- Windows 10/11
- Visual Studio 2022 (v17.8+)
- .NET 8.0 SDK
- Windows App SDK
```

### Adım Adım Kurulum

1. **Visual Studio Kurulumu**
   - "Windows application development" workload'ı seçin
   - "Windows App SDK" bileşenini ekleyin

2. **Projeyi Klonlayın**
   ```bash
   git clone https://github.com/yourusername/FastRDP.git
   ```

3. **Dependencies'i Yükleyin**
   ```bash
   cd FastRDP
   dotnet restore
   ```

4. **Projeyi Derleyin**
   ```bash
   dotnet build
   ```

5. **Uygulamayı Çalıştırın**
   ```bash
   dotnet run
   ```

## 📐 Kod Standartları

### C# Standartları

```csharp
// ✅ İYİ
public class RdpProfile
{
    /// <summary>
    /// Profil için benzersiz kimlik
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Bağlantı adı
    /// </summary>
    public string Name { get; set; }
}

// ❌ KÖTÜ
public class rdpprofile
{
    public string id;
    public string name;
}
```

### İsimlendirme Kuralları

**Sınıflar**: PascalCase
```csharp
public class RdpFileService { }
```

**Metodlar**: PascalCase
```csharp
public void ConnectToServer() { }
```

**Değişkenler**: camelCase (private), PascalCase (public)
```csharp
private string _connectionString;
public string ConnectionString { get; set; }
```

**Sabitler**: PascalCase
```csharp
public const int MaxRetryCount = 3;
```

### MVVM Pattern

```csharp
// Model
public class RdpProfile { }

// ViewModel
public class MainViewModel : BaseViewModel
{
    private string _searchQuery;
    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }
}

// View (XAML)
<TextBox Text="{x:Bind ViewModel.SearchQuery, Mode=TwoWay}" />
```

### XML Dokümantasyon

Tüm public metodlar ve property'ler için XML dokümantasyon ekleyin:

```csharp
/// <summary>
/// RDP bağlantısı başlatır
/// </summary>
/// <param name="profile">Bağlanılacak profil</param>
/// <exception cref="FileNotFoundException">RDP dosyası bulunamazsa</exception>
public void ConnectToRdp(RdpProfile profile)
{
    // ...
}
```

### Kod Organizasyonu

```csharp
public class MyClass
{
    // 1. Constants
    private const int DefaultTimeout = 30;
    
    // 2. Fields
    private readonly IService _service;
    
    // 3. Constructor
    public MyClass(IService service)
    {
        _service = service;
    }
    
    // 4. Properties
    public string Name { get; set; }
    
    // 5. Public Methods
    public void DoSomething() { }
    
    // 6. Private Methods
    private void Helper() { }
}
```

## 🔄 Pull Request Süreci

### PR Oluşturma

1. **Başlık**: Açıklayıcı ve kısa
   ```
   feat: Jump List entegrasyonu eklendi
   fix: Profil silme hatası düzeltildi
   docs: README güncellendi
   ```

2. **Açıklama**: Ne ve neden
   ```markdown
   ## Değişiklikler
   - Jump List API entegrasyonu
   - Son kullanılan profiller Jump List'e ekleniyor
   
   ## Motivasyon
   Kullanıcılar taskbar'dan hızlı erişim istiyordu (#42)
   
   ## Test
   - [x] Manuel test yapıldı
   - [x] Windows 10'da test edildi
   - [x] Windows 11'de test edildi
   ```

3. **Checklist**
   - [ ] Kod standartlarına uygun
   - [ ] Testler yazıldı
   - [ ] Dokümantasyon güncellendi
   - [ ] Değişiklikler test edildi

### PR İnceleme Süreci

1. En az 1 onay gereklidir
2. Tüm testler geçmeli
3. Kod kalitesi kontrolü yapılır
4. Değişiklikler tartışılır

### Commit Mesajları

Commit mesajları için [Conventional Commits](https://www.conventionalcommits.org/) standardını kullanıyoruz:

```
feat: yeni özellik eklendi
fix: bug düzeltildi
docs: dokümantasyon güncellendi
style: kod formatlaması
refactor: kod yeniden yapılandırıldı
test: test eklendi
chore: genel bakım
```

**Örnekler:**
```bash
git commit -m "feat: profil favorilere ekleme özelliği"
git commit -m "fix: arama kutusunda türkçe karakter sorunu düzeltildi"
git commit -m "docs: kurulum adımları güncellendi"
```

## 🐛 Issue Raporlama

### Bug Raporu Şablonu

```markdown
**Bug Açıklaması**
Kısa ve net açıklama.

**Tekrarlama Adımları**
1. '...' sayfasına git
2. '...' butonuna tıkla
3. Hatayı gör

**Beklenen Davranış**
Ne olmasını bekliyordunuz?

**Ekran Görüntüleri**
Varsa ekleyin.

**Ortam:**
 - OS: [örn. Windows 11]
 - .NET Sürümü: [örn. 8.0]
 - Uygulama Sürümü: [örn. 1.0.0]

**Ek Bilgi**
Başka eklemek istediğiniz bilgi.
```

### Feature Request Şablonu

```markdown
**Özellik Açıklaması**
Ne istersiniz?

**Motivasyon**
Neden bu özellik gerekli?

**Alternatifler**
Başka çözümler düşündünüz mü?

**Ek Bilgi**
Mockup, örnek, vb.
```

## 🎨 UI/UX Katkısı

### Tasarım Prensipleri

1. **Fluent Design**: Microsoft'un tasarım diline uyum
2. **Accessibility**: Erişilebilirlik standartları
3. **Responsive**: Farklı ekran boyutlarında çalışma
4. **Consistent**: Tutarlı kullanıcı deneyimi

### Mockup ve Prototipler

- Figma veya Adobe XD kullanabilirsiniz
- PNG/SVG formatında paylaşın
- İşlevselliği açıklayın

## 📞 İletişim

Sorularınız için:
- GitHub Discussions
- Issue'larda etiketleme (@username)
- E-mail: dev@fastrdp.com

## 🙏 Teşekkürler

Katkınız için teşekkür ederiz! Her katkı, büyük veya küçük, değerlidir.

---

**Mutlu kodlamalar! 🚀**

