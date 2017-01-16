namespace NzbDrone.Core.Extras.Metadata.Files
{
    public class MetadataFileResult
    {
        public string RelativePath { get; set; }
        public string Contents { get; set; }

        public MetadataFileResult(string relativePath, string contents)
        {
            RelativePath = relativePath;
            Contents = contents;
        }
    }
}
