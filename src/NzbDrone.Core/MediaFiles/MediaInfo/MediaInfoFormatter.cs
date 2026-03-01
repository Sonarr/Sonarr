using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Instrumentation.Extensions;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public static partial class MediaInfoFormatter
    {
        private const string VideoDynamicRangeHdr = "HDR";

        [GeneratedRegex(@"(?<position>^\d\.\d)", RegexOptions.Compiled)]
        private static partial Regex PositionRegex();

        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(MediaInfoFormatter));

        public static decimal FormatAudioChannels(MediaInfoAudioStreamModel audioStream)
        {
            var audioChannels = FormatAudioChannelsFromAudioChannelPositions(audioStream);

            if (audioChannels is null or 0.0m)
            {
                audioChannels = audioStream.Channels;
            }

            return audioChannels.Value;
        }

        public static string FormatAudioCodec(MediaInfoAudioStreamModel audioStream, string sceneName)
        {
            if (audioStream.Format == null)
            {
                return null;
            }

            var audioFormat = audioStream.Format;
            var audioCodecId = audioStream.CodecId ?? string.Empty;
            var audioProfile = audioStream.Profile ?? string.Empty;

            if (audioFormat.Empty())
            {
                return string.Empty;
            }

            // see definitions here https://github.com/FFmpeg/FFmpeg/blob/master/libavcodec/codec_desc.c
            if (audioCodecId == "thd+")
            {
                return "TrueHD Atmos";
            }

            if (audioFormat == "truehd")
            {
                return audioProfile switch
                {
                    "Dolby TrueHD + Dolby Atmos" => "TrueHD Atmos",
                    _ => "TrueHD"
                };
            }

            if (audioFormat == "flac")
            {
                return "FLAC";
            }

            if (audioFormat == "dts")
            {
                return audioProfile switch
                {
                    "DTS:X" or "DTS-HD MA + DTS:X IMAX" => "DTS-X",
                    "DTS-HD MA" => "DTS-HD MA",
                    "DTS-ES" => "DTS-ES",
                    "DTS-HD HRA" => "DTS-HD HRA",
                    "DTS Express" => "DTS Express",
                    "DTS 96/24" => "DTS 96/24",
                    _ => "DTS"
                };
            }

            if (audioCodecId == "ec+3")
            {
                return "EAC3 Atmos";
            }

            if (audioFormat == "eac3")
            {
                return audioProfile switch
                {
                    "Dolby Digital Plus + Dolby Atmos" => "EAC3 Atmos",
                    _ => "EAC3"
                };
            }

            if (audioFormat == "ac3")
            {
                return "AC3";
            }

            if (audioFormat == "aac")
            {
                if (audioCodecId == "A_AAC/MPEG4/LC/SBR")
                {
                    return "HE-AAC";
                }

                return audioProfile switch
                {
                    "HE-AAC" => "HE-AAC",
                    "xHE-AAC" => "xHE-AAC",
                    _ => "AAC"
                };
            }

            if (audioFormat == "mp3")
            {
                return "MP3";
            }

            if (audioFormat == "mp2")
            {
                return "MP2";
            }

            if (audioFormat == "opus")
            {
                return "Opus";
            }

            if (audioFormat.StartsWith("pcm_") || audioFormat.StartsWith("adpcm_"))
            {
                return "PCM";
            }

            if (audioFormat == "vorbis")
            {
                return "Vorbis";
            }

            if (audioFormat == "wmav1" ||
                audioFormat == "wmav2" ||
                audioFormat == "wmapro")
            {
                return "WMA";
            }

            Logger.ForDebugEvent()
                  .Message("Unknown audio format: '{0}' in '{1}'", audioFormat, sceneName)
                  .WriteSentryWarn("UnknownAudioFormatFFProbe", audioStream.Format, audioCodecId)
                  .Log();

            return audioStream.Format;
        }

        public static string FormatVideoCodec(MediaInfoModel mediaInfo, string sceneName)
        {
            if (mediaInfo.VideoFormat == null)
            {
                return null;
            }

            var videoFormat = mediaInfo.VideoFormat;
            var videoCodecId = mediaInfo.VideoCodecID ?? string.Empty;

            var result = videoFormat.Trim();

            if (videoFormat.Empty())
            {
                return result;
            }

            // see definitions here: https://github.com/FFmpeg/FFmpeg/blob/master/libavcodec/codec_desc.c
            if (videoCodecId == "x264")
            {
                return "x264";
            }

            if (videoFormat == "h264")
            {
                return GetSceneNameMatch(sceneName, "AVC", "x264", "h264");
            }

            if (videoCodecId == "x265")
            {
                return "x265";
            }

            if (videoFormat == "hevc")
            {
                return GetSceneNameMatch(sceneName, "HEVC", "x265", "h265");
            }

            if (videoFormat == "mpeg2video")
            {
                return "MPEG2";
            }

            if (videoFormat == "mpeg1video")
            {
                return "MPEG";
            }

            if (videoFormat == "mpeg4" || videoFormat.Contains("msmpeg4"))
            {
                if (videoCodecId.ToUpperInvariant() == "XVID")
                {
                    return "XviD";
                }

                if (videoCodecId == "DIV3" ||
                    videoCodecId == "DX50" ||
                    videoCodecId.ToUpperInvariant() == "DIVX")
                {
                    return "DivX";
                }

                return "";
            }

            if (videoFormat == "vc1")
            {
                return "VC1";
            }

            if (videoFormat == "av1")
            {
                return "AV1";
            }

            if (videoFormat.Contains("vp6"))
            {
                return "VP6";
            }

            if (videoFormat == "vp7" ||
                videoFormat == "vp8" ||
                videoFormat == "vp9")
            {
                return videoFormat.ToUpperInvariant();
            }

            if (videoFormat == "wmv1" ||
                videoFormat == "wmv2" ||
                videoFormat == "wmv3")
            {
                return "WMV";
            }

            if (videoFormat == "qtrle" ||
                videoFormat == "rpza" ||
                videoFormat == "rv10" ||
                videoFormat == "rv20" ||
                videoFormat == "rv30" ||
                videoFormat == "rv40" ||
                videoFormat == "cinepak" ||
                videoFormat == "rawvideo" ||
                videoFormat == "msvideo1")
            {
                return "";
            }

            Logger.ForDebugEvent()
                  .Message("Unknown video format: '{0}' in '{1}'. Streams: {2}", videoFormat, sceneName, mediaInfo.RawStreamData)
                  .WriteSentryWarn("UnknownVideoFormatFFProbe", mediaInfo.ContainerFormat, videoFormat, videoCodecId)
                  .Log();

            return result;
        }

        private static decimal? FormatAudioChannelsFromAudioChannelPositions(MediaInfoAudioStreamModel audioStream)
        {
            if (audioStream.ChannelPositions == null)
            {
                return 0;
            }

            var match = PositionRegex().Match(audioStream.ChannelPositions);
            if (match.Success)
            {
                return decimal.Parse(match.Groups["position"].Value, NumberStyles.Number, CultureInfo.InvariantCulture);
            }

            return 0;
        }

        private static string GetSceneNameMatch(string sceneName, params string[] tokens)
        {
            sceneName = sceneName.IsNotNullOrWhiteSpace() ? FileExtensions.RemoveFileExtension(sceneName) : string.Empty;

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

        public static string FormatVideoDynamicRange(MediaInfoModel mediaInfo)
        {
            return mediaInfo.VideoHdrFormat != HdrFormat.None ? VideoDynamicRangeHdr : "";
        }

        public static string FormatVideoDynamicRangeType(MediaInfoModel mediaInfo)
        {
            return mediaInfo.VideoHdrFormat switch
            {
                HdrFormat.DolbyVision => "DV",
                HdrFormat.DolbyVisionHdr10 => "DV HDR10",
                HdrFormat.DolbyVisionHdr10Plus => "DV HDR10Plus",
                HdrFormat.DolbyVisionHlg => "DV HLG",
                HdrFormat.DolbyVisionSdr => "DV SDR",
                HdrFormat.Hdr10 => "HDR10",
                HdrFormat.Hdr10Plus => "HDR10Plus",
                HdrFormat.Hlg10 => "HLG",
                HdrFormat.Pq10 => "PQ",
                HdrFormat.UnknownHdr => "HDR",
                _ => ""
            };
        }
    }
}
