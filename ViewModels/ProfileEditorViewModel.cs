using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FastRDP.Models;
using FastRDP.Services;

namespace FastRDP.ViewModels
{
    /// <summary>
    /// Profil düzenleme/oluşturma penceresi için ViewModel
    /// </summary>
    public class ProfileEditorViewModel : BaseViewModel
    {
        private readonly RdpFileService _rdpFileService;
        private readonly SettingsService _settingsService;
        
        private string _name;
        private string _host;
        private string _username;
        private string _domain;
        private string _resolution;
        private string _notes;
        private string _newTag;
        private bool _favorite;
        private bool _useMultiMonitor;
        private bool _useAllMonitors;
        private bool _isEditMode;

        public ProfileEditorViewModel(RdpProfile profile = null)
        {
            var settings = new SettingsService().LoadSettings();
            _rdpFileService = new RdpFileService(settings.RdpFolder);
            _settingsService = new SettingsService();

            Tags = new ObservableCollection<string>();
            AvailableResolutions = new List<string>
            {
                "Auto",
                "Fullscreen",
                "1920x1080",
                "1680x1050",
                "1600x900",
                "1440x900",
                "1366x768",
                "1280x1024",
                "1280x720",
                "Custom"
            };

            InitializeCommands();

            if (profile != null)
            {
                LoadProfile(profile);
                IsEditMode = true;
            }
            else
            {
                CurrentProfile = new RdpProfile();
                Resolution = "Auto";
            }
        }

        #region Properties

        /// <summary>
        /// Düzenlenen/oluşturulan profil
        /// </summary>
        public RdpProfile CurrentProfile { get; private set; }

        /// <summary>
        /// Düzenleme modu mu (true) yoksa yeni oluşturma modu mu (false)
        /// </summary>
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        /// <summary>
        /// Bağlantı adı
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// Hostname veya IP adresi
        /// </summary>
        public string Host
        {
            get => _host;
            set => SetProperty(ref _host, value);
        }

        /// <summary>
        /// Kullanıcı adı
        /// </summary>
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        /// <summary>
        /// Domain
        /// </summary>
        public string Domain
        {
            get => _domain;
            set => SetProperty(ref _domain, value);
        }

        /// <summary>
        /// Ekran çözünürlüğü
        /// </summary>
        public string Resolution
        {
            get => _resolution;
            set => SetProperty(ref _resolution, value);
        }

        /// <summary>
        /// Notlar
        /// </summary>
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        /// <summary>
        /// Favori durumu
        /// </summary>
        public bool Favorite
        {
            get => _favorite;
            set => SetProperty(ref _favorite, value);
        }

        /// <summary>
        /// Çoklu monitör kullan
        /// </summary>
        public bool UseMultiMonitor
        {
            get => _useMultiMonitor;
            set => SetProperty(ref _useMultiMonitor, value);
        }

        /// <summary>
        /// Tüm monitörleri kullan
        /// </summary>
        public bool UseAllMonitors
        {
            get => _useAllMonitors;
            set => SetProperty(ref _useAllMonitors, value);
        }

        /// <summary>
        /// Yeni eklenecek etiket
        /// </summary>
        public string NewTag
        {
            get => _newTag;
            set => SetProperty(ref _newTag, value);
        }

        /// <summary>
        /// Etiket listesi
        /// </summary>
        public ObservableCollection<string> Tags { get; }

        /// <summary>
        /// Kullanılabilir çözünürlük seçenekleri
        /// </summary>
        public List<string> AvailableResolutions { get; }

        /// <summary>
        /// Formu kaydetmek için geçerli mi
        /// </summary>
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Host);

        #endregion

        #region Commands

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand AddTagCommand { get; private set; }
        public ICommand RemoveTagCommand { get; private set; }
        public ICommand TestConnectionCommand { get; private set; }

