using System;
using System.Linq;
using NLog;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Profiles.Qualities.Commands;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Profiles.Qualities
{
    public class QualityProfileDowngradeService : IExecute<ApplyQualityProfileDowngradeCommand>
    {
        private readonly IQualityProfileService _qualityProfileService;
        private readonly ISeriesService _seriesService;
        private readonly IMediaFileRepository _mediaFileRepository;
        private readonly Logger _logger;

        public QualityProfileDowngradeService(IQualityProfileService qualityProfileService,
                                              ISeriesService seriesService,
                                              IMediaFileRepository mediaFileRepository,
                                              Logger logger)
        {
            _qualityProfileService = qualityProfileService;
            _seriesService = seriesService;
            _mediaFileRepository = mediaFileRepository;
            _logger = logger;
        }

        public void Execute(ApplyQualityProfileDowngradeCommand message)
        {
            var profiles = _qualityProfileService.All()
                .Where(p => p.DowngradeAllowed && p.DowngradeToProfileId.HasValue && p.DowngradeAfterDays.HasValue && p.DowngradeAfterDays.Value > 0)
                .ToList();

            if (!profiles.Any())
            {
                _logger.Trace("No quality profiles configured for downgrade");
                return;
            }

            var allSeries = _seriesService.GetAllSeries();

            foreach (var profile in profiles)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-profile.DowngradeAfterDays.Value);
                var targetProfileId = profile.DowngradeToProfileId!.Value;

                var seriesForProfile = allSeries.Where(s => s.QualityProfileId == profile.Id).ToList();

                foreach (var series in seriesForProfile)
                {
                    var files = _mediaFileRepository.GetFilesBySeries(series.Id);
                    if (files == null || files.Count == 0)
                    {
                        continue;
                    }

                    var lastAdded = files.Max(f => f.DateAdded);
                    if (lastAdded <= cutoffDate)
                    {
                        if (series.QualityProfileId == targetProfileId)
                        {
                            continue;
                        }

                        _logger.Info("Downgrading series '{0}' (ID {1}) from profile {2} to profile {3}", series.Title, series.Id, series.QualityProfileId, targetProfileId);
                        series.QualityProfileId = targetProfileId;
                        _seriesService.UpdateSeries(series, publishUpdatedEvent: true);
                    }
                }
            }
        }
    }
}

