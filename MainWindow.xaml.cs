using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using FastRDP.Models;
using FastRDP.Services;
using FastRDP.ViewModels;
using FastRDP.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;

namespace FastRDP
{
    public sealed partial class MainWindow : Window
    {
        private readonly RdpFileService _rdpFileService;
        private readonly SettingsService _settingsService;
        private readonly JumpListService _jumpListService;
        private readonly SystemTrayService _systemTrayService;
        private readonly ErrorHandlerService _errorHandler;
        private RdpProfile _selectedProfile;

        public MainWindow()
        {
            this.InitializeComponent();
            
            // Error handler'ı başlat
            _errorHandler = ErrorHandlerService.Instance;
            _errorHandler.OnErrorOccurred += OnErrorOccurred;
            _errorHandler.OnInfoMessageOccurred += OnInfoMessageOccurred;
            
            // Pencere boyutunu ayarla
            SetWindowSize(1200, 700);
            
            // Başlığı ayarla
            this.Title = "FastRDP - RDP Bağlantı Yöneticisi";
            
            _settingsService = new SettingsService();
            _rdpFileService = new RdpFileService(_settingsService.LoadSettings().RdpFolder);
            _jumpListService = new JumpListService(_settingsService);
            _systemTrayService = new SystemTrayService(this);
            
            // Event'leri bağla
            this.Closed += OnWindowClosed;
            
            ApplyTheme();
            LoadProfiles();
            
            // Jump List'i güncelle
            _ = UpdateJumpListAsync();
            
            // Sistem tepsisi ayarlarını kontrol et
            InitializeSystemTray();
        }

        private void InitializeSystemTray()
        {
            var settings = _settingsService.LoadSettings();
            if (settings.MinimizeToTray)
            {
                _systemTrayService.AddTrayIcon("FastRDP - RDP Bağlantı Yöneticisi");
                _systemTrayService.OnDoubleClick += (s, e) => _systemTrayService.ShowWindow();
            }
        }

        private void OnWindowClosed(object sender, WindowEventArgs args)
        {
            _systemTrayService?.Dispose();
            _errorHandler.OnErrorOccurred -= OnErrorOccurred;
            _errorHandler.OnInfoMessageOccurred -= OnInfoMessageOccurred;
        }

