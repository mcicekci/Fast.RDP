using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FastRDP.Models;
using FastRDP.Services;
using Microsoft.UI.Xaml.Input;

namespace FastRDP.ViewModels
{
    /// <summary>
    /// Ana pencere için ViewModel
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly RdpFileService _rdpFileService;
        private readonly SettingsService _settingsService;
        private readonly JumpListService _jumpListService;

        private string _searchQuery;
        private RdpProfile _selectedProfile;
        private string _currentFilter = "all";
        private bool _isLoading;

        public MainViewModel()
        {
            _settingsService = new SettingsService();
            _rdpFileService = new RdpFileService(_settingsService.LoadSettings().RdpFolder);
            _jumpListService = new JumpListService(_settingsService);

            Profiles = new ObservableCollection<RdpProfile>();
            FilteredProfiles = new ObservableCollection<RdpProfile>();
            RecentProfiles = new ObservableCollection<RdpProfile>();
            FavoriteProfiles = new ObservableCollection<RdpProfile>();

            InitializeCommands();
            LoadProfiles();
        }

        #region Properties

        /// <summary>
        /// Tüm profiller
        /// </summary>
        public ObservableCollection<RdpProfile> Profiles { get; }

        /// <summary>
        /// Filtrelenmiş profiller (görüntülenen liste)
        /// </summary>
        public ObservableCollection<RdpProfile> FilteredProfiles { get; }

        /// <summary>
        /// Son kullanılan profiller
        /// </summary>
        public ObservableCollection<RdpProfile> RecentProfiles { get; }

        /// <summary>
        /// Favori profiller
        /// </summary>
        public ObservableCollection<RdpProfile> FavoriteProfiles { get; }

