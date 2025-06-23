# TreaYT Downloader

A modern Windows application for downloading YouTube videos, built with WinUI 3 and yt-dlp. Featuring a beautiful, native Windows 11 design and powerful download capabilities.

## Features

- Modern Windows 11 design using WinUI 3
- Download YouTube videos in any available quality (up to 8K)
- Download entire playlists with customizable options
- Extract audio in various formats (MP3, M4A, etc.)
- Light/Dark/System theme support
- Comprehensive download history with search and filtering
- Multiple concurrent downloads with progress tracking
- Thumbnail and metadata embedding
- Subtitle download and embedding support
- System tray integration with notifications
- Auto-updates via GitHub releases

## Requirements

- Windows 10 version 1809 (build 17763) or later
- [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- [FFmpeg](https://www.ffmpeg.org/download.html) (optional, required for some features)

## Installation

### Microsoft Store (Recommended)

Coming soon!

### Manual Installation

1. Download the latest release from the [Releases](https://github.com/firozalambda/treayt/releases) page
2. Extract the ZIP file to your desired location
3. Run `TreaYT.exe`

### Building from Source

1. Prerequisites:
   - Visual Studio 2022 with Windows App SDK workload
   - Windows 10 SDK (10.0.19041.0)
   - .NET 8.0 SDK

2. Clone the repository:
   ```powershell
   git clone https://github.com/firozalambda/treayt.git
   cd treayt
   ```

3. Build and run:
   ```powershell
   dotnet build
   dotnet run
   ```

## Usage

1. Launch TreaYT Downloader
2. Paste a YouTube URL in the input field
3. Click "Analyze" to fetch available formats
4. Select your preferred format and options:
   - Video quality
   - Audio format (for audio-only downloads)
   - Embed thumbnail
   - Embed metadata
   - Download subtitles
5. Choose download location
6. Click "Download"

### Settings

- **Theme**: Choose between Light, Dark, or System theme
- **Download Path**: Set default download location
- **FFmpeg Path**: Set path to FFmpeg executable
- **Concurrent Downloads**: Set maximum number of simultaneous downloads
- **Format Options**: Configure preferred video/audio formats
- **Embedding Options**: Configure thumbnail and metadata embedding
- **Subtitle Options**: Configure subtitle download preferences

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [yt-dlp](https://github.com/yt-dlp/yt-dlp) - The powerful YouTube downloader
- [Windows App SDK](https://github.com/microsoft/WindowsAppSDK) - Modern Windows UI framework
- [YoutubeDLSharp](https://github.com/Bluegrams/YoutubeDLSharp) - .NET wrapper for yt-dlp
- [Seal](https://github.com/JunkFood02/Seal) - Inspiration for the project