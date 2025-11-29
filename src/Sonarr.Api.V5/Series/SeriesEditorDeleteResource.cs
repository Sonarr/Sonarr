namespace Sonarr.Api.V5.Series;

public class SeriesEditorDeleteResource
{
    public List<int> SeriesIds { get; set; } = [];
    public bool DeleteFiles { get; set; }
}
