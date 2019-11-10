using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.ImportLists
{
    public interface IImportListSettings : IProviderConfig
    {
        string BaseUrl { get; set; }
    }
}
