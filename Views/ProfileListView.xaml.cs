using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FastRDP.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FastRDP.Views
{
    public class ProfileGroup
    {
        public string Key { get; set; }
        public ObservableCollection<RdpProfile> Items { get; set; }
    }

    public sealed partial class ProfileListView : UserControl
    {
        public ProfileListView()
        {
            this.InitializeComponent();
            Profiles = new ObservableCollection<RdpProfile>();
            GroupedProfilesList = new ObservableCollection<ProfileGroup>();
            
            // Profiles değiştiğinde grupları güncelle
            Profiles.CollectionChanged += (s, e) => UpdateGroupedProfiles();
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
                new PropertyMetadata(null, OnProfilesChanged));

        public ObservableCollection<ProfileGroup> GroupedProfilesList
        {
            get { return (ObservableCollection<ProfileGroup>)GetValue(GroupedProfilesListProperty); }
            set { SetValue(GroupedProfilesListProperty, value); }
        }

        public static readonly DependencyProperty GroupedProfilesListProperty =
            DependencyProperty.Register(
                nameof(GroupedProfilesList),
                typeof(ObservableCollection<ProfileGroup>),
                typeof(ProfileListView),
                new PropertyMetadata(null));

        private static void OnProfilesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProfileListView view)
            {
                view.UpdateGroupedProfiles();
            }
        }

        private void UpdateGroupedProfiles()
        {
            GroupedProfilesList.Clear();

            if (Profiles == null || Profiles.Count == 0)
                return;

            var groups = Profiles
                .GroupBy(p => string.IsNullOrWhiteSpace(p.Group) ? "Genel" : p.Group)
                .OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                GroupedProfilesList.Add(new ProfileGroup
                {
                    Key = group.Key,
                    Items = new ObservableCollection<RdpProfile>(group)
                });
            }
        }

        public event ItemClickEventHandler ProfileItemClick;

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            ProfileItemClick?.Invoke(sender, e);
        }
    }
}

