using System;
using System.Collections.Generic;

namespace FastRDP.Models
{
    /// <summary>
    /// RDP bağlantı profilini temsil eden model sınıfı
    /// </summary>
    public class RdpProfile
    {
        /// <summary>
        /// Profil için benzersiz kimlik
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Bağlantı adı (kullanıcının göreceği isim)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// RDP dosya adı (örn: server1.rdp)
        /// </summary>
        public string File { get; set; } = string.Empty;

        /// <summary>
        /// Hedef sunucu hostname veya IP adresi
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcı adı
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Domain (opsiyonel)
        /// </summary>
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// Ekran çözünürlüğü ayarı
        /// </summary>
        public string Resolution { get; set; } = "Auto";

        /// <summary>
        /// Profil notları
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Etiketler (kategorilendirme için)
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Son kullanım tarihi
        /// </summary>
        public DateTime? LastUsed { get; set; }

        /// <summary>
        /// Favori olarak işaretlenmiş mi
        /// </summary>
        public bool Favorite { get; set; } = false;

        /// <summary>
        /// Oluşturulma tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// RDP dosyasının tam yolu
        /// </summary>
        public string FilePath => System.IO.Path.Combine("Data", "profiles", File);
    }
}

