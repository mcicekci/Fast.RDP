using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastRDP.Models;

namespace FastRDP.Services
{
    /// <summary>
    /// RDP dosyası işlemlerini yöneten servis
    /// </summary>
    public class RdpFileService
    {
        private readonly string _profilesPath;

        public RdpFileService(string profilesPath = "Data/profiles")
        {
            // Assembly'nin bulunduğu dizini al
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appDirectory = Path.GetDirectoryName(assemblyLocation);
            
            // Relative path ise, uygulama dizinine göre absolute path oluştur
            if (!Path.IsPathRooted(profilesPath))
            {
                _profilesPath = Path.Combine(appDirectory, profilesPath);
            }
            else
            {
                _profilesPath = profilesPath;
            }
            
            Console.WriteLine($"RdpFileService başlatıldı. Profiles klasörü: {_profilesPath}");
            EnsureDirectoryExists();
        }

        /// <summary>
        /// Profil klasörünün var olduğundan emin olur
        /// </summary>
        private void EnsureDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_profilesPath))
                {
                    Directory.CreateDirectory(_profilesPath);
                    Console.WriteLine($"Profiles klasörü oluşturuldu: {Path.GetFullPath(_profilesPath)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profiles klasörü oluşturma hatası: {ex.Message}");
                Console.WriteLine($"Hedef yol: {_profilesPath}");
                throw;
            }
        }

        /// <summary>
        /// Yeni RDP profili oluşturur ve dosyaya kaydeder
        /// </summary>
        public async Task CreateRdpFileAsync(RdpProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.File))
            {
                profile.File = $"{SanitizeFileName(profile.Name)}_{Guid.NewGuid().ToString().Substring(0, 8)}.rdp";
            }

            var filePath = Path.Combine(_profilesPath, profile.File);
            var lines = BuildRdpFileContent(profile);
            
            await File.WriteAllLinesAsync(filePath, lines, Encoding.UTF8);
        }

        /// <summary>
        /// Yeni RDP profili oluşturur (senkron - eski versiyon için)
        /// </summary>
        public void CreateRdpFile(RdpProfile profile)
        {
            Task.Run(() => CreateRdpFileAsync(profile)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Mevcut RDP dosyasını günceller
        /// </summary>
        public async Task UpdateRdpFileAsync(RdpProfile profile)
        {
            var filePath = Path.Combine(_profilesPath, profile.File);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"RDP dosyası bulunamadı: {filePath}");
            }

            var lines = BuildRdpFileContent(profile);
            await File.WriteAllLinesAsync(filePath, lines, Encoding.UTF8);
        }

        /// <summary>
        /// Mevcut RDP dosyasını günceller (senkron - eski versiyon için)
        /// </summary>
        public void UpdateRdpFile(RdpProfile profile)
        {
            Task.Run(() => UpdateRdpFileAsync(profile)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// RDP dosyasını siler
        /// </summary>
        public void DeleteRdpFile(string fileName)
        {
            var filePath = Path.Combine(_profilesPath, fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// Belirtilen RDP dosyasını okur ve profil bilgilerini döner
        /// </summary>
        public async Task<RdpProfile> ReadRdpFileAsync(string fileName)
        {
            var filePath = Path.Combine(_profilesPath, fileName);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"RDP dosyası bulunamadı: {filePath}");
            }

            var lines = await File.ReadAllLinesAsync(filePath);
            var profile = new RdpProfile { File = fileName };

            foreach (var line in lines)
            {
                ParseRdpLine(line, profile);
            }

            return profile;
        }

        /// <summary>
        /// Belirtilen RDP dosyasını okur (senkron - eski versiyon için)
        /// </summary>
        public RdpProfile ReadRdpFile(string fileName)
        {
            return Task.Run(() => ReadRdpFileAsync(fileName)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Tüm RDP dosyalarını tarar
        /// </summary>
        public async Task<List<string>> ScanRdpFilesAsync()
        {
            await Task.Run(() => EnsureDirectoryExists());
            
            var files = await Task.Run(() => Directory.GetFiles(_profilesPath, "*.rdp", SearchOption.TopDirectoryOnly));
            return files.Select(Path.GetFileName).ToList();
        }

        /// <summary>
        /// Tüm RDP dosyalarını tarar (senkron - eski versiyon için)
        /// </summary>
        public List<string> ScanRdpFiles()
        {
            return Task.Run(() => ScanRdpFilesAsync()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// RDP bağlantısını başlatır
        /// </summary>
        public void ConnectToRdp(RdpProfile profile)
        {
            var filePath = Path.Combine(_profilesPath, profile.File);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"RDP dosyası bulunamadı: {filePath}");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "mstsc.exe",
                Arguments = $"\"{Path.GetFullPath(filePath)}\"",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }

        /// <summary>
        /// RDP dosyasının bulunduğu klasörü açar
        /// </summary>
        public void OpenRdpFolder(RdpProfile profile)
        {
            var filePath = Path.Combine(_profilesPath, profile.File);
            
            if (File.Exists(filePath))
            {
                Process.Start("explorer.exe", $"/select,\"{Path.GetFullPath(filePath)}\"");
            }
        }

        /// <summary>
        /// RDP dosya içeriğini oluşturur
        /// </summary>
        private List<string> BuildRdpFileContent(RdpProfile profile)
        {
            var lines = new List<string>
            {
                $"full address:s:{profile.Host}",
                $"username:s:{(!string.IsNullOrEmpty(profile.Domain) ? $"{profile.Domain}\\{profile.Username}" : profile.Username)}",
                "session bpp:i:32",
                "redirectclipboard:i:1",
                "redirectprinters:i:1",
                "redirectcomports:i:0",
                "redirectsmartcards:i:1",
                "authentication level:i:2",
                "prompt for credentials:i:1",
                "negotiate security layer:i:1",
                "remoteapplicationmode:i:0",
                "alternate shell:s:",
                "shell working directory:s:",
                "disable wallpaper:i:0",
                "disable full window drag:i:0",
                "disable menu anims:i:0",
                "disable themes:i:0",
                "disable cursor setting:i:0",
                "bitmapcachepersistenable:i:1",
                "audiomode:i:0",
                "redirectdirectx:i:1",
                "audiocapturemode:i:0",
                "videoplaybackmode:i:1",
                "connection type:i:7",
                "networkautodetect:i:1",
                "bandwidthautodetect:i:1",
                "enableworkspacereconnect:i:0",
                "use redirection server name:i:0",
                "rdgiskdcproxy:i:0",
                "kdcproxyname:s:"
            };

            // Çözünürlük ayarı
            switch (profile.Resolution.ToLower())
            {
                case "full":
                case "fullscreen":
                    lines.Add("screen mode id:i:2");
                    break;
                case "auto":
                    lines.Add("screen mode id:i:1");
                    lines.Add("smart sizing:i:1");
                    break;
                default:
                    if (profile.Resolution.Contains("x"))
                    {
                        var parts = profile.Resolution.Split('x');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
                        {
                            lines.Add("screen mode id:i:1");
                            lines.Add($"desktopwidth:i:{width}");
                            lines.Add($"desktopheight:i:{height}");
                        }
                    }
                    break;
            }

            // Multi-monitor desteği
            if (profile.UseMultiMonitor || profile.UseAllMonitors)
            {
                lines.Add("use multimon:i:1");
                
                if (profile.UseAllMonitors)
                {
                    // Tüm monitörleri kullan
                    lines.Add("span monitors:i:1");
                    lines.Add("screen mode id:i:2"); // Fullscreen gerekli
                }
                else
                {
                    // Sadece multi-monitor desteği
                    lines.Add("span monitors:i:0");
                }
                
                // Multi-monitor ayarları
                lines.Add("selectedmonitors:s:");
            }
            else
            {
                lines.Add("use multimon:i:0");
                lines.Add("span monitors:i:0");
            }

            return lines;
        }

        /// <summary>
        /// RDP dosyasından bir satırı parse eder
        /// </summary>
        private void ParseRdpLine(string line, RdpProfile profile)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains(":"))
                    return;

                var parts = line.Split(new[] { ':' }, 3);
                if (parts.Length < 3)
                    return;

                var key = parts[0].Trim();
                var value = parts[2].Trim();

            switch (key)
            {
                case "full address":
                    profile.Host = value;
                    break;
                case "username":
                    if (value.Contains("\\"))
                    {
                        var userParts = value.Split('\\');
                        if (userParts.Length >= 2)
                        {
                            profile.Domain = userParts[0];
                            profile.Username = userParts[1];
                        }
                        else if (userParts.Length == 1)
                        {
                            profile.Username = userParts[0];
                        }
                    }
                    else
                    {
                        profile.Username = value;
                    }
                    break;
                case "screen mode id":
                    if (int.TryParse(value, out int screenMode))
                    {
                        switch (screenMode)
                        {
                            case 1:
                                profile.Resolution = "auto";
                                break;
                            case 2:
                                profile.Resolution = "fullscreen";
                                break;
                        }
                    }
                    break;
                case "desktopwidth":
                    if (int.TryParse(value, out int width))
                    {
                        profile.Resolution = $"{width}x{profile.Resolution?.Split('x')[1] ?? "768"}";
                    }
                    break;
                case "desktopheight":
                    if (int.TryParse(value, out int height))
                    {
                        profile.Resolution = $"{profile.Resolution?.Split('x')[0] ?? "1024"}x{height}";
                    }
                    break;
                case "use multimon":
                    if (int.TryParse(value, out int useMultiMon) && useMultiMon == 1)
                    {
                        profile.UseMultiMonitor = true;
                    }
                    break;
                case "span monitors":
                    if (int.TryParse(value, out int spanMonitors) && spanMonitors == 1)
                    {
                        profile.UseAllMonitors = true;
                    }
                    break;
            }
            }
            catch (Exception ex)
            {
                // Parsing hatalarını logla ama işlemi durdurma
                Console.WriteLine($"RDP satır parse hatası: {line} - Hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Dosya adı için geçersiz karakterleri temizler
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            return string.IsNullOrWhiteSpace(sanitized) ? "profile" : sanitized;
        }
    }
}

