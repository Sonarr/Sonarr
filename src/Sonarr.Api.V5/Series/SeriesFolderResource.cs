using NzbDrone.Core.Languages;

namespace Sonarr.Api.V5.Series;

public class SeriesFolderResource
{
    public required Language Language { get; set; }
    public required string Folder { get; set; }
}