        private void OnErrorOccurred(object sender, ErrorHandlerService.ErrorEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                NotificationInfoBar.Title = e.Title;
                NotificationInfoBar.Message = e.Message;
                NotificationInfoBar.Severity = e.Level switch
                {
                    ErrorHandlerService.ErrorLevel.Info => InfoBarSeverity.Informational,
                    ErrorHandlerService.ErrorLevel.Warning => InfoBarSeverity.Warning,
                    ErrorHandlerService.ErrorLevel.Error => InfoBarSeverity.Error,
                    ErrorHandlerService.ErrorLevel.Critical => InfoBarSeverity.Error,
                    _ => InfoBarSeverity.Error
                };
                NotificationInfoBar.IsOpen = true;
            });
        }

        private void OnInfoMessageOccurred(object sender, ErrorHandlerService.InfoEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                NotificationInfoBar.Title = e.Title;
                NotificationInfoBar.Message = e.Message;
                NotificationInfoBar.Severity = e.IsWarning ? InfoBarSeverity.Warning : 
                                              e.IsSuccess ? InfoBarSeverity.Success : 
                                              InfoBarSeverity.Informational;
                NotificationInfoBar.IsOpen = true;
            });
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
                            
                            // Profil adı yoksa dosya adından oluştur
                            if (string.IsNullOrWhiteSpace(profile.Name))
                            {
                                profile.Name = System.IO.Path.GetFileNameWithoutExtension(file);
                            }
                            
                            // Profil ID'si yoksa oluştur
                            if (string.IsNullOrWhiteSpace(profile.Id))
                            {
                                profile.Id = Guid.NewGuid().ToString();
                            }
                            
                            // Host bilgisi yoksa varsayılan değer
                            if (string.IsNullOrWhiteSpace(profile.Host))
                            {
                                profile.Host = "localhost";
                            }
                            
                            profiles.Add(profile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"RDP dosyası okunamadı: {file} - {ex.Message}");
                        }
                    }
                }

                // Metadata'yı güncelle
                _settingsService.SaveProfiles(profiles);

                // ListView'e ekle
                ProfilesList.Items.Clear();
                foreach (var profile in profiles.OrderBy(p => p.Name))
                {
                    var item = CreateProfileCardItem(profile);
                    ProfilesList.Items.Add(item);
                }

                StatusText.Text = $"Toplam Profil: {profiles.Count}";
            }
            catch (Exception ex)
            {
                ShowError("Profiller yüklenirken hata: " + ex.Message);
            }
        }

        private ListViewItem CreateProfileCardItem(RdpProfile profile)
        {
            var grid = new Grid
            {
                Padding = new Thickness(12),
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(8),
                Margin = new Thickness(4)
            };

            grid.Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];
            grid.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"];
            grid.BorderThickness = new Thickness(1);

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Icon/Thumbnail Border
            var iconBorder = new Border
            {
                Width = 48,
                Height = 48,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(8),
                Margin = new Thickness(0, 0, 12, 0)
            };
            iconBorder.Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];

            var icon = new FontIcon
            {
                Glyph = "\uE968",
                FontSize = 24,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconBorder.Child = icon;
            Grid.SetColumn(iconBorder, 0);
            grid.Children.Add(iconBorder);

            // Profil Bilgileri Panel
            var infoPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 4
            };

            var nameText = new TextBlock
            {
                Text = profile.Name,
                FontSize = 15,
                FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            var hostText = new TextBlock
            {
                Text = profile.Host,
                FontSize = 13,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            var userText = new TextBlock
            {
                Text = string.IsNullOrEmpty(profile.Username) ? "" : profile.Username,
                FontSize = 12,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorTertiaryBrush"],
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            infoPanel.Children.Add(nameText);
            infoPanel.Children.Add(hostText);
            if (!string.IsNullOrEmpty(profile.Username))
            {
                infoPanel.Children.Add(userText);
            }

            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            // Favori Badge
            if (profile.Favorite)
            {
                var favoriteGrid = new Grid
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(8, 0, 0, 0)
                };

                var favoriteIcon = new FontIcon
                {
                    Glyph = "\uE735",
                    FontSize = 16,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 255, 185, 0))
                };

                favoriteGrid.Children.Add(favoriteIcon);
                Grid.SetColumn(favoriteGrid, 2);
                grid.Children.Add(favoriteGrid);
            }

            var item = new ListViewItem
            {
                Content = grid,
                Tag = profile,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(0),
                Margin = new Thickness(0, 0, 0, 8)
            };

            return item;
        }

        private async void OnThemeButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentTheme = Content is FrameworkElement root 
                    ? root.ActualTheme 
                    : ElementTheme.Default;

                var newTheme = currentTheme == ElementTheme.Dark 
                    ? ElementTheme.Light 
                    : ElementTheme.Dark;

                // Fade out animasyonu
                if (Content is FrameworkElement element)
                {
                    var storyboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
                    var fadeOut = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.8,
                        Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                        EasingFunction = new Microsoft.UI.Xaml.Media.Animation.QuadraticEase 
                        { 
                            EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut 
                        }
                    };

                    Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(fadeOut, element);
                    Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(fadeOut, "Opacity");
                    storyboard.Children.Add(fadeOut);

                    storyboard.Begin();
                    await System.Threading.Tasks.Task.Delay(150);

                    // Tema değiştir
                    element.RequestedTheme = newTheme;

                    // Fade in animasyonu
                    var storyboard2 = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
                    var fadeIn = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
                    {
                        From = 0.8,
                        To = 1.0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                        EasingFunction = new Microsoft.UI.Xaml.Media.Animation.QuadraticEase 
                        { 
                            EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseIn 
                        }
                    };

                    Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(fadeIn, element);
                    Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(fadeIn, "Opacity");
                    storyboard2.Children.Add(fadeIn);

                    storyboard2.Begin();
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
                
                // Gelişmiş arama: isim, host, kullanıcı adı, domain, etiket ve notlar
                var filtered = profiles.Where(p =>
                    // İsim araması
                    p.Name.ToLower().Contains(query) ||
                    // Host/IP araması
                    p.Host.ToLower().Contains(query) ||
                    // Kullanıcı adı araması
                    (!string.IsNullOrEmpty(p.Username) && p.Username.ToLower().Contains(query)) ||
                    // Domain araması
                    (!string.IsNullOrEmpty(p.Domain) && p.Domain.ToLower().Contains(query)) ||
                    // Etiket araması
                    (p.Tags != null && p.Tags.Any(t => t.ToLower().Contains(query))) ||
                    // Not araması
                    (!string.IsNullOrEmpty(p.Notes) && p.Notes.ToLower().Contains(query))
                ).ToList();

                // Öncelik sıralaması: İsim eşleşmesi > Host eşleşmesi > Diğerleri
                var sorted = filtered.OrderByDescending(p =>
                {
                    if (p.Name.ToLower().StartsWith(query)) return 3; // İsim tam başlangıç
                    if (p.Name.ToLower().Contains(query)) return 2;   // İsim içerir
                    if (p.Host.ToLower().Contains(query)) return 1;   // Host içerir
                    return 0; // Diğerleri
                }).ThenBy(p => p.Name);

                ProfilesList.Items.Clear();
                foreach (var profile in sorted)
                {
                    var item = CreateProfileCardItem(profile);
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
                        var item = CreateProfileCardItem(profile);
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
                
                // Jump List'i güncelle
                _ = UpdateJumpListAsync();
            }
            catch (Exception ex)
            {
                ShowError("Bağlantı hatası: " + ex.Message);
            }
        }

        public void ConnectToProfileById(string profileId)
        {
            try
            {
                var profiles = _settingsService.LoadProfiles();
                var profile = profiles.FirstOrDefault(p => p.Id == profileId);
                
                if (profile != null)
                {
                    ConnectToProfile(profile);
                }
                else
                {
                    ShowError("Profil bulunamadı: " + profileId);
                }
            }
            catch (Exception ex)
            {
                ShowError("Profil bağlantısı başlatılamadı: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task UpdateJumpListAsync()
        {
            try
            {
                await _jumpListService.UpdateJumpListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Jump List güncellenemedi: {ex.Message}");
            }
        }

        private async void OnEditClick(object sender, RoutedEventArgs e)
        {
            if (_selectedProfile != null)
            {
                await ShowProfileEditorDialog(_selectedProfile);
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

        private async void OnCreateNewProfileClick(object sender, RoutedEventArgs e)
        {
            await ShowProfileEditorDialog(null);
        }

        private void OnBackupClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _settingsService.BackupProfiles();
                ShowSuccess("Yedekleme başarıyla tamamlandı!");
            }
            catch (Exception ex)
            {
                ShowError("Yedekleme hatası: " + ex.Message);
            }
        }

        private async void OnImportRdpClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var openPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                openPicker.FileTypeFilter.Add(".rdp");

                // WinUI 3 için window handle gerekli
                var hwnd = WindowNative.GetWindowHandle(this);
                InitializeWithWindow.Initialize(openPicker, hwnd);

                var files = await openPicker.PickMultipleFilesAsync();
                if (files != null && files.Count > 0)
                {
                    var importedCount = 0;
                    var errors = new List<string>();

                    foreach (var file in files)
                    {
                        try
                        {
                            // Dosyayı RDP klasörüne kopyala
                            var settings = _settingsService.LoadSettings();
                            var targetPath = Path.Combine(settings.RdpFolder, file.Name);

                            // Hedef klasörün var olduğundan emin ol
                            Directory.CreateDirectory(settings.RdpFolder);

                            // Dosya zaten varsa farklı isim kullan
                            if (File.Exists(targetPath))
                            {
                                var fileName = Path.GetFileNameWithoutExtension(file.Name);
                                var extension = Path.GetExtension(file.Name);
                                var counter = 1;

                                while (File.Exists(targetPath))
                                {
                                    targetPath = Path.Combine(
                                        settings.RdpFolder,
                                        $"{fileName}_{counter}{extension}"
                                    );
                                    counter++;
                                }
                            }

                            // Windows.Storage.StorageFile'dan normal file path'e kopyalama
                            var sourceStream = await file.OpenStreamForReadAsync();
                            using (var targetStream = File.Create(targetPath))
                            {
                                await sourceStream.CopyToAsync(targetStream);
                            }
                            sourceStream.Dispose();

                            // Dosyayı okuyup profil olarak ekle
                            try
                            {
                                var profile = _rdpFileService.ReadRdpFile(Path.GetFileName(targetPath));
                                
                                // Profil adı yoksa dosya adından oluştur
                                if (string.IsNullOrWhiteSpace(profile.Name))
                                {
                                    profile.Name = Path.GetFileNameWithoutExtension(file.Name);
                                }
                                
                                // Profil ID'si yoksa oluştur
                                if (string.IsNullOrWhiteSpace(profile.Id))
                                {
                                    profile.Id = Guid.NewGuid().ToString();
                                }
                                
                                // Host bilgisi yoksa varsayılan değer
                                if (string.IsNullOrWhiteSpace(profile.Host))
                                {
                                    profile.Host = "localhost";
                                }
                                
                                // Profili kaydet
                                _settingsService.SaveProfile(profile);
                            }
                            catch (Exception profileEx)
                            {
                                Console.WriteLine($"Profil oluşturulamadı: {file.Name} - {profileEx.Message}");
                            }

                            importedCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"{file.Name}: {ex.Message}");
                        }
                    }

                    // Profilleri yeniden yükle
                    if (importedCount > 0)
                    {
                        LoadProfiles();
                        ShowSuccess($"✅ {importedCount} RDP dosyası başarıyla içe aktarıldı!");
                    }

                    if (errors.Count > 0)
                    {
                        ShowError($"⚠️ Bazı dosyalar içe aktarılamadı:\n{string.Join("\n", errors)}");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"RDP içe aktarma hatası: {ex.Message}");
            }
        }

        private async void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            await ShowSettingsDialog();
        }

        private async System.Threading.Tasks.Task ShowSettingsDialog()
        {
            var viewModel = new SettingsViewModel();
            var settingsView = new SettingsView
            {
                ViewModel = viewModel
            };

            var dialog = new ContentDialog
            {
                Title = "Ayarlar",
                Content = settingsView,
                PrimaryButtonText = "Kaydet",
                CloseButtonText = "İptal",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            // ViewModel event'lerini dinle
            var dialogClosed = false;
            viewModel.OnSaveCompleted += (s, e) =>
            {
                dialogClosed = true;
                dialog.Hide();
                
                // Tema değişikliği varsa uygula
                ApplyTheme();
                
                ShowInfo("Ayarlar başarıyla kaydedildi!");
            };

            viewModel.OnSaveError += (s, error) =>
            {
                ShowError(error);
            };

            viewModel.OnBackupCompleted += (s, message) =>
            {
                ShowInfo(message);
            };

            // Dialog butonlarına göre işlem yap
            dialog.PrimaryButtonClick += (s, args) =>
            {
                // Kaydetme işlemini başlat
                viewModel.SaveCommand.Execute(null);
                
                // Eğer hata yoksa dialog kapanacak
                if (!dialogClosed)
                {
                    args.Cancel = true;
                }
            };

            await dialog.ShowAsync();
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;

            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.DragUIOverride.Caption = "RDP dosyasını ekle";
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.IsContentVisible = true;
                e.DragUIOverride.IsGlyphVisible = true;

                // Overlay'i göster
                ShowDragDropOverlay(true);
            }
        }

        private async void OnDrop(object sender, DragEventArgs e)
        {
            // Overlay'i gizle
            ShowDragDropOverlay(false);

            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                try
                {
                    var items = await e.DataView.GetStorageItemsAsync();
                    var importedCount = 0;
                    var errors = new List<string>();

                    foreach (var item in items)
                    {
                        if (item is Windows.Storage.StorageFile file)
                        {
                            if (Path.GetExtension(file.Name).ToLower() == ".rdp")
                            {
                                try
                                {
                                    // Dosyayı RDP klasörüne kopyala
                                    var settings = _settingsService.LoadSettings();
                                    var targetPath = Path.Combine(settings.RdpFolder, file.Name);

                                    // Hedef klasörün var olduğundan emin ol
                                    Directory.CreateDirectory(settings.RdpFolder);

                                    // Dosya zaten varsa farklı isim kullan
                                    if (File.Exists(targetPath))
                                    {
                                        var fileName = Path.GetFileNameWithoutExtension(file.Name);
                                        var extension = Path.GetExtension(file.Name);
                                        var counter = 1;
                                        
                                        while (File.Exists(targetPath))
                                        {
                                            targetPath = Path.Combine(
                                                settings.RdpFolder,
                                                $"{fileName}_{counter}{extension}"
                                            );
                                            counter++;
                                        }
                                    }

                                    // Windows.Storage.StorageFile'dan normal file path'e kopyalama
                                    var sourceStream = await file.OpenStreamForReadAsync();
                                    using (var targetStream = File.Create(targetPath))
                                    {
                                        await sourceStream.CopyToAsync(targetStream);
                                    }
                                    sourceStream.Dispose();

                                    // Dosyayı okuyup profil olarak ekle
                                    try
                                    {
                                        var profile = _rdpFileService.ReadRdpFile(Path.GetFileName(targetPath));
                                        
                                        // Profil adı yoksa dosya adından oluştur
                                        if (string.IsNullOrWhiteSpace(profile.Name))
                                        {
                                            profile.Name = Path.GetFileNameWithoutExtension(file.Name);
                                        }
                                        
                                        // Profil ID'si yoksa oluştur
                                        if (string.IsNullOrWhiteSpace(profile.Id))
                                        {
                                            profile.Id = Guid.NewGuid().ToString();
                                        }
                                        
                                        // Host bilgisi yoksa varsayılan değer
                                        if (string.IsNullOrWhiteSpace(profile.Host))
                                        {
                                            profile.Host = "localhost";
                                        }
                                        
                                        // Profili kaydet
                                        _settingsService.SaveProfile(profile);
                                    }
                                    catch (Exception profileEx)
                                    {
                                        Console.WriteLine($"Profil oluşturulamadı: {file.Name} - {profileEx.Message}");
                                    }

                                    importedCount++;
                                }
                                catch (Exception ex)
                                {
                                    errors.Add($"{file.Name}: {ex.Message}");
                                }
                            }
                        }
                    }

                    // Profilleri yeniden yükle (UI thread'de)
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        if (importedCount > 0)
                        {
                            LoadProfiles();
                            ShowSuccess($"✅ {importedCount} RDP dosyası başarıyla içe aktarıldı!");
                        }

                        if (errors.Count > 0)
                        {
                            ShowError($"⚠️ Bazı dosyalar içe aktarılamadı:\n{string.Join("\n", errors)}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    ShowError($"Dosya içe aktarma hatası: {ex.Message}");
                }
            }
        }

        private async void ShowDragDropOverlay(bool show)
        {
            if (DragDropOverlay == null) return;

            var storyboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
            var animation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                To = show ? 0.15 : 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                EasingFunction = new Microsoft.UI.Xaml.Media.Animation.QuadraticEase
                {
                    EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseInOut
                }
            };

            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animation, DragDropOverlay);
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);
            storyboard.Begin();

            await System.Threading.Tasks.Task.Delay(200);
        }

        private void ShowError(string message)
        {
            _errorHandler.ShowWarning(message, "Hata");
        }

        private void ShowInfo(string message)
        {
            _errorHandler.ShowInfo(message);
        }

        private void ShowSuccess(string message)
        {
            _errorHandler.ShowSuccess(message);
        }

        private async System.Threading.Tasks.Task ShowProfileEditorDialog(RdpProfile profileToEdit)
        {
            var viewModel = new ProfileEditorViewModel(profileToEdit);
            var editorView = new ProfileEditorView
            {
                ViewModel = viewModel
            };

            var dialog = new ContentDialog
            {
                Title = profileToEdit == null ? "Yeni Profil Oluştur" : "Profili Düzenle",
                Content = editorView,
                PrimaryButtonText = "Kaydet",
                CloseButtonText = "İptal",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            // ViewModel'deki kaydetme event'lerini dinle
            var dialogClosed = false;
            viewModel.OnSaveCompleted += (s, profile) =>
            {
                dialogClosed = true;
                dialog.Hide();
                LoadProfiles();
                ShowInfo(profileToEdit == null ? "Profil başarıyla oluşturuldu!" : "Profil başarıyla güncellendi!");
            };

            viewModel.OnSaveError += (s, error) =>
            {
                ShowError(error);
            };

            // Dialog butonlarına göre işlem yap
            dialog.PrimaryButtonClick += (s, args) =>
            {
                if (!viewModel.IsValid)
                {
                    args.Cancel = true;
                    ShowError("Lütfen tüm zorunlu alanları doldurun!\n\n* Bağlantı Adı\n* Host/IP Adresi");
                    return;
                }

                // Kaydetme işlemini başlat
                viewModel.SaveCommand.Execute(null);
                
                // Eğer hata yoksa dialog kapanacak (OnSaveCompleted event'i tetiklenecek)
                if (!dialogClosed)
                {
                    args.Cancel = true;
                }
            };

            await dialog.ShowAsync();
        }
    }
}
