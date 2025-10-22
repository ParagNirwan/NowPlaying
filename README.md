🎵 Now Playing – Desktop Media Popup

Now Playing is a lightweight WPF desktop app that displays the title and artist of your currently playing media in a small popup notification — even while you’re in a game or another fullscreen application.

It integrates with the Windows Global Media Controls API (SMTC) to detect what’s playing in apps like Chrome, Spotify, or YouTube Music, and shows a clean, minimal popup with the song details and album art.

✨ Features

🪶 Lightweight & non-intrusive — built using WPF and runs quietly in the background.

🎮 Game-friendly — the popup appears on top of fullscreen games or other applications.

🎧 Auto-detects media — works with any app that supports Windows media controls (Chrome, Spotify, VLC, etc.).

🖼️ Shows album art when available.

⚙️ Customizable popup position — choose between top or bottom of the screen.

🧠 Smart detection — hides automatically when playback stops or a tab closes.

🧩 How It Works

The app uses the GlobalSystemMediaTransportControlsSessionManager API from Windows.Media.Control to track active media sessions.
When a session starts playing, it extracts metadata such as:

Title

Artist

Album Art

Then, it displays this information in a transparent WPF popup that fades in and out automatically.

🛠️ Tech Stack
Component	Description
Language	C# (.NET 8 / WPF)
API	Windows Global System Media Transport Controls (SMTC)
UI	WPF Popup Window
Frameworks	Windows SDK, WinRT Interop
Platform	Windows 10 / 11
