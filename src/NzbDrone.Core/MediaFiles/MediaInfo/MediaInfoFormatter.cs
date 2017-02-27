using System;
using System.Globalization;
using System.IO;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public static class MediaInfoFormatter
    {
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

        public static string FormatAudioCodec(MediaInfoModel mediaInfo)
        {
            var audioFormat = mediaInfo.AudioFormat;

            if (audioFormat == "AC-3")
            {
                return "AC3";
            }

            if (audioFormat == "E-AC-3")
            {
                return "EAC3";
            }

            if (audioFormat == "MPEG Audio")
            {
                return mediaInfo.AudioProfile == "Layer 3" ? "MP3" : audioFormat;
            }

            return audioFormat;
        }

        public static string FormatVideoCodec(MediaInfoModel mediaInfo, string sceneName)
        {
            var videoCodec = mediaInfo.VideoCodec;

            if (videoCodec == "AVC")
            {
                return sceneName.IsNotNullOrWhiteSpace() && Path.GetFileNameWithoutExtension(sceneName).Contains("h264")
                    ? "h264"
                    : "x264";
            }

            if (videoCodec == "V_MPEGH/ISO/HEVC")
            {
                return sceneName.IsNotNullOrWhiteSpace() && Path.GetFileNameWithoutExtension(sceneName).Contains("h265")
                    ? "h265"
                    : "x265";
            }

            if (videoCodec == "MPEG-2 Video")
            {
                return "MPEG2";
            }

            return videoCodec;
        }
    }
}
