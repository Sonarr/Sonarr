using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras.Others
{
    public interface IOtherExtraFileService : IExtraFileService<OtherExtraFile>
    {
    }

    public class OtherExtraFileService : ExtraFileService<OtherExtraFile>, IOtherExtraFileService
    {
        public OtherExtraFileService(IExtraFileRepository<OtherExtraFile> repository, ISeriesService seriesService, IDiskProvider diskProvider, Logger logger)
            : base(repository, seriesService, diskProvider, logger)
        {
        }
    }
}
