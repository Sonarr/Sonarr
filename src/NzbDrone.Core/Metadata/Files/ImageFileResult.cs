using System;

namespace NzbDrone.Core.Metadata.Files
{
    public class ImageFileResult
    {
        public String RelativePath { get; set; }
        public String Url { get; set; }

        public ImageFileResult(string relativePath, string url)
        {
            RelativePath = relativePath;
            Url = url;
        }
    }
}
