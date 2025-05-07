using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace ReplayOverlay
{
    public partial class NotificationWindow : Window
    {
        private static NotificationWindow? _instance;
        public static NotificationWindow Instance => _instance ??= new NotificationWindow();

        private NotificationWindow()
        {
            InitializeComponent();
            Opacity = 0;
            Visibility = Visibility.Hidden;
        }

        public async void ShowNotification()
        {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Left + 20;
            Top = workArea.Top + 20;

            Visibility = Visibility.Visible;
            Opacity = 0;

            BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250)));
            await Task.Delay(250);
            await Task.Delay(3000);
            BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250)));
            await Task.Delay(250);
            Visibility = Visibility.Hidden;
        }
    }
}
