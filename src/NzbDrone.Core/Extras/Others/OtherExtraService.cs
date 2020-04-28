using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras.Others
{
    public class OtherExtraService : ExtraFileManager<OtherExtraFile>
    {
        private readonly IOtherExtraFileService _otherExtraFileService;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;

        public OtherExtraService(IConfigService configService,
                                 IDiskProvider diskProvider,
                                 IDiskTransferService diskTransferService,
                                 IOtherExtraFileService otherExtraFileService,
                                 IMediaFileAttributeService mediaFileAttributeService,
                                 Logger logger)
            : base(configService, diskProvider, diskTransferService, logger)
        {
            _otherExtraFileService = otherExtraFileService;
            _mediaFileAttributeService = mediaFileAttributeService;
        }

        public override int Order => 2;

        public override IEnumerable<ExtraFile> CreateAfterMediaCoverUpdate(Series series)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterSeriesScan(Series series, List<EpisodeFile> episodeFiles)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodeImport(Series series, EpisodeFile episodeFile)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodeFolder(Series series, string seriesFolder, string seasonFolder)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Series series, List<EpisodeFile> episodeFiles)
        {
            var extraFiles = _otherExtraFileService.GetFilesBySeries(series.Id);
            var movedFiles = new List<OtherExtraFile>();

            foreach (var episodeFile in episodeFiles)
            {
                var extraFilesForEpisodeFile = extraFiles.Where(m => m.EpisodeFileId == episodeFile.Id).ToList();

                foreach (var extraFile in extraFilesForEpisodeFile)
                {
                    movedFiles.AddIfNotNull(MoveFile(series, episodeFile, extraFile));
                }
            }

            _otherExtraFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override ExtraFile Import(Series series, EpisodeFile episodeFile, string path, string extension, bool readOnly)
        {
            var extraFile = ImportFile(series, episodeFile, path, readOnly, extension, null);

            _mediaFileAttributeService.SetFilePermissions(path);
            _otherExtraFileService.Upsert(extraFile);

            return extraFile;
        }
    }
}
