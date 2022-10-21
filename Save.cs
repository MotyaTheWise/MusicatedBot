using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NReco.VideoConverter;
using System.Diagnostics;
using System.Collections;
using YoutubeExplode;
using YoutubeExplode.Converter;
using FFMpegSharp;
using FFMpegSharp.FFMPEG;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MusicatedBot
{
    class Saver
    {
        

        public static async Task SaveTrackAsync(string SaveToFolder, string VideoURL, string MP3Name)
        {

            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(VideoURL);
            var title = video.Title;
            var author = video.Author;
            var duration = video.Duration;

            await youtube.Videos.DownloadAsync(VideoURL, "track.mp3", o => o
                .SetContainer("mp3") // override format
                .SetPreset(ConversionPreset.UltraFast) // change preset

                .SetFFmpegPath("ffmpeg.exe") // custom FFmpeg location
);
            string oldPath = $"track.mp3";
            string newPath = $"compressed\\{MP3Name}.mp3";
            File.Copy(oldPath, newPath, true);
            File.Delete(oldPath);

            Console.WriteLine("checked");

        }

        public static async Task SaveVideoAsync(string SaveToFolder, string VideoURL, string VideoName)
        {

            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(VideoURL);
            var title = video.Title;
            var author = video.Author;
            var duration = video.Duration;

            await youtube.Videos.DownloadAsync(VideoURL, "video.webm", o => o
                .SetContainer("webm") // override format
                .SetPreset(ConversionPreset.VerySlow) // change preset
                
                .SetFFmpegPath("ffmpeg.exe") // custom FFmpeg location
);
            string oldPath = $"video.webm";
            string newPath = $"compressed\\{VideoName}.webm";
            File.Copy(oldPath, newPath, true);



            File.Delete(oldPath);

            Console.WriteLine("checked");


        }
    }
}

