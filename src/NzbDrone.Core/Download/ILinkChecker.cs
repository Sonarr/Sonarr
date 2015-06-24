using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IFilehostLinkChecker
    {
        IList<FilehostLinkCheckInfo> CheckLinks(IList<ReleaseInfo> releases);
    }

    public class FilehostLinkCheckInfo
    {
        public bool IsOnline { get; set; }
        public ReleaseInfo Release { get; set; }
        public string Title { get; set; }
        public long Size { get; set; }
    }
}