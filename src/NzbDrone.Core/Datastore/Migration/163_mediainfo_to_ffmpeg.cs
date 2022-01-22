using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(163)]
    public class mediainfo_to_ffmpeg : NzbDroneMigrationBase
    {
        private readonly JsonSerializerOptions _serializerSettings;

        public mediainfo_to_ffmpeg()
        {
            var serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            serializerSettings.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, true));
            serializerSettings.Converters.Add(new STJTimeSpanConverter());
            serializerSettings.Converters.Add(new STJUtcConverter());

            _serializerSettings = serializerSettings;
        }

        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(MigrateToFfprobe);
        }

        private void MigrateToFfprobe(IDbConnection conn, IDbTransaction tran)
        {
            var existing = conn.Query<MediaInfoRaw>("SELECT Id, MediaInfo, SceneName FROM EpisodeFiles");

            var updated = new List<MediaInfoRaw>();

            foreach (var row in existing)
            {
                if (row.MediaInfo.IsNullOrWhiteSpace())
                {
                    continue;
                }

                // basic parse to check schema revision
                // in case user already tested ffmpeg branch
                var mediaInfoVersion = JsonSerializer.Deserialize<MediaInfoBase>(row.MediaInfo, _serializerSettings);
                if (mediaInfoVersion.SchemaRevision >= 8)
                {
                    continue;
                }

                // parse and migrate
                var mediaInfo = JsonSerializer.Deserialize<MediaInfo162>(row.MediaInfo, _serializerSettings);

                var ffprobe = MigrateMediaInfo(mediaInfo, row.SceneName);

                updated.Add(new MediaInfoRaw
                {
                    Id = row.Id,
                    MediaInfo = JsonSerializer.Serialize(ffprobe, _serializerSettings)
                });
            }

            var updateSql = "UPDATE EpisodeFiles SET MediaInfo = @MediaInfo WHERE Id = @Id";
            conn.Execute(updateSql, updated, transaction: tran);
        }

        public MediaInfo163 MigrateMediaInfo(MediaInfo162 old, string sceneName)
        {
            var m = new MediaInfo163
            {
                SchemaRevision = old.SchemaRevision,
                ContainerFormat = old.ContainerFormat,
                VideoProfile = old.VideoProfile,
                VideoBitrate = old.VideoBitrate,
                VideoBitDepth = old.VideoBitDepth,
                VideoMultiViewCount = old.VideoMultiViewCount,
                VideoColourPrimaries = MigratePrimaries(old.VideoColourPrimaries),
                VideoTransferCharacteristics = MigrateTransferCharacteristics(old.VideoTransferCharacteristics),
                Height = old.Height,
                Width = old.Width,
                AudioBitrate = old.AudioBitrate,
                RunTime = old.RunTime,
                AudioStreamCount = old.AudioStreamCount,
                VideoFps = old.VideoFps,
                ScanType = old.ScanType,
                AudioLanguages = MigrateLanguages(old.AudioLanguages),
                Subtitles = MigrateLanguages(old.Subtitles)
            };

            m.VideoHdrFormat = MigrateHdrFormat(old);

            MigrateVideoCodec(old, m, sceneName);
            MigrateAudioCodec(old, m);
            MigrateAudioChannelPositions(old, m);

            m.AudioChannels = old.AudioChannelsStream > 0 ? old.AudioChannelsStream : old.AudioChannelsContainer;

            return m;
        }

        private void MigrateVideoCodec(MediaInfo162 mediaInfo, MediaInfo163 m, string sceneName)
        {
            if (mediaInfo.VideoFormat == null)
            {
                MigrateVideoCodecLegacy(mediaInfo, m, sceneName);
                return;
            }

            var videoFormat = mediaInfo.VideoFormat.Trim().Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
            var videoCodecID = mediaInfo.VideoCodecID ?? string.Empty;
            var videoCodecLibrary = mediaInfo.VideoCodecLibrary ?? string.Empty;

            var result = mediaInfo.VideoFormat.Trim();

            m.VideoFormat = result;
            m.VideoCodecID = null;

            if (videoFormat.ContainsIgnoreCase("x264"))
            {
                m.VideoFormat = "h264";
                m.VideoCodecID = "x264";
                return;
            }

            if (videoFormat.ContainsIgnoreCase("AVC") || videoFormat.ContainsIgnoreCase("V.MPEG4/ISO/AVC"))
            {
                m.VideoFormat = "h264";

                if (videoCodecLibrary.StartsWithIgnoreCase("x264"))
                {
                    m.VideoCodecID = "x264";
                }

                return;
            }

            if (videoFormat.ContainsIgnoreCase("HEVC") || videoFormat.ContainsIgnoreCase("V_MPEGH/ISO/HEVC"))
            {
                m.VideoFormat = "hevc";
                if (videoCodecLibrary.StartsWithIgnoreCase("x265"))
                {
                    m.VideoCodecID = "x265";
                }

                return;
            }

            if (videoFormat.ContainsIgnoreCase("MPEG Video"))
            {
                if (videoCodecID == "2" || videoCodecID == "V_MPEG2")
                {
                    m.VideoFormat = "mpeg2video";
                }

                if (videoCodecID.IsNullOrWhiteSpace())
                {
                    m.VideoFormat = "MPEG";
                }
            }

            if (videoFormat.ContainsIgnoreCase("MPEG-2 Video"))
            {
                m.VideoFormat = "mpeg2video";
            }

            if (videoFormat.ContainsIgnoreCase("MPEG-4 Visual"))
            {
                m.VideoFormat = "mpeg4";

                if (videoCodecID.ContainsIgnoreCase("XVID") ||
                    videoCodecLibrary.StartsWithIgnoreCase("XviD"))
                {
                    m.VideoCodecID = "XVID";
                }

                if (videoCodecID.ContainsIgnoreCase("DIV3") ||
                    videoCodecID.ContainsIgnoreCase("DIVX") ||
                    videoCodecID.ContainsIgnoreCase("DX50") ||
                    videoCodecLibrary.StartsWithIgnoreCase("DivX"))
                {
                    m.VideoCodecID = "DIVX";
                }

                return;
            }

            if (videoFormat.ContainsIgnoreCase("MPEG-4 Visual") || videoFormat.ContainsIgnoreCase("mp4v"))
            {
                m.VideoFormat = "mpeg4";

                result = GetSceneNameMatch(sceneName, "XviD", "DivX", "");

                if (result == "XviD")
                {
                    m.VideoCodecID = "XVID";
                }

                if (result == "DivX")
                {
                    m.VideoCodecID = "DIVX";
                }

                return;
            }

            if (videoFormat.ContainsIgnoreCase("VC-1"))
            {
                m.VideoFormat = "vc1";
                return;
            }

            if (videoFormat.ContainsIgnoreCase("AV1"))
            {
                m.VideoFormat = "av1";
                return;
            }

            if (videoFormat.ContainsIgnoreCase("VP6") || videoFormat.ContainsIgnoreCase("VP7") ||
                videoFormat.ContainsIgnoreCase("VP8") || videoFormat.ContainsIgnoreCase("VP9"))
            {
                m.VideoFormat = videoFormat.First().ToLowerInvariant();
                return;
            }

            if (videoFormat.ContainsIgnoreCase("WMV1") || videoFormat.ContainsIgnoreCase("WMV2"))
            {
                m.VideoFormat = "WMV";
                return;
            }

            if (videoFormat.ContainsIgnoreCase("DivX") || videoFormat.ContainsIgnoreCase("div3"))
            {
                m.VideoFormat = "mpeg4";
                m.VideoCodecID = "DIVX";
                return;
            }

            if (videoFormat.ContainsIgnoreCase("XviD"))
            {
                m.VideoFormat = "mpeg4";
                m.VideoCodecID = "XVID";
                return;
            }

            if (videoFormat.ContainsIgnoreCase("V_QUICKTIME") ||
                videoFormat.ContainsIgnoreCase("RealVideo 4"))
            {
                m.VideoFormat = "qtrle";
                return;
            }

            if (videoFormat.ContainsIgnoreCase("mp42") ||
                videoFormat.ContainsIgnoreCase("mp43"))
            {
                m.VideoFormat = "mpeg4";
                return;
            }
        }

        private void MigrateVideoCodecLegacy(MediaInfo162 mediaInfo, MediaInfo163 m, string sceneName)
        {
            var videoCodec = mediaInfo.VideoCodec;

            m.VideoFormat = videoCodec;
            m.VideoCodecID = null;

            if (videoCodec.IsNullOrWhiteSpace())
            {
                m.VideoFormat = null;
                return;
            }

            if (videoCodec == "AVC")
            {
                m.VideoFormat = "h264";
            }

            if (videoCodec == "V_MPEGH/ISO/HEVC" || videoCodec == "HEVC")
            {
                m.VideoFormat = "hevc";
            }

            if (videoCodec == "MPEG-2 Video")
            {
                m.VideoFormat = "mpeg2video";
            }

            if (videoCodec == "MPEG-4 Visual")
            {
                var result = GetSceneNameMatch(sceneName, "DivX", "XviD");
                if (result == "DivX")
                {
                    m.VideoFormat = "mpeg4";
                    m.VideoCodecID = "DIVX";
                }

                m.VideoFormat = "mpeg4";
                m.VideoCodecID = "XVID";
            }

            if (videoCodec.StartsWithIgnoreCase("XviD"))
            {
                m.VideoFormat = "mpeg4";
                m.VideoCodecID = "XVID";
            }

            if (videoCodec.StartsWithIgnoreCase("DivX"))
            {
                m.VideoFormat = "mpeg4";
                m.VideoCodecID = "DIVX";
            }

            if (videoCodec.EqualsIgnoreCase("VC-1"))
            {
                m.VideoFormat = "vc1";
            }
        }

        private HdrFormat MigrateHdrFormat(MediaInfo162 mediaInfo)
        {
            if (mediaInfo.VideoHdrFormatCompatibility.IsNotNullOrWhiteSpace())
            {
                if (mediaInfo.VideoHdrFormatCompatibility.ContainsIgnoreCase("HLG"))
                {
                    return HdrFormat.Hlg10;
                }

                if (mediaInfo.VideoHdrFormatCompatibility.ContainsIgnoreCase("dolby"))
                {
                    return HdrFormat.DolbyVision;
                }

                if (mediaInfo.VideoHdrFormatCompatibility.ContainsIgnoreCase("dolby"))
                {
                    return HdrFormat.DolbyVision;
                }

                if (mediaInfo.VideoHdrFormatCompatibility.ContainsIgnoreCase("hdr10+"))
                {
                    return HdrFormat.Hdr10Plus;
                }

                if (mediaInfo.VideoHdrFormatCompatibility.ContainsIgnoreCase("hdr10"))
                {
                    return HdrFormat.Hdr10;
                }
            }

            return VideoFileInfoReader.GetHdrFormat(mediaInfo.VideoBitDepth, mediaInfo.VideoColourPrimaries, mediaInfo.VideoTransferCharacteristics, new ());
        }

        private void MigrateAudioCodec(MediaInfo162 mediaInfo, MediaInfo163 m)
        {
            if (mediaInfo.AudioCodecID == null)
            {
                MigrateAudioCodecLegacy(mediaInfo, m);
                return;
            }

            var audioFormat = mediaInfo.AudioFormat.Trim().Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
            var audioCodecID = mediaInfo.AudioCodecID ?? string.Empty;
            var splitAdditionalFeatures = (mediaInfo.AudioAdditionalFeatures ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            m.AudioFormat = "";

            if (audioFormat.Empty())
            {
                return;
            }

            if (audioFormat.ContainsIgnoreCase("Atmos"))
            {
                m.AudioFormat = "truehd";
                m.AudioCodecID = "thd+";
                return;
            }

            if (audioFormat.ContainsIgnoreCase("MLP FBA"))
            {
                m.AudioFormat = "truehd";

                if (splitAdditionalFeatures.ContainsIgnoreCase("16-ch"))
                {
                    m.AudioCodecID = "thd+";
                    return;
                }

                return;
            }

            if (audioFormat.ContainsIgnoreCase("TrueHD"))
            {
                m.AudioFormat = "truehd";
                return;
            }

            if (audioFormat.ContainsIgnoreCase("FLAC"))
            {
                m.AudioFormat = "flac";
                return;
            }

            if (audioFormat.ContainsIgnoreCase("DTS"))
            {
                m.AudioFormat = "dts";

                if (splitAdditionalFeatures.ContainsIgnoreCase("XLL"))
                {
                    if (splitAdditionalFeatures.ContainsIgnoreCase("X"))
                    {
                        m.AudioProfile = "DTS:X";
                        return;
                    }

                    m.AudioProfile = "DTS-HD MA";
                    return;
                }

                if (splitAdditionalFeatures.ContainsIgnoreCase("ES"))
                {
                    m.AudioProfile = "DTS-ES";
                    return;
                }

                if (splitAdditionalFeatures.ContainsIgnoreCase("XBR"))
                {
                    m.AudioProfile = "DTS-HD HRA";
                    return;
                }

                return;
            }

            if (audioFormat.ContainsIgnoreCase("E-AC-3"))
            {
                m.AudioFormat = "eac3";

                if (splitAdditionalFeatures.ContainsIgnoreCase("JOC"))
                {
                    m.AudioCodecID = "ec+3";
                }

                return;
            }

            if (audioFormat.ContainsIgnoreCase("AC-3"))
            {
                m.AudioFormat = "ac3";
                return;
            }

            if (audioFormat.ContainsIgnoreCase("AAC"))
            {
                m.AudioFormat = "aac";

                if (audioCodecID == "A_AAC/MPEG4/LC/SBR")
                {
                    m.AudioCodecID = audioCodecID;
                }

                return;
            }

            if (audioFormat.ContainsIgnoreCase("mp3"))
            {
                m.AudioFormat = "mp3";
                return;
            }

            if (audioFormat.ContainsIgnoreCase("MPEG Audio"))
            {
                if (mediaInfo.AudioCodecID == "55" || mediaInfo.AudioCodecID == "A_MPEG/L3" || mediaInfo.AudioProfile == "Layer 3")
                {
                    m.AudioFormat = "mp3";
                    return;
                }

                if (mediaInfo.AudioCodecID == "A_MPEG/L2" || mediaInfo.AudioProfile == "Layer 2")
                {
                    m.AudioFormat = "mp2";
                }
            }

            if (audioFormat.ContainsIgnoreCase("Opus"))
            {
                m.AudioFormat = "opus";
                return;
            }

            if (audioFormat.ContainsIgnoreCase("PCM"))
            {
                m.AudioFormat = "pcm_s16le";
                return;
            }

            if (audioFormat.ContainsIgnoreCase("ADPCM"))
            {
                m.AudioFormat = "pcm_s16le";
                return;
            }

            if (audioFormat.ContainsIgnoreCase("Vorbis"))
            {
                m.AudioFormat = "vorbis";
                return;
            }

            if (audioFormat.ContainsIgnoreCase("WMA"))
            {
                m.AudioFormat = "wmav1";
                return;
            }
        }

        private void MigrateAudioCodecLegacy(MediaInfo162 mediaInfo, MediaInfo163 m)
        {
            var audioFormat = mediaInfo.AudioFormat;

            m.AudioFormat = audioFormat;

            if (audioFormat.IsNullOrWhiteSpace())
            {
                m.AudioFormat = null;
                return;
            }

            if (audioFormat.EqualsIgnoreCase("AC-3"))
            {
                m.AudioFormat = "ac3";
                return;
            }

            if (audioFormat.EqualsIgnoreCase("E-AC-3"))
            {
                m.AudioFormat = "eac3";
                return;
            }

            if (audioFormat.EqualsIgnoreCase("AAC"))
            {
                m.AudioFormat = "aac";
                return;
            }

            if (audioFormat.EqualsIgnoreCase("MPEG Audio") && mediaInfo.AudioProfile == "Layer 3")
            {
                m.AudioFormat = "mp3";
                return;
            }

            if (audioFormat.EqualsIgnoreCase("DTS"))
            {
                m.AudioFormat = "DTS";
                return;
            }

            if (audioFormat.EqualsIgnoreCase("TrueHD"))
            {
                m.AudioFormat = "truehd";
                return;
            }

            if (audioFormat.EqualsIgnoreCase("FLAC"))
            {
                m.AudioFormat = "flac";
                return;
            }

            if (audioFormat.EqualsIgnoreCase("Vorbis"))
            {
                m.AudioFormat = "vorbis";
                return;
            }

            if (audioFormat.EqualsIgnoreCase("Opus"))
            {
                m.AudioFormat = "opus";
                return;
            }
        }

        private void MigrateAudioChannelPositions(MediaInfo162 mediaInfo, MediaInfo163 m)
        {
            var audioChannels = FormatAudioChannelsFromAudioChannelPositions(mediaInfo);

            if (audioChannels == null || audioChannels == 0.0m)
            {
                audioChannels = FormatAudioChannelsFromAudioChannelPositionsText(mediaInfo);
            }

            if (audioChannels == null || audioChannels == 0.0m)
            {
                audioChannels = FormatAudioChannelsFromAudioChannels(mediaInfo);
            }

            audioChannels ??= 0;

            m.AudioChannelPositions = audioChannels.ToString();
        }

        private decimal? FormatAudioChannelsFromAudioChannelPositions(MediaInfo162 mediaInfo)
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

                if (audioChannelPositions.Contains("/"))
                {
                    var channelStringList = Regex.Replace(audioChannelPositions,
                            @"^\d+\sobjects",
                            "",
                            RegexOptions.Compiled | RegexOptions.IgnoreCase)
                        .Replace("Object Based / ", "")
                        .Split(new string[] { " / " }, StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault()
                        ?.Split('/');

                    var positions = default(decimal);

                    if (channelStringList == null)
                    {
                        return 0;
                    }

                    foreach (var channel in channelStringList)
                    {
                        var channelSplit = channel.Split(new string[] { "." }, StringSplitOptions.None);

                        if (channelSplit.Length == 3)
                        {
                            positions += decimal.Parse(string.Format("{0}.{1}", channelSplit[1], channelSplit[2]), CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            positions += decimal.Parse(channel, CultureInfo.InvariantCulture);
                        }
                    }

                    return positions;
                }
            }
            catch
            {
            }

            return null;
        }

        private decimal? FormatAudioChannelsFromAudioChannelPositionsText(MediaInfo162 mediaInfo)
        {
            var audioChannelPositionsTextContainer = mediaInfo.AudioChannelPositionsTextContainer;
            var audioChannelPositionsTextStream = mediaInfo.AudioChannelPositionsTextStream;
            var audioChannelsContainer = mediaInfo.AudioChannelsContainer;
            var audioChannelsStream = mediaInfo.AudioChannelsStream;

            //Skip if the positions texts give us nothing
            if ((audioChannelPositionsTextContainer.IsNullOrWhiteSpace() || audioChannelPositionsTextContainer == "Object Based") &&
                (audioChannelPositionsTextStream.IsNullOrWhiteSpace() || audioChannelPositionsTextStream == "Object Based"))
            {
                return null;
            }

            try
            {
                if (audioChannelsStream > 0)
                {
                    return audioChannelPositionsTextStream.ContainsIgnoreCase("LFE") ? audioChannelsStream - 1 + 0.1m : audioChannelsStream;
                }

                return audioChannelPositionsTextContainer.ContainsIgnoreCase("LFE") ? audioChannelsContainer - 1 + 0.1m : audioChannelsContainer;
            }
            catch
            {
            }

            return null;
        }

        private decimal? FormatAudioChannelsFromAudioChannels(MediaInfo162 mediaInfo)
        {
            var audioChannelsContainer = mediaInfo.AudioChannelsContainer;
            var audioChannelsStream = mediaInfo.AudioChannelsStream;

            var audioFormat = (mediaInfo.AudioFormat ?? string.Empty).Trim().Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
            var splitAdditionalFeatures = (mediaInfo.AudioAdditionalFeatures ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Workaround https://github.com/MediaArea/MediaInfo/issues/299 for DTS-X Audio
            if (audioFormat.ContainsIgnoreCase("DTS") &&
                splitAdditionalFeatures.ContainsIgnoreCase("XLL") &&
                splitAdditionalFeatures.ContainsIgnoreCase("X") &&
                audioChannelsContainer > 0)
            {
                return audioChannelsContainer - 1 + 0.1m;
            }

            // FLAC 6 channels is likely 5.1
            if (audioFormat.ContainsIgnoreCase("FLAC") && audioChannelsContainer == 6)
            {
                return 5.1m;
            }

            if (mediaInfo.SchemaRevision > 5)
            {
                return audioChannelsStream > 0 ? audioChannelsStream : audioChannelsContainer;
            }

            if (mediaInfo.SchemaRevision >= 3)
            {
                return audioChannelsContainer;
            }

            return null;
        }

        private List<string> MigrateLanguages(string mediaInfoLanguages)
        {
            var languages = new List<string>();

            var tokens = mediaInfoLanguages.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            var cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] == "Swedis")
                {
                    // Probably typo in mediainfo (should be 'Swedish')
                    languages.Add("swe");
                    continue;
                }

                if (tokens[i] == "Chinese" && OsInfo.IsNotWindows)
                {
                    // Mono only has 'Chinese (Simplified)' & 'Chinese (Traditional)'
                    languages.Add("zho");
                    continue;
                }

                if (tokens[i] == "Norwegian")
                {
                    languages.Add("nor");
                    continue;
                }

                try
                {
                    var cultureInfo = cultures.FirstOrDefault(p => p.EnglishName.RemoveAccent() == tokens[i]);

                    if (cultureInfo != null)
                    {
                        languages.Add(cultureInfo.ThreeLetterISOLanguageName.ToLowerInvariant());
                    }
                }
                catch
                {
                }
            }

            return languages;
        }

        private string MigratePrimaries(string primary)
        {
            return primary.IsNotNullOrWhiteSpace() ? primary.Replace("BT.", "bt") : primary;
        }

        private string MigrateTransferCharacteristics(string transferCharacteristics)
        {
            if (transferCharacteristics == "PQ")
            {
                return "smpte2084";
            }

            if (transferCharacteristics == "HLG")
            {
                return "arib-std-b67";
            }

            return "bt709";
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

        public class MediaInfoRaw : ModelBase
        {
            public string MediaInfo { get; set; }
            public string SceneName { get; set; }
        }

        public class MediaInfoBase
        {
            public int SchemaRevision { get; set; }
        }

        public class MediaInfo162 : MediaInfoBase
        {
            public string ContainerFormat { get; set; }

            // Deprecated according to MediaInfo
            public string VideoCodec { get; set; }
            public string VideoFormat { get; set; }
            public string VideoCodecID { get; set; }
            public string VideoProfile { get; set; }
            public string VideoCodecLibrary { get; set; }
            public int VideoBitrate { get; set; }
            public int VideoBitDepth { get; set; }
            public int VideoMultiViewCount { get; set; }
            public string VideoColourPrimaries { get; set; }
            public string VideoTransferCharacteristics { get; set; }
            public string VideoHdrFormat { get; set; }
            public string VideoHdrFormatCompatibility { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string AudioFormat { get; set; }
            public string AudioCodecID { get; set; }
            public string AudioCodecLibrary { get; set; }
            public string AudioAdditionalFeatures { get; set; }
            public int AudioBitrate { get; set; }
            public TimeSpan RunTime { get; set; }
            public int AudioStreamCount { get; set; }
            public int AudioChannelsContainer { get; set; }
            public int AudioChannelsStream { get; set; }
            public string AudioChannelPositions { get; set; }
            public string AudioChannelPositionsTextContainer { get; set; }
            public string AudioChannelPositionsTextStream { get; set; }
            public string AudioProfile { get; set; }
            public decimal VideoFps { get; set; }
            public string AudioLanguages { get; set; }
            public string Subtitles { get; set; }
            public string ScanType { get; set; }
        }

        public class MediaInfo163 : MediaInfoBase
        {
            public string ContainerFormat { get; set; }
            public string VideoFormat { get; set; }
            public string VideoCodecID { get; set; }
            public string VideoProfile { get; set; }
            public int VideoBitrate { get; set; }
            public int VideoBitDepth { get; set; }
            public int VideoMultiViewCount { get; set; }
            public string VideoColourPrimaries { get; set; }
            public string VideoTransferCharacteristics { get; set; }
            public HdrFormat VideoHdrFormat { get; set; }
            public int Height { get; set; }
            public int Width { get; set; }
            public string AudioFormat { get; set; }
            public string AudioCodecID { get; set; }
            public string AudioProfile { get; set; }
            public int AudioBitrate { get; set; }
            public TimeSpan RunTime { get; set; }
            public int AudioStreamCount { get; set; }
            public int AudioChannels { get; set; }
            public string AudioChannelPositions { get; set; }
            public decimal VideoFps { get; set; }
            public List<string> AudioLanguages { get; set; }
            public List<string> Subtitles { get; set; }
            public string ScanType { get; set; }
        }
    }
}
