using System;

namespace NzbDrone.Core.Metadata.Files
{
    public class ImageFileResult
    {
        public String Path { get; set; }
        public String Url { get; set; }

        public ImageFileResult(string path, string url)
        {
            Path = path;
            Url = url;
        }
    }
}
