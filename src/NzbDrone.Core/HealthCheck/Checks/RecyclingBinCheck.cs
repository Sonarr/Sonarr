using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(EpisodeImportedEvent), CheckOnCondition.FailedOnly)]
    [CheckOn(typeof(EpisodeImportFailedEvent), CheckOnCondition.SuccessfulOnly)]
    public class RecyclingBinCheck : HealthCheckBase
    {
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;

        public RecyclingBinCheck(IConfigService configService, IDiskProvider diskProvider)
        {
            _configService = configService;
            _diskProvider = diskProvider;
        }

        public override HealthCheck Check()
        {
            var recycleBin = _configService.RecycleBin;

            if (recycleBin.IsNullOrWhiteSpace())
            {
                return new HealthCheck(GetType());
            }

            if (!_diskProvider.FolderWritable(recycleBin))
            {
                return new HealthCheck(GetType(), HealthCheckResult.Error, $"Unable to write to configured recycling bin folder: {recycleBin}. Ensure this path exists and is writable by the user running Sonarr", "#cannot-write-recycle-bin");
            }

            return new HealthCheck(GetType());
        }
    }
}
