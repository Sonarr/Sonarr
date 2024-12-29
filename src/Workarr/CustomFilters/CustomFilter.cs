using Workarr.Datastore;

namespace Workarr.CustomFilters
{
    public class CustomFilter : ModelBase
    {
        public string Type { get; set; }
        public string Label { get; set; }
        public string Filters { get; set; }
    }
}
