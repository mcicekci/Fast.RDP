using System;

namespace FastRDP.Models
{
    /// <summary>
    /// Uygulama ayarlarını tutan model sınıfı
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// RDP dosyalarının saklandığı klasör
        /// </summary>
        public string RdpFolder { get; set; } = "Data/profiles";

        /// <summary>
        /// Uygulama teması (dark/light)
        /// </summary>
        public string Theme { get; set; } = "dark";

        /// <summary>
        /// Thumbnail gösterilsin mi
        /// </summary>
        public bool ShowThumbnails { get; set; } = false;

        /// <summary>
        /// Son kullanılanlar listesinde gösterilecek öğe sayısı
        /// </summary>
        public int RecentCount { get; set; } = 10;

        /// <summary>
        /// Uygulama başlangıçta açılsın mı
        /// </summary>
        public bool StartWithWindows { get; set; } = false;

        /// <summary>
        /// Sistem tepsisine minimize edilsin mi
        /// </summary>
        public bool MinimizeToTray { get; set; } = true;

        /// <summary>
        /// Son yedekleme tarihi
        /// </summary>
        public DateTime? LastBackup { get; set; }

        /// <summary>
        /// Otomatik yedekleme aktif mi
        /// </summary>
        public bool AutoBackup { get; set; } = true;
    }
}