        /// <summary>
        /// Arama sorgusu
        /// </summary>
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    ApplyFilters();
                }
            }
        }

        /// <summary>
        /// Seçili profil
        /// </summary>
        public RdpProfile SelectedProfile
        {
            get => _selectedProfile;
            set => SetProperty(ref _selectedProfile, value);
        }

        /// <summary>
        /// Mevcut filtre türü
        /// </summary>
        public string CurrentFilter
        {
            get => _currentFilter;
            set
            {
                if (SetProperty(ref _currentFilter, value))
                {
                    ApplyFilters();
                }
            }
        }

        /// <summary>
        /// Yükleme durumu
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Toplam profil sayısı
        /// </summary>
        public int TotalProfileCount => Profiles.Count;

        #endregion

        #region Commands

        public ICommand ConnectCommand { get; private set; }
        public ICommand EditProfileCommand { get; private set; }
        public ICommand DeleteProfileCommand { get; private set; }
        public ICommand ToggleFavoriteCommand { get; private set; }
        public ICommand OpenFolderCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand CreateNewProfileCommand { get; private set; }
        public ICommand BackupProfilesCommand { get; private set; }

        private void InitializeCommands()
        {
            ConnectCommand = new RelayCommand<RdpProfile>(Connect);
            EditProfileCommand = new RelayCommand<RdpProfile>(EditProfile);
            DeleteProfileCommand = new RelayCommand<RdpProfile>(DeleteProfile);
            ToggleFavoriteCommand = new RelayCommand<RdpProfile>(ToggleFavorite);
            OpenFolderCommand = new RelayCommand<RdpProfile>(OpenFolder);
            RefreshCommand = new RelayCommand(LoadProfiles);
            CreateNewProfileCommand = new RelayCommand(CreateNewProfile);
            BackupProfilesCommand = new RelayCommand(BackupProfiles);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Profilleri yükler
        /// </summary>
        private void LoadProfiles()
        {
            IsLoading = true;

            try
            {
                Profiles.Clear();
                
                // Metadata'dan profilleri yükle
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
                        catch (Exception ex)
                        {
                            Console.WriteLine($"RDP dosyası okunamadı ({file}): {ex.Message}");
                        }
                    }
                }

                foreach (var profile in profiles)
                {
                    Profiles.Add(profile);
                }

                // Metadata'yı güncelle
                _settingsService.SaveProfiles(profiles);

                UpdateCollections();
                ApplyFilters();
                OnPropertyChanged(nameof(TotalProfileCount));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profiller yüklenirken hata: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Koleksiyonları günceller (favoriler, son kullanılanlar)
        /// </summary>
        private void UpdateCollections()
        {
            // Favoriler
            FavoriteProfiles.Clear();
            foreach (var profile in Profiles.Where(p => p.Favorite).OrderBy(p => p.Name))
            {
                FavoriteProfiles.Add(profile);
            }

            // Son kullanılanlar
            RecentProfiles.Clear();
            foreach (var profile in Profiles.Where(p => p.LastUsed.HasValue)
                .OrderByDescending(p => p.LastUsed).Take(10))
            {
                RecentProfiles.Add(profile);
            }
        }

        /// <summary>
        /// Filtreleri uygular
        /// </summary>
        private void ApplyFilters()
        {
            FilteredProfiles.Clear();

            IEnumerable<RdpProfile> filtered = Profiles;

            // Filtre türüne göre
            switch (CurrentFilter.ToLower())
            {
                case "favorites":
                    filtered = filtered.Where(p => p.Favorite);
                    break;
                case "recent":
                    filtered = filtered.Where(p => p.LastUsed.HasValue)
                        .OrderByDescending(p => p.LastUsed);
                    break;
                default:
                    filtered = filtered.OrderBy(p => p.Name);
                    break;
            }

            // Arama sorgusuna göre
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                filtered = filtered.Where(p =>
                    p.Name.ToLower().Contains(query) ||
                    p.Host.ToLower().Contains(query) ||
                    p.Tags.Any(t => t.ToLower().Contains(query))
                );
            }

            foreach (var profile in filtered)
            {
                FilteredProfiles.Add(profile);
            }
        }

        /// <summary>
        /// RDP bağlantısı başlatır
        /// </summary>
        private void Connect(RdpProfile profile)
        {
            if (profile == null) return;

            try
            {
                _rdpFileService.ConnectToRdp(profile);
                
                // Son kullanım zamanını güncelle
                profile.LastUsed = DateTime.Now;
                _settingsService.UpdateProfile(profile);
                
                UpdateCollections();
                
                // Jump List'i güncelle
                _ = _jumpListService.UpdateJumpListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bağlantı hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Profil düzenleme penceresini açar
        /// </summary>
        private void EditProfile(RdpProfile profile)
        {
            if (profile == null) return;
            
            // Bu metod View tarafından yakalanıp dialog açılacak
            // Event veya Messenger pattern ile implement edilebilir
        }

        /// <summary>
        /// Profili siler
        /// </summary>
        private void DeleteProfile(RdpProfile profile)
        {
            if (profile == null) return;

            try
            {
                _rdpFileService.DeleteRdpFile(profile.File);
                _settingsService.DeleteProfile(profile.Id);
                
                Profiles.Remove(profile);
                UpdateCollections();
                ApplyFilters();
                OnPropertyChanged(nameof(TotalProfileCount));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Silme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Favori durumunu değiştirir
        /// </summary>
        private void ToggleFavorite(RdpProfile profile)
        {
            if (profile == null) return;

            profile.Favorite = !profile.Favorite;
            _settingsService.UpdateProfile(profile);
            
            UpdateCollections();
        }

        /// <summary>
        /// RDP dosyasının bulunduğu klasörü açar
        /// </summary>
        private void OpenFolder(RdpProfile profile)
        {
            if (profile == null) return;

            try
            {
                _rdpFileService.OpenRdpFolder(profile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Klasör açma hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Yeni profil oluşturma penceresini açar
        /// </summary>
        private void CreateNewProfile()
        {
            // Bu metod View tarafından yakalanıp dialog açılacak
        }

        /// <summary>
        /// Profilleri yedekler
        /// </summary>
        private void BackupProfiles()
        {
            try
            {
                _settingsService.BackupProfiles();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yedekleme hatası: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Basit RelayCommand implementasyonu
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Generic RelayCommand implementasyonu
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;

        public void Execute(object parameter) => _execute((T)parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Async RelayCommand implementasyonu
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<System.Threading.Tasks.Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<System.Threading.Tasks.Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            RaiseCanExecuteChanged();

            try
            {
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Generic Async RelayCommand implementasyonu
    /// </summary>
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T, System.Threading.Tasks.Task> _execute;
        private readonly Func<T, bool> _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<T, System.Threading.Tasks.Task> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => !_isExecuting && (_canExecute?.Invoke((T)parameter) ?? true);

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            RaiseCanExecuteChanged();

            try
            {
                await _execute((T)parameter);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

