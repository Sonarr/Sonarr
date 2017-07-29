using System;
using System.Globalization;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public static class MediaInfoFormatter
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(MediaInfoFormatter));

        public static decimal FormatAudioChannels(MediaInfoModel mediaInfo)
        {
            var audioChannelPositions = mediaInfo.AudioChannelPositions;
            var audioChannelPositionsText = mediaInfo.AudioChannelPositionsText;
            var audioChannels = mediaInfo.AudioChannels;

            if (audioChannelPositions.IsNullOrWhiteSpace())
            {
                if (audioChannelPositionsText.IsNullOrWhiteSpace())
                {
                    if (mediaInfo.SchemaRevision >= 3)
                    {
                        return audioChannels;
                    }

                    return 0;
                }

                return mediaInfo.AudioChannelPositionsText.ContainsIgnoreCase("LFE") ? audioChannels - 1 + 0.1m : audioChannels;
            }

            return audioChannelPositions.Replace("Object Based / ", "")
                                        .Split(new string[] { " / " }, StringSplitOptions.None)
                                        .First()
                                        .Split('/')
                                        .Sum(s => decimal.Parse(s, CultureInfo.InvariantCulture));
        }

        public static string FormatAudioCodec(MediaInfoModel mediaInfo, string sceneName)
        {
            var audioFormat = mediaInfo.AudioFormat;

            if (audioFormat.IsNullOrWhiteSpace())
            {
                return audioFormat;
            }

            if (audioFormat == "AC-3")
            {
                return "AC3";
            }

            if (audioFormat == "E-AC-3")
            {
                return "EAC3";
            }

            if (audioFormat == "AAC")
            {
                return "AAC";
            }

            if (audioFormat == "MPEG Audio")
            {
                return mediaInfo.AudioProfile == "Layer 3" ? "MP3" : audioFormat;
            }

            if (audioFormat == "DTS")
            {
                return "DTS";
            }

            if (audioFormat.Equals("FLAC", StringComparison.OrdinalIgnoreCase))
            {
                return "FLAC";
            }

            if (audioFormat.Equals("Vorbis", StringComparison.OrdinalIgnoreCase))
            {
                return "Vorbis";
            }

            Logger.Error(new UnknownCodecException(audioFormat, sceneName), "Unknown audio format: {0} in '{1}'. Please notify Sonarr developers.", audioFormat, sceneName);
            return audioFormat;
        }

        public static string FormatVideoCodec(MediaInfoModel mediaInfo, string sceneName)
        {
            var videoCodec = mediaInfo.VideoCodec;

            if (videoCodec.IsNullOrWhiteSpace())
            {
                return videoCodec;
            }

            if (videoCodec == "AVC")
            {
                return sceneName.IsNotNullOrWhiteSpace() && Path.GetFileNameWithoutExtension(sceneName).Contains("h264")
                    ? "h264"
                    : "x264";
            }

            if (videoCodec == "V_MPEGH/ISO/HEVC" || videoCodec == "HEVC")
            {
                return sceneName.IsNotNullOrWhiteSpace() && Path.GetFileNameWithoutExtension(sceneName).Contains("h265")
                    ? "h265"
                    : "x265";
            }

            if (videoCodec == "MPEG-2 Video")
            {
                return "MPEG2";
            }

            if (videoCodec.StartsWith("XviD", StringComparison.OrdinalIgnoreCase))
            {
                return "XviD";
            }

            if (videoCodec.StartsWith("DivX", StringComparison.OrdinalIgnoreCase))
            {
                return "DivX";
            }

            if (videoCodec.Equals("VC-1", StringComparison.OrdinalIgnoreCase))
            {
                return "VC1";
            }

            Logger.Error(new UnknownCodecException(videoCodec, sceneName), "Unknown video codec: {0} in '{1}'. Please notify Sonarr developers.", videoCodec, sceneName);
            return videoCodec;
        }
    }
}
