using System;
using System.IO;
using System.Threading.Tasks;

namespace FastRDP.Services
{
    /// <summary>
    /// Merkezi hata yönetimi servisi
    /// </summary>
    public class ErrorHandlerService
    {
        private readonly string _logPath;
        private static ErrorHandlerService _instance;
        private static readonly object _lock = new object();

        private ErrorHandlerService()
        {
            _logPath = Path.Combine("Data", "logs");
            EnsureLogDirectoryExists();
        }

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static ErrorHandlerService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ErrorHandlerService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Hata seviyesi
        /// </summary>
        public enum ErrorLevel
        {
            Info,
            Warning,
            Error,
            Critical
        }

        #region Events

        /// <summary>
        /// Kullanıcı dostu bir hata mesajı gösterilmesi gerektiğinde tetiklenir
        /// </summary>
        public event EventHandler<ErrorEventArgs> OnErrorOccurred;

        /// <summary>
        /// Bilgi mesajı gösterilmesi gerektiğinde tetiklenir
        /// </summary>
        public event EventHandler<InfoEventArgs> OnInfoMessageOccurred;

        #endregion

        #region Public Methods

        /// <summary>
        /// Hata loglar ve kullanıcıya gösterir
        /// </summary>
        public async Task HandleErrorAsync(Exception exception, string userMessage = null, ErrorLevel level = ErrorLevel.Error)
        {
            // Log dosyasına yaz
            await LogErrorAsync(exception, level);

            // Kullanıcıya göster
            var message = string.IsNullOrWhiteSpace(userMessage) 
                ? GetUserFriendlyMessage(exception) 
                : userMessage;

            OnErrorOccurred?.Invoke(this, new ErrorEventArgs
            {
                Title = GetErrorTitle(level),
                Message = message,
                Level = level,
                Exception = exception
            });
        }

        /// <summary>
        /// Bilgi mesajı gösterir
        /// </summary>
        public void ShowInfo(string message, string title = "Bilgi")
        {
            OnInfoMessageOccurred?.Invoke(this, new InfoEventArgs
            {
                Title = title,
                Message = message
            });
        }

        /// <summary>
        /// Başarı mesajı gösterir
        /// </summary>
        public void ShowSuccess(string message, string title = "Başarılı")
        {
            OnInfoMessageOccurred?.Invoke(this, new InfoEventArgs
            {
                Title = title,
                Message = message,
                IsSuccess = true
            });
        }

        /// <summary>
        /// Uyarı mesajı gösterir
        /// </summary>
        public void ShowWarning(string message, string title = "Uyarı")
        {
            OnInfoMessageOccurred?.Invoke(this, new InfoEventArgs
            {
                Title = title,
                Message = message,
                IsWarning = true
            });
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Log klasörünün var olduğundan emin olur
        /// </summary>
        private void EnsureLogDirectoryExists()
        {
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
        }

        /// <summary>
        /// Hatayı log dosyasına yazar
        /// </summary>
        private async Task LogErrorAsync(Exception exception, ErrorLevel level)
        {
            try
            {
                var logFileName = $"errors_{DateTime.Now:yyyyMMdd}.log";
                var logFilePath = Path.Combine(_logPath, logFileName);

                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {exception.Message}\n" +
                              $"Stack Trace: {exception.StackTrace}\n" +
                              $"--------------------------------\n";

                await File.AppendAllTextAsync(logFilePath, logEntry);

                // Eski log dosyalarını temizle (30 günden eskiler)
                CleanOldLogs();
            }
            catch
            {
                // Log yazarken hata olursa sessizce geç
            }
        }

        /// <summary>
        /// 30 günden eski log dosyalarını siler
        /// </summary>
        private void CleanOldLogs()
        {
            try
            {
                var files = Directory.GetFiles(_logPath, "errors_*.log");
                var threshold = DateTime.Now.AddDays(-30);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < threshold)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch
            {
                // Temizlik başarısız olursa sessizce geç
            }
        }

        /// <summary>
        /// Exception'dan kullanıcı dostu mesaj üretir
        /// </summary>
        private string GetUserFriendlyMessage(Exception exception)
        {
            return exception switch
            {
                FileNotFoundException _ => "Dosya bulunamadı. Lütfen dosyanın var olduğundan emin olun.",
                UnauthorizedAccessException _ => "Dosyaya erişim reddedildi. Lütfen yönetici olarak çalıştırmayı deneyin.",
                IOException _ => "Dosya işlemi sırasında bir hata oluştu. Dosyanın başka bir program tarafından kullanılmadığından emin olun.",
                InvalidOperationException _ => "Geçersiz işlem. Lütfen işlemi tekrar deneyin.",
                ArgumentException _ => "Geçersiz parametre. Lütfen girdiğiniz bilgileri kontrol edin.",
                _ => $"Beklenmeyen bir hata oluştu: {exception.Message}"
            };
        }

        /// <summary>
        /// Hata seviyesine göre başlık döner
        /// </summary>
        private string GetErrorTitle(ErrorLevel level)
        {
            return level switch
            {
                ErrorLevel.Info => "Bilgi",
                ErrorLevel.Warning => "Uyarı",
                ErrorLevel.Error => "Hata",
                ErrorLevel.Critical => "Kritik Hata",
                _ => "Hata"
            };
        }

        #endregion

        #region Event Args

        public class ErrorEventArgs : EventArgs
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public ErrorLevel Level { get; set; }
            public Exception Exception { get; set; }
        }

        public class InfoEventArgs : EventArgs
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public bool IsSuccess { get; set; }
            public bool IsWarning { get; set; }
        }

        #endregion
    }
}

