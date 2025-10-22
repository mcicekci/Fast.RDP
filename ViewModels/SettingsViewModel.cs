using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FastRDP.Models;
using FastRDP.Services;

namespace FastRDP.ViewModels
{
    /// <summary>
    /// Ayarlar penceresi için ViewModel
    /// </summary>
    public class SettingsViewModel : BaseViewModel
    {
        private readonly SettingsService _settingsService;
        private AppSettings _currentSettings;

        private string _rdpFolder;
        private string _theme;
        private string _accentColor;
        private string _fontSize;
        private bool _showThumbnails;
        private int _recentCount;
        private bool _startWithWindows;
        private bool _minimizeToTray;
        private bool _autoBackup;
        private DateTime? _lastBackup;

        public SettingsViewModel()
        {
            _settingsService = new SettingsService();
            _currentSettings = _settingsService.LoadSettings();

            // Mevcut ayarları property'lere yükle
            LoadCurrentSettings();

            InitializeCommands();
        }

        #region Properties

        /// <summary>
        /// RDP klasör yolu
        /// </summary>
        public string RdpFolder
        {
            get => _rdpFolder;
            set => SetProperty(ref _rdpFolder, value);
        }

        /// <summary>
        /// Tema seçimi
        /// </summary>
        public string Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }

        /// <summary>
        /// Accent color seçimi
        /// </summary>
        public string AccentColor
        {
            get => _accentColor;
            set => SetProperty(ref _accentColor, value);
        }

        /// <summary>
        /// Font boyutu seçimi
        /// </summary>
        public string FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        /// <summary>
        /// Kullanılabilir accent color'lar
        /// </summary>
        public System.Collections.Generic.List<string> AvailableAccentColors { get; } = new System.Collections.Generic.List<string>
        {
            "Blue",
            "Purple",
            "Green",
            "Orange",
            "Red",
            "Pink",
            "Teal"
        };

        /// <summary>
        /// Kullanılabilir font boyutları
        /// </summary>
        public System.Collections.Generic.List<string> AvailableFontSizes { get; } = new System.Collections.Generic.List<string>
        {
            "Small",
            "Medium",
            "Large"
        };

        /// <summary>
        /// Thumbnail gösterimi
        /// </summary>
        public bool ShowThumbnails
        {
            get => _showThumbnails;
            set => SetProperty(ref _showThumbnails, value);
        }

        /// <summary>
        /// Son kullanılanlar sayısı
        /// </summary>
        public int RecentCount
        {
            get => _recentCount;
            set
            {
                if (value < 1) value = 1;
                if (value > 50) value = 50;
                SetProperty(ref _recentCount, value);
            }
        }

        /// <summary>
        /// Windows ile başlat
        /// </summary>
        public bool StartWithWindows
        {
            get => _startWithWindows;
            set => SetProperty(ref _startWithWindows, value);
        }

        /// <summary>
        /// Sistem tepsisine minimize et
        /// </summary>
        public bool MinimizeToTray
        {
            get => _minimizeToTray;
            set => SetProperty(ref _minimizeToTray, value);
        }

        /// <summary>
        /// Otomatik yedekleme
        /// </summary>
        public bool AutoBackup
        {
            get => _autoBackup;
            set => SetProperty(ref _autoBackup, value);
        }

        /// <summary>
        /// Son yedekleme tarihi
        /// </summary>
        public DateTime? LastBackup
        {
            get => _lastBackup;
            set => SetProperty(ref _lastBackup, value);
        }

        /// <summary>
        /// Son yedekleme bilgisi (görüntüleme için)
        /// </summary>
        public string LastBackupInfo
        {
            get
            {
                if (!LastBackup.HasValue)
                    return "Henüz yedekleme yapılmamış";

                var timeSpan = DateTime.Now - LastBackup.Value;
                if (timeSpan.TotalMinutes < 1)
                    return "Az önce";
                if (timeSpan.TotalHours < 1)
                    return $"{(int)timeSpan.TotalMinutes} dakika önce";
                if (timeSpan.TotalDays < 1)
                    return $"{(int)timeSpan.TotalHours} saat önce";
                if (timeSpan.TotalDays < 7)
                    return $"{(int)timeSpan.TotalDays} gün önce";
                
                return LastBackup.Value.ToString("dd.MM.yyyy HH:mm");
            }
        }

        /// <summary>
        /// Dark tema mı?
        /// </summary>
        public bool IsDarkTheme
        {
            get => Theme == "dark";
            set
            {
                Theme = value ? "dark" : "light";
                OnPropertyChanged(nameof(IsDarkTheme));
            }
        }

        #endregion

