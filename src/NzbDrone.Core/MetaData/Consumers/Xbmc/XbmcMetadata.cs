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

namespace NzbDrone.Core.Metadata.Consumers.Xbmc
{
    public class XbmcMetadata : MetadataBase<XbmcMetadataSettings>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMetadataFileService _metadataFileService;
        private readonly IDiskProvider _diskProvider;
        private readonly IHttpProvider _httpProvider;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public XbmcMetadata(IEventAggregator eventAggregator,
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

        private static readonly Regex SeriesImagesRegex = new Regex(@"^(?<type>poster|banner|fanart)\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SeasonImagesRegex = new Regex(@"^season(?<season>\d{2,}|-all|-specials)-(?<type>poster|banner|fanart)\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex EpisodeImageRegex = new Regex(@"-thumb\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override void OnSeriesUpdated(Series series, List<MetadataFile> existingMetadataFiles, List<EpisodeFile> episodeFiles)
        {
            var metadataFiles = new List<MetadataFile>();

            if (!_diskProvider.FolderExists(series.Path))
            {
                _logger.Info("Series folder does not exist, skipping metadata creation");
                return;
            }

            if (Settings.SeriesMetadata)
            {
                metadataFiles.Add(WriteTvShowNfo(series, existingMetadataFiles));
            }

            if (Settings.SeriesImages)
            {
                metadataFiles.AddRange(WriteSeriesImages(series, existingMetadataFiles));
            }

            if (Settings.SeasonImages)
            {
                metadataFiles.AddRange(WriteSeasonImages(series, existingMetadataFiles));
            }

            foreach (var episodeFile in episodeFiles)
            {
                if (Settings.EpisodeMetadata)
                {
                    metadataFiles.Add(WriteEpisodeNfo(series, episodeFile, existingMetadataFiles));
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

            _eventAggregator.PublishEvent(new MetadataFilesUpdated(metadataFiles));
        }

        public override void OnEpisodeImport(Series series, EpisodeFile episodeFile, bool newDownload)
        {
            var metadataFiles = new List<MetadataFile>();

            if (Settings.EpisodeMetadata)
            {
                metadataFiles.Add(WriteEpisodeNfo(series, episodeFile, new List<MetadataFile>()));
            }

            if (Settings.EpisodeImages)
            {
                var metadataFile = WriteEpisodeImages(series, episodeFile, new List<MetadataFile>());

                if (metadataFile != null)
                {
                    metadataFiles.Add(metadataFile);
                }
                WriteEpisodeImages(series, episodeFile, new List<MetadataFile>());
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
                        newFilename = GetEpisodeNfoFilename(episodeFile.Path);
                    }

                    else
                    {
                        _logger.Debug("Unknown episode file metadata: {0}", metadataFile.RelativePath);
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

            var metadata = new MetadataFile
                           {
                               SeriesId = series.Id,
                               Consumer = GetType().Name,
                               RelativePath = DiskProviderBase.GetRelativePath(series.Path, path)
                           };

            if (SeriesImagesRegex.IsMatch(filename))
            {
                metadata.Type = MetadataType.SeriesImage;
                return metadata;
            }

            var seasonMatch = SeasonImagesRegex.Match(filename);

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

            if (EpisodeImageRegex.IsMatch(filename))
            {
                metadata.Type = MetadataType.EpisodeImage;
                return metadata;
            }

            if (filename.Equals("tvshow.nfo", StringComparison.InvariantCultureIgnoreCase))
            {
                metadata.Type = MetadataType.SeriesMetadata;
                return metadata;
            }

            var parseResult = Parser.Parser.ParseTitle(filename);

            if (parseResult != null &&
                !parseResult.FullSeason &&
                Path.GetExtension(filename) == ".nfo")
            {
                metadata.Type = MetadataType.EpisodeMetadata;
                return metadata;
            }

            return null;
        }

        private MetadataFile WriteTvShowNfo(Series series, List<MetadataFile> existingMetadataFiles)
        {
            _logger.Debug("Generating tvshow.nfo for: {0}", series.Title);
            var sb = new StringBuilder();
            var xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = false;

            using (var xw = XmlWriter.Create(sb, xws))
            {
                var tvShow = new XElement("tvshow");

                tvShow.Add(new XElement("title", series.Title));
                tvShow.Add(new XElement("rating", (decimal)series.Ratings.Percentage/10));
                tvShow.Add(new XElement("plot", series.Overview));

                //Todo: probably will need to use TVDB to use this feature...
//                tvShow.Add(new XElement("episodeguide", new XElement("url", episodeGuideUrl)));
//                tvShow.Add(new XElement("episodeguideurl", episodeGuideUrl));
                tvShow.Add(new XElement("mpaa", series.Certification));
                tvShow.Add(new XElement("id", series.TvdbId));

                foreach (var genre in series.Genres)
                {
                    tvShow.Add(new XElement("genre", genre));
                }

                if (series.FirstAired.HasValue)
                {
                    tvShow.Add(new XElement("premiered", series.FirstAired.Value.ToString("yyyy-MM-dd")));
                }

                tvShow.Add(new XElement("studio", series.Network));

                foreach (var actor in series.Actors)
                {
                    tvShow.Add(new XElement("actor",
                                    new XElement("name", actor.Name),
                                    new XElement("role", actor.Character),
                                    new XElement("thumb", actor.Images.First().Url)
                            ));
                }

                var doc = new XDocument(tvShow);
                doc.Save(xw);

                _logger.Debug("Saving tvshow.nfo for {0}", series.Title);

                var path = Path.Combine(series.Path, "tvshow.nfo");

                _diskProvider.WriteAllText(path, doc.ToString());

                var metadata = existingMetadataFiles.SingleOrDefault(c => c.Type == MetadataType.SeriesMetadata) ??
                               new MetadataFile
                               {
                                   SeriesId = series.Id,
                                   Consumer = GetType().Name,
                                   Type = MetadataType.SeriesMetadata,
                                   RelativePath = DiskProviderBase.GetRelativePath(series.Path, path)
                               };

                return metadata;
            }
        }

        private IEnumerable<MetadataFile> WriteSeriesImages(Series series, List<MetadataFile> existingMetadataFiles)
        {
            foreach (var image in series.Images)
            {
                var source = _mediaCoverService.GetCoverPath(series.Id, image.CoverType);
                var destination = Path.Combine(series.Path, image.CoverType.ToString().ToLowerInvariant() + Path.GetExtension(source));

                //TODO: Do we want to overwrite the file if it exists?
                if (_diskProvider.FileExists(destination))
                {
                    _logger.Debug("Series image: {0} already exists.", image.CoverType);
                    continue;
                }

                _diskProvider.CopyFile(source, destination, false);

                var metadata = existingMetadataFiles.SingleOrDefault(c => c.Type == MetadataType.SeriesImage) ??
                               new MetadataFile
                               {
                                   SeriesId = series.Id,
                                   Consumer = GetType().Name,
                                   Type = MetadataType.SeriesImage,
                                   RelativePath = DiskProviderBase.GetRelativePath(series.Path, destination)
                               };

                yield return metadata;
            }
        }

        private IEnumerable<MetadataFile> WriteSeasonImages(Series series, List<MetadataFile> existingMetadataFiles)
        {
            foreach (var season in series.Seasons)
            {
                foreach (var image in season.Images)
                {
                    var filename = String.Format("season{0:00}-{1}.jpg", season.SeasonNumber, image.CoverType.ToString().ToLower());

                    if (season.SeasonNumber == 0)
                    {
                        filename = String.Format("season-specials-{0}.jpg", image.CoverType.ToString().ToLower());
                    }

                    var path = Path.Combine(series.Path, filename);
                    
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
        }

        private MetadataFile WriteEpisodeNfo(Series series, EpisodeFile episodeFile, List<MetadataFile> existingMetadataFiles)
        {
            var filename = GetEpisodeNfoFilename(episodeFile.Path);
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
                    var image = episode.Images.SingleOrDefault(i => i.CoverType == MediaCoverTypes.Screenshot);

                    var details = new XElement("episodedetails");
                    details.Add(new XElement("title", episode.Title));
                    details.Add(new XElement("season", episode.SeasonNumber));
                    details.Add(new XElement("episode", episode.EpisodeNumber));
                    details.Add(new XElement("aired", episode.AirDate));
                    details.Add(new XElement("plot", episode.Overview));

                    //If trakt ever gets airs before information for specials we should add set it
                    details.Add(new XElement("displayseason"));
                    details.Add(new XElement("displayepisode"));

                    if (image == null)
                    {
                        details.Add(new XElement("thumb"));
                    }

                    else
                    {
                        details.Add(new XElement("thumb", image.Url));
                    }
                    
                    details.Add(new XElement("watched", "false"));
                    details.Add(new XElement("rating", (decimal)episode.Ratings.Percentage/10));

                    //Todo: get guest stars, writer and director
                    //details.Add(new XElement("credits", tvdbEpisode.Writer.FirstOrDefault()));
                    //details.Add(new XElement("director", tvdbEpisode.Directors.FirstOrDefault()));

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
                _logger.Debug("Episode screenshot not available");
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

        private string GetEpisodeNfoFilename(string episodeFilePath)
        {
            return Path.ChangeExtension(episodeFilePath, "nfo");
        }

        private string GetEpisodeImageFilename(string episodeFilePath)
        {
            return Path.ChangeExtension(episodeFilePath, "").Trim('.') + "-thumb.jpg";
        }
    }
}
