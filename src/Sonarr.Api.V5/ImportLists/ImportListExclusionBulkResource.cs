namespace Sonarr.Api.V5.ImportLists;

public class ImportListExclusionBulkResource
{
    public required HashSet<int> Ids { get; set; }
}
