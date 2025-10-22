using System;
using System.Linq;
using FastRDP.Models;
using FastRDP.Services;
using FastRDP.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using Windows.Graphics;

namespace FastRDP
{
    public sealed partial class MainWindow : Window
    {
        private readonly RdpFileService _rdpFileService;
        private readonly SettingsService _settingsService;
        private RdpProfile _selectedProfile;

        public MainWindow()
        {
            this.InitializeComponent();
            
            // Pencere boyutunu ayarla
            SetWindowSize(1200, 700);
            
            // Başlığı ayarla
            this.Title = "FastRDP - RDP Bağlantı Yöneticisi";
            
            _settingsService = new SettingsService();
            _rdpFileService = new RdpFileService(_settingsService.LoadSettings().RdpFolder);
            
            ApplyTheme();
            LoadProfiles();
        }

        private void SetWindowSize(int width, int height)
        {
            try
            {
                // WinUI 3 için AppWindow API kullan
                var hWnd = WindowNative.GetWindowHandle(this);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);
                
                if (appWindow != null)
                {
                    appWindow.Resize(new SizeInt32(width, height));
                }
            }
            catch
            {
                // Hata durumunda varsayılan boyutta aç
            }
        }

        private void ApplyTheme()
        {
            try
            {
                var settings = _settingsService.LoadSettings();
                if (this.Content is FrameworkElement element)
                {
                    element.RequestedTheme = settings.Theme == "dark" 
                        ? ElementTheme.Dark 
                        : ElementTheme.Light;
                }
            }
            catch { }
        }

        private void LoadProfiles()
        {
            try
            {
                var profiles = _settingsService.LoadProfiles();
                
                // RDP klasöründeki dosyaları tara
                var rdpFiles = _rdpFileService.ScanRdpFiles();
                
                // Metadata'da olmayan RDP dosyalarını ekle
                foreach (var file in rdpFiles)
                {
                    if (!profiles.Any(p => p.File == file))
                    {
                        try
                        {
                            var profile = _rdpFileService.ReadRdpFile(file);
                            profile.Name = System.IO.Path.GetFileNameWithoutExtension(file);
                            profiles.Add(profile);
                        }
                        catch { }
                    }
                }

                // Metadata'yı güncelle
                _settingsService.SaveProfiles(profiles);

                // ListView'e ekle
                ProfilesList.Items.Clear();
                foreach (var profile in profiles.OrderBy(p => p.Name))
                {
                    var item = new ListViewItem
                    {
                        Content = CreateProfileDisplay(profile),
                        Tag = profile
                    };
                    ProfilesList.Items.Add(item);
                }

                StatusText.Text = $"Toplam Profil: {profiles.Count}";
            }
            catch (Exception ex)
            {
                ShowError("Profiller yüklenirken hata: " + ex.Message);
            }
        }

        private StackPanel CreateProfileDisplay(RdpProfile profile)
        {
            var panel = new StackPanel { Spacing = 4, Padding = new Thickness(8) };
            
            var nameText = new TextBlock
            {
                Text = profile.Name,
                FontSize = 15,
                FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 }
            };
            
            var hostText = new TextBlock
            {
                Text = profile.Host,
                FontSize = 13,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            };
            
            var userText = new TextBlock
            {
                Text = profile.Username,
                FontSize = 12,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorTertiaryBrush"]
            };

            panel.Children.Add(nameText);
            panel.Children.Add(hostText);
            if (!string.IsNullOrEmpty(profile.Username))
            {
                panel.Children.Add(userText);
            }

