using System;

namespace NzbDrone.Core.Metadata.Files
{
    public class ImageFileResult
    {
        public string RelativePath { get; set; }
        public string Url { get; set; }

        public ImageFileResult(string relativePath, string url)
        {
            RelativePath = relativePath;
            Url = url;
        }
    }
}
