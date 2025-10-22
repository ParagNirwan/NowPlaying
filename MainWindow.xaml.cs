using System;
using System.Threading.Tasks;
using System.Windows;
using Windows.Media.Control;
using Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using Windows.Storage.Streams;
using System.Windows.Controls.Primitives;

namespace Now_Playing
{
    public partial class MainWindow : Window
    {
        private GlobalSystemMediaTransportControlsSessionManager _mediaManager;
        private NowPlayingPopup? popup;
        private string popupPosition = "Bottom"; // default

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeMediaManager();
        }

        private async Task InitializeMediaManager()
        {
            _mediaManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            _mediaManager.CurrentSessionChanged += MediaManager_CurrentSessionChanged;

            var currentSession = _mediaManager.GetCurrentSession();
            if (currentSession != null)
            {
                SubscribeToSession(currentSession);
                await ShowCurrentTrack(currentSession);
            }
        }

        private void SubscribeToSession(GlobalSystemMediaTransportControlsSession session)
        {
            session.MediaPropertiesChanged -= CurrentSession_MediaPropertiesChanged;
            session.PlaybackInfoChanged -= CurrentSession_PlaybackInfoChanged;

            session.MediaPropertiesChanged += CurrentSession_MediaPropertiesChanged;
            session.PlaybackInfoChanged += CurrentSession_PlaybackInfoChanged;
        }

        private async void MediaManager_CurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        {
            var currentSession = sender.GetCurrentSession();
            if (currentSession != null)
            {
                SubscribeToSession(currentSession);
                await ShowCurrentTrack(currentSession);
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    popup?.HidePopup();
                });
            }
        }

        private async void CurrentSession_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession session, MediaPropertiesChangedEventArgs args)
        {
            await ShowCurrentTrack(session);
        }

        private async void CurrentSession_PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession session, PlaybackInfoChangedEventArgs args)
        {
            await ShowCurrentTrack(session);
        }

        private async Task ShowCurrentTrack(GlobalSystemMediaTransportControlsSession session)
        {
            try
            {
                var playbackInfo = session.GetPlaybackInfo();
                var status = playbackInfo?.PlaybackStatus;

                // If not playing, hide popup
                if (status != GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                {
                    Dispatcher.Invoke(() =>
                    {
                        popup?.HidePopup();
                    });
                    return;
                }

                var props = await session.TryGetMediaPropertiesAsync();
                var title = props.Title;
                var artist = props.Artist;
                var albumArt = await GetAlbumArt(props);

                Dispatcher.Invoke(() =>
                {
                    if (popup == null)
                        popup = new NowPlayingPopup();

                    popup.ShowPopup(title, artist, albumArt, popupPosition);
                });

                Console.WriteLine($"{title} by {artist}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ShowCurrentTrack: {ex.Message}");
            }
        }

        private async Task<BitmapImage?> GetAlbumArt(GlobalSystemMediaTransportControlsSessionMediaProperties props)
        {
            if (props.Thumbnail == null)
                return null;

            var stream = await props.Thumbnail.OpenReadAsync();
            var bitmap = new BitmapImage();

            using (var memoryStream = new MemoryStream())
            {
                var dataReader = new DataReader(stream);
                var size = (uint)stream.Size;
                await dataReader.LoadAsync(size);
                byte[] bytes = new byte[size];
                dataReader.ReadBytes(bytes);
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Position = 0;

                bitmap.BeginInit();
                bitmap.StreamSource = memoryStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
            }

            return bitmap;
        }

        private void PopupPositionRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (TopCenterRadio.IsChecked == true)
                popupPosition = "Top";
            else if (BottomCenterRadio.IsChecked == true)
                popupPosition = "Bottom";
        }
    }
}
