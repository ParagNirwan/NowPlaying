using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Now_Playing
{
    public partial class NowPlayingPopup : Window
    {
        private readonly DispatcherTimer autoHideTimer;

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int value);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        private double startOffset;
        private double targetTop;

        public NowPlayingPopup()
        {
            InitializeComponent();

            // Initialize transform for animation
            RootGrid.RenderTransform = new System.Windows.Media.TranslateTransform();

            autoHideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            autoHideTimer.Tick += (s, e) =>
            {
                autoHideTimer.Stop();
                SlideOut();
            };

            Loaded += (s, e) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TOPMOST | WS_EX_TOOLWINDOW);
            };
        }

        public void ShowPopup(string title, string artist, BitmapImage? albumArt = null, string popupPosition = "Bottom")
        {
            Dispatcher.Invoke(() =>
            {
                TitleText.Text = title;
                ArtistText.Text = artist;
                AlbumArt.Source = albumArt;

                // Horizontal center
                Left = (SystemParameters.WorkArea.Width - Width) / 2;

                // Determine vertical position
                if (popupPosition == "Top")
                {
                    targetTop = 20;
                    startOffset = -Height; // slide down from above
                }
                else
                {
                    targetTop = SystemParameters.WorkArea.Height - Height - 20;
                    startOffset = Height; // slide up from below
                }

                Top = targetTop;

                // Reset transform for new slide
                ((System.Windows.Media.TranslateTransform)RootGrid.RenderTransform).Y = startOffset;

                Show();

                SlideIn();
            });
        }

        private void SlideIn()
        {
            var anim = new DoubleAnimation
            {
                From = startOffset,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(400),
                DecelerationRatio = 0.7
            };
            anim.Completed += (s, e) => autoHideTimer.Start();
            ((System.Windows.Media.TranslateTransform)RootGrid.RenderTransform).BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, anim);
        }

        private void SlideOut()
        {
            double endOffset = (targetTop < SystemParameters.WorkArea.Height / 2) ? -Height : Height;
            var anim = new DoubleAnimation
            {
                From = 0,
                To = endOffset,
                Duration = TimeSpan.FromMilliseconds(200),
                AccelerationRatio = 1.0
            };
            anim.Completed += (s, e) => Hide();
            ((System.Windows.Media.TranslateTransform)RootGrid.RenderTransform).BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, anim);
        }
    }
}
