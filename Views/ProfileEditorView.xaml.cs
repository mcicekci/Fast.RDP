using FastRDP.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FastRDP.Views
{
    public sealed partial class ProfileEditorView : UserControl
    {
        public ProfileEditorViewModel ViewModel { get; set; }

        public ProfileEditorView()
        {
            this.InitializeComponent();
        }

        private void OnRemoveTagClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                ViewModel.RemoveTagCommand.Execute(tag);
            }
        }
    }
}

