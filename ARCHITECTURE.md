# 🏛️ FastRDP Mimari Dokümantasyonu

Bu dokümant FastRDP uygulamasının teknik mimarisini, tasarım kararlarını ve kod organizasyonunu açıklar.

## 📐 Genel Mimari

FastRDP, **MVVM (Model-View-ViewModel)** pattern'i kullanarak geliştirilmiş bir WinUI 3 uygulamasıdır.

```
┌─────────────────────────────────────────────────┐
│                    View Layer                    │
│  (XAML UI, User Controls, Windows)              │
└─────────────┬───────────────────────────────────┘
              │ Data Binding
              │ Commands
              │ Events
┌─────────────▼───────────────────────────────────┐
│                 ViewModel Layer                  │
│  (Business Logic, UI State, Commands)           │
└─────────────┬───────────────────────────────────┘
              │ Service Calls
              │ Data Transformation
┌─────────────▼───────────────────────────────────┐
│                  Service Layer                   │
│  (Business Services, File I/O, API Calls)       │
└─────────────┬───────────────────────────────────┘
              │ CRUD Operations
              │ Data Access
┌─────────────▼───────────────────────────────────┐
│                   Model Layer                    │
│  (Data Entities, DTOs, Domain Models)           │
└─────────────────────────────────────────────────┘
```

## 🗂️ Katmanlar

### 1. Model Layer (Veri Katmanı)

**Sorumluluklar:**
- Veri yapılarını tanımlar
- İş kurallarını içermez
- Sadece veri ve property'leri içerir

**Sınıflar:**
```csharp
Models/
├── RdpProfile.cs      // RDP bağlantı profili
└── AppSettings.cs     // Uygulama ayarları
```

**Örnek:**
```csharp
public class RdpProfile
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Host { get; set; }
    // ... diğer property'ler
}
```

### 2. Service Layer (İş Mantığı Katmanı)

**Sorumluluklar:**
- İş mantığını uygular
- Dosya I/O işlemleri
- API çağrıları
- Veri manipülasyonu

**Sınıflar:**
```csharp
Services/
├── RdpFileService.cs      // RDP dosya işlemleri
├── SettingsService.cs     // Ayar yönetimi
└── JumpListService.cs     // Windows entegrasyonu
```

**Tasarım Kararları:**
- Her servis tek sorumluluk prensibine (SRP) uyar
- Interface'ler kullanılabilir (dependency injection için)
- Static metod yerine instance metodlar tercih edilir

### 3. ViewModel Layer (Sunum Mantığı Katmanı)

**Sorumluluklar:**
- View için veri hazırlar
- Command'ları implement eder
- Property değişikliklerini bildirir (INotifyPropertyChanged)
- View state'ini yönetir

**Sınıflar:**
```csharp
ViewModels/
├── BaseViewModel.cs              // Temel ViewModel sınıfı
├── MainViewModel.cs              // Ana pencere logic
└── ProfileEditorViewModel.cs    // Profil düzenleme logic
```

**Pattern:**
```csharp
public class MainViewModel : BaseViewModel
{
    private string _searchQuery;
    
    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }
    
    public ICommand ConnectCommand { get; }
}
```

### 4. View Layer (Sunum Katmanı)

**Sorumluluklar:**
- Kullanıcı arayüzünü tanımlar
- ViewModel'e data binding ile bağlanır
- Sadece UI ile ilgili kod içerir

**Sınıflar:**
```csharp
Views/
├── MainWindow.xaml/.cs
├── ProfileListView.xaml/.cs
└── ProfileEditorView.xaml/.cs
```

## 🔄 Veri Akışı

### Okuma İşlemi (Data Flow - Read)

```
1. User Action (View)
   ↓
2. Command Execution (ViewModel)
   ↓
3. Service Call (Service)
   ↓
4. Data Retrieval (File/API)
   ↓
5. Data Transformation (Service)
   ↓
6. Update ViewModel Properties
   ↓
7. UI Update (Data Binding)
```

### Yazma İşlemi (Data Flow - Write)

```
1. User Input (View)
   ↓
2. Property Change (ViewModel)
   ↓
3. Validation (ViewModel)
   ↓
4. Service Call (ViewModel → Service)
   ↓
5. Data Processing (Service)
   ↓
6. File/API Write (Service)
   ↓
7. Confirmation (ViewModel)
   ↓
8. UI Feedback (View)
```

## 🎯 Tasarım Kararları

### 1. MVVM Pattern Kullanımı

**Neden?**
- Separation of Concerns
- Testability
- Code reusability
- Maintainability

**Alternatifler:**
- MVC: Web uygulamaları için daha uygun
- MVP: Daha karmaşık, WinUI için gerekli değil

### 2. JSON ile Veri Depolama

**Neden?**
```csharp
// Kolay okunabilir
{
  "name": "Server1",
  "host": "192.168.1.1"
}

// Human-readable
// Version control friendly
// Kolay debug
```

**Alternatifler:**
- SQLite: Daha büyük veri setleri için
- XML: Daha verbose
- Binary: Debug zorluğu

### 3. Command Pattern

**Implementasyon:**
```csharp
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;
    
    public void Execute(object parameter) 
        => _execute();
        
    public bool CanExecute(object parameter) 
        => _canExecute?.Invoke() ?? true;
}
```

**Avantajları:**
- UI'dan logic ayrımı
- Testability
- Reusability

### 4. ObservableCollection Kullanımı

**Neden?**
```csharp
public ObservableCollection<RdpProfile> Profiles { get; }

// Otomatik UI update
Profiles.Add(newProfile);  // UI otomatik güncellenir
```

**Alternatif:**
```csharp
public List<RdpProfile> Profiles { get; set; }

// Manuel UI update gerekli
Profiles.Add(newProfile);
OnPropertyChanged(nameof(Profiles));  // Tüm liste yeniden render
```

## 🧩 Dependency Management

### Mevcut Dependencies

```xml
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.5.*" />
<PackageReference Include="Microsoft.Graphics.Win2D" Version="1.1.*" />
```

### Neden Az Dependency?

1. **Performance**: Daha az dependency = daha hızlı startup
2. **Security**: Daha az attack surface
3. **Maintenance**: Daha az breaking change riski
4. **Size**: Daha küçük binary boyutu

## 🔐 Security Considerations

### 1. RDP Şifre Yönetimi

**Yaklaşım:**
```csharp
// ❌ Şifre dosyada saklanmaz
profile.Password = "123456";  // YAPMAYIN!

// ✅ Windows Credential Manager kullanın
prompt for credentials:i:1    // Her defasında sor
```

### 2. Dosya İşlemleri

**Güvenli Yol:**
```csharp
// Path.Combine kullan
var path = Path.Combine(_profilesPath, fileName);

// ❌ String concatenation
var path = _profilesPath + "\\" + fileName;  // YAPMAYIN!
```

### 3. Input Validation

```csharp
// Dosya adı sanitization
private string SanitizeFileName(string fileName)
{
    var invalid = Path.GetInvalidFileNameChars();
    return string.Join("_", fileName.Split(invalid));
}
```

## 🎨 UI Architecture

### Fluent Design System

**Kullanılan Prensipler:**
1. **Light**: Acrylic, Shadow effects
2. **Depth**: Z-axis, Layering
3. **Motion**: Smooth transitions
4. **Material**: Visual consistency
5. **Scale**: Responsive design

### Theme Management

```csharp
// Runtime tema değiştirme
if (Content is FrameworkElement element)
{
    element.RequestedTheme = ElementTheme.Dark;
}
```

## 📊 State Management

### ViewModel State

```csharp
public class MainViewModel
{
    // UI State
    public bool IsLoading { get; set; }
    public RdpProfile SelectedProfile { get; set; }
    public string CurrentFilter { get; set; }
    
    // Data Collections
    public ObservableCollection<RdpProfile> Profiles { get; }
    public ObservableCollection<RdpProfile> FilteredProfiles { get; }
}
```

### Application State

```json
// settings.json
{
  "theme": "dark",
  "recentCount": 10,
  "lastBackup": "2025-10-22T12:00:00"
}
```

## 🔄 Event Flow

### 1. User Clicks "Connect" Button

```
View (XAML)
  ↓ Button Click
ViewModel (Command)
  ↓ Execute Command
Service (RdpFileService)
  ↓ Process.Start
Windows (mstsc.exe)
  ↓ RDP Connection
Remote Server
```

### 2. User Searches Profile

```
View (TextBox)
  ↓ Text Changed
ViewModel (Property Changed)
  ↓ ApplyFilters()
LINQ Query
  ↓ Filter Results
ObservableCollection Update
  ↓ CollectionChanged Event
View (ListView)
  ↓ UI Update
```

## 🧪 Testing Strategy

### Unit Tests

```csharp
[TestClass]
public class RdpFileServiceTests
{
    [TestMethod]
    public void CreateRdpFile_ValidProfile_CreatesFile()
    {
        // Arrange
        var service = new RdpFileService("test");
        var profile = new RdpProfile { ... };
        
        // Act
        service.CreateRdpFile(profile);
        
        // Assert
        Assert.IsTrue(File.Exists(profile.FilePath));
    }
}
```

### Integration Tests

```csharp
[TestClass]
public class MainViewModelTests
{
    [TestMethod]
    public void ConnectCommand_ValidProfile_UpdatesLastUsed()
    {
        // ViewModel + Service entegrasyonu testi
    }
}
```

## 📈 Performance Considerations

### 1. Lazy Loading

```csharp
// RDP dosyalarını lazy load et
private List<RdpProfile> _allProfiles;
public List<RdpProfile> AllProfiles 
    => _allProfiles ??= LoadProfiles();
```

### 2. Virtual Scrolling

```xaml
<!-- ListView otomatik virtualization -->
<ListView ItemsSource="{x:Bind Profiles}" />
```

### 3. Async Operations

```csharp
public async Task LoadProfilesAsync()
{
    IsLoading = true;
    Profiles = await Task.Run(() => _service.LoadProfiles());
    IsLoading = false;
}
```

## 🚀 Future Enhancements

### Planlanıyor

1. **Dependency Injection**
   ```csharp
   services.AddSingleton<IRdpFileService, RdpFileService>();
   ```

2. **MVVM Toolkit**
   ```csharp
   [ObservableProperty]
   private string _searchQuery;
   
   [RelayCommand]
   private void Connect() { }
   ```

3. **Caching Layer**
   ```csharp
   public class CachedSettingsService : ISettingsService
   {
       private AppSettings _cache;
       // ...
   }
   ```

## 📚 Referanslar

- [WinUI 3 Documentation](https://docs.microsoft.com/windows/apps/winui/winui3/)
- [MVVM Pattern](https://docs.microsoft.com/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

Bu dokümant, projenin mimari kararlarını ve tasarım prensiplerini açıklar. Sorularınız için issue açabilirsiniz.

