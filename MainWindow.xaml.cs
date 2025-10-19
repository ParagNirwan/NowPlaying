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
                currentSession.MediaPropertiesChanged += CurrentSession_MediaPropertiesChanged;
                await ShowCurrentTrack(currentSession);
            }
        }

        private async void MediaManager_CurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        {
            var currentSession = sender.GetCurrentSession();
            if (currentSession != null)
            {
                currentSession.MediaPropertiesChanged += CurrentSession_MediaPropertiesChanged;
                await ShowCurrentTrack(currentSession);
            }
        }

        private async void CurrentSession_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession session, MediaPropertiesChangedEventArgs args)
        {
            await ShowCurrentTrack(session);
        }

        private async Task ShowCurrentTrack(GlobalSystemMediaTransportControlsSession session)
        {
            var props = await session.TryGetMediaPropertiesAsync();
            var title = props.Title;
            var artist = props.Artist;
            var albumArt = await GetAlbumArt(props);

            Dispatcher.Invoke(() =>
            {
                if (popup == null)
                    popup = new NowPlayingPopup();

                popup.ShowPopup(title, artist, albumArt);
            });

            Console.WriteLine($"{title} by {artist}");
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

                // Make the bitmap cross-thread accessible
                bitmap.Freeze();
            }

            return bitmap;
        }


    }
}
