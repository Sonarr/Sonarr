using System;

namespace NzbDrone.Core.Metadata.Files
{
    public class MetadataFileResult
    {
        public String Path { get; set; }
        public String Contents { get; set; }

        public MetadataFileResult(string path, string contents)
        {
            Path = path;
            Contents = contents;
        }
    }
}
