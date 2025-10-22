using System.Collections.ObjectModel;
using FastRDP.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FastRDP.Views
{
    public sealed partial class ProfileListView : UserControl
    {
        public ProfileListView()
        {
            this.InitializeComponent();
            Profiles = new ObservableCollection<RdpProfile>();
        }

        public ObservableCollection<RdpProfile> Profiles
        {
            get { return (ObservableCollection<RdpProfile>)GetValue(ProfilesProperty); }
            set { SetValue(ProfilesProperty, value); }
        }

        public static readonly DependencyProperty ProfilesProperty =
            DependencyProperty.Register(
                nameof(Profiles),
                typeof(ObservableCollection<RdpProfile>),
                typeof(ProfileListView),
                new PropertyMetadata(null));

        public event ItemClickEventHandler ProfileItemClick;

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            ProfileItemClick?.Invoke(sender, e);
        }
    }
}

