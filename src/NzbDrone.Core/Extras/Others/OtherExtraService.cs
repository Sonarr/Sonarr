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
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public OtherExtraService(IConfigService configService,
                                 IDiskTransferService diskTransferService,
                                 IOtherExtraFileService otherExtraFileService,
                                 IDiskProvider diskProvider,
                                 Logger logger)
            : base(configService, diskTransferService, otherExtraFileService)
        {
            _otherExtraFileService = otherExtraFileService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public override int Order => 2;

        public override IEnumerable<ExtraFile> CreateAfterSeriesScan(Series series, List<EpisodeFile> episodeFiles)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodeImport(Series series, EpisodeFile episodeFile)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> CreateAfterEpisodeImport(Series series, string seriesFolder, string seasonFolder)
        {
            return Enumerable.Empty<ExtraFile>();
        }

        public override IEnumerable<ExtraFile> MoveFilesAfterRename(Series series, List<EpisodeFile> episodeFiles)
        {
            // TODO: Remove
            // We don't want to move files after rename yet.

            return Enumerable.Empty<ExtraFile>();

            var extraFiles = _otherExtraFileService.GetFilesBySeries(series.Id);
            var movedFiles = new List<OtherExtraFile>();

            foreach (var episodeFile in episodeFiles)
            {
                var extraFilesForEpisodeFile = extraFiles.Where(m => m.EpisodeFileId == episodeFile.Id).ToList();

                foreach (var extraFile in extraFilesForEpisodeFile)
                {
                    var existingFileName = Path.Combine(series.Path, extraFile.RelativePath);
                    var extension = Path.GetExtension(existingFileName).TrimStart('.');
                    var newFileName = Path.ChangeExtension(Path.Combine(series.Path, episodeFile.RelativePath), extension);

                    if (newFileName.PathNotEquals(existingFileName))
                    {
                        try
                        {
                            _diskProvider.MoveFile(existingFileName, newFileName);
                            extraFile.RelativePath = series.Path.GetRelativePath(newFileName);
                            movedFiles.Add(extraFile);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn(ex, "Unable to move extra file: {0}", existingFileName);
                        }
                    }
                }
            }

            _otherExtraFileService.Upsert(movedFiles);

            return movedFiles;
        }

        public override ExtraFile Import(Series series, EpisodeFile episodeFile, string path, string extension, bool readOnly)
        {
            // If the extension is .nfo we need to change it to .nfo-orig
            if (Path.GetExtension(path).Equals(".nfo"))
            {
                extension += "-orig";
            }

            var extraFile = ImportFile(series, episodeFile, path, extension, readOnly);

            _otherExtraFileService.Upsert(extraFile);

            return extraFile;
        }
    }
}
