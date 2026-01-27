using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using FFMpegCore;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public interface IVideoFileInfoReader
    {
        MediaInfoModel GetMediaInfo(string filename);
        TimeSpan? GetRunTime(string filename);
    }

    public class VideoFileInfoReader : IVideoFileInfoReader
    {
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public const int MINIMUM_MEDIA_INFO_SCHEMA_REVISION = 14;
        public const int CURRENT_MEDIA_INFO_SCHEMA_REVISION = 14;

        private static readonly string[] ValidHdrColourPrimaries = { "bt2020" };
        private static readonly string[] HlgTransferFunctions = { "arib-std-b67" };
        private static readonly string[] PqTransferFunctions = { "smpte2084" };
        private static readonly string[] ValidHdrTransferFunctions = HlgTransferFunctions.Concat(PqTransferFunctions).ToArray();

        public VideoFileInfoReader(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;

            // We bundle ffprobe for all platforms
            GlobalFFOptions.Configure(options => options.BinaryFolder = AppDomain.CurrentDomain.BaseDirectory);
        }

        public MediaInfoModel GetMediaInfo(string filename)
        {
            if (!_diskProvider.FileExists(filename))
            {
                throw new FileNotFoundException("Media file does not exist: " + filename);
            }

            if (MediaFileExtensions.DiskExtensions.Contains(Path.GetExtension(filename)))
            {
                return null;
            }

            // TODO: Cache media info by path, mtime and length so we don't need to read files multiple times

            try
            {
                _logger.Debug("Getting media info from {0}", filename);

                var analysis = FFProbe.Analyse(filename, customArguments: "-probesize 50000000");
                var primaryVideoStream = GetPrimaryVideoStream(analysis);

                if (analysis.PrimaryAudioStream?.ChannelLayout.IsNullOrWhiteSpace() ?? true)
                {
                    analysis = FFProbe.Analyse(filename, customArguments: "-probesize 150000000 -analyzeduration 150000000");
                }

                var mediaInfoModel = new MediaInfoModel();
                mediaInfoModel.ContainerFormat = analysis.Format.FormatName;
                mediaInfoModel.VideoFormat = primaryVideoStream?.CodecName;
                mediaInfoModel.VideoCodecID = primaryVideoStream?.CodecTagString;
                mediaInfoModel.VideoProfile = primaryVideoStream?.Profile;
                mediaInfoModel.VideoBitrate = GetBitrate(primaryVideoStream);
                mediaInfoModel.VideoBitDepth = GetPixelFormat(primaryVideoStream?.PixelFormat)?.Components.Min(x => x.BitDepth) ?? 8;
                mediaInfoModel.VideoColourPrimaries = primaryVideoStream?.ColorPrimaries;
                mediaInfoModel.VideoTransferCharacteristics = primaryVideoStream?.ColorTransfer;
                mediaInfoModel.Height = primaryVideoStream?.Height ?? 0;
                mediaInfoModel.Width = primaryVideoStream?.Width ?? 0;
                mediaInfoModel.RunTime = GetBestRuntime(analysis.PrimaryAudioStream?.Duration, primaryVideoStream?.Duration, analysis.Format.Duration);
                mediaInfoModel.VideoFps = primaryVideoStream?.FrameRate ?? 0;
                mediaInfoModel.ScanType = primaryVideoStream?.FieldOrder switch
                {
                    "tt" or "bb" or "tb" or "bt" => "Interlaced",
                    _ => "Progressive"
                };
                mediaInfoModel.RawStreamData = string.Concat(analysis.OutputData);

                mediaInfoModel.AudioStreams = analysis.AudioStreams?
                    .Where(stream => stream.Language.IsNotNullOrWhiteSpace())
                    .OrderBy(stream => stream.Index)
                    .Select(stream =>
                    {
                        var model = new MediaInfoAudioStreamModel
                        {
                            Language = stream.Language,
                            Format = stream.CodecName,
                            CodecId = stream.CodecTagString,
                            Profile = stream.Profile,
                            Bitrate = GetBitrate(stream),
                            Channels = stream.Channels,
                            ChannelPositions = stream.ChannelLayout
                        };

                        if ((stream.Tags?.TryGetValue("title", out var audioTitle) ?? false) && audioTitle.IsNotNullOrWhiteSpace())
                        {
                            model.Title = audioTitle.Trim();
                        }

                        return  model;
                    })
                    .ToList();

                mediaInfoModel.SubtitleStreams = analysis.SubtitleStreams?
                    .Where(stream => stream.Language.IsNotNullOrWhiteSpace())
                    .OrderBy(stream => stream.Index)
                    .Select(stream =>
                    {
                        var model = new MediaInfoSubtitleStreamModel
                        {
                            Language = stream.Language,
                            Format = stream.CodecName,
                        };

                        if ((stream.Tags?.TryGetValue("title", out var subtitleTitle) ?? false) && subtitleTitle.IsNotNullOrWhiteSpace())
                        {
                            model.Title = subtitleTitle.Trim();
                        }

                        if (stream.Disposition?.TryGetValue("forced", out var forcedSubtitle) ?? false)
                        {
                            model.Forced = forcedSubtitle;
                        }

                        if (stream.Disposition?.TryGetValue("hearing_impaired", out var hearingImpairedSubtitle) ?? false)
                        {
                            model.HearingImpaired = hearingImpairedSubtitle;
                        }

                        return  model;
                    })
                    .ToList();

                mediaInfoModel.SchemaRevision = CURRENT_MEDIA_INFO_SCHEMA_REVISION;

                if (analysis.Format.Tags?.TryGetValue("title", out var title) ?? false)
                {
                    mediaInfoModel.Title = title;
                }

                FFProbeFrames frames = null;

                // if it looks like PQ10 or similar HDR, do a frame analysis to figure out which type it is
                if (PqTransferFunctions.Contains(mediaInfoModel.VideoTransferCharacteristics))
                {
                    var videoStreamIndex = analysis.VideoStreams.FindIndex(stream => stream.Index == primaryVideoStream?.Index);
                    frames = FFProbe.GetFrames(filename, customArguments: $"-read_intervals \"%+#1\" -select_streams v:{(videoStreamIndex == -1 ? 0 : videoStreamIndex)}");
                }

                var streamSideData = primaryVideoStream?.SideData ?? new();
                var framesSideData = frames?.Frames.FirstOrDefault()?.SideData ?? new();

                var sideData = streamSideData.Concat(framesSideData).ToList();
                mediaInfoModel.VideoHdrFormat = GetHdrFormat(mediaInfoModel.VideoBitDepth, mediaInfoModel.VideoColourPrimaries, mediaInfoModel.VideoTransferCharacteristics, sideData);

                return mediaInfoModel;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to parse media info from file: {0}", filename);
            }

            return null;
        }

        public TimeSpan? GetRunTime(string filename)
        {
            var info = GetMediaInfo(filename);

            return info?.RunTime;
        }

        private static TimeSpan GetBestRuntime(TimeSpan? audio, TimeSpan? video, TimeSpan general)
        {
            if (!video.HasValue || video.Value.TotalMilliseconds == 0)
            {
                if (!audio.HasValue || audio.Value.TotalMilliseconds == 0)
                {
                    return general;
                }

                return audio.Value;
            }

            return video.Value;
        }

        private static long GetBitrate(MediaStream mediaStream)
        {
            if (mediaStream?.BitRate is > 0)
            {
                return mediaStream.BitRate;
            }

            if ((mediaStream?.Tags?.TryGetValue("BPS", out var bitratePerSecond) ?? false) && bitratePerSecond.IsNotNullOrWhiteSpace())
            {
                return Convert.ToInt64(bitratePerSecond);
            }

            return 0;
        }

        private static VideoStream GetPrimaryVideoStream(IMediaAnalysis mediaAnalysis)
        {
            if (mediaAnalysis.VideoStreams.Count <= 1)
            {
                return mediaAnalysis.PrimaryVideoStream;
            }

            // motion image codec streams are often in front of the main video stream
            var codecFilter = new[] { "mjpeg", "png" };

            return mediaAnalysis.VideoStreams.FirstOrDefault(s => !codecFilter.Contains(s.CodecName)) ?? mediaAnalysis.PrimaryVideoStream;
        }

        private static FFProbePixelFormat GetPixelFormat(string format)
        {
            if (format.IsNullOrWhiteSpace())
            {
                return null;
            }

            return FFProbe.TryGetPixelFormat(format, out var pixelFormat) ? pixelFormat : null;
        }

        public static HdrFormat GetHdrFormat(int bitDepth, string colorPrimaries, string transferFunction, List<Dictionary<string, JsonNode>> sideData)
        {
            if (bitDepth < 10)
            {
                return HdrFormat.None;
            }

            if (TryGetSideData(sideData, FFMpegCoreSideDataTypes.DoviConfigurationRecordSideData, out var dovi))
            {
                var hasHdr10Plus = TryGetSideData(sideData, FFMpegCoreSideDataTypes.HdrDynamicMetadataSpmte2094, out _);

                dovi.TryGetValue("dv_bl_signal_compatibility_id", out var dvBlSignalCompatibilityId);

                return dvBlSignalCompatibilityId?.GetValue<int>() switch
                {
                    1 => hasHdr10Plus ? HdrFormat.DolbyVisionHdr10Plus : HdrFormat.DolbyVisionHdr10,
                    2 => HdrFormat.DolbyVisionSdr,
                    4 => HdrFormat.DolbyVisionHlg,
                    6 => hasHdr10Plus ? HdrFormat.DolbyVisionHdr10Plus : HdrFormat.DolbyVisionHdr10,
                    _ => HdrFormat.DolbyVision
                };
            }

            if (!ValidHdrColourPrimaries.Contains(colorPrimaries) || !ValidHdrTransferFunctions.Contains(transferFunction))
            {
                return HdrFormat.None;
            }

            if (HlgTransferFunctions.Contains(transferFunction))
            {
                return HdrFormat.Hlg10;
            }

            if (PqTransferFunctions.Contains(transferFunction))
            {
                if (TryGetSideData(sideData, FFMpegCoreSideDataTypes.HdrDynamicMetadataSpmte2094, out _))
                {
                    return HdrFormat.Hdr10Plus;
                }

                if (TryGetSideData(sideData, FFMpegCoreSideDataTypes.MasteringDisplayMetadata, out _) ||
                    TryGetSideData(sideData, FFMpegCoreSideDataTypes.ContentLightLevelMetadata, out _))
                {
                    return HdrFormat.Hdr10;
                }

                return HdrFormat.Pq10;
            }

            return HdrFormat.None;
        }

        private static bool TryGetSideData(IReadOnlyList<Dictionary<string, JsonNode>> list, string name, out Dictionary<string, JsonNode> result)
        {
            result = list?.FirstOrDefault(item =>
                item.TryGetValue("side_data_type", out var rawSideDataType) &&
                rawSideDataType.GetValue<string>().Equals(name, StringComparison.OrdinalIgnoreCase));

            return result != null;
        }
    }
}
