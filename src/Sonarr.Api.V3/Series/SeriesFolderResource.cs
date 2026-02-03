using NzbDrone.Core.Languages;

namespace Sonarr.Api.V3.Series
{
    public class SeriesFolderResource
    {
        public Language Language { get; set; }
        public string Folder { get; set; }
    }
}
