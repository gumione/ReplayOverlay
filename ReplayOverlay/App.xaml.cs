using System.Linq;
using System.Windows;
using ReplayOverlay.Utils;

namespace ReplayOverlay
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var settings = SettingsService.Load();
            if (settings.ShowQuickSetup)
            {
                var quickSetupWindow = new QuickSetupWindow(settings);
                quickSetupWindow.ShowDialog();
            }

            var bootstrapper = new Bootstrapper(settings.WebsocketPort, settings.WebsocketPassword);
            await bootstrapper.RunAsync();

            var overlayWindow = Current.Windows
                .OfType<OverlayWindow>()
                .FirstOrDefault();
            if (overlayWindow != null)
            {
                MainWindow = overlayWindow;
                ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
        }
    }
}
