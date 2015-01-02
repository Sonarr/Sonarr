using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Metadata;
using NzbDrone.Core.Metadata.Files;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class DeleteBadMediaCovers : IHousekeepingTask
    {
        private readonly ISeriesService _seriesService;
        private readonly IMetadataFileService _metadataFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DeleteBadMediaCovers(ISeriesService seriesService, IMetadataFileService metadataFileService, IDiskProvider diskProvider, IConfigService configService, Logger logger)
        {
            _seriesService = seriesService;
            _metadataFileService = metadataFileService;
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public void Clean()
        {
            //if (!_configService.CleanupMetadataImages) return;

            var series = _seriesService.GetAllSeries();

            _logger.Debug("Processing image files for {0} series", series.Count);

            foreach (var show in series)
            {
                var images = _metadataFileService.GetFilesBySeries(show.Id)
                    .Where(c => c.LastUpdated > new DateTime(2014, 12, 27) &&
                        (c.Type == MetadataType.EpisodeImage || c.Type == MetadataType.SeasonImage || c.Type == MetadataType.SeriesImage)).ToList();



                _logger.Debug("Processing {0} images for {1}", images.Count, show.Title);
                foreach (var image in images)
                {
                    try
                    {
                        var path = Path.Combine(show.Path, image.RelativePath);


                        if (!IsValid(path))
                        {
                            _logger.Debug("Deleting invalid image file " + path);
                            DeleteMetadata(image.Id, path);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.ErrorException("Couldn't validate image " + image.RelativePath, e);
                    }
                }
            }


            _configService.CleanupMetadataImages = false;
        }

        private void DeleteMetadata(int id, string path)
        {
            _metadataFileService.Delete(id);
            _diskProvider.DeleteFile(path);
        }

        private bool IsValid(string path)
        {
            _logger.Trace("Checking {0}", path);


            var buffer = new byte[10];

            using (var imageStream = _diskProvider.StreamFile(path))
            {
                if (imageStream.Length < buffer.Length) return false;
                imageStream.Read(buffer, 0, buffer.Length);
            }

            var text = System.Text.Encoding.Default.GetString(buffer);

            return !text.ToLowerInvariant().Contains("html");
        }
    }
}