# 🏗️ Derleme Rehberi

FastRDP'yi kaynak koddan derlemek için detaylı rehber.

## 📋 Gereksinimler

### Yazılım Gereksinimleri

| Araç | Minimum Sürüm | Önerilen Sürüm |
|------|---------------|----------------|
| Windows | 10 (Build 17763) | 11 (Build 22000) |
| Visual Studio | 2022 v17.8 | 2022 v17.10+ |
| .NET SDK | 8.0 | 8.0.x (En son) |
| Windows App SDK | 1.4 | 1.5+ |

### Visual Studio Workloads

Gerekli workload'lar:
- ✅ .NET desktop development
- ✅ Universal Windows Platform development
- ✅ Windows application development

Gerekli bileşenler:
- Windows App SDK C# Templates
- MSIX Packaging Tools
- Windows 10/11 SDK (10.0.19041.0 veya üzeri)

## 🚀 Hızlı Başlangıç

### 1. Repoyu Klonlayın

```bash
git clone https://github.com/yourusername/FastRDP.git
cd FastRDP
```

### 2. Visual Studio ile Açın

```bash
start FastRDP.sln
```

veya

Visual Studio'yu açın → File → Open → Project/Solution → `FastRDP.sln`

### 3. NuGet Paketlerini Restore Edin

Visual Studio otomatik olarak restore edecektir. Manuel olarak yapmak için:

```bash
dotnet restore
```

### 4. Derleyin

**Visual Studio:**
- `Ctrl + Shift + B` veya
- Build → Build Solution

**Komut Satırı:**
```bash
dotnet build
```

### 5. Çalıştırın

**Visual Studio:**
- `F5` (Debug) veya `Ctrl + F5` (Release)

**Komut Satırı:**
```bash
dotnet run
```

## 🔧 Derleme Konfigürasyonları

### Debug Build

Geliştirme için optimize edilmiştir:
```bash
dotnet build --configuration Debug
```

**Özellikler:**
- Debugging sembolleri dahil
- Optimizasyon kapalı
- Detaylı hata mesajları
- Assert'ler aktif

### Release Build

Production için optimize edilmiştir:
```bash
dotnet build --configuration Release
```

**Özellikler:**
- Kod optimizasyonu
- Küçük binary boyutu
- Performance iyileştirmeleri
- Debugging sembolleri minimum

## 📦 Platform-Specific Build

### x64 (Önerilen)

```bash
dotnet build -c Release -r win-x64
```

### x86

```bash
dotnet build -c Release -r win-x86
```

### ARM64

```bash
dotnet build -c Release -r win-arm64
```

## 🎁 Paketleme (MSIX)

### Visual Studio ile

1. Solution Explorer'da projeye sağ tıklayın
2. **Publish** → **Create App Packages**
3. Dağıtım türünü seçin (Sideloading/Microsoft Store)
4. Sertifika oluşturun/seçin
5. Platform ve konfigürasyon seçin
6. **Create**

### Komut Satırı ile

```bash
# MSIX paketi oluştur
msbuild FastRDP.csproj /p:Configuration=Release /p:Platform=x64 /p:UapAppxPackageBuildMode=StoreUpload
```

### Self-Contained Deployment

Tüm dependency'leri içeren standalone paket:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 🧪 Test

### Birim Testleri Çalıştırma

```bash
dotnet test
```

### Test Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 🐛 Yaygın Sorunlar ve Çözümler

### Sorun 1: NuGet Restore Hatası

**Hata:**
```
error NU1101: Unable to find package...
```

**Çözüm:**
```bash
# NuGet önbelleğini temizle
dotnet nuget locals all --clear

# Paketleri tekrar restore et
dotnet restore --force
```

### Sorun 2: Windows App SDK Bulunamadı

**Hata:**
```
error APPX0101: Cannot find 'Microsoft.WindowsAppSDK'
```

**Çözüm:**
1. Visual Studio Installer'ı açın
2. Modify → Individual Components
3. "Windows App SDK" araması yapın ve yükleyin

### Sorun 3: WinUI 3 Project Templates Eksik

**Çözüm:**
```bash
dotnet new install Microsoft.WindowsAppSDK.Templates
```

### Sorun 4: XAML Designer Yüklenmiyor

**Çözüm:**
1. Tools → Options → XAML Designer
2. "Enable XAML Designer" seçeneğini aktif edin
3. Visual Studio'yu yeniden başlatın

### Sorun 5: Derleme Süresi Çok Uzun

**Optimizasyonlar:**
```xml
<!-- FastRDP.csproj içine ekleyin -->
<PropertyGroup>
  <UseRidGraph>true</UseRidGraph>
  <PublishReadyToRun>true</PublishReadyToRun>
</PropertyGroup>
```

## 🔍 Derleme Çıktıları

```
FastRDP/
├── bin/
│   ├── Debug/
│   │   └── net8.0-windows10.0.19041.0/
│   │       └── win-x64/
│   │           ├── FastRDP.exe
│   │           ├── FastRDP.dll
│   │           └── ...
│   └── Release/
│       └── ...
└── obj/
    └── ...
```

## 📊 Derleme İstatistikleri

**Tipik Derleme Süreleri:**
- Clean Build: ~30-60 saniye
- Incremental Build: ~5-10 saniye
- Full Rebuild: ~45-90 saniye

**Binary Boyutları:**
- Debug Build: ~25-30 MB
- Release Build: ~15-20 MB
- Self-Contained: ~80-100 MB

## 🚢 CI/CD Pipeline

### GitHub Actions

`.github/workflows/build.yml` dosyası:

```yaml
name: Build

on: [push, pull_request]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

## 📝 Pre-Build Scripts

Otomatik versiyon güncelleme için `prebuild.ps1`:

```powershell
# Version bilgisini güncelle
$version = (Get-Date -Format "yyyy.MM.dd.HHmm")
(Get-Content FastRDP.csproj) -replace '<Version>.*</Version>', "<Version>$version</Version>" | Set-Content FastRDP.csproj
```

## 🔐 Code Signing

### Sertifika Oluşturma

```powershell
# Self-signed certificate oluştur
New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=FastRDP" -KeyUsage DigitalSignature -FriendlyName "FastRDP Code Signing" -CertStoreLocation "Cert:\CurrentUser\My"
```

### MSIX'i İmzalama

```bash
SignTool sign /fd SHA256 /a /f MyCertificate.pfx /p YourPassword FastRDP.msix
```

## 📚 Ek Kaynaklar

- [WinUI 3 Documentation](https://docs.microsoft.com/windows/apps/winui/winui3/)
- [Windows App SDK Documentation](https://docs.microsoft.com/windows/apps/windows-app-sdk/)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)

## 💡 İpuçları

1. **Hızlı Derleme**: Sadece değiştirdiğiniz projeyi derleyin
2. **Parallel Build**: `/m` parametresi ile paralel derleme
3. **Output Verbosity**: Detaylı log için `/v:detailed`

```bash
dotnet build /m /v:detailed
```

---

Sorularınız için [GitHub Issues](https://github.com/yourusername/FastRDP/issues) sayfasını ziyaret edin.

