using System;
using System.IO;
using MediaInfoLib;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model;

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
                throw new FileNotFoundException("Media file does not exist: " + filename);

            var mediaInfo = new MediaInfoLib.MediaInfo();

            try
            {
                _logger.Trace("Getting media info from {0}", filename);

                mediaInfo.Option("ParseSpeed", "0.2");
                int open = mediaInfo.Open(filename);

                if (open != 0)
                {
                    int width;
                    int height;
                    int videoBitRate;
                    int audioBitRate;
                    int runTime;
                    int streamCount;
                    int audioChannels;

                    string subtitles = mediaInfo.Get(StreamKind.General, 0, "Text_Language_List");
                    string scanType = mediaInfo.Get(StreamKind.Video, 0, "ScanType");
                    Int32.TryParse(mediaInfo.Get(StreamKind.Video, 0, "Width"), out width);
                    Int32.TryParse(mediaInfo.Get(StreamKind.Video, 0, "Height"), out height);
                    Int32.TryParse(mediaInfo.Get(StreamKind.Video, 0, "BitRate"), out videoBitRate);

                    string aBitRate = mediaInfo.Get(StreamKind.Audio, 0, "BitRate");
                    int ABindex = aBitRate.IndexOf(" /");
                    if (ABindex > 0)
                        aBitRate = aBitRate.Remove(ABindex);

                    Int32.TryParse(aBitRate, out audioBitRate);
                    Int32.TryParse(mediaInfo.Get(StreamKind.General, 0, "PlayTime"), out runTime);
                    Int32.TryParse(mediaInfo.Get(StreamKind.Audio, 0, "StreamCount"), out streamCount);

                    string audioChannelsStr = mediaInfo.Get(StreamKind.Audio, 0, "Channel(s)");
                    int ACindex = audioChannelsStr.IndexOf(" /");
                    if (ACindex > 0)
                        audioChannelsStr = audioChannelsStr.Remove(ACindex);

                    string audioLanguages = mediaInfo.Get(StreamKind.General, 0, "Audio_Language_List");
                    decimal videoFrameRate = Decimal.Parse(mediaInfo.Get(StreamKind.Video, 0, "FrameRate"));
                    string audioProfile = mediaInfo.Get(StreamKind.Audio, 0, "Format_Profile");

                    int APindex = audioProfile.IndexOf(" /");
                    if (APindex > 0)
                        audioProfile = audioProfile.Remove(APindex);

                    Int32.TryParse(audioChannelsStr, out audioChannels);
                    var mediaInfoModel = new MediaInfoModel
                                                {
                                                    VideoCodec = mediaInfo.Get(StreamKind.Video, 0, "Codec/String"),
                                                    VideoBitrate = videoBitRate,
                                                    Height = height,
                                                    Width = width,
                                                    AudioFormat = mediaInfo.Get(StreamKind.Audio, 0, "Format"),
                                                    AudioBitrate = audioBitRate,
                                                    RunTime = (runTime / 1000), //InSeconds
                                                    AudioStreamCount = streamCount,
                                                    AudioChannels = audioChannels,
                                                    AudioProfile = audioProfile.Trim(),
                                                    VideoFps = videoFrameRate,
                                                    AudioLanguages = audioLanguages,
                                                    Subtitles = subtitles,
                                                    ScanType = scanType
                                                };

                    mediaInfo.Close();
                    return mediaInfoModel;
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to parse media info from file: " + filename, ex);
                mediaInfo.Close();
            }

            return null;
        }

        public TimeSpan GetRunTime(string filename)
        {
            MediaInfoLib.MediaInfo mediaInfo = null;
            try
            {
                mediaInfo = new MediaInfoLib.MediaInfo();
                _logger.Trace("Getting media info from {0}", filename);

                mediaInfo.Option("ParseSpeed", "0.2");
                int open = mediaInfo.Open(filename);

                if (open != 0)
                {
                    int runTime;
                    Int32.TryParse(mediaInfo.Get(StreamKind.General, 0, "PlayTime"), out runTime);

                    mediaInfo.Close();
                    return TimeSpan.FromMilliseconds(runTime);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to parse media info from file: " + filename, ex);
            }
            finally
            {
                if (mediaInfo != null)
                {
                    mediaInfo.Close();
                }
            }

            return new TimeSpan();
        }
    }
}
