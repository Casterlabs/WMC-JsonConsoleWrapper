using System;
using WindowsMediaController;
using Windows.Media.Control;
using System.Text.Json;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using Windows.Media;

namespace WMC_JsonConsoleWrapper
{
    class Program
    {
        public static void Main(string[] args)
        {

            // Parse the commandline args.
            // no_thumbnail : Disables the generation of the thumbnail base64
            foreach (string arg in args)
            {
                switch (arg)
                {

                    case "no_thumbnail": {
                        MediaSessionListener.ParseThumbnail = false;
                        break;
                    }

                }
            }

            MediaSessionListener.StartSessionListener();

            // Wait for input, when we receive that the application exits.
            // Is this entire library janky? You betcha!
            Console.ReadLine();

            MediaSessionListener.StopSessionListener();
        }
    }

    class MediaSessionListener
    {
        private static MediaManager mediaManager;
        private static bool started = false;

        public static bool ParseThumbnail { get; set; } = true;

        public static void StartSessionListener()
        {
            if (!started)
            {
                mediaManager = new MediaManager();
                started = true;

                mediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;
                mediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;
                mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
                mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;

                mediaManager.Start();
            }
        }

        public static void StopSessionListener()
        {
            if (started)
            {
                mediaManager.Dispose();
                started = false;
            }
        }

        private static void MediaManager_OnAnySessionOpened(MediaManager.MediaSession session)
        {
            WriteEvent(session.Id, "SESSION_STARTED", null);
        }
        private static void MediaManager_OnAnySessionClosed(MediaManager.MediaSession session)
        {
            WriteEvent(session.Id, "SESSION_CLOSED", null);
        }

        private static void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
        {
            WriteEvent(
                sender.Id,
                "PLAYBACK_STATE_CHANGE",
                new PlaybackStateEvent
                {
                    PlaybackStatus = args.PlaybackStatus.ToString(),
                    PlaybackType = args.PlaybackType.ToString()
                }
            );
        }

        private static async void MediaManager_OnAnyMediaPropertyChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
        {
            string thumbnailB64 = null;
            string thumbnailType = null;

            if (args.Thumbnail != null && ParseThumbnail) {
                // Grab the stream and it's size.
                var stream = await args.Thumbnail.OpenReadAsync();
                uint streamSize = (uint)stream.Size;

                // Read said stream into a buffer, taking care to dispose the stream.
                var buf = new Windows.Storage.Streams.Buffer(streamSize);

                await stream.ReadAsync(buf, streamSize, Windows.Storage.Streams.InputStreamOptions.None);
                stream.Dispose();

                // Get the buffer as an array of bytes, then convert the bytes to base64.
                byte[] bytes = buf.ToArray();

                thumbnailB64 = Convert.ToBase64String(bytes);

                // We don't actually know the true mime type, however some googling suggests that it's just jpeg.
                thumbnailType = "image/jpeg";
            }
            
            WriteEvent(
                sender.Id, 
                "PROPERTY_CHANGE", 
                new MediaInfo
                {
                    AlbumArtist     = args.AlbumArtist,
                    AlbumTitle      = args.AlbumTitle,
                    AlbumTrackCount = args.AlbumTrackCount,
                    Artist          = args.Artist,
                    Genres          = args.Genres,
                    PlaybackType    = args.PlaybackType?.ToString(),
                    Subtitle        = args.Subtitle,
                    Thumbnail       = thumbnailB64,
                    ThumbnailType   = thumbnailType,
                    Title           = args.Title,
                    TrackNumber     = args.TrackNumber
                }
            ); 
        }


        private static void WriteEvent(string sessionId, string type, object data)
        {
            var playbackEvent = new PlaybackEvent
            {
                Type = type,
                SessionId = sessionId,
                Data = data
            };

            var jsonEvent = JsonSerializer.Serialize(playbackEvent);

            Console.WriteLine(jsonEvent);
        }
    }

    public class PlaybackEvent
    {
        public string Type { get; set; }
        public string SessionId { get; set; }
        public object Data { get; set; }
    }

    // ----------------------------------------------
    // Everything below this line is just stuff that needed to be remade for convenience, apologies for the shitcode.
    // ----------------------------------------------

    public class PlaybackStateEvent
    {
        public string PlaybackType { get; set; }
        public string PlaybackStatus { get; set; }
    }

    // This is a copy of GlobalSystemMediaTransportControlsSessionMediaProperties.
    public class MediaInfo
    {
   
        public string AlbumArtist { get; set; }
        
        public string AlbumTitle { get; set; }
        
        public int AlbumTrackCount { get; set; }
        
        public string Artist { get; set; }
       
        public IReadOnlyList<string> Genres { get; set; }

        // Changed.
        public String PlaybackType { get; set; }

        public string Subtitle { get; set; }

        // Changed.
        public String Thumbnail { get; set; }

        // Added.
        public String ThumbnailType { get; set; }

        public String Title { get; set; }

        public int TrackNumber { get; set; }
    }

}