        private void InitializeCommands()
        {
            SaveCommand = new AsyncRelayCommand(SaveAsync, () => IsValid);
            CancelCommand = new RelayCommand(Cancel);
            AddTagCommand = new RelayCommand(AddTag, () => !string.IsNullOrWhiteSpace(NewTag));
            RemoveTagCommand = new RelayCommand<string>(RemoveTag);
            TestConnectionCommand = new AsyncRelayCommand(TestConnectionAsync, () => !string.IsNullOrWhiteSpace(Host));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Mevcut profili form alanlarına yükler
        /// </summary>
        private void LoadProfile(RdpProfile profile)
        {
            CurrentProfile = profile;
            Name = profile.Name;
            Host = profile.Host;
            Username = profile.Username;
            Domain = profile.Domain;
            Resolution = profile.Resolution;
            Notes = profile.Notes;
            Favorite = profile.Favorite;
            UseMultiMonitor = profile.UseMultiMonitor;
            UseAllMonitors = profile.UseAllMonitors;

            Tags.Clear();
            foreach (var tag in profile.Tags)
            {
                Tags.Add(tag);
            }
        }

        /// <summary>
        /// Profili kaydeder
        /// </summary>
        private async Task SaveAsync()
        {
            try
            {
                // Profil bilgilerini güncelle
                CurrentProfile.Name = Name;
                CurrentProfile.Host = Host;
                CurrentProfile.Username = Username ?? string.Empty;
                CurrentProfile.Domain = Domain ?? string.Empty;
                CurrentProfile.Resolution = Resolution ?? "Auto";
                CurrentProfile.Notes = Notes ?? string.Empty;
                CurrentProfile.Favorite = Favorite;
                CurrentProfile.UseMultiMonitor = UseMultiMonitor;
                CurrentProfile.UseAllMonitors = UseAllMonitors;
                CurrentProfile.Tags = Tags.ToList();

                if (IsEditMode)
                {
                    // Mevcut profili güncelle
                    await _rdpFileService.UpdateRdpFileAsync(CurrentProfile);
                    await _settingsService.UpdateProfileAsync(CurrentProfile);
                }
                else
                {
                    // Yeni profil oluştur
                    CurrentProfile.CreatedAt = DateTime.Now;
                    await _rdpFileService.CreateRdpFileAsync(CurrentProfile);
                    await _settingsService.AddProfileAsync(CurrentProfile);
                }

                OnSaveCompleted?.Invoke(this, CurrentProfile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kaydetme hatası: {ex.Message}");
                OnSaveError?.Invoke(this, ex.Message);
            }
        }

        /// <summary>
        /// İşlemi iptal eder
        /// </summary>
        private void Cancel()
        {
            OnCancelled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Yeni etiket ekler
        /// </summary>
        private void AddTag()
        {
            if (string.IsNullOrWhiteSpace(NewTag))
                return;

            var tag = NewTag.Trim();
            
            if (!Tags.Contains(tag))
            {
                Tags.Add(tag);
            }

            NewTag = string.Empty;
        }

        /// <summary>
        /// Etiketi kaldırır
        /// </summary>
        private void RemoveTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                Tags.Remove(tag);
            }
        }

        /// <summary>
        /// Bağlantıyı test eder
        /// </summary>
        private async Task TestConnectionAsync()
        {
            if (string.IsNullOrWhiteSpace(Host))
                return;

            try
            {
                // Geçici profil oluştur ve bağlan
                var tempProfile = new RdpProfile
                {
                    Name = "Test Connection",
                    Host = Host,
                    Username = Username ?? string.Empty,
                    Domain = Domain ?? string.Empty,
                    Resolution = Resolution ?? "Auto",
                    File = $"temp_test_{Guid.NewGuid()}.rdp"
                };

                await _rdpFileService.CreateRdpFileAsync(tempProfile);
                _rdpFileService.ConnectToRdp(tempProfile);

                // Test dosyasını sil
                await Task.Delay(2000);
                try
                {
                    _rdpFileService.DeleteRdpFile(tempProfile.File);
                }
                catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test bağlantısı hatası: {ex.Message}");
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Kaydetme tamamlandığında tetiklenir
        /// </summary>
        public event EventHandler<RdpProfile> OnSaveCompleted;

        /// <summary>
        /// İptal edildiğinde tetiklenir
        /// </summary>
        public event EventHandler OnCancelled;

        /// <summary>
        /// Kaydetme hatası olduğunda tetiklenir
        /// </summary>
        public event EventHandler<string> OnSaveError;

        #endregion
    }
}

