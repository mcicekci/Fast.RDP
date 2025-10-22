using System;
using FastRDP.ViewModels;
using FastRDP.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace FastRDP
{
    /// <summary>
    /// Ana pencere
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            this.InitializeComponent();
            ViewModel = new MainViewModel();
            
            // Pencere başlangıç ayarları
            Title = "FastRDP - RDP Bağlantı Yöneticisi";
            
            // Tema ayarı
            ApplyTheme();
        }

        /// <summary>
        /// Tema değiştirme butonu tıklandığında
        /// </summary>
        private void OnThemeButtonClick(object sender, RoutedEventArgs e)
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

            // Ayarlara kaydet
            var settingsService = new Services.SettingsService();
            var settings = settingsService.LoadSettings();
            settings.Theme = newTheme == ElementTheme.Dark ? "dark" : "light";
            settingsService.SaveSettings(settings);
        }

        /// <summary>
        /// Temayı uygular
        /// </summary>
        private void ApplyTheme()
        {
            var settingsService = new Services.SettingsService();
            var settings = settingsService.LoadSettings();

            if (Content is FrameworkElement element)
            {
                element.RequestedTheme = settings.Theme == "dark" 
                    ? ElementTheme.Dark 
                    : ElementTheme.Light;
            }
        }

        /// <summary>
        /// Filtre değiştirildiğinde
        /// </summary>
        private void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Tag is string filterTag)
            {
                ViewModel.CurrentFilter = filterTag;
            }
        }

        /// <summary>
        /// Profil çift tıklandığında
        /// </summary>
        private void OnProfileDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (ViewModel.SelectedProfile != null)
            {
                ViewModel.ConnectCommand.Execute(ViewModel.SelectedProfile);
            }
        }

        /// <summary>
        /// Yeni profil oluştur butonu tıklandığında
        /// </summary>
        private async void OnCreateNewProfileClick(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Yeni RDP Profili",
                Content = new ProfileEditorView(),
                PrimaryButtonText = "Kaydet",
                CloseButtonText = "İptal",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var editorViewModel = new ProfileEditorViewModel();
            (dialog.Content as ProfileEditorView).DataContext = editorViewModel;

            editorViewModel.OnSaveCompleted += (s, profile) =>
            {
                ViewModel.RefreshCommand.Execute(null);
                dialog.Hide();
            };

            await dialog.ShowAsync();
        }
    }
}

