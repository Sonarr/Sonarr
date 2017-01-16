using System;
using System.Globalization;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.MediaFiles.MediaInfo
{
    public interface IVideoFileInfoReader
    {
        MediaInfoModel GetMediaInfo(string filename);
        TimeSpan GetRunTime(string filename);
    }

    public class VideoFileInfoReader : IVideoFileInfoReader
    {
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;


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

            try
            {
                mediaInfo = new MediaInfo();
                _logger.Debug("Getting media info from {0}", filename);

                if (filename.ToLower().EndsWith(".ts"))
                {
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

                    //Runtime
                    int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "PlayTime"), out videoRuntime);
                    int.TryParse(mediaInfo.Get(StreamKind.Audio, 0, "PlayTime"), out audioRuntime);
                    int.TryParse(mediaInfo.Get(StreamKind.General, 0, "PlayTime"), out generalRuntime);

                    if (audioRuntime == 0 && videoRuntime == 0 && generalRuntime == 0)
                    {
                        mediaInfo.Option("ParseSpeed", "1.0");

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
                    int videoBitDepth;
                    decimal videoFrameRate;

                    string subtitles = mediaInfo.Get(StreamKind.General, 0, "Text_Language_List");
                    string scanType = mediaInfo.Get(StreamKind.Video, 0, "ScanType");
                    int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "Width"), out width);
                    int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "Height"), out height);
                    int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "BitRate"), out videoBitRate);
                    decimal.TryParse(mediaInfo.Get(StreamKind.Video, 0, "FrameRate"), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out videoFrameRate);
                    int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "BitDepth"), out videoBitDepth);

                    //Runtime
                    int.TryParse(mediaInfo.Get(StreamKind.Video, 0, "PlayTime"), out videoRuntime);
                    int.TryParse(mediaInfo.Get(StreamKind.Audio, 0, "PlayTime"), out audioRuntime);
                    int.TryParse(mediaInfo.Get(StreamKind.General, 0, "PlayTime"), out generalRuntime);

                    string aBitRate = mediaInfo.Get(StreamKind.Audio, 0, "BitRate");
                    int aBindex = aBitRate.IndexOf(" /", StringComparison.InvariantCultureIgnoreCase);
                    if (aBindex > 0)
                    {
                        aBitRate = aBitRate.Remove(aBindex);
                    }

                    int.TryParse(aBitRate, out audioBitRate);
                    int.TryParse(mediaInfo.Get(StreamKind.Audio, 0, "StreamCount"), out streamCount);


                    string audioChannelsStr = mediaInfo.Get(StreamKind.Audio, 0, "Channel(s)");
                    int aCindex = audioChannelsStr.IndexOf(" /", StringComparison.InvariantCultureIgnoreCase);

                    if (aCindex > 0)
                    {
                        audioChannelsStr = audioChannelsStr.Remove(aCindex);
                    }

                    var audioChannelPositions = mediaInfo.Get(StreamKind.Audio, 0, "ChannelPositions/String2");
                    var audioChannelPositionsText = mediaInfo.Get(StreamKind.Audio, 0, "ChannelPositions");

                    string audioLanguages = mediaInfo.Get(StreamKind.General, 0, "Audio_Language_List");
                    string audioProfile = mediaInfo.Get(StreamKind.Audio, 0, "Format_Profile");

                    int aPindex = audioProfile.IndexOf(" /", StringComparison.InvariantCultureIgnoreCase);

                    if (aPindex > 0)
                    {
                        audioProfile = audioProfile.Remove(aPindex);
                    }

                    int.TryParse(audioChannelsStr, out audioChannels);
                    var mediaInfoModel = new MediaInfoModel
                    {
                        VideoCodec = mediaInfo.Get(StreamKind.Video, 0, "Codec/String"),
                        VideoBitrate = videoBitRate,
                        VideoBitDepth = videoBitDepth,
                        Height = height,
                        Width = width,
                        AudioFormat = mediaInfo.Get(StreamKind.Audio, 0, "Format"),
                        AudioBitrate = audioBitRate,
                        RunTime = GetBestRuntime(audioRuntime, videoRuntime, generalRuntime),
                        AudioStreamCount = streamCount,
                        AudioChannels = audioChannels,
                        AudioChannelPositions = audioChannelPositions,
                        AudioChannelPositionsText = audioChannelPositionsText,
                        AudioProfile = audioProfile.Trim(),
                        VideoFps = videoFrameRate,
                        AudioLanguages = audioLanguages,
                        Subtitles = subtitles,
                        ScanType = scanType
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

        public TimeSpan GetRunTime(string filename)
        {
            var info = GetMediaInfo(filename);

            if (info == null)
            {
                return new TimeSpan();
            }

            return info.RunTime;
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
