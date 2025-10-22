using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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
        public async Task<AppSettings> LoadSettingsAsync()
        {
            var filePath = Path.Combine(_dataPath, SettingsFileName);

            if (!File.Exists(filePath))
            {
                var defaultSettings = new AppSettings();
                await SaveSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ayarlar yüklenirken hata: {ex.Message}");
                return new AppSettings();
            }
        }

        /// <summary>
        /// Uygulama ayarlarını yükler (senkron - eski versiyon için)
        /// </summary>
        public AppSettings LoadSettings()
        {
            // UI thread deadlock'unu önlemek için Task.Run kullan
            return Task.Run(() => LoadSettingsAsync()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Uygulama ayarlarını kaydeder
        /// </summary>
        public async Task SaveSettingsAsync(AppSettings settings)
        {
            var filePath = Path.Combine(_dataPath, SettingsFileName);
            
            try
            {
                var json = JsonSerializer.Serialize(settings, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ayarlar kaydedilirken hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Uygulama ayarlarını kaydeder (senkron - eski versiyon için)
        /// </summary>
        public void SaveSettings(AppSettings settings)
        {
            Task.Run(() => SaveSettingsAsync(settings)).GetAwaiter().GetResult();
        }

        #endregion

        #region Profil Metadata

        /// <summary>
        /// Tüm profil metadata'larını yükler
        /// </summary>
        public async Task<List<RdpProfile>> LoadProfilesAsync()
        {
            var filePath = Path.Combine(_dataPath, ProfilesMetaFileName);

            if (!File.Exists(filePath))
            {
                return new List<RdpProfile>();
            }

            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<List<RdpProfile>>(json, _jsonOptions) ?? new List<RdpProfile>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profiller yüklenirken hata: {ex.Message}");
                return new List<RdpProfile>();
            }
        }

        /// <summary>
        /// Tüm profil metadata'larını yükler (senkron - eski versiyon için)
        /// </summary>
        public List<RdpProfile> LoadProfiles()
        {
            return Task.Run(() => LoadProfilesAsync()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Tüm profil metadata'larını kaydeder
        /// </summary>
        public async Task SaveProfilesAsync(List<RdpProfile> profiles)
        {
            var filePath = Path.Combine(_dataPath, ProfilesMetaFileName);
            
            try
            {
                var json = JsonSerializer.Serialize(profiles, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profiller kaydedilirken hata: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Tüm profil metadata'larını kaydeder (senkron - eski versiyon için)
        /// </summary>
        public void SaveProfiles(List<RdpProfile> profiles)
        {
            Task.Run(() => SaveProfilesAsync(profiles)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Tek bir profili kaydeder (senkron)
        /// </summary>
        public void SaveProfile(RdpProfile profile)
        {
            Task.Run(() => AddProfileAsync(profile)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Yeni profil ekler
        /// </summary>
        public async Task AddProfileAsync(RdpProfile profile)
        {
            var profiles = await LoadProfilesAsync();
            
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

            await SaveProfilesAsync(profiles);
        }

        /// <summary>
        /// Yeni profil ekler (senkron - eski versiyon için)
        /// </summary>
        public void AddProfile(RdpProfile profile)
        {
            AddProfileAsync(profile).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Profil günceller
        /// </summary>
        public async Task UpdateProfileAsync(RdpProfile profile)
        {
            var profiles = await LoadProfilesAsync();
            var index = profiles.FindIndex(p => p.Id == profile.Id);
            
            if (index >= 0)
            {
                profiles[index] = profile;
                await SaveProfilesAsync(profiles);
            }
            else
            {
                throw new InvalidOperationException($"Profil bulunamadı: {profile.Id}");
            }
        }

        /// <summary>
        /// Profil günceller (senkron - eski versiyon için)
        /// </summary>
        public void UpdateProfile(RdpProfile profile)
        {
            UpdateProfileAsync(profile).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Profil siler
        /// </summary>
        public async Task DeleteProfileAsync(string profileId)
        {
            var profiles = await LoadProfilesAsync();
            profiles.RemoveAll(p => p.Id == profileId);
            await SaveProfilesAsync(profiles);
        }

        /// <summary>
        /// Profil siler (senkron - eski versiyon için)
        /// </summary>
        public void DeleteProfile(string profileId)
        {
            DeleteProfileAsync(profileId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Profil arar (gelişmiş arama: isim, host, kullanıcı, domain, etiket, not bazlı)
        /// </summary>
        public async Task<List<RdpProfile>> SearchProfilesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await LoadProfilesAsync();
            }

            var profiles = await LoadProfilesAsync();
            var term = searchTerm.ToLower();

            var filtered = profiles.Where(p =>
                // İsim araması
                p.Name.ToLower().Contains(term) ||
                // Host/IP araması
                p.Host.ToLower().Contains(term) ||
                // Kullanıcı adı araması
                (!string.IsNullOrEmpty(p.Username) && p.Username.ToLower().Contains(term)) ||
                // Domain araması
                (!string.IsNullOrEmpty(p.Domain) && p.Domain.ToLower().Contains(term)) ||
                // Etiket araması
                (p.Tags != null && p.Tags.Any(t => t.ToLower().Contains(term))) ||
                // Not araması
                (!string.IsNullOrEmpty(p.Notes) && p.Notes.ToLower().Contains(term))
            ).ToList();

            // Relevance sorting
            return filtered.OrderByDescending(p =>
            {
                // Scoring system for relevance
                int score = 0;
                if (p.Name.ToLower() == term) score += 100;                    // Exact name match
                else if (p.Name.ToLower().StartsWith(term)) score += 50;       // Name starts with
                else if (p.Name.ToLower().Contains(term)) score += 25;         // Name contains
                
                if (p.Host.ToLower() == term) score += 75;                     // Exact host match
                else if (p.Host.ToLower().Contains(term)) score += 20;         // Host contains
                
                if (!string.IsNullOrEmpty(p.Username) && p.Username.ToLower().Contains(term)) score += 15;
                if (!string.IsNullOrEmpty(p.Domain) && p.Domain.ToLower().Contains(term)) score += 15;
                if (p.Tags != null && p.Tags.Any(t => t.ToLower() == term)) score += 30;
                if (p.Tags != null && p.Tags.Any(t => t.ToLower().Contains(term))) score += 10;
                
                return score;
            }).ThenBy(p => p.Name).ToList();
        }

        /// <summary>
        /// Profil arar (senkron - eski versiyon için)
        /// </summary>
        public List<RdpProfile> SearchProfiles(string searchTerm)
        {
            return SearchProfilesAsync(searchTerm).GetAwaiter().GetResult();
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
        public async Task BackupProfilesAsync()
        {
            var profiles = await LoadProfilesAsync();
            var backupFileName = $"profiles_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var backupPath = Path.Combine(_dataPath, backupFileName);

            try
            {
                var json = JsonSerializer.Serialize(profiles, _jsonOptions);
                await File.WriteAllTextAsync(backupPath, json);

                // Ayarlarda son yedekleme tarihini güncelle
                var settings = await LoadSettingsAsync();
                settings.LastBackup = DateTime.Now;
                await SaveSettingsAsync(settings);

                Console.WriteLine($"Yedekleme başarılı: {backupPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yedekleme hatası: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Profillerin yedeğini alır (senkron - eski versiyon için)
        /// </summary>
        public void BackupProfiles()
        {
            BackupProfilesAsync().GetAwaiter().GetResult();
        }

        #endregion
    }
}

