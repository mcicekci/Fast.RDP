using FastRDP.ViewModels;
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
    }
}