        #region Commands

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand BrowseFolderCommand { get; private set; }
        public ICommand BackupNowCommand { get; private set; }
        public ICommand ResetToDefaultsCommand { get; private set; }

        private void InitializeCommands()
        {
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(Cancel);
            BrowseFolderCommand = new RelayCommand(BrowseFolder);
            BackupNowCommand = new AsyncRelayCommand(BackupNowAsync);
            ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Mevcut ayarları yükler
        /// </summary>
        private void LoadCurrentSettings()
        {
            RdpFolder = _currentSettings.RdpFolder;
            Theme = _currentSettings.Theme;
            AccentColor = _currentSettings.AccentColor;
            FontSize = _currentSettings.FontSize;
            ShowThumbnails = _currentSettings.ShowThumbnails;
            RecentCount = _currentSettings.RecentCount;
            StartWithWindows = _currentSettings.StartWithWindows;
            MinimizeToTray = _currentSettings.MinimizeToTray;
            AutoBackup = _currentSettings.AutoBackup;
            LastBackup = _currentSettings.LastBackup;
        }

        /// <summary>
        /// Ayarları kaydeder
        /// </summary>
        private async Task SaveAsync()
        {
            try
            {
                // Property'lerden AppSettings nesnesini güncelle
                _currentSettings.RdpFolder = RdpFolder;
                _currentSettings.Theme = Theme;
                _currentSettings.AccentColor = AccentColor;
                _currentSettings.FontSize = FontSize;
                _currentSettings.ShowThumbnails = ShowThumbnails;
                _currentSettings.RecentCount = RecentCount;
                _currentSettings.StartWithWindows = StartWithWindows;
                _currentSettings.MinimizeToTray = MinimizeToTray;
                _currentSettings.AutoBackup = AutoBackup;

                // Kaydet
                await _settingsService.SaveSettingsAsync(_currentSettings);

                // Windows başlangıç ayarını uygula
                if (StartWithWindows)
                {
                    EnableStartup();
                }
                else
                {
                    DisableStartup();
                }

                OnSaveCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                OnSaveError?.Invoke(this, ex.Message);
            }
        }

        /// <summary>
        /// İptal eder
        /// </summary>
        private void Cancel()
        {
            OnCancelled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Klasör seçici açar
        /// </summary>
        private void BrowseFolder()
        {
            // Bu metod View tarafından yakalanacak
            OnBrowseFolderRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Şimdi yedekle
        /// </summary>
        private async Task BackupNowAsync()
        {
            try
            {
                await _settingsService.BackupProfilesAsync();
                _currentSettings = await _settingsService.LoadSettingsAsync();
                LastBackup = _currentSettings.LastBackup;
                OnPropertyChanged(nameof(LastBackupInfo));
                
                OnBackupCompleted?.Invoke(this, "Yedekleme başarıyla tamamlandı!");
            }
            catch (Exception ex)
            {
                OnSaveError?.Invoke(this, $"Yedekleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Varsayılan ayarlara döner
        /// </summary>
        private void ResetToDefaults()
        {
            var defaultSettings = new AppSettings();
            _currentSettings = defaultSettings;
            LoadCurrentSettings();
            
            OnPropertyChanged(nameof(IsDarkTheme));
        }

        /// <summary>
        /// Windows başlangıcına ekle
        /// </summary>
        private void EnableStartup()
        {
            try
            {
                // Registry ile Windows başlangıç ayarı
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                        key.SetValue("FastRDP", $"\"{exePath}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Başlangıç ayarı eklenemedi: {ex.Message}");
            }
        }

        /// <summary>
        /// Windows başlangıcından kaldır
        /// </summary>
        private void DisableStartup()
        {
            try
            {
                // Registry'den Windows başlangıç ayarını kaldır
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null && key.GetValue("FastRDP") != null)
                    {
                        key.DeleteValue("FastRDP");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Başlangıç ayarı kaldırılamadı: {ex.Message}");
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Kaydetme tamamlandığında tetiklenir
        /// </summary>
        public event EventHandler OnSaveCompleted;

        /// <summary>
        /// İptal edildiğinde tetiklenir
        /// </summary>
        public event EventHandler OnCancelled;

        /// <summary>
        /// Kaydetme hatası olduğunda tetiklenir
        /// </summary>
        public event EventHandler<string> OnSaveError;

        /// <summary>
        /// Yedekleme tamamlandığında tetiklenir
        /// </summary>
        public event EventHandler<string> OnBackupCompleted;

        /// <summary>
        /// Klasör seçici istendiğinde tetiklenir
        /// </summary>
        public event EventHandler OnBrowseFolderRequested;

        #endregion
    }
}

