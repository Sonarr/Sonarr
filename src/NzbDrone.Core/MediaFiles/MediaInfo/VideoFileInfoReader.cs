using System;
using System.IO;
using MediaInfoLib;
using NLog;
using NzbDrone.Common;

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

            MediaInfoLib.MediaInfo mediaInfo = null;

            try
            {
                mediaInfo = new MediaInfoLib.MediaInfo();
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
                    decimal videoFrameRate;

                    string subtitles = mediaInfo.Get(StreamKind.General, 0, "Text_Language_List");
                    string scanType = mediaInfo.Get(StreamKind.Video, 0, "ScanType");
                    Int32.TryParse(mediaInfo.Get(StreamKind.Video, 0, "Width"), out width);
                    Int32.TryParse(mediaInfo.Get(StreamKind.Video, 0, "Height"), out height);
                    Int32.TryParse(mediaInfo.Get(StreamKind.Video, 0, "BitRate"), out videoBitRate);
                    Int32.TryParse(mediaInfo.Get(StreamKind.Video, 0, "PlayTime"), out runTime);
                    Decimal.TryParse(mediaInfo.Get(StreamKind.Video, 0, "FrameRate"), out videoFrameRate);

                    string aBitRate = mediaInfo.Get(StreamKind.Audio, 0, "BitRate");
                    int aBindex = aBitRate.IndexOf(" /", StringComparison.InvariantCultureIgnoreCase);
                    if (aBindex > 0)
                        aBitRate = aBitRate.Remove(aBindex);

                    Int32.TryParse(aBitRate, out audioBitRate);
                    Int32.TryParse(mediaInfo.Get(StreamKind.Audio, 0, "StreamCount"), out streamCount);
                    

                    string audioChannelsStr = mediaInfo.Get(StreamKind.Audio, 0, "Channel(s)");
                    int aCindex = audioChannelsStr.IndexOf(" /", StringComparison.InvariantCultureIgnoreCase);
                    if (aCindex > 0)
                        audioChannelsStr = audioChannelsStr.Remove(aCindex);

                    string audioLanguages = mediaInfo.Get(StreamKind.General, 0, "Audio_Language_List");
                    string audioProfile = mediaInfo.Get(StreamKind.Audio, 0, "Format_Profile");

                    int aPindex = audioProfile.IndexOf(" /", StringComparison.InvariantCultureIgnoreCase);
                    if (aPindex > 0)
                        audioProfile = audioProfile.Remove(aPindex);

                    Int32.TryParse(audioChannelsStr, out audioChannels);
                    var mediaInfoModel = new MediaInfoModel
                                                {
                                                    VideoCodec = mediaInfo.Get(StreamKind.Video, 0, "Codec/String"),
                                                    VideoBitrate = videoBitRate,
                                                    Height = height,
                                                    Width = width,

                                                    AudioFormat = mediaInfo.Get(StreamKind.Audio, 0, "Format"),
                                                    AudioBitrate = audioBitRate,
                                                    RunTime = TimeSpan.FromMilliseconds(runTime),
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
            catch (DllNotFoundException ex)
            {
                _logger.ErrorException("mediainfo is required but was not found", ex);
                throw;
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
    }
}
