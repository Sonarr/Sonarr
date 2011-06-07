using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class PostProcessingProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ConfigProvider _configProvider;
        private readonly DiskProvider _diskProvider;
        private readonly SeriesProvider _seriesProvider;
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly EpisodeProvider _episodeProvider;

        public PostProcessingProvider(ConfigProvider configProvider, DiskProvider diskProvider,
                                        SeriesProvider seriesProvider, MediaFileProvider mediaFileProvider,
                                        EpisodeProvider episodeProvider)
        {
            _configProvider = configProvider;
            _diskProvider = diskProvider;
            _seriesProvider = seriesProvider;
            _mediaFileProvider = mediaFileProvider;
            _episodeProvider = episodeProvider;
        }

        //Scan folder
        //Delete Existing episode(s)
        //Move file(s)
        //Import file(s)

        public virtual void Scan()
        {
            var dropFolder = _configProvider.SabDropDirectory;

            if (String.IsNullOrEmpty(dropFolder))
            {
                Logger.Warn("Unable to Scan for New Downloads - Folder Name is Empty");
                return;
            }

            if (!_diskProvider.FolderExists(dropFolder))
            {
                Logger.Warn("Unable to Scan for New Downloads - Folder Doesn't Exist");
                return;
            }

            var subfolders = _diskProvider.GetDirectories(dropFolder);

            foreach (var subfolder in subfolders)
            {
                var di = new DirectoryInfo(subfolder);

                if (di.Name.StartsWith("_UNPACK_"))
                {
                    Logger.Info("Folder [{0}] is still being unpacked", subfolder);
                    continue;
                }

                if (di.Name.StartsWith("_FAILED_"))
                {
                    Logger.Info("Folder [{0}] is marked as failed", subfolder);
                    continue;
                }

                //Parse the Folder name
                var seriesName = Parser.ParseSeriesName(di.Name);
                var series = _seriesProvider.FindSeries(seriesName);

                if (series == null)
                {
                    Logger.Warn("Unable to Import new download, series is not being watched");
                    return;
                }

                _mediaFileProvider.ImportNewFiles(subfolder, series);
            }
        }
    }
}
