using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FastRDP.Models;

namespace FastRDP.Services
{
    /// <summary>
    /// Uygulama ayarları ve profil metadata'larını yöneten servis
    /// </summary>
    public class SettingsService
    {
        private const string SettingsFileName = "settings.json";
        private const string ProfilesMetaFileName = "profiles.json";
        private readonly string _dataPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public SettingsService(string dataPath = "Data")
        {
            _dataPath = dataPath;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
            
            EnsureDataPathExists();
        }

        /// <summary>
        /// Data klasörünün var olduğundan emin olur
        /// </summary>
        private void EnsureDataPathExists()
        {
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }
        }

        #region Uygulama Ayarları

        /// <summary>
        /// Uygulama ayarlarını yükler
        /// </summary>
        public AppSettings LoadSettings()
        {
            var filePath = Path.Combine(_dataPath, SettingsFileName);

            if (!File.Exists(filePath))
            {
                var defaultSettings = new AppSettings();
                SaveSettings(defaultSettings);
                return defaultSettings;
            }

            try
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ayarlar yüklenirken hata: {ex.Message}");
                return new AppSettings();
            }
        }

        /// <summary>
        /// Uygulama ayarlarını kaydeder
        /// </summary>
        public void SaveSettings(AppSettings settings)
        {
            var filePath = Path.Combine(_dataPath, SettingsFileName);
            
            try
            {
                var json = JsonSerializer.Serialize(settings, _jsonOptions);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ayarlar kaydedilirken hata: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Profil Metadata

        /// <summary>
        /// Tüm profil metadata'larını yükler
        /// </summary>
        public List<RdpProfile> LoadProfiles()
        {
            var filePath = Path.Combine(_dataPath, ProfilesMetaFileName);

            if (!File.Exists(filePath))
            {
                return new List<RdpProfile>();
            }

            try
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<RdpProfile>>(json, _jsonOptions) ?? new List<RdpProfile>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profiller yüklenirken hata: {ex.Message}");
                return new List<RdpProfile>();
            }
        }

        /// <summary>
        /// Tüm profil metadata'larını kaydeder
        /// </summary>
        public void SaveProfiles(List<RdpProfile> profiles)
        {
            var filePath = Path.Combine(_dataPath, ProfilesMetaFileName);
            
            try
            {
                var json = JsonSerializer.Serialize(profiles, _jsonOptions);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profiller kaydedilirken hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Yeni profil ekler
        /// </summary>
        public void AddProfile(RdpProfile profile)
        {
            var profiles = LoadProfiles();
            
            // Aynı ID'ye sahip profil varsa güncelle
            var existingIndex = profiles.FindIndex(p => p.Id == profile.Id);
            if (existingIndex >= 0)
            {
                profiles[existingIndex] = profile;
            }
            else
            {
                profiles.Add(profile);
            }

            SaveProfiles(profiles);
        }

        /// <summary>
        /// Profil günceller
        /// </summary>
        public void UpdateProfile(RdpProfile profile)
        {
            var profiles = LoadProfiles();
            var index = profiles.FindIndex(p => p.Id == profile.Id);
            
            if (index >= 0)
            {
                profiles[index] = profile;
                SaveProfiles(profiles);
            }
            else
            {
                throw new InvalidOperationException($"Profil bulunamadı: {profile.Id}");
            }
        }

        /// <summary>
        /// Profil siler
        /// </summary>
        public void DeleteProfile(string profileId)
        {
            var profiles = LoadProfiles();
            profiles.RemoveAll(p => p.Id == profileId);
            SaveProfiles(profiles);
        }

        /// <summary>
        /// Profil arar (isim, host, etiket bazlı)
        /// </summary>
        public List<RdpProfile> SearchProfiles(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return LoadProfiles();
            }

            var profiles = LoadProfiles();
            var term = searchTerm.ToLower();

            return profiles.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Host.ToLower().Contains(term) ||
                p.Tags.Any(t => t.ToLower().Contains(term)) ||
                (p.Notes?.ToLower().Contains(term) ?? false)
            ).ToList();
        }

        /// <summary>
        /// Favori profilleri getirir
        /// </summary>
        public List<RdpProfile> GetFavorites()
        {
            return LoadProfiles().Where(p => p.Favorite).OrderBy(p => p.Name).ToList();
        }

        /// <summary>
        /// Son kullanılan profilleri getirir
        /// </summary>
        public List<RdpProfile> GetRecentProfiles(int count = 10)
        {
            return LoadProfiles()
                .Where(p => p.LastUsed.HasValue)
                .OrderByDescending(p => p.LastUsed)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Profilin son kullanım zamanını günceller
        /// </summary>
        public void UpdateLastUsed(string profileId)
        {
            var profiles = LoadProfiles();
            var profile = profiles.FirstOrDefault(p => p.Id == profileId);
            
            if (profile != null)
            {
                profile.LastUsed = DateTime.Now;
                SaveProfiles(profiles);
            }
        }

        /// <summary>
        /// Profillerin yedeğini alır
        /// </summary>
        public void BackupProfiles()
        {
            var profiles = LoadProfiles();
            var backupFileName = $"profiles_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var backupPath = Path.Combine(_dataPath, backupFileName);

            try
            {
                var json = JsonSerializer.Serialize(profiles, _jsonOptions);
                File.WriteAllText(backupPath, json);

                // Ayarlarda son yedekleme tarihini güncelle
                var settings = LoadSettings();
                settings.LastBackup = DateTime.Now;
                SaveSettings(settings);

                Console.WriteLine($"Yedekleme başarılı: {backupPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yedekleme hatası: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}

