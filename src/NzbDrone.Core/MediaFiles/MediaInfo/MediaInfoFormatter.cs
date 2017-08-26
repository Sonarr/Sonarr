using System;
using System.Globalization;
using System.IO;
using System.Linq;
using NLog;
using NLog.Fluent;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Instrumentation.Extensions;

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
                                        .Split(new string[] { " / " }, StringSplitOptions.RemoveEmptyEntries)
                                        .First()
                                        .Split('/')
                                        .Sum(s => decimal.Parse(s, CultureInfo.InvariantCulture));
        }

        public static string FormatAudioCodec(MediaInfoModel mediaInfo, string sceneName)
        {
            if (mediaInfo.AudioCodecID == null)
            {
                return FormatAudioCodecLegacy(mediaInfo, sceneName);
            }

            var audioFormat = mediaInfo.AudioFormat;
            var audioCodecID = mediaInfo.AudioCodecID ?? string.Empty;
            var audioProfile = mediaInfo.AudioProfile ?? string.Empty;
            var audioCodecLibrary = mediaInfo.AudioCodecLibrary ?? string.Empty;

            if (audioFormat.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            if (audioFormat.EqualsIgnoreCase("AC-3"))
            {
                return "AC3";
            }

            if (audioFormat.EqualsIgnoreCase("E-AC-3"))
            {
                return "EAC3";
            }

            if (audioFormat.EqualsIgnoreCase("AAC"))
            {
                if (audioCodecID == "A_AAC/MPEG4/LC/SBR")
                {
                    return "HE-AAC";
                }

                return "AAC";
            }

            if (audioFormat.EqualsIgnoreCase("DTS"))
            {
                return "DTS";
            }

            if (audioFormat.EqualsIgnoreCase("FLAC"))
            {
                return "FLAC";
            }

            if (audioFormat.Trim().EqualsIgnoreCase("mp3"))
            {
                return "MP3";
            }

            if (audioFormat.EqualsIgnoreCase("MPEG Audio"))
            {
                if (mediaInfo.AudioCodecID == "55" || mediaInfo.AudioCodecID == "A_MPEG/L3" || mediaInfo.AudioProfile == "Layer 3")
                {
                    return "MP3";
                }

                if (mediaInfo.AudioCodecID == "A_MPEG/L2" || mediaInfo.AudioProfile == "Layer 2")
                {
                    return "MP2";
                }
            }

            if (audioFormat.EqualsIgnoreCase("Opus"))
            {
                return "Opus";
            }

            if (audioFormat.EqualsIgnoreCase("PCM"))
            {
                return "PCM";
            }

            if (audioFormat.EqualsIgnoreCase("TrueHD"))
            {
                return "TrueHD";
            }

            if (audioFormat.EqualsIgnoreCase("Vorbis"))
            {
                return "Vorbis";
            }

            if (audioFormat == "WMA")
            {
                return "WMA";
            }

            Logger.Debug()
                  .Message("Unknown audio format: '{0}' in '{1}'.", string.Join(", ", audioFormat, audioCodecID, audioProfile, audioCodecLibrary), sceneName)
                  .WriteSentryWarn("UnknownAudioFormat", mediaInfo.ContainerFormat, audioFormat, audioCodecID)
                  .Write();

            return audioFormat;
        }

        public static string FormatAudioCodecLegacy(MediaInfoModel mediaInfo, string sceneName)
        {
            var audioFormat = mediaInfo.AudioFormat;

            if (audioFormat.IsNullOrWhiteSpace())
            {
                return audioFormat;
            }

            if (audioFormat.EqualsIgnoreCase("AC-3"))
            {
                return "AC3";
            }

            if (audioFormat.EqualsIgnoreCase("E-AC-3"))
            {
                return "EAC3";
            }

            if (audioFormat.EqualsIgnoreCase("AAC"))
            {
                return "AAC";
            }

            if (audioFormat.EqualsIgnoreCase("MPEG Audio") && mediaInfo.AudioProfile == "Layer 3")
            {
                return "MP3";
            }

            if (audioFormat.EqualsIgnoreCase("DTS"))
            {
                return "DTS";
            }

            if (audioFormat.EqualsIgnoreCase("TrueHD"))
            {
                return "TrueHD";
            }

            if (audioFormat.EqualsIgnoreCase("FLAC"))
            {
                return "FLAC";
            }

            if (audioFormat.EqualsIgnoreCase("Vorbis"))
            {
                return "Vorbis";
            }

            if (audioFormat.EqualsIgnoreCase("Opus"))
            {
                return "Opus";
            }

            return audioFormat;
        }

        public static string FormatVideoCodec(MediaInfoModel mediaInfo, string sceneName)
        {
            if (mediaInfo.VideoFormat == null)
            {
                return FormatVideoCodecLegacy(mediaInfo, sceneName);
            }

            var videoFormat = mediaInfo.VideoFormat;
            var videoCodecID = mediaInfo.VideoCodecID ?? string.Empty;
            var videoProfile = mediaInfo.VideoProfile ?? string.Empty;
            var videoCodecLibrary = mediaInfo.VideoCodecLibrary ?? string.Empty;

            if (videoFormat.IsNullOrWhiteSpace())
            {
                return videoFormat;
            }

            if (videoFormat == "AVC" || videoFormat == "V.MPEG4/ISO/AVC")
            {
                if (videoCodecLibrary.StartsWithIgnoreCase("x264"))
                {
                    return "x264";
                }

                return GetSceneNameMatch(sceneName, "AVC", "h264");
            }

            if (videoFormat.EqualsIgnoreCase("DivX") || videoFormat.EqualsIgnoreCase("div3"))
            {
                return "DivX";
            }

            if (videoFormat == "HEVC")
            {
                if (videoCodecLibrary.StartsWithIgnoreCase("x265"))
                {
                    return "x265";
                }

                return GetSceneNameMatch(sceneName, "HEVC", "h265");
            }

            if (videoFormat == "MPEG Video")
            {
                if (videoCodecID == "2" || videoCodecID == "V_MPEG2")
                {
                    return "MPEG2";
                }

                if (videoCodecID.IsNullOrWhiteSpace())
                {
                    return "MPEG";
                }
            }

            if (videoFormat == "MPEG-2 Video")
            {
                return "MPEG2";
            }

            if (videoFormat == "MPEG-4 Visual")
            {
                if (videoCodecID.ContainsIgnoreCase("XVID"))
                {
                    return "XviD";
                }

                if (videoCodecID.ContainsIgnoreCase("DIV3") ||
                    videoCodecID.ContainsIgnoreCase("DIVX") ||
                    videoCodecID.ContainsIgnoreCase("DX50"))
                {
                    return "DivX";
                }
            }

            if (videoFormat == "VC-1")
            {
                return "VC1";
            }

            if (videoFormat.EqualsIgnoreCase("VP6") || videoFormat.EqualsIgnoreCase("VP7") ||
                videoFormat.EqualsIgnoreCase("VP8") || videoFormat.EqualsIgnoreCase("VP9"))
            {
                return videoFormat.ToUpperInvariant();
            }

            if (videoFormat == "WMV2")
            {
                return "WMV";
            }

            if (videoFormat.EqualsIgnoreCase("XviD"))
            {
                return "XviD";
            }

            Logger.Debug()
                  .Message("Unknown video format: '{0}' in '{1}'.", string.Join(", ", videoFormat, videoCodecID, videoProfile, videoCodecLibrary), sceneName)
                  .WriteSentryWarn("UnknownVideoFormat", mediaInfo.ContainerFormat, videoFormat, videoCodecID)
                  .Write();

            return videoFormat;
        }

        public static string FormatVideoCodecLegacy(MediaInfoModel mediaInfo, string sceneName)
        {
            var videoCodec = mediaInfo.VideoCodec;

            if (videoCodec.IsNullOrWhiteSpace())
            {
                return videoCodec;
            }

            if (videoCodec == "AVC")
            {
                return GetSceneNameMatch(sceneName, "AVC", "h264", "x264");
            }

            if (videoCodec == "V_MPEGH/ISO/HEVC" || videoCodec == "HEVC")
            {
                return GetSceneNameMatch(sceneName, "HEVC", "h265", "x265");
            }

            if (videoCodec == "MPEG-2 Video")
            {
                return "MPEG2";
            }

            if (videoCodec == "MPEG-4 Visual")
            {
                return GetSceneNameMatch(sceneName, "DivX", "XviD");
            }

            if (videoCodec.StartsWithIgnoreCase("XviD"))
            {
                return "XviD";
            }

            if (videoCodec.StartsWithIgnoreCase("DivX"))
            {
                return "DivX";
            }

            if (videoCodec.EqualsIgnoreCase("VC-1"))
            {
                return "VC1";
            }

            return videoCodec;
        }

        private static string GetSceneNameMatch(string sceneName, params string[] tokens)
        {
            sceneName = sceneName.IsNotNullOrWhiteSpace() ? Path.GetFileNameWithoutExtension(sceneName) : string.Empty;

            foreach (var token in tokens)
            {
                if (sceneName.ContainsIgnoreCase(token))
                {
                    return token;
                }
            }

            // Last token is the default.
            return tokens.Last();
        }
    }
}
