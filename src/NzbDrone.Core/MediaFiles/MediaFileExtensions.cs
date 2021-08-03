using System;
using System.Collections.Generic;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles
{
    public static class MediaFileExtensions
    {
        private static Dictionary<string, Quality> _fileExtensions;

        static MediaFileExtensions()
        {
            _fileExtensions = new Dictionary<string, Quality>(StringComparer.OrdinalIgnoreCase)
            {
                //Unknown
                { ".webm", Quality.Unknown },

                //SDTV
                { ".m4v", Quality.SDTV },
                { ".3gp", Quality.SDTV },
                { ".nsv", Quality.SDTV },
                { ".ty", Quality.SDTV },
                { ".strm", Quality.SDTV },
                { ".rm", Quality.SDTV },
                { ".rmvb", Quality.SDTV },
                { ".m3u", Quality.SDTV },
                { ".ifo", Quality.SDTV },
                { ".mov", Quality.SDTV },
                { ".qt", Quality.SDTV },
                { ".divx", Quality.SDTV },
                { ".xvid", Quality.SDTV },
                { ".bivx", Quality.SDTV },
                { ".nrg", Quality.SDTV },
                { ".pva", Quality.SDTV },
                { ".wmv", Quality.SDTV },
                { ".asf", Quality.SDTV },
                { ".asx", Quality.SDTV },
                { ".ogm", Quality.SDTV },
                { ".ogv", Quality.SDTV },
                { ".m2v", Quality.SDTV },
                { ".avi", Quality.SDTV },
                { ".bin", Quality.SDTV },
                { ".dat", Quality.SDTV },
                { ".dvr-ms", Quality.SDTV },
                { ".mpg", Quality.SDTV },
                { ".mpeg", Quality.SDTV },
                { ".mp4", Quality.SDTV },
                { ".avc", Quality.SDTV },
                { ".vp3", Quality.SDTV },
                { ".svq3", Quality.SDTV },
                { ".nuv", Quality.SDTV },
                { ".viv", Quality.SDTV },
                { ".dv", Quality.SDTV },
                { ".fli", Quality.SDTV },
                { ".flv", Quality.SDTV },
                { ".wpl", Quality.SDTV },

                //DVD
                { ".img", Quality.DVD },
                { ".iso", Quality.DVD },
                { ".vob", Quality.DVD },

                //HD
                { ".mkv", Quality.HDTV720p },
                { ".ts", Quality.HDTV720p },
                { ".wtv", Quality.HDTV720p },

                //Bluray
                { ".m2ts", Quality.Bluray720p }
            };
        }

        public static HashSet<string> Extensions => new HashSet<string>(_fileExtensions.Keys, StringComparer.OrdinalIgnoreCase);

        public static Quality GetQualityForExtension(string extension)
        {
            if (_fileExtensions.ContainsKey(extension))
            {
                return _fileExtensions[extension];
            }

            return Quality.Unknown;
        }
    }
}
