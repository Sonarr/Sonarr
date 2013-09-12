using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using NzbDrone.Core.Qualities;


namespace NzbDrone.Core.MediaFiles
{
    public static class MediaFileExtensions
    {
        private static HashSet<MediaFileExtension> _fileExtensions;

        static MediaFileExtensions()
        {
            _fileExtensions = new HashSet<MediaFileExtension>
            {
                new MediaFileExtension(".m4v", Quality.SDTV),
                new MediaFileExtension(".3gp", Quality.SDTV),
                new MediaFileExtension(".nsv", Quality.SDTV),
                new MediaFileExtension(".ty", Quality.SDTV),
                new MediaFileExtension(".strm", Quality.SDTV),
                new MediaFileExtension(".rm", Quality.SDTV),
                new MediaFileExtension(".rmvb", Quality.SDTV),
                new MediaFileExtension(".m3u", Quality.SDTV),
                new MediaFileExtension(".ifo", Quality.SDTV),
                new MediaFileExtension(".mov", Quality.SDTV),
                new MediaFileExtension(".qt", Quality.SDTV),
                new MediaFileExtension(".divx", Quality.SDTV),
                new MediaFileExtension(".xvid", Quality.SDTV),
                new MediaFileExtension(".bivx", Quality.SDTV),
                new MediaFileExtension(".vob", Quality.SDTV),
                new MediaFileExtension(".nrg", Quality.SDTV),
                new MediaFileExtension(".pva", Quality.SDTV),
                new MediaFileExtension(".wmv", Quality.SDTV),
                new MediaFileExtension(".asf", Quality.SDTV),
                new MediaFileExtension(".asx", Quality.SDTV),
                new MediaFileExtension(".ogm", Quality.SDTV),
                new MediaFileExtension(".m2v", Quality.SDTV),
                new MediaFileExtension(".avi", Quality.SDTV),
                new MediaFileExtension(".bin", Quality.SDTV),
                new MediaFileExtension(".dat", Quality.SDTV),
                new MediaFileExtension(".dvr-ms", Quality.SDTV),
                new MediaFileExtension(".mpg", Quality.SDTV),
                new MediaFileExtension(".mpeg", Quality.SDTV),
                new MediaFileExtension(".mp4", Quality.SDTV),
                new MediaFileExtension(".avc", Quality.SDTV),
                new MediaFileExtension(".vp3", Quality.SDTV),
                new MediaFileExtension(".svq3", Quality.SDTV),
                new MediaFileExtension(".nuv", Quality.SDTV),
                new MediaFileExtension(".viv", Quality.SDTV),
                new MediaFileExtension(".dv", Quality.SDTV),
                new MediaFileExtension(".fli", Quality.SDTV),
                new MediaFileExtension(".flv", Quality.SDTV),
                new MediaFileExtension(".wpl", Quality.SDTV),

                //DVD
                new MediaFileExtension(".img", Quality.DVD),
                new MediaFileExtension(".iso", Quality.DVD),

                //HD
                new MediaFileExtension(".mkv", Quality.HDTV720p),
                new MediaFileExtension(".ts", Quality.HDTV720p),
                new MediaFileExtension(".m2ts", Quality.HDTV720p)
            };
        }

        public static HashSet<String> Extensions
        {
            get { return new HashSet<String>(_fileExtensions.Select(e => e.Extension.ToLower())); }
        }

        public static Quality FindQuality(string extension)
        {
            var mediaFileExtension = _fileExtensions.SingleOrDefault(e => e.Extension.Equals(extension, StringComparison.InvariantCultureIgnoreCase));

            if (mediaFileExtension == null)
            {
                return Quality.Unknown;
            }

            return mediaFileExtension.Quality;
        }
    }

    public class MediaFileExtension
    {
        public string Extension { get; set; }
        public Quality Quality { get; set; }

        public MediaFileExtension(string extension, Quality quality)
        {
            Extension = extension;
            Quality = quality;
        }
    }
}
