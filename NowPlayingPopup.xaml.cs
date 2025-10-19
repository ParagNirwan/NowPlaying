using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Now_Playing
{
    public partial class NowPlayingPopup : Window
    {
        private readonly DoubleAnimation fadeIn;
        private readonly DoubleAnimation fadeOut;
        private readonly System.Windows.Threading.DispatcherTimer autoHideTimer;

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int value);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        public NowPlayingPopup()
        {
            InitializeComponent();

            fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));

            // Timer for auto-hide
            autoHideTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            autoHideTimer.Tick += (s, e) =>
            {
                autoHideTimer.Stop();
                BeginAnimation(OpacityProperty, fadeOut);
                fadeOut.Completed += (o, ev) => Hide();
            };

            // Make window topmost over games
            Loaded += (s, e) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TOPMOST | WS_EX_TOOLWINDOW);
            };
        }

        public void ShowPopup(string title, string artist, BitmapImage? albumArt = null)
        {
            Dispatcher.Invoke(() =>
            {
                // Update song info
                TitleText.Text = title;
                ArtistText.Text = artist;

                if (albumArt != null)
                    AlbumArt.Source = albumArt;

                // Position at bottom right
                Left = SystemParameters.WorkArea.Width - Width - 20;
                Top = SystemParameters.WorkArea.Height - Height - 20;

                // Fade in
                BeginAnimation(OpacityProperty, fadeIn);
                Show();

                // Reset timer for rapid song changes
                autoHideTimer.Stop();
                autoHideTimer.Start();
            });
        }
    }
}
