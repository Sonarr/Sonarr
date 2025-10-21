using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.MediaFiles.Encoding
{
    public class FfmpegEncodingService : IFfmpegEncodingService
    {
        private readonly IConfigService _configService;
        private readonly IProcessProvider _processProvider;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        private static readonly Regex ProgressRegex = new Regex(@"time=(\d+):(\d+):(\d+\.\d+)", RegexOptions.Compiled);

        public FfmpegEncodingService(
            IConfigService configService,
            IProcessProvider processProvider,
            IDiskProvider diskProvider,
            Logger logger)
        {
            _configService = configService;
            _processProvider = processProvider;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public bool ShouldEncode(string filePath, MediaInfoModel mediaInfo)
        {
            if (!_configService.EnableFfmpegEncoding)
            {
                _logger.Trace("FFmpeg encoding is disabled");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_configService.FfmpegPath))
            {
                _logger.Warn("FFmpeg encoding is enabled but FFmpeg path is not configured");
                return false;
            }

            if (!_diskProvider.FileExists(filePath))
            {
                _logger.Warn("File does not exist: {0}", filePath);
                return false;
            }

            // Check minimum size requirement (in GB)
            var minimumSizeBytes = (long)_configService.FfmpegMinimumSize * 1024L * 1024L * 1024L;
            if (minimumSizeBytes > 0)
            {
                var fileSize = _diskProvider.GetFileSize(filePath);
                if (fileSize < minimumSizeBytes)
                {
                    _logger.Debug("File {0} is {1} bytes, below minimum threshold of {2} GB. Skipping encoding.",
                        filePath, fileSize, _configService.FfmpegMinimumSize);
                    return false;
                }
            }

            // Check if file is already HEVC/H265
            if (_configService.FfmpegSkipHevc && mediaInfo != null)
            {
                var videoCodec = mediaInfo.VideoCodec?.ToLowerInvariant() ?? "";
                if (videoCodec.Contains("hevc") || videoCodec.Contains("h265") || videoCodec.Contains("h.265"))
                {
                    _logger.Debug("File {0} is already encoded with HEVC/H265. Skipping encoding.", filePath);
                    return false;
                }
            }

            return true;
        }

        public string EncodeFile(string filePath, MediaInfoModel mediaInfo)
        {
            if (!ShouldEncode(filePath, mediaInfo))
            {
                return filePath;
            }

            var ffmpegPath = _configService.FfmpegPath;
            var ffmpegArguments = _configService.FfmpegArguments;
            var deleteOriginal = _configService.FfmpegDeleteOriginal;
            var timeoutMinutes = _configService.FfmpegTimeout;

            _logger.Info("Starting FFmpeg encoding for file: {0}", filePath);

            // Create temporary output file path
            var directory = Path.GetDirectoryName(filePath);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var tempOutputPath = Path.Combine(directory, $"{fileNameWithoutExtension}.encoding{extension}");

            try
            {
                // Build FFmpeg command arguments
                var arguments = $"-i \"{filePath}\" {ffmpegArguments} \"{tempOutputPath}\" -y";

                _logger.Debug("Executing FFmpeg: {0} {1}", ffmpegPath, arguments);

                // Calculate timeout in milliseconds (0 = no timeout)
                var timeoutMs = timeoutMinutes > 0 ? timeoutMinutes * 60 * 1000 : 0;

                // Execute FFmpeg with progress monitoring
                ProcessOutput processOutput;

                if (timeoutMs > 0)
                {
                    var processStartTime = DateTime.UtcNow;

                    processOutput = _processProvider.StartAndCapture(
                        ffmpegPath,
                        arguments,
                        onOutputDataReceived: (data) =>
                        {
                            if (!string.IsNullOrWhiteSpace(data))
                            {
                                _logger.Trace("FFmpeg: {0}", data);

                                // Extract progress information from FFmpeg output
                                var match = ProgressRegex.Match(data);
                                if (match.Success)
                                {
                                    var hours = int.Parse(match.Groups[1].Value);
                                    var minutes = int.Parse(match.Groups[2].Value);
                                    var seconds = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                                    var totalSeconds = hours * 3600 + minutes * 60 + seconds;
                                    _logger.Debug("FFmpeg encoding progress: {0:F2}s encoded", totalSeconds);
                                }
                            }
                        });
                }
                else
                {
                    processOutput = _processProvider.StartAndCapture(
                        ffmpegPath,
                        arguments,
                        onOutputDataReceived: (data) =>
                        {
                            if (!string.IsNullOrWhiteSpace(data))
                            {
                                _logger.Trace("FFmpeg: {0}", data);
                            }
                        });
                }

                // Check if encoding was successful
                if (processOutput.ExitCode != 0)
                {
                    var errorOutput = string.Join(Environment.NewLine, processOutput.Error);
                    _logger.Error("FFmpeg encoding failed with exit code {0}. Error: {1}",
                        processOutput.ExitCode, errorOutput);

                    // Clean up temporary file if it exists
                    if (_diskProvider.FileExists(tempOutputPath))
                    {
                        _diskProvider.DeleteFile(tempOutputPath);
                    }

                    // Return original file path on error
                    return filePath;
                }

                _logger.Info("FFmpeg encoding completed successfully for: {0}", filePath);

                // Verify the encoded file exists and has content
                if (!_diskProvider.FileExists(tempOutputPath))
                {
                    _logger.Error("FFmpeg encoding completed but output file does not exist: {0}", tempOutputPath);
                    return filePath;
                }

                var outputSize = _diskProvider.GetFileSize(tempOutputPath);
                if (outputSize == 0)
                {
                    _logger.Error("FFmpeg encoding completed but output file is empty: {0}", tempOutputPath);
                    _diskProvider.DeleteFile(tempOutputPath);
                    return filePath;
                }

                _logger.Debug("Encoded file size: {0} bytes", outputSize);

                // Handle original file based on configuration
                if (deleteOriginal)
                {
                    _logger.Debug("Deleting original file: {0}", filePath);
                    _diskProvider.DeleteFile(filePath);

                    // Rename encoded file to original name
                    _diskProvider.MoveFile(tempOutputPath, filePath);
                    _logger.Info("Encoded file moved to original location: {0}", filePath);
                    return filePath;
                }
                else
                {
                    _logger.Debug("Keeping both original and encoded files");

                    // Create a new filename for the encoded file
                    var encodedFileName = $"{fileNameWithoutExtension}.encoded{extension}";
                    var encodedFilePath = Path.Combine(directory, encodedFileName);

                    // If file already exists, add timestamp
                    if (_diskProvider.FileExists(encodedFilePath))
                    {
                        encodedFileName = $"{fileNameWithoutExtension}.encoded.{DateTime.Now:yyyyMMddHHmmss}{extension}";
                        encodedFilePath = Path.Combine(directory, encodedFileName);
                    }

                    _diskProvider.MoveFile(tempOutputPath, encodedFilePath);
                    _logger.Info("Encoded file saved as: {0}", encodedFilePath);

                    // Return the encoded file path for import
                    return encodedFilePath;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during FFmpeg encoding of file: {0}", filePath);

                // Clean up temporary file if it exists
                try
                {
                    if (_diskProvider.FileExists(tempOutputPath))
                    {
                        _diskProvider.DeleteFile(tempOutputPath);
                    }
                }
                catch (Exception cleanupEx)
                {
                    _logger.Warn(cleanupEx, "Failed to clean up temporary file: {0}", tempOutputPath);
                }

                // Return original file path on error
                return filePath;
            }
        }
    }
}
