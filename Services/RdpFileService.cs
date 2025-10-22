using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
            _profilesPath = profilesPath;
            EnsureDirectoryExists();
        }

        /// <summary>
        /// Profil klasörünün var olduğundan emin olur
        /// </summary>
        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_profilesPath))
            {
                Directory.CreateDirectory(_profilesPath);
            }
        }

        /// <summary>
        /// Yeni RDP profili oluşturur ve dosyaya kaydeder
        /// </summary>
        public void CreateRdpFile(RdpProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.File))
            {
                profile.File = $"{SanitizeFileName(profile.Name)}_{Guid.NewGuid().ToString().Substring(0, 8)}.rdp";
            }

            var filePath = Path.Combine(_profilesPath, profile.File);
            var lines = BuildRdpFileContent(profile);
            
            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }

        /// <summary>
        /// Mevcut RDP dosyasını günceller
        /// </summary>
        public void UpdateRdpFile(RdpProfile profile)
        {
            var filePath = Path.Combine(_profilesPath, profile.File);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"RDP dosyası bulunamadı: {filePath}");
            }

            var lines = BuildRdpFileContent(profile);
            File.WriteAllLines(filePath, lines, Encoding.UTF8);
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
        public RdpProfile ReadRdpFile(string fileName)
        {
            var filePath = Path.Combine(_profilesPath, fileName);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"RDP dosyası bulunamadı: {filePath}");
            }

            var lines = File.ReadAllLines(filePath);
            var profile = new RdpProfile { File = fileName };

            foreach (var line in lines)
            {
                ParseRdpLine(line, profile);
            }

            return profile;
        }

        /// <summary>
        /// Tüm RDP dosyalarını tarar
        /// </summary>
        public List<string> ScanRdpFiles()
        {
            EnsureDirectoryExists();
            
            var files = Directory.GetFiles(_profilesPath, "*.rdp", SearchOption.TopDirectoryOnly);
            return files.Select(Path.GetFileName).ToList();
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

            return lines;
        }

        /// <summary>
        /// RDP dosyasından bir satırı parse eder
        /// </summary>
        private void ParseRdpLine(string line, RdpProfile profile)
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
                        profile.Domain = userParts[0];
                        profile.Username = userParts[1];
                    }
                    else
                    {
                        profile.Username = value;
                    }
                    break;
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

