using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras.Subtitles
{
    public interface ISubtitleFileService : IExtraFileService<SubtitleFile>
    {
    }

    public class SubtitleFileService : ExtraFileService<SubtitleFile>, ISubtitleFileService
    {
        public SubtitleFileService(IExtraFileRepository<SubtitleFile> repository, ISeriesService seriesService, IDiskProvider diskProvider, Logger logger)
            : base(repository, seriesService, diskProvider, logger)
        {
        }
    }
}
