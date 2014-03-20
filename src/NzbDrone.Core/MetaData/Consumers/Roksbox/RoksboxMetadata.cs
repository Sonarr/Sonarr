using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Metadata.Files;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Metadata.Consumers.Roksbox
{
    public class RoksboxMetadata : MetadataBase<RoksboxMetadataSettings>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMetadataFileService _metadataFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IHttpProvider _httpProvider;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public RoksboxMetadata(IEventAggregator eventAggregator,
                            IMapCoversToLocal mediaCoverService,
                            IMediaFileService mediaFileService,
                            IMetadataFileService metadataFileService,
                            IDiskProvider diskProvider,
                            IHttpProvider httpProvider,
                            IEpisodeService episodeService,
                            Logger logger)
            : base(diskProvider, httpProvider, logger)
        {
            _eventAggregator = eventAggregator;
            _mediaCoverService = mediaCoverService;
            _mediaFileService = mediaFileService;
            _metadataFileService = metadataFileService;
            _diskProvider = diskProvider;
            _httpProvider = httpProvider;
            _episodeService = episodeService;
            _logger = logger;
        }

        private static List<string> ValidCertification = new List<string> { "G", "NC-17", "PG", "PG-13", "R", "UR", "UNRATED", "NR", "TV-Y", "TV-Y7", "TV-Y7-FV", "TV-G", "TV-PG", "TV-14", "TV-MA" };
        private static readonly Regex SeasonImagesRegex = new Regex(@"^(season (?<season>\d+))|(?<season>specials)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override void OnSeriesUpdated(Series series, List<MetadataFile> existingMetadataFiles, List<EpisodeFile> episodeFiles)
        {
            var metadataFiles = new List<MetadataFile>();

            if (!_diskProvider.FolderExists(series.Path))
            {
                _logger.Info("Series folder ({0}) does not exist, skipping metadata creation", series.Path);
                return;
            }

            if (Settings.SeriesImages)
            {
                var metadata = WriteSeriesImages(series, existingMetadataFiles);
                if (metadata != null)
                {
                    metadataFiles.Add(metadata);
                }
            }

            if (Settings.SeasonImages)
            {
                var metadata = WriteSeasonImages(series, existingMetadataFiles);
                if (metadata != null)
                {
                    metadataFiles.AddRange(metadata);
                }
            }

            foreach (var episodeFile in episodeFiles)
            {
                if (Settings.EpisodeMetadata)
                {
                    var metadata = WriteEpisodeMetadata(series, episodeFile, existingMetadataFiles);
                    if (metadata != null)
                    {
                        metadataFiles.Add(metadata);
                    }
                }
            }

            foreach (var episodeFile in episodeFiles)
            {
                if (Settings.EpisodeImages)
                {
                    var metadataFile = WriteEpisodeImages(series, episodeFile, existingMetadataFiles);

                    if (metadataFile != null)
                    {
                        metadataFiles.Add(metadataFile);
                    }
                }
            }
            metadataFiles.RemoveAll(c => c == null);
            _eventAggregator.PublishEvent(new MetadataFilesUpdated(metadataFiles));
        }

        public override void OnEpisodeImport(Series series, EpisodeFile episodeFile, bool newDownload)
        {
            var metadataFiles = new List<MetadataFile>();

            if (Settings.EpisodeMetadata)
            {
                metadataFiles.Add(WriteEpisodeMetadata(series, episodeFile, new List<MetadataFile>()));
            }

            if (Settings.EpisodeImages)
            {
                var metadataFile = WriteEpisodeImages(series, episodeFile, new List<MetadataFile>());

                if (metadataFile != null)
                {
                    metadataFiles.Add(metadataFile);
                }
            }

            _eventAggregator.PublishEvent(new MetadataFilesUpdated(metadataFiles));
        }

        public override void AfterRename(Series series, List<MetadataFile> existingMetadataFiles, List<EpisodeFile> episodeFiles)
        {
            var episodeFilesMetadata = existingMetadataFiles.Where(c => c.EpisodeFileId > 0).ToList();
            var updatedMetadataFiles = new List<MetadataFile>();

            foreach (var episodeFile in episodeFiles)
            {
                var metadataFiles = episodeFilesMetadata.Where(m => m.EpisodeFileId == episodeFile.Id).ToList();

                foreach (var metadataFile in metadataFiles)
                {
                    string newFilename;

                    if (metadataFile.Type == MetadataType.EpisodeImage)
                    {
                        newFilename = GetEpisodeImageFilename(episodeFile.Path);
                    }

                    else if (metadataFile.Type == MetadataType.EpisodeMetadata)
                    {
                        newFilename = GetEpisodeMetadataFilename(episodeFile.Path);
                    }

                    else
                    {
                        _logger.Trace("Unknown episode file metadata: {0}", metadataFile.RelativePath);
                        continue;
                    }

                    var existingFilename = Path.Combine(series.Path, metadataFile.RelativePath);

                    if (!newFilename.PathEquals(existingFilename))
                    {
                        _diskProvider.MoveFile(existingFilename, newFilename);
                        metadataFile.RelativePath = DiskProviderBase.GetRelativePath(series.Path, newFilename);

                        updatedMetadataFiles.Add(metadataFile);
                    }
                }
            }

            _eventAggregator.PublishEvent(new MetadataFilesUpdated(updatedMetadataFiles));
        }

        public override MetadataFile FindMetadataFile(Series series, string path)
        {
            var filename = Path.GetFileName(path);

            if (filename == null) return null;
            var parentdir = Directory.GetParent(path);

            var metadata = new MetadataFile
                           {
                               SeriesId = series.Id,
                               Consumer = GetType().Name,
                               RelativePath = DiskProviderBase.GetRelativePath(series.Path, path)
                           };

            //Series and season images are both named folder.jpg, only season ones sit in season folders
            if (String.Compare(filename, parentdir.Name, true) == 0)
            {
                var seasonMatch = SeasonImagesRegex.Match(parentdir.Name);
                if (seasonMatch.Success)
                {
                    metadata.Type = MetadataType.SeasonImage;

                    var seasonNumber = seasonMatch.Groups["season"].Value;

                    if (seasonNumber.Contains("specials"))
                    {
                        metadata.SeasonNumber = 0;
                    }
                    else
                    {
                        metadata.SeasonNumber = Convert.ToInt32(seasonNumber);
                    }

                    return metadata;
                }
                else
                {
                    metadata.Type = MetadataType.SeriesImage;
                    return metadata;
                }
            }

            var parseResult = Parser.Parser.ParseTitle(filename);

            if (parseResult != null &&
                !parseResult.FullSeason)
            {
                switch (Path.GetExtension(filename).ToLowerInvariant())
                {
                    case ".xml":
                        metadata.Type = MetadataType.EpisodeMetadata;
                        return metadata;
                    case ".jpg":
                        metadata.Type = MetadataType.EpisodeImage;
                        return metadata;
                }
                
            }

            return null;
        }

        private MetadataFile WriteSeriesImages(Series series, List<MetadataFile> existingMetadataFiles)
        {
            //Because we only support one image, attempt to get the Poster type, then if that fails grab the first
            var image = series.Images.SingleOrDefault(c => c.CoverType == MediaCoverTypes.Poster) ?? series.Images.FirstOrDefault();
            if (image == null)
            {
                _logger.Trace("Failed to find suitable Series image for series {0}.", series.Title);
                return null;
            }

            var source = _mediaCoverService.GetCoverPath(series.Id, image.CoverType);
            var destination = Path.Combine(series.Path, Path.GetFileName(series.Path) + Path.GetExtension(source));

            //TODO: Do we want to overwrite the file if it exists?
            if (_diskProvider.FileExists(destination))
            {
                _logger.Debug("Series image: {0} already exists.", image.CoverType);
                return null;
            }
            else
            {

                _diskProvider.CopyFile(source, destination, false);

                var metadata = existingMetadataFiles.SingleOrDefault(c => c.Type == MetadataType.SeriesImage) ??
                                new MetadataFile
                                {
                                    SeriesId = series.Id,
                                    Consumer = GetType().Name,
                                    Type = MetadataType.SeriesImage,
                                    RelativePath = DiskProviderBase.GetRelativePath(series.Path, destination)
                                };

                return metadata;
            }
        }

        private IEnumerable<MetadataFile> WriteSeasonImages(Series series, List<MetadataFile> existingMetadataFiles)
        {
            _logger.Debug("Writing season images for {0}.", series.Title);
            //Create a dictionary between season number and output folder
            var seasonFolderMap = new Dictionary<int, string>();
            foreach (var folder in Directory.EnumerateDirectories(series.Path))
            {
                var directoryinfo = new DirectoryInfo(folder);
                var seasonMatch = SeasonImagesRegex.Match(directoryinfo.Name);
                if (seasonMatch.Success)
                {
                    var seasonNumber = seasonMatch.Groups["season"].Value;

                    if (seasonNumber.Contains("specials"))
                    {
                        seasonFolderMap[0] = folder;
                    }
                    else
                    {
                        int matchedSeason;
                        if (Int32.TryParse(seasonNumber, out matchedSeason))
                        {
                            seasonFolderMap[matchedSeason] = folder;
                        }
                        else
                        {
                            _logger.Debug("Failed to parse season number from {0} for series {1}.", folder, series.Title);
                        }
                    }
                }
                else
                {
                    _logger.Debug("Rejecting folder {0} for series {1}.", Path.GetDirectoryName(folder), series.Title);
                }
            }
            foreach (var season in series.Seasons)
            {
                //Work out the path to this season - if we don't have a matching path then skip this season.
                string seasonFolder;
                if (!seasonFolderMap.TryGetValue(season.SeasonNumber, out seasonFolder))
                {
                    _logger.Trace("Failed to find season folder for series {0}, season {1}.", series.Title, season.SeasonNumber);
                    continue;
                }

                //Roksbox only supports one season image, so first of all try for poster otherwise just use whatever is first in the collection
                var image = season.Images.SingleOrDefault(c => c.CoverType == MediaCoverTypes.Poster) ?? season.Images.FirstOrDefault();
                if (image == null)
                {
                    _logger.Trace("Failed to find suitable season image for series {0}, season {1}.", series.Title, season.SeasonNumber);
                    continue;
                }


                var filename = Path.GetFileName(seasonFolder) + ".jpg";

                var path = Path.Combine(series.Path, seasonFolder, filename);
                _logger.Debug("Writing season image for series {0}, season {1} to {2}.", series.Title, season.SeasonNumber, path);
                DownloadImage(series, image.Url, path);

                var metadata = existingMetadataFiles.SingleOrDefault(c => c.Type == MetadataType.SeasonImage &&
                                                                            c.SeasonNumber == season.SeasonNumber) ??
                                new MetadataFile
                                {
                                    SeriesId = series.Id,
                                    SeasonNumber = season.SeasonNumber,
                                    Consumer = GetType().Name,
                                    Type = MetadataType.SeasonImage,
                                    RelativePath = DiskProviderBase.GetRelativePath(series.Path, path)
                                };

                yield return metadata;
            }
        }

        private MetadataFile WriteEpisodeMetadata(Series series, EpisodeFile episodeFile, List<MetadataFile> existingMetadataFiles)
        {
            var filename = GetEpisodeMetadataFilename(episodeFile.Path);
            var relativePath = DiskProviderBase.GetRelativePath(series.Path, filename);

            var existingMetadata = existingMetadataFiles.SingleOrDefault(c => c.Type == MetadataType.EpisodeMetadata &&
                                                                              c.EpisodeFileId == episodeFile.Id);

            if (existingMetadata != null)
            {
                var fullPath = Path.Combine(series.Path, existingMetadata.RelativePath);
                if (!filename.PathEquals(fullPath))
                {
                    _diskProvider.MoveFile(fullPath, filename);
                    existingMetadata.RelativePath = relativePath;
                }
            }

            _logger.Debug("Generating {0} for: {1}", filename, episodeFile.Path);

            var xmlResult = String.Empty;
            foreach (var episode in episodeFile.Episodes.Value)
            {
                var sb = new StringBuilder();
                var xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;
                xws.Indent = false;

                using (var xw = XmlWriter.Create(sb, xws))
                {
                    var doc = new XDocument();

                    var details = new XElement("video");
                    details.Add(new XElement("title", String.Format("{0} - {1}x{2} - {3}", series.Title, episode.SeasonNumber, episode.EpisodeNumber, episode.Title)));
                    details.Add(new XElement("year", episode.AirDate));
                    details.Add(new XElement("genre", String.Join(" / ", series.Genres)));
                    var actors = String.Join(" , ", series.Actors.ConvertAll(c => c.Name + " - " + c.Character).GetRange(0, Math.Min(3, series.Actors.Count)));
                    details.Add(new XElement("actors", actors));
                    details.Add(new XElement("description", episode.Overview));
                    details.Add(new XElement("length", series.Runtime));
                    details.Add(new XElement("mpaa", ValidCertification.Contains( series.Certification.ToUpperInvariant() ) ? series.Certification.ToUpperInvariant() : "UNRATED" ) );
                    doc.Add(details);
                    doc.Save(xw);

                    xmlResult += doc.ToString();
                    xmlResult += Environment.NewLine;
                }
            }
            
            _logger.Debug("Saving episodedetails to: {0}", filename);
            _diskProvider.WriteAllText(filename, xmlResult.Trim(Environment.NewLine.ToCharArray()));

            var metadata = existingMetadata ??
                           new MetadataFile
                           {
                               SeriesId = series.Id,
                               EpisodeFileId = episodeFile.Id,
                               Consumer = GetType().Name,
                               Type = MetadataType.EpisodeMetadata,
                               RelativePath = DiskProviderBase.GetRelativePath(series.Path, filename)
                           };

            return metadata;
        }

        private MetadataFile WriteEpisodeImages(Series series, EpisodeFile episodeFile, List<MetadataFile> existingMetadataFiles)
        {
            var screenshot = episodeFile.Episodes.Value.First().Images.SingleOrDefault(i => i.CoverType == MediaCoverTypes.Screenshot);

            if (screenshot == null)
            {
                _logger.Trace("Episode screenshot not available");
                return null;
            }

            var filename = GetEpisodeImageFilename(episodeFile.Path);
            var relativePath = DiskProviderBase.GetRelativePath(series.Path, filename);

            var existingMetadata = existingMetadataFiles.SingleOrDefault(c => c.Type == MetadataType.EpisodeImage &&
                                                                              c.EpisodeFileId == episodeFile.Id);

            if (existingMetadata != null)
            {
                var fullPath = Path.Combine(series.Path, existingMetadata.RelativePath);
                if (!filename.PathEquals(fullPath))
                {
                    _diskProvider.MoveFile(fullPath, filename);
                    existingMetadata.RelativePath = relativePath;
                }
            }

            DownloadImage(series, screenshot.Url, filename);

            var metadata = existingMetadata ??
                           new MetadataFile
                           {
                               SeriesId = series.Id,
                               EpisodeFileId = episodeFile.Id,
                               Consumer = GetType().Name,
                               Type = MetadataType.EpisodeImage,
                               RelativePath = DiskProviderBase.GetRelativePath(series.Path, filename)
                           };

            return metadata;
        }

        private string GetEpisodeMetadataFilename(string episodeFilePath)
        {
            return Path.ChangeExtension(episodeFilePath, "xml");
        }

        private string GetEpisodeImageFilename(string episodeFilePath)
        {
            return Path.ChangeExtension(episodeFilePath, "jpg");
        }
    }
}
