using System;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListStatus : ProviderStatusBase
    {
        public DateTime? LastInfoSync { get; set; }
    }
}
