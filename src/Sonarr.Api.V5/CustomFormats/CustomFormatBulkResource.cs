namespace Sonarr.Api.V5.CustomFormats;

public class CustomFormatBulkResource
{
    public HashSet<int> Ids { get; set; } = [];
    public bool? IncludeCustomFormatWhenRenaming { get; set; }
}
