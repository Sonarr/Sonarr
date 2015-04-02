using System;

namespace NzbDrone.Core.Metadata.Files
{
    public class MetadataFileResult
    {
        public String RelativePath { get; set; }
        public String Contents { get; set; }

        public MetadataFileResult(string relativePath, string contents)
        {
            RelativePath = relativePath;
            Contents = contents;
        }
    }
}
