using System;
using System.Globalization;
using System.IO;
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

        public const int MINIMUM_MEDIA_INFO_SCHEMA_REVISION = 3;
        public const int CURRENT_MEDIA_INFO_SCHEMA_REVISION = 7;

        public VideoFileInfoReader(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public MediaInfoModel GetMediaInfo(string filename)
        {
            if (!_diskProvider.FileExists(filename))
            {
                throw new FileNotFoundException("Media file does not exist: " + filename);
            }

            MediaInfo mediaInfo = null;

            // TODO: Cache media info by path, mtime and length so we don't need to read files multiple times

            try
            {
                mediaInfo = new MediaInfo();
                _logger.Debug("Getting media info from {0}", filename);

                if (filename.ToLower().EndsWith(".ts"))
                {
                    // For .ts files we often have to scan more of the file to get all the info we need
                    mediaInfo.Option("ParseSpeed", "0.3");
                }
                else
                {
                    mediaInfo.Option("ParseSpeed", "0.0");
                }

                int open;

                using (var stream = _diskProvider.OpenReadStream(filename))
                {
                    open = mediaInfo.Open(stream);
                }

                if (open != 0)
                {
                    int audioRuntime;
                    int videoRuntime;
                    int generalRuntime;

                    // Runtime
                    int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "PlayTime"), out videoRuntime);
                    int.TryParse(mediaInfo.Get(StreamKind.Audio, 0, "PlayTime"), out audioRuntime);
                    int.TryParse(mediaInfo.Get(StreamKind.General, 0, "PlayTime"), out generalRuntime);

                    // Audio Channels
                    var audioChannelsStr = mediaInfo.Get(StreamKind.Audio, 0, "Channel(s)").Split(new string[] { " /" }, StringSplitOptions.None)[0].Trim();
                    var audioChannelPositions = mediaInfo.Get(StreamKind.Audio, 0, "ChannelPositions/String2");
                    int.TryParse(audioChannelsStr, out var audioChannels);

                    if (audioRuntime == 0 && videoRuntime == 0 && generalRuntime == 0)
                    {
                        // No runtime, ask mediainfo to scan the whole file
                        _logger.Trace("No runtime value found, rescanning at 1.0 scan depth");
                        mediaInfo.Option("ParseSpeed", "1.0");

                        using (var stream = _diskProvider.OpenReadStream(filename))
                        {
                            open = mediaInfo.Open(stream);
                        }
                    }
                    else if (audioChannels > 2 && audioChannelPositions.IsNullOrWhiteSpace())
                    {
                        // Some files with DTS don't have ChannelPositions unless more of the file is scanned
                        _logger.Trace("DTS audio without expected channel information, rescanning at 0.3 scan depth");
                        mediaInfo.Option("ParseSpeed", "0.3");

                        using (var stream = _diskProvider.OpenReadStream(filename))
                        {
                            open = mediaInfo.Open(stream);
                        }
                    }
                }

                if (open != 0)
                {
                    int width;
                    int height;
                    int videoBitRate;
                    int audioBitRate;
                    int audioRuntime;
                    int videoRuntime;
                    int generalRuntime;
                    int streamCount;
                    int audioChannels;
                    int audioChannelsOrig;
                    int videoBitDepth;
                    decimal videoFrameRate;
                    int videoMultiViewCount;

                    string subtitles = mediaInfo.Get(StreamKind.General, 0, "Text_Language_List");
                    string scanType = mediaInfo.Get(StreamKind.Video, 0, "ScanType");
                    int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "Width"), out width);
                    int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "Height"), out height);
                    int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "BitRate"), out videoBitRate);
                    if (videoBitRate <= 0)
                    {
                        int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "BitRate_Nominal"), out videoBitRate);
                    }

                    decimal.TryParse(mediaInfo.Get(StreamKind.Video, 0, "FrameRate"), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out videoFrameRate);
                    int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "BitDepth"), out videoBitDepth);
                    int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "MultiView_Count"), out videoMultiViewCount);

                    //Runtime
                    int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "PlayTime"), out videoRuntime);
                    int.TryParse(mediaInfo.Get(StreamKind.Audio, 0, "PlayTime"), out audioRuntime);
                    int.TryParse(mediaInfo.Get(StreamKind.General, 0, "PlayTime"), out generalRuntime);

                    string aBitRate = mediaInfo.Get(StreamKind.Audio, 0, "BitRate").Split(new string[] { " /" }, StringSplitOptions.None)[0].Trim();

                    int.TryParse(aBitRate, out audioBitRate);
                    int.TryParse(mediaInfo.Get(StreamKind.Audio, 0, "StreamCount"), out streamCount);

                    var audioChannelsStr = mediaInfo.Get(StreamKind.Audio, 0, "Channel(s)").Split(new string[] { " /" }, StringSplitOptions.None)[0].Trim();
                    int.TryParse(audioChannelsStr, out audioChannels);

                    var audioChannelsStrOrig = mediaInfo.Get(StreamKind.Audio, 0, "Channel(s)_Original").Split(new string[] { " /" }, StringSplitOptions.None)[0].Trim();
                    int.TryParse(audioChannelsStrOrig, out audioChannelsOrig);

                    var audioChannelPositionsText = mediaInfo.Get(StreamKind.Audio, 0, "ChannelPositions");
                    var audioChannelPositionsTextOrig = mediaInfo.Get(StreamKind.Audio, 0, "ChannelPositions_Original");
                    var audioChannelPositions = mediaInfo.Get(StreamKind.Audio, 0, "ChannelPositions/String2");

                    string audioLanguages = mediaInfo.Get(StreamKind.General, 0, "Audio_Language_List");

                    string videoProfile = mediaInfo.Get(StreamKind.Video, 0, "Format_Profile").Split(new string[] { " /" }, StringSplitOptions.None)[0].Trim();
                    string audioProfile = mediaInfo.Get(StreamKind.Audio, 0, "Format_Profile").Split(new string[] { " /" }, StringSplitOptions.None)[0].Trim();

                    var mediaInfoModel = new MediaInfoModel
                    {
                        ContainerFormat = mediaInfo.Get(StreamKind.General, 0, "Format"),
                        VideoFormat = mediaInfo.Get(StreamKind.Video, 0, "Format"),
                        VideoCodecID = mediaInfo.Get(StreamKind.Video, 0, "CodecID"),
                        VideoProfile = videoProfile,
                        VideoCodecLibrary = mediaInfo.Get(StreamKind.Video, 0, "Encoded_Library"),
                        VideoBitrate = videoBitRate,
                        VideoBitDepth = videoBitDepth,
                        VideoMultiViewCount = videoMultiViewCount,
                        VideoColourPrimaries = mediaInfo.Get(StreamKind.Video, 0, "colour_primaries"),
                        VideoTransferCharacteristics = mediaInfo.Get(StreamKind.Video, 0, "transfer_characteristics"),
                        VideoHdrFormat = mediaInfo.Get(StreamKind.Video, 0, "HDR_Format"),
                        VideoHdrFormatCompatibility = mediaInfo.Get(StreamKind.Video, 0, "HDR_Format_Compatibility"),
                        Height = height,
                        Width = width,
                        AudioFormat = mediaInfo.Get(StreamKind.Audio, 0, "Format"),
                        AudioCodecID = mediaInfo.Get(StreamKind.Audio, 0, "CodecID"),
                        AudioProfile = audioProfile,
                        AudioCodecLibrary = mediaInfo.Get(StreamKind.Audio, 0, "Encoded_Library"),
                        AudioAdditionalFeatures = mediaInfo.Get(StreamKind.Audio, 0, "Format_AdditionalFeatures"),
                        AudioBitrate = audioBitRate,
                        RunTime = GetBestRuntime(audioRuntime, videoRuntime, generalRuntime),
                        AudioStreamCount = streamCount,
                        AudioChannelsContainer = audioChannels,
                        AudioChannelsStream = audioChannelsOrig,
                        AudioChannelPositions = audioChannelPositions,
                        AudioChannelPositionsTextContainer = audioChannelPositionsText,
                        AudioChannelPositionsTextStream = audioChannelPositionsTextOrig,
                        VideoFps = videoFrameRate,
                        AudioLanguages = audioLanguages,
                        Subtitles = subtitles,
                        ScanType = scanType,
                        SchemaRevision = CURRENT_MEDIA_INFO_SCHEMA_REVISION
                    };

                    return mediaInfoModel;
                }
                else
                {
                    _logger.Warn("Unable to open media info from file: " + filename);
                }
            }
            catch (DllNotFoundException ex)
            {
                _logger.Error(ex, "mediainfo is required but was not found");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to parse media info from file: {0}", filename);
            }
            finally
            {
                mediaInfo?.Close();
            }

            return null;
        }

        public TimeSpan? GetRunTime(string filename)
        {
            var info = GetMediaInfo(filename);

            return info?.RunTime;
        }

        private TimeSpan GetBestRuntime(int audio, int video, int general)
        {
            if (video == 0)
            {
                if (audio == 0)
                {
                    return TimeSpan.FromMilliseconds(general);
                }

                return TimeSpan.FromMilliseconds(audio);
            }

            return TimeSpan.FromMilliseconds(video);
        }
    }
}
