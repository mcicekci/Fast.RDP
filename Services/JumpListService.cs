using System;
using System.Collections.Generic;
using System.Linq;
using FastRDP.Models;

namespace FastRDP.Services
{
    /// <summary>
    /// Windows Taskbar Jump List yönetimi için servis
    /// Not: WinUI 3 için tam Jump List desteği Package.appxmanifest ve 
    /// Windows.UI.StartScreen.JumpList API'si ile yapılır
    /// </summary>
    public class JumpListService
    {
        private readonly SettingsService _settingsService;

        public JumpListService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        /// <summary>
        /// Jump List'i günceller (son kullanılan profilleri ekler)
        /// </summary>
        public async System.Threading.Tasks.Task UpdateJumpListAsync()
        {
            try
            {
                // WinUI 3 için Jump List oluştur
                var jumpList = await Windows.UI.StartScreen.JumpList.LoadCurrentAsync();
                jumpList.Items.Clear();

                // Son kullanılan profilleri al
                var recentProfiles = _settingsService.GetRecentProfiles(5);

                foreach (var profile in recentProfiles)
                {
                    var item = Windows.UI.StartScreen.JumpListItem.CreateWithArguments(
                        $"connect:{profile.Id}",
                        profile.Name
                    );
                    
                    item.Description = $"Bağlan: {profile.Host}";
                    item.GroupName = "Son Kullanılanlar";
                    
                    // Favori ise logo ekle
                    if (profile.Favorite)
                    {
                        item.Logo = new Uri("ms-appx:///Assets/Icons/favorite.png");
                    }

                    jumpList.Items.Add(item);
                }

                await jumpList.SaveAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Jump List güncellenirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Jump List öğesinden profil ID'sini parse eder
        /// </summary>
        public string ParseJumpListArgument(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
                return null;

            if (argument.StartsWith("connect:"))
            {
                return argument.Substring("connect:".Length);
            }

            return null;
        }

        /// <summary>
        /// Favori profiller için Jump List kategorisi oluşturur
        /// </summary>
        public async System.Threading.Tasks.Task AddFavoritesToJumpListAsync()
        {
            try
            {
                var jumpList = await Windows.UI.StartScreen.JumpList.LoadCurrentAsync();
                
                var favorites = _settingsService.GetFavorites().Take(5);

                foreach (var profile in favorites)
                {
                    var item = Windows.UI.StartScreen.JumpListItem.CreateWithArguments(
                        $"connect:{profile.Id}",
                        $"⭐ {profile.Name}"
                    );
                    
                    item.Description = $"Favori: {profile.Host}";
                    item.GroupName = "Favoriler";
                    item.Logo = new Uri("ms-appx:///Assets/Icons/favorite.png");

                    jumpList.Items.Add(item);
                }

                await jumpList.SaveAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Favoriler Jump List'e eklenirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Jump List'i tamamen temizler
        /// </summary>
        public async System.Threading.Tasks.Task ClearJumpListAsync()
        {
            try
            {
                var jumpList = await Windows.UI.StartScreen.JumpList.LoadCurrentAsync();
                jumpList.Items.Clear();
                await jumpList.SaveAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Jump List temizlenirken hata: {ex.Message}");
            }
        }
    }
}

