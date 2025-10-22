using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace FastRDP.Services
{
    /// <summary>
    /// Windows Credential Manager ve DPAPI kullanarak güvenli kimlik bilgisi yönetimi
    /// </summary>
    public class CredentialService
    {
        private const string TargetPrefix = "FastRDP_";

        #region Windows Credential Manager P/Invoke

        [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] UInt32 flags);

        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredRead(string target, CRED_TYPE type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("Advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredDelete(string target, CRED_TYPE type, int flags);

        [DllImport("Advapi32.dll", SetLastError = true)]
        private static extern bool CredFree([In] IntPtr cred);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
        {
            public UInt32 Flags;
            public CRED_TYPE Type;
            public IntPtr TargetName;
            public IntPtr Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public UInt32 CredentialBlobSize;
            public IntPtr CredentialBlob;
            public UInt32 Persist;
            public UInt32 AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            public IntPtr UserName;
        }

        private enum CRED_TYPE : uint
        {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            GENERIC_CERTIFICATE = 5,
            DOMAIN_EXTENDED = 6,
            MAXIMUM = 7,
            MAXIMUM_EX = 1007
        }

        #endregion

        /// <summary>
        /// Kimlik bilgilerini Windows Credential Manager'a kaydeder
        /// </summary>
        /// <param name="profileId">Profil ID'si</param>
        /// <param name="username">Kullanıcı adı</param>
        /// <param name="password">Şifre</param>
        /// <param name="useEncryption">DPAPI şifreleme kullanılsın mı</param>
        public bool SaveCredential(string profileId, string username, string password, bool useEncryption = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(profileId) || string.IsNullOrWhiteSpace(password))
                    return false;

                var target = TargetPrefix + profileId;
                var passwordBytes = Encoding.UTF8.GetBytes(password);

                // DPAPI ile şifreleme
                if (useEncryption)
                {
                    passwordBytes = ProtectedData.Protect(
                        passwordBytes,
                        GetEntropy(),
                        DataProtectionScope.CurrentUser
                    );
                }

                var credential = new CREDENTIAL
                {
                    Type = CRED_TYPE.GENERIC,
                    TargetName = Marshal.StringToCoTaskMemUni(target),
                    UserName = Marshal.StringToCoTaskMemUni(username ?? string.Empty),
                    CredentialBlob = Marshal.AllocCoTaskMem(passwordBytes.Length),
                    CredentialBlobSize = (uint)passwordBytes.Length,
                    Persist = 2, // CRED_PERSIST_LOCAL_MACHINE
                    Comment = Marshal.StringToCoTaskMemUni($"FastRDP Profile Credential - {DateTime.Now:yyyy-MM-dd}")
                };

                Marshal.Copy(passwordBytes, 0, credential.CredentialBlob, passwordBytes.Length);

                bool result = CredWrite(ref credential, 0);

                // Belleği temizle
                Marshal.FreeCoTaskMem(credential.TargetName);
                Marshal.FreeCoTaskMem(credential.UserName);
                Marshal.FreeCoTaskMem(credential.Comment);
                Marshal.FreeCoTaskMem(credential.CredentialBlob);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kimlik bilgisi kaydetme hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kimlik bilgilerini Windows Credential Manager'dan okur
        /// </summary>
        /// <param name="profileId">Profil ID'si</param>
        /// <param name="useDecryption">DPAPI şifre çözme kullanılsın mı</param>
        /// <returns>(username, password) tuple</returns>
        public (string username, string password) GetCredential(string profileId, bool useDecryption = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(profileId))
                    return (null, null);

                var target = TargetPrefix + profileId;

                if (!CredRead(target, CRED_TYPE.GENERIC, 0, out IntPtr credPtr))
                {
                    return (null, null);
                }

                var credential = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
                
                // Username
                var username = Marshal.PtrToStringUni(credential.UserName);

                // Password
                var passwordBytes = new byte[credential.CredentialBlobSize];
                Marshal.Copy(credential.CredentialBlob, passwordBytes, 0, (int)credential.CredentialBlobSize);

                // DPAPI şifre çözme
                if (useDecryption)
                {
                    try
                    {
                        passwordBytes = ProtectedData.Unprotect(
                            passwordBytes,
                            GetEntropy(),
                            DataProtectionScope.CurrentUser
                        );
                    }
                    catch
                    {
                        // Şifrelenmemiş olabilir, devam et
                    }
                }

                var password = Encoding.UTF8.GetString(passwordBytes);

                CredFree(credPtr);

                return (username, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kimlik bilgisi okuma hatası: {ex.Message}");
                return (null, null);
            }
        }

        /// <summary>
        /// Kimlik bilgilerini Windows Credential Manager'dan siler
        /// </summary>
        /// <param name="profileId">Profil ID'si</param>
        public bool DeleteCredential(string profileId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(profileId))
                    return false;

                var target = TargetPrefix + profileId;
                return CredDelete(target, CRED_TYPE.GENERIC, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kimlik bilgisi silme hatası: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kimlik bilgilerinin mevcut olup olmadığını kontrol eder
        /// </summary>
        /// <param name="profileId">Profil ID'si</param>
        public bool CredentialExists(string profileId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(profileId))
                    return false;

                var target = TargetPrefix + profileId;

                if (!CredRead(target, CRED_TYPE.GENERIC, 0, out IntPtr credPtr))
                {
                    return false;
                }

                CredFree(credPtr);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// DPAPI için entropy (ek güvenlik katmanı)
        /// </summary>
        private byte[] GetEntropy()
        {
            // Makine bazlı bir entropy kullan
            var machineName = Environment.MachineName;
            var userName = Environment.UserName;
            var combined = $"{machineName}_{userName}_FastRDP_Salt";
            return SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        }

        /// <summary>
        /// Şifreyi DPAPI ile şifreler (dosyaya kaydetmek için)
        /// </summary>
        /// <param name="plainText">Şifrelenmemiş metin</param>
        public string EncryptString(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return string.Empty;

            try
            {
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var encryptedBytes = ProtectedData.Protect(
                    plainBytes,
                    GetEntropy(),
                    DataProtectionScope.CurrentUser
                );
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Şifreleme hatası: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// DPAPI ile şifrelenmiş metni çözer
        /// </summary>
        /// <param name="encryptedText">Şifrelenmiş metin (Base64)</param>
        public string DecryptString(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
                return string.Empty;

            try
            {
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                var decryptedBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    GetEntropy(),
                    DataProtectionScope.CurrentUser
                );
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Şifre çözme hatası: {ex.Message}");
                return string.Empty;
            }
        }
    }
}

