using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.MediaFiles.Encoding
{
    public interface IFfmpegEncodingService
    {
        /// <summary>
        /// Encodes a video file using FFmpeg if encoding is enabled and conditions are met
        /// </summary>
        /// <param name="filePath">Path to the video file to encode</param>
        /// <param name="mediaInfo">MediaInfo of the file for codec detection</param>
        /// <returns>Path to the encoded file, or original path if encoding was skipped</returns>
        string EncodeFile(string filePath, MediaInfoModel mediaInfo);

        /// <summary>
        /// Checks if a file should be encoded based on current settings
        /// </summary>
        /// <param name="filePath">Path to the video file</param>
        /// <param name="mediaInfo">MediaInfo of the file</param>
        /// <returns>True if file should be encoded</returns>
        bool ShouldEncode(string filePath, MediaInfoModel mediaInfo);
    }
}
