using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ReplayOverlay
{
    internal sealed class Bootstrapper
    {
        private const string ObsExePath = @"C:\Program Files\obs-studio\bin\64bit\obs64.exe";
        private readonly int _port;
        private readonly string _password;

        public Bootstrapper(int port, string password)
        {
            _port = port;
            _password = password;
        }

        public async Task RunAsync()
        {
            await EnsureObsRunningAsync();

            var overlayWindow = new OverlayWindow();
            overlayWindow.Show();

            var obsConnector = new ObsConnector(_port, _password);
            obsConnector.Connected += async () => await obsConnector.EnsureReplayBufferRunningAsync();

            await obsConnector.ConnectAsync();
            overlayWindow.AttachObs(obsConnector);
        }

        private static async Task EnsureObsRunningAsync()
        {
            if (Process.GetProcessesByName("obs64").Any())
                return;

            if (!File.Exists(ObsExePath))
            {
                MessageBox.Show($"OBS not found: {ObsExePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = ObsExePath,
                WorkingDirectory = Path.GetDirectoryName(ObsExePath)!,
                Arguments = "--minimize-to-tray --startreplaybuffer --disable-shutdown-check",
                UseShellExecute = true
            };
            Process.Start(startInfo);
            await Task.Delay(4000);
        }
    }
}
