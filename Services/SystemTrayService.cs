using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace FastRDP.Services
{
    /// <summary>
    /// Sistem tepsisi (system tray) yönetimi için servis
    /// Windows Shell_NotifyIcon API kullanır
    /// </summary>
    public class SystemTrayService : IDisposable
    {
        private const int WM_USER = 0x0400;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_LBUTTONDBLCLK = 0x0203;

        private readonly Window _window;
        private NOTIFYICONDATA _notifyIconData;
        private IntPtr _windowHandle;
        private bool _disposed = false;

#pragma warning disable CS0067 // Event is declared but never used (gelecek kullanım için)
        public event EventHandler OnLeftClick;
        public event EventHandler OnRightClick;
        public event EventHandler OnDoubleClick;
#pragma warning restore CS0067

        public SystemTrayService(Window window)
        {
            _window = window;
            _windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        }

        /// <summary>
        /// Sistem tepsisine icon ekler
        /// </summary>
        public bool AddTrayIcon(string tooltip = "FastRDP")
        {
            try
            {
                _notifyIconData = new NOTIFYICONDATA
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                    hWnd = _windowHandle,
                    uID = 1,
                    uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP,
                    uCallbackMessage = WM_USER + 1,
                    hIcon = LoadApplicationIcon(),
                    szTip = tooltip
                };

                return Shell_NotifyIcon(NIM_ADD, ref _notifyIconData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tray icon eklenemedi: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sistem tepsisinden icon'u kaldırır
        /// </summary>
        public bool RemoveTrayIcon()
        {
            try
            {
                return Shell_NotifyIcon(NIM_DELETE, ref _notifyIconData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tray icon kaldırılamadı: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tray icon tooltip'ini günceller
        /// </summary>
        public bool UpdateTooltip(string tooltip)
        {
            try
            {
                _notifyIconData.szTip = tooltip;
                return Shell_NotifyIcon(NIM_MODIFY, ref _notifyIconData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tooltip güncellenemedi: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Pencereyi göster/gizle
        /// </summary>
        public void ToggleWindowVisibility()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
            
            if (IsWindowVisible(hWnd))
            {
                ShowWindow(hWnd, SW_HIDE);
            }
            else
            {
                ShowWindow(hWnd, SW_SHOW);
                SetForegroundWindow(hWnd);
            }
        }

        /// <summary>
        /// Pencereyi göster
        /// </summary>
        public void ShowWindow()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
            ShowWindow(hWnd, SW_SHOW);
            SetForegroundWindow(hWnd);
        }

        /// <summary>
        /// Pencereyi gizle
        /// </summary>
        public void HideWindow()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
            ShowWindow(hWnd, SW_HIDE);
        }

        private IntPtr LoadApplicationIcon()
        {
            // Uygulama iconunu yükle (varsayılan olarak LoadIcon kullan)
            return LoadIcon(IntPtr.Zero, (IntPtr)32512); // IDI_APPLICATION
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                RemoveTrayIcon();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        #region P/Invoke Declarations

        private const uint NIF_MESSAGE = 0x00000001;
        private const uint NIF_ICON = 0x00000002;
        private const uint NIF_TIP = 0x00000004;

        private const uint NIM_ADD = 0x00000000;
        private const uint NIM_MODIFY = 0x00000001;
        private const uint NIM_DELETE = 0x00000002;

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NOTIFYICONDATA
        {
            public uint cbSize;
            public IntPtr hWnd;
            public uint uID;
            public uint uFlags;
            public uint uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        #endregion
    }
}

