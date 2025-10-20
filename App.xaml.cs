using System;
using System.Windows;
using System.Drawing;
using WinForms = System.Windows.Forms;

namespace Now_Playing
{
    public partial class App : System.Windows.Application
    {
        private WinForms.NotifyIcon? _notifyIcon;
        private bool _isExit;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create NotifyIcon (system tray)
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("Resources/NowPlaying.ico"),
                Visible = true,
                Text = "Now Playing"
            };

            // Double-click tray icon to restore
            _notifyIcon.DoubleClick += (s, ev) => ShowMainWindow();

            // Context menu for tray icon (use ContextMenuStrip)
            var menu = new WinForms.ContextMenuStrip();
            menu.Items.Add("Open", null, (s, ev) => ShowMainWindow());
            menu.Items.Add("Exit", null, (s, ev) => ExitApp());
            _notifyIcon.ContextMenuStrip = menu;

            // Create main window and handle closing
            MainWindow = new MainWindow();
            MainWindow.Closing += MainWindow_Closing;
            MainWindow.Show();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true; // Cancel closing
                MainWindow.Hide(); // Hide instead
                _notifyIcon?.ShowBalloonTip(
                    1000,
                    "Now Playing",
                    "App minimized to tray",
                    WinForms.ToolTipIcon.Info
                );
            }
        }

        private void ShowMainWindow()
        {
            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
                MainWindow.Closing += MainWindow_Closing;
            }

            MainWindow.Show();
            MainWindow.WindowState = WindowState.Normal;
            MainWindow.Activate();
        }

        private void ExitApp()
        {
            _isExit = true;
            _notifyIcon?.Dispose();
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _isExit = true;
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