            return panel;
        }

        private void OnThemeButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentTheme = Content is FrameworkElement root 
                    ? root.ActualTheme 
                    : ElementTheme.Default;

                var newTheme = currentTheme == ElementTheme.Dark 
                    ? ElementTheme.Light 
                    : ElementTheme.Dark;

                if (Content is FrameworkElement element)
                {
                    element.RequestedTheme = newTheme;
                }

                var settings = _settingsService.LoadSettings();
                settings.Theme = newTheme == ElementTheme.Dark ? "dark" : "light";
                _settingsService.SaveSettings(settings);
            }
            catch { }
        }

        private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var query = sender.Text?.ToLower() ?? "";
            
            if (string.IsNullOrWhiteSpace(query))
            {
                LoadProfiles();
                return;
            }

            try
            {
                var profiles = _settingsService.LoadProfiles();
                var filtered = profiles.Where(p =>
                    p.Name.ToLower().Contains(query) ||
                    p.Host.ToLower().Contains(query) ||
                    p.Tags.Any(t => t.ToLower().Contains(query))
                ).ToList();

                ProfilesList.Items.Clear();
                foreach (var profile in filtered.OrderBy(p => p.Name))
                {
                    var item = new ListViewItem
                    {
                        Content = CreateProfileDisplay(profile),
                        Tag = profile
                    };
                    ProfilesList.Items.Add(item);
                }

                StatusText.Text = $"Bulunan Profil: {filtered.Count}";
            }
            catch { }
        }

        private void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Tag is string filterTag)
            {
                try
                {
                    var profiles = _settingsService.LoadProfiles();

                    var filtered = filterTag switch
                    {
                        "favorites" => profiles.Where(p => p.Favorite).ToList(),
                        "recent" => profiles.Where(p => p.LastUsed.HasValue)
                                          .OrderByDescending(p => p.LastUsed).ToList(),
                        _ => profiles.OrderBy(p => p.Name).ToList()
                    };

                    ProfilesList.Items.Clear();
                    foreach (var profile in filtered)
                    {
                        var item = new ListViewItem
                        {
                            Content = CreateProfileDisplay(profile),
                            Tag = profile
                        };
                        ProfilesList.Items.Add(item);
                    }

                    StatusText.Text = $"Gösterilen Profil: {filtered.Count}";
                }
                catch { }
            }
        }

        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            LoadProfiles();
        }

        private void OnProfileSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfilesList.SelectedItem is ListViewItem item && item.Tag is RdpProfile profile)
            {
                _selectedProfile = profile;
                ProfileDetails.Visibility = Visibility.Visible;
                ProfileName.Text = profile.Name;
                ProfileHost.Text = profile.Host;
                ProfileUsername.Text = profile.Username ?? "Belirtilmemiş";
            }
            else
            {
                _selectedProfile = null;
                ProfileDetails.Visibility = Visibility.Collapsed;
            }
        }

        private void OnProfileDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (_selectedProfile != null)
            {
                ConnectToProfile(_selectedProfile);
            }
        }

        private void OnConnectClick(object sender, RoutedEventArgs e)
        {
            if (_selectedProfile != null)
            {
                ConnectToProfile(_selectedProfile);
            }
        }

        private void ConnectToProfile(RdpProfile profile)
        {
            try
            {
                _rdpFileService.ConnectToRdp(profile);
                
                profile.LastUsed = DateTime.Now;
                _settingsService.UpdateProfile(profile);
                
                LoadProfiles();
            }
            catch (Exception ex)
            {
                ShowError("Bağlantı hatası: " + ex.Message);
            }
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            if (_selectedProfile != null)
            {
                ShowInfo("Profil düzenleme özelliği çok yakında eklenecek!");
            }
        }

        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (_selectedProfile == null) return;

            var dialog = new ContentDialog
            {
                Title = "Profil Sil",
                Content = $"'{_selectedProfile.Name}' profilini silmek istediğinizden emin misiniz?",
                PrimaryButtonText = "Sil",
                CloseButtonText = "İptal",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    _rdpFileService.DeleteRdpFile(_selectedProfile.File);
                    _settingsService.DeleteProfile(_selectedProfile.Id);
                    _selectedProfile = null;
                    LoadProfiles();
                }
                catch (Exception ex)
                {
                    ShowError("Silme hatası: " + ex.Message);
                }
            }
        }

        private void OnCreateNewProfileClick(object sender, RoutedEventArgs e)
        {
            ShowInfo("Yeni profil oluşturma özelliği çok yakında eklenecek!");
        }

        private void OnBackupClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _settingsService.BackupProfiles();
                ShowInfo("Yedekleme başarıyla tamamlandı!");
            }
            catch (Exception ex)
            {
                ShowError("Yedekleme hatası: " + ex.Message);
            }
        }

        private async void ShowError(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Hata",
                Content = message,
                CloseButtonText = "Tamam",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void ShowInfo(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Bilgi",
                Content = message,
                CloseButtonText = "Tamam",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
