using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            var audioChannels = FormatAudioChannelsFromAudioChannelPositions(mediaInfo);

            if (audioChannels == null)
            {
                audioChannels = FormatAudioChannelsFromAudioChannelPositionsText(mediaInfo);
            }

            if (audioChannels == null)
            {
                audioChannels = FormatAudioChannelsFromAudioChannels(mediaInfo);
            }

            return audioChannels ?? 0;
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

            var result = videoFormat;

            if (videoFormat.IsNullOrWhiteSpace())
            {
                return result;
            }

            if (videoFormat == "x264")
            {
                return "x264";
            }

            if (videoFormat == "AVC" || videoFormat == "V.MPEG4/ISO/AVC")
            {
                if (videoCodecLibrary.StartsWithIgnoreCase("x264"))
                {
                    return "x264";
                }

                return GetSceneNameMatch(sceneName, "AVC", "x264", "h264");
            }

            if (videoFormat == "HEVC" || videoFormat == "V_MPEGH/ISO/HEVC")
            {
                if (videoCodecLibrary.StartsWithIgnoreCase("x265"))
                {
                    return "x265";
                }

                return GetSceneNameMatch(sceneName, "HEVC", "x265", "h265");
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
                if (videoCodecID.ContainsIgnoreCase("XVID") ||
                    videoCodecLibrary.StartsWithIgnoreCase("XviD"))
                {
                    return "XviD";
                }

                if (videoCodecID.ContainsIgnoreCase("DIV3") ||
                    videoCodecID.ContainsIgnoreCase("DIVX") ||
                    videoCodecID.ContainsIgnoreCase("DX50") ||
                    videoCodecLibrary.StartsWithIgnoreCase("DivX"))
                {
                    return "DivX";
                }
            }

            if (videoFormat == "MPEG-4 Visual" || videoFormat == "mp4v")
            {
                result = GetSceneNameMatch(sceneName, "XviD", "DivX", "");
                if (result.IsNotNullOrWhiteSpace())
                {
                    return result;
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

            if (videoFormat.EqualsIgnoreCase("DivX") || videoFormat.EqualsIgnoreCase("div3"))
            {
                return "DivX";
            }

            if (videoFormat.EqualsIgnoreCase("XviD"))
            {
                return "XviD";
            }

            Logger.Debug()
                  .Message("Unknown video format: '{0}' in '{1}'.", string.Join(", ", videoFormat, videoCodecID, videoProfile, videoCodecLibrary), sceneName)
                  .WriteSentryWarn("UnknownVideoFormat", mediaInfo.ContainerFormat, videoFormat, videoCodecID)
                  .Write();

            return result;
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

        private static decimal? FormatAudioChannelsFromAudioChannelPositions(MediaInfoModel mediaInfo)
        {
            var audioChannelPositions = mediaInfo.AudioChannelPositions;

            if (audioChannelPositions.IsNullOrWhiteSpace())
            {
                return null;
            }

            try
            {
                if (audioChannelPositions.Contains("+"))
                {
                    return audioChannelPositions.Split('+')
                                                .Sum(s => decimal.Parse(s.Trim(), CultureInfo.InvariantCulture));
                }


                return Regex.Replace(audioChannelPositions, @"^\d+\sobjects", "", RegexOptions.Compiled | RegexOptions.IgnoreCase)
                                            .Replace("Object Based / ", "")
                                            .Split(new string[] { " / " }, StringSplitOptions.RemoveEmptyEntries)
                                            .FirstOrDefault()
                                           ?.Split('/')
                                            .Sum(s => decimal.Parse(s, CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Unable to format audio channels using 'AudioChannelPositions', with a value of: '{0}'", audioChannelPositions);
            }

            return null;
        }

        private static decimal? FormatAudioChannelsFromAudioChannelPositionsText(MediaInfoModel mediaInfo)
        {
            var audioChannelPositionsText = mediaInfo.AudioChannelPositionsText;
            var audioChannels = mediaInfo.AudioChannels;

            if (audioChannelPositionsText.IsNullOrWhiteSpace())
            {
                return null;
            }

            try
            {
                return mediaInfo.AudioChannelPositionsText.ContainsIgnoreCase("LFE") ? audioChannels - 1 + 0.1m : audioChannels;
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Unable to format audio channels using 'AudioChannelPositionsText', with a value of: '{0}'", audioChannelPositionsText);
            }

            return null;
        }

        private static decimal? FormatAudioChannelsFromAudioChannels(MediaInfoModel mediaInfo)
        {
            var audioChannels = mediaInfo.AudioChannels;

            if (mediaInfo.SchemaRevision >= 3)
            {
                return audioChannels;
            }

            Logger.Warn("Unable to format audio channels using 'AudioChannels', with a value of: '{0}'", audioChannels);

            return null;
        }

        private static string GetSceneNameMatch(string sceneName, params string[] tokens)
        {
            sceneName = sceneName.IsNotNullOrWhiteSpace() ? Parser.Parser.RemoveFileExtension(sceneName) : string.Empty;

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
