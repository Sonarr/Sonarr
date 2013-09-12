using System;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser
{
    public class QualityParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Regex SourceRegex = new Regex(@"(?<bluray>BluRay)|
                                                                (?<webdl>WEB-DL|WEBDL|WEB\sDL|WEB\-DL|WebRip)|
                                                                (?<hdtv>HDTV)|
                                                                (?<bdrip>BDRiP)|(?<brrip>BRRip)|(?<dvd>\bDVD\b|DVDRip|NTSC|PAL|xvidvd)|
                                                                (?<dsr>WS\sDSR|WS_DSR|WS\.DSR|DSR)|(?<pdtv>PDTV)|(?<sdtv>SDTV)",
                                                              RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex ResolutionRegex = new Regex(@"(?<_480p>480p)|(?<_720p>720p)|(?<_1080p>1080p)",
                                                                  RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex CodecRegex = new Regex(@"(?<x264>x264)|(?<h264>h264)|(?<xvidhd>XvidHD)|(?<xvid>Xvid)|(?<divx>divx)",
                                                             RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static QualityModel ParseQuality(string name)
        {
            Logger.Trace("Trying to parse quality for {0}", name);

            name = name.Trim();
            var normalizedName = name.CleanSeriesTitle();
            var result = new QualityModel { Quality = Quality.Unknown };
            result.Proper = (normalizedName.Contains("proper") || normalizedName.Contains("repack"));

            if (normalizedName.Contains("trollhd") || normalizedName.Contains("rawhd"))
            {
                result.Quality = Quality.RAWHD;
                return result;
            }

            var sourceMatch = SourceRegex.Match(name);
            var resolution = ParseResolution(name);
            var codecRegex = CodecRegex.Match(name);

            if (sourceMatch.Groups["bluray"].Success)
            {
                if (codecRegex.Groups["xvid"].Success || codecRegex.Groups["divx"].Success)
                {
                    result.Quality = Quality.DVD;
                    return result;
                }

                if (resolution == Resolution._1080p)
                {
                    result.Quality = Quality.Bluray1080p;
                    return result;
                }

                result.Quality = Quality.Bluray720p;
                return result;
            }

            if (sourceMatch.Groups["webdl"].Success)
            {
                if (resolution == Resolution._1080p)
                {
                    result.Quality = Quality.WEBDL1080p;
                    return result;
                }

                if (resolution == Resolution._720p)
                {
                    result.Quality = Quality.WEBDL720p;
                    return result;
                }

                if (name.Contains("[WEBDL]"))
                {
                    result.Quality = Quality.WEBDL720p;
                    return result;
                }

                result.Quality = Quality.WEBDL480p;
                return result;
            }

            if (sourceMatch.Groups["hdtv"].Success)
            {
                if (resolution == Resolution._1080p)
                {
                    result.Quality = Quality.HDTV1080p;
                    return result;
                }

                if (resolution == Resolution._720p)
                {
                    result.Quality = Quality.HDTV720p;
                    return result;
                }

                if (name.Contains("[HDTV]"))
                {
                    result.Quality = Quality.HDTV720p;
                    return result;
                }

                result.Quality = Quality.SDTV;
                return result;
            }

            if (sourceMatch.Groups["dvd"].Success ||
                sourceMatch.Groups["bdrip"].Success ||
                sourceMatch.Groups["brrip"].Success)
            {
                result.Quality = Quality.DVD;
                return result;
            }

            if (sourceMatch.Groups["pdtv"].Success ||
                sourceMatch.Groups["sdtv"].Success ||
                sourceMatch.Groups["dsr"].Success)
            {
                result.Quality = Quality.SDTV;
                return result;
            }

            if (resolution == Resolution._1080p)
            {
                result.Quality = Quality.HDTV1080p;
                return result;
            }

            if (resolution == Resolution._720p)
            {
                result.Quality = Quality.HDTV720p;
                return result;
            }

            if (codecRegex.Groups["x264"].Success)
            {
                result.Quality = Quality.SDTV;
                return result;
            }

            //Based on extension
            if (result.Quality == Quality.Unknown && !name.ContainsInvalidPathChars())
            {
                try
                {
                    result.Quality = MediaFileExtensions.GetQualityForExtension(Path.GetExtension(name));
                }
                catch (ArgumentException)
                {
                    //Swallow exception for cases where string contains illegal 
                    //path characters.
                }
            }

            return result;
        }

        private static Resolution ParseResolution(string name)
        {
            var match = ResolutionRegex.Match(name);

            if (!match.Success) return Resolution.Unknown;
            if (match.Groups["_480p"].Success) return Resolution._480p;
            if (match.Groups["_720p"].Success) return Resolution._720p;
            if (match.Groups["_1080p"].Success) return Resolution._1080p;

            return Resolution.Unknown;
        }
    }

    public enum Resolution
    {
        _480p,
        _720p,
        _1080p,
        Unknown
    }
}
