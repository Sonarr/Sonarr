using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras
{
    public interface IExtraService
    {
        void ImportEpisode(LocalEpisode localEpisode, EpisodeFile episodeFile, bool isReadOnly);
    }

    public class ExtraService : IExtraService,
                                IHandle<MediaCoversUpdatedEvent>,
                                IHandle<EpisodeFolderCreatedEvent>,
                                IHandle<SeriesScannedEvent>,
                                IHandle<SeriesRenamedEvent>
    {
        private readonly IMediaFileService _mediaFileService;
        private readonly IEpisodeService _episodeService;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly List<IManageExtraFiles> _extraFileManagers;

        public ExtraService(IMediaFileService mediaFileService,
                            IEpisodeService episodeService,
                            IDiskProvider diskProvider,
                            IConfigService configService,
                            IEnumerable<IManageExtraFiles> extraFileManagers,
                            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _episodeService = episodeService;
            _diskProvider = diskProvider;
            _configService = configService;
            _extraFileManagers = extraFileManagers.OrderBy(e => e.Order).ToList();
        }

        public void ImportEpisode(LocalEpisode localEpisode, EpisodeFile episodeFile, bool isReadOnly)
        {
            ImportExtraFiles(localEpisode, episodeFile, isReadOnly);

            CreateAfterEpisodeImport(localEpisode.Series, episodeFile);
        }

        private void ImportExtraFiles(LocalEpisode localEpisode, EpisodeFile episodeFile, bool isReadOnly)
        {
            if (!_configService.ImportExtraFiles)
            {
                return;
            }

            var folderSearchOption = localEpisode.FolderEpisodeInfo == null
                ? SearchOption.TopDirectoryOnly
                : SearchOption.AllDirectories;

            var wantedExtensions = _configService.ExtraFileExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                                     .Select(e => e.Trim(' ', '.')
                                                                     .Insert(0, "."))
                                                                     .ToList();

            var sourceFolder = _diskProvider.GetParentFolder(localEpisode.Path);
            var files = _diskProvider.GetFiles(sourceFolder, folderSearchOption);
            var managedFiles = _extraFileManagers.Select((i) => new List<string>()).ToArray();

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                var matchingExtension = wantedExtensions.FirstOrDefault(e => e.Equals(extension));

                if (matchingExtension == null)
                {
                    continue;
                }

                for (int i = 0; i < _extraFileManagers.Count; i++)
                {
                    if (_extraFileManagers[i].CanImportFile(localEpisode, episodeFile, file, extension, isReadOnly))
                    {
                        managedFiles[i].Add(file);
                        break;
                    }
                }
            }

            for (int i = 0; i < _extraFileManagers.Count; i++)
            {
                _extraFileManagers[i].ImportFiles(localEpisode, episodeFile, managedFiles[i], isReadOnly);
            }
        }

        private void CreateAfterEpisodeImport(Series series, EpisodeFile episodeFile)
        {
            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterEpisodeImport(series, episodeFile);
            }
        }

        public void Handle(MediaCoversUpdatedEvent message)
        {
            if (message.Updated)
            {
                var series = message.Series;

                foreach (var extraFileManager in _extraFileManagers)
                {
                    extraFileManager.CreateAfterMediaCoverUpdate(series);
                }
            }
        }

        public void Handle(SeriesScannedEvent message)
        {
            var series = message.Series;
            var episodeFiles = GetEpisodeFiles(series.Id);

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterSeriesScan(series, episodeFiles);
            }
        }

        public void Handle(EpisodeFolderCreatedEvent message)
        {
            var series = message.Series;

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.CreateAfterEpisodeFolder(series, message.SeriesFolder, message.SeasonFolder);
            }
        }

        public void Handle(SeriesRenamedEvent message)
        {
            var series = message.Series;
            var episodeFiles = GetEpisodeFiles(series.Id);

            foreach (var extraFileManager in _extraFileManagers)
            {
                extraFileManager.MoveFilesAfterRename(series, episodeFiles);
            }
        }

        private List<EpisodeFile> GetEpisodeFiles(int seriesId)
        {
            var episodeFiles = _mediaFileService.GetFilesBySeries(seriesId);
            var episodes = _episodeService.GetEpisodeBySeries(seriesId);

            foreach (var episodeFile in episodeFiles)
            {
                var localEpisodeFile = episodeFile;
                episodeFile.Episodes = new List<Episode>(episodes.Where(e => e.EpisodeFileId == localEpisodeFile.Id));
            }

            return episodeFiles;
        }
    }
}
