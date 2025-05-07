using System;
using System.Diagnostics;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace ReplayOverlay
{
    public partial class OverlayWindow : Window
    {
        private ObsConnector? _obsConnector;
        private bool _isVisible = true;

        private const int HotkeyId = 0xBEEF;
        private const int WM_HOTKEY = 0x0312;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, int vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public OverlayWindow()
        {
            InitializeComponent();
            BtnExit.Click += OnExitClicked;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd)!.AddHook(WndProc);
            RegisterHotKey(hwnd, HotkeyId, MOD_CONTROL | MOD_ALT,
                KeyInterop.VirtualKeyFromKey(Key.O));
        }

        private async void OnExitClicked(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Exit overlay and OBS?",
                "Confirm Exit",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            if (_obsConnector != null)
            {
                _obsConnector.StopReplayBuffer();
                await Task.Delay(500);
            }

            foreach (var proc in Process.GetProcessesByName("obs64"))
            {
                try
                {
                    if (proc.CloseMainWindow() && !proc.WaitForExit(3000))
                        proc.Kill();
                    else if (!proc.CloseMainWindow())
                        proc.Kill();
                }
                catch
                {
                    proc.Kill();
                }
            }

            Application.Current.Shutdown();
        }

        public void AttachObs(ObsConnector connector)
        {
            _obsConnector = connector;
            _obsConnector.ReplayBufferStateChanged += OnBufferStateChanged;
            _obsConnector.ReplayBufferSaved += OnBufferSaved;
            Task.Run(() => OnBufferStateChanged(_obsConnector.GetReplayBufferStatus()));
        }

        private void OnBufferStateChanged(bool isActive)
        {
            Dispatcher.Invoke(() =>
            {
                StatusLabel.Text = isActive ? "Buffer: ON" : "Buffer: OFF";
            });
        }

        private void OnBufferSaved()
        {
            Dispatcher.Invoke(() =>
            {
                NotificationWindow.Instance.ShowNotification();
                try { SystemSounds.Asterisk.Play(); } catch { }
            });
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HotkeyId)
            {
                _isVisible = !_isVisible;
                Dispatcher.Invoke(() =>
                {
                    Visibility = _isVisible ? Visibility.Visible : Visibility.Hidden;
                });
                handled = true;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(hwnd, HotkeyId);
            base.OnClosed(e);
        }
    }
}
