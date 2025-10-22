using System;
using FastRDP.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace FastRDP.Views
{
    public sealed partial class SettingsView : UserControl
    {
        public SettingsViewModel ViewModel { get; set; }

        public SettingsView()
        {
            this.InitializeComponent();
        }

        private async void OnBrowseFolderClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                folderPicker.FileTypeFilter.Add("*");

                // WinUI 3 için window handle gerekli
                var window = (Application.Current as App)?.m_window;
                if (window != null)
                {
                    var hwnd = WindowNative.GetWindowHandle(window);
                    InitializeWithWindow.Initialize(folderPicker, hwnd);
                }

                var folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    ViewModel.RdpFolder = folder.Path;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Klasör seçme hatası: {ex.Message}");
            }
        }
    }
}

