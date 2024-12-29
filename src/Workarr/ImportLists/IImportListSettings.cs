using Workarr.ThingiProvider;

namespace Workarr.ImportLists
{
    public interface IImportListSettings : IProviderConfig
    {
        string BaseUrl { get; set; }
    }
}
