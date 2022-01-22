using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly List<FFProbePixelFormat> _pixelFormats;

        public const int MINIMUM_MEDIA_INFO_SCHEMA_REVISION = 8;
        public const int CURRENT_MEDIA_INFO_SCHEMA_REVISION = 8;

        private static readonly string[] ValidHdrColourPrimaries = { "bt2020" };
        private static readonly string[] HlgTransferFunctions = { "bt2020-10", "arib-std-b67" };
        private static readonly string[] PqTransferFunctions = { "smpte2084" };
        private static readonly string[] ValidHdrTransferFunctions = HlgTransferFunctions.Concat(PqTransferFunctions).ToArray();

        public VideoFileInfoReader(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;

            // We bundle ffprobe for all platforms
            GlobalFFOptions.Configure(options => options.BinaryFolder = AppDomain.CurrentDomain.BaseDirectory);

            try
            {
                _pixelFormats = FFProbe.GetPixelFormats();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to get supported pixel formats from ffprobe");
                _pixelFormats = new List<FFProbePixelFormat>();
            }
        }

        public MediaInfoModel GetMediaInfo(string filename)
        {
            if (!_diskProvider.FileExists(filename))
            {
                throw new FileNotFoundException("Media file does not exist: " + filename);
            }

            // TODO: Cache media info by path, mtime and length so we don't need to read files multiple times

            try
            {
                _logger.Debug("Getting media info from {0}", filename);
                var ffprobeOutput = FFProbe.GetStreamJson(filename, ffOptions: new FFOptions { ExtraArguments = "-probesize 50000000" });

                var analysis = FFProbe.AnalyseStreamJson(ffprobeOutput);

                if (analysis.PrimaryAudioStream?.ChannelLayout.IsNullOrWhiteSpace() ?? true)
                {
                    ffprobeOutput = FFProbe.GetStreamJson(filename, ffOptions: new FFOptions { ExtraArguments = "-probesize 150000000 -analyzeduration 150000000" });
                    analysis = FFProbe.AnalyseStreamJson(ffprobeOutput);
                }

                var mediaInfoModel = new MediaInfoModel();
                mediaInfoModel.ContainerFormat = analysis.Format.FormatName;
                mediaInfoModel.VideoFormat = analysis.PrimaryVideoStream?.CodecName;
                mediaInfoModel.VideoCodecID = analysis.PrimaryVideoStream?.CodecTagString;
                mediaInfoModel.VideoProfile = analysis.PrimaryVideoStream?.Profile;
                mediaInfoModel.VideoBitrate = analysis.PrimaryVideoStream?.BitRate ?? 0;
                mediaInfoModel.VideoMultiViewCount = 1;
                mediaInfoModel.VideoBitDepth = GetPixelFormat(analysis.PrimaryVideoStream?.PixelFormat)?.Components.Min(x => x.BitDepth) ?? 8;
                mediaInfoModel.VideoColourPrimaries = analysis.PrimaryVideoStream?.ColorPrimaries;
                mediaInfoModel.VideoTransferCharacteristics = analysis.PrimaryVideoStream?.ColorTransfer;
                mediaInfoModel.DoviConfigurationRecord = analysis.PrimaryVideoStream?.SideDataList?.Find(x => x.GetType().Name == nameof(DoviConfigurationRecordSideData)) as DoviConfigurationRecordSideData;
                mediaInfoModel.Height = analysis.PrimaryVideoStream?.Height ?? 0;
                mediaInfoModel.Width = analysis.PrimaryVideoStream?.Width ?? 0;
                mediaInfoModel.AudioFormat = analysis.PrimaryAudioStream?.CodecName;
                mediaInfoModel.AudioCodecID = analysis.PrimaryAudioStream?.CodecTagString;
                mediaInfoModel.AudioProfile = analysis.PrimaryAudioStream?.Profile;
                mediaInfoModel.AudioBitrate = analysis.PrimaryAudioStream?.BitRate ?? 0;
                mediaInfoModel.RunTime = GetBestRuntime(analysis.PrimaryAudioStream?.Duration, analysis.PrimaryVideoStream?.Duration, analysis.Format.Duration);
                mediaInfoModel.AudioStreamCount = analysis.AudioStreams.Count;
                mediaInfoModel.AudioChannels = analysis.PrimaryAudioStream?.Channels ?? 0;
                mediaInfoModel.AudioChannelPositions = analysis.PrimaryAudioStream?.ChannelLayout;
                mediaInfoModel.VideoFps = analysis.PrimaryVideoStream?.FrameRate ?? 0;
                mediaInfoModel.AudioLanguages = analysis.AudioStreams?.Select(x => x.Language)
                    .Where(l => l.IsNotNullOrWhiteSpace())
                    .ToList();
                mediaInfoModel.Subtitles = analysis.SubtitleStreams?.Select(x => x.Language)
                    .Where(l => l.IsNotNullOrWhiteSpace())
                    .ToList();
                mediaInfoModel.ScanType = "Progressive";
                mediaInfoModel.RawStreamData = ffprobeOutput;
                mediaInfoModel.SchemaRevision = CURRENT_MEDIA_INFO_SCHEMA_REVISION;

                FFProbeFrames frames = null;

                // if it looks like PQ10 or similar HDR, do a frame analysis to figure out which type it is
                if (PqTransferFunctions.Contains(mediaInfoModel.VideoTransferCharacteristics))
                {
                    var frameOutput = FFProbe.GetFrameJson(filename, ffOptions: new () { ExtraArguments = "-read_intervals \"%+#1\" -select_streams v" });
                    mediaInfoModel.RawFrameData = frameOutput;

                    frames = FFProbe.AnalyseFrameJson(frameOutput);
                }

                var streamSideData = analysis.PrimaryVideoStream?.SideDataList ?? new ();
                var framesSideData = frames?.Frames?.Count > 0 ? frames?.Frames[0]?.SideDataList ?? new () : new ();

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

        private FFProbePixelFormat GetPixelFormat(string format)
        {
            return _pixelFormats.Find(x => x.Name == format);
        }

        public static HdrFormat GetHdrFormat(int bitDepth, string colorPrimaries, string transferFunction, List<SideData> sideData)
        {
            if (bitDepth < 10)
            {
                return HdrFormat.None;
            }

            if (TryGetSideData<DoviConfigurationRecordSideData>(sideData, out var dovi))
            {
                return dovi.DvBlSignalCompatibilityId switch
                {
                    1 => HdrFormat.DolbyVisionHdr10,
                    2 => HdrFormat.DolbyVisionSdr,
                    4 => HdrFormat.DolbyVisionHlg,
                    6 => HdrFormat.DolbyVisionHdr10,
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
                if (TryGetSideData<HdrDynamicMetadataSpmte2094>(sideData, out _))
                {
                    return HdrFormat.Hdr10Plus;
                }

                if (TryGetSideData<MasteringDisplayMetadata>(sideData, out _) ||
                    TryGetSideData<ContentLightLevelMetadata>(sideData, out _))
                {
                    return HdrFormat.Hdr10;
                }

                return HdrFormat.Pq10;
            }

            return HdrFormat.None;
        }

        private static bool TryGetSideData<T>(List<SideData> list, out T result)
        where T : SideData
        {
            result = (T)list?.FirstOrDefault(x => x.GetType().Name == typeof(T).Name);

            return result != null;
        }
    }
}
