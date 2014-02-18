using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
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
        private readonly Logger _logger;

        public XbmcMetadata(IEventAggregator eventAggregator,
                            IMapCoversToLocal mediaCoverService,
                            IMediaFileService mediaFileService,
                            IMetadataFileService metadataFileService,
                            IDiskProvider diskProvider,
                            IHttpProvider httpProvider,
                            Logger logger)
            : base(diskProvider, httpProvider, logger)
        {
            _eventAggregator = eventAggregator;
            _mediaCoverService = mediaCoverService;
            _mediaFileService = mediaFileService;
            _metadataFileService = metadataFileService;
            _diskProvider = diskProvider;
            _httpProvider = httpProvider;
            _logger = logger;
        }

        private static readonly Regex SeriesImagesRegex = new Regex(@"^(?<type>poster|banner|fanart)\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SeasonImagesRegex = new Regex(@"^season(?<season>\d{2,}|-all|-specials)-(?<type>poster|banner|fanart)\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex EpisodeImageRegex = new Regex(@"-thumb\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override void OnSeriesUpdated(Series series, List<MetadataFile> existingMetadataFiles)
        {
            if (Settings.SeriesMetadata)
            {
                EnsureFolder(series.Path);
                WriteTvShowNfo(series, existingMetadataFiles);
            }

            if (Settings.SeriesImages)
            {
                EnsureFolder(series.Path);
                WriteSeriesImages(series, existingMetadataFiles);
            }

            if (Settings.SeasonImages)
            {
                EnsureFolder(series.Path);
                WriteSeasonImages(series, existingMetadataFiles);
            }
        }

        public override void OnEpisodeImport(Series series, EpisodeFile episodeFile, bool newDownload)
        {
            if (Settings.EpisodeMetadata)
            {
                WriteEpisodeNfo(series, episodeFile);
            }

            if (Settings.EpisodeImages)
            {
                WriteEpisodeImages(series, episodeFile);
            }
        }

        public override void AfterRename(Series series)
        {
            var episodeFiles = _mediaFileService.GetFilesBySeries(series.Id);
            var episodeFilesMetadata = _metadataFileService.GetFilesBySeries(series.Id).Where(c => c.EpisodeFileId > 0).ToList();

            foreach (var episodeFile in episodeFiles)
            {
                var metadataFiles = episodeFilesMetadata.Where(m => m.EpisodeFileId == episodeFile.Id).ToList();
                var episodeFilenameWithoutExtension =
                    Path.GetFileNameWithoutExtension(DiskProviderBase.GetRelativePath(series.Path, episodeFile.Path));

                foreach (var metadataFile in metadataFiles)
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(metadataFile.RelativePath);
                    var extension = Path.GetExtension(metadataFile.RelativePath);

                    if (!fileNameWithoutExtension.Equals(episodeFilenameWithoutExtension))
                    {
                        var source = Path.Combine(series.Path, metadataFile.RelativePath);
                        var destination = Path.Combine(series.Path, fileNameWithoutExtension + extension);

                        _diskProvider.MoveFile(source, destination);
                        metadataFile.RelativePath = fileNameWithoutExtension + extension;

                        _eventAggregator.PublishEvent(new MetadataFileUpdated(metadataFile));
                    }
                }
            }
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

        private void WriteTvShowNfo(Series series, List<MetadataFile> existingMetadataFiles)
        {
            _logger.Trace("Generating tvshow.nfo for: {0}", series.Title);
            var sb = new StringBuilder();
            var xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = false;

            using (var xw = XmlWriter.Create(sb, xws))
            {
                var tvShow = new XElement("tvshow");

                tvShow.Add(new XElement("title", series.Title));
                tvShow.Add(new XElement("rating", series.Ratings.Percentage));
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

                _eventAggregator.PublishEvent(new MetadataFileUpdated(metadata));
            }
        }

        private void WriteSeriesImages(Series series, List<MetadataFile> existingMetadataFiles)
        {
            foreach (var image in series.Images)
            {
                var source = _mediaCoverService.GetCoverPath(series.Id, image.CoverType);
                var destination = Path.Combine(series.Path, image.CoverType.ToString().ToLowerInvariant() + Path.GetExtension(source));

                //TODO: Do we want to overwrite the file if it exists?
                if (_diskProvider.FileExists(destination))
                {
                    _logger.Trace("Series image: {0} already exists.", image.CoverType);
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

                _eventAggregator.PublishEvent(new MetadataFileUpdated(metadata));
            }
        }

        private void WriteSeasonImages(Series series, List<MetadataFile> existingMetadataFiles)
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

                    _eventAggregator.PublishEvent(new MetadataFileUpdated(metadata));
                }
            }
        }

        private void WriteEpisodeNfo(Series series, EpisodeFile episodeFile)
        {
            var filename = episodeFile.Path.Replace(Path.GetExtension(episodeFile.Path), ".nfo");

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

                    var details = new XElement("episodedetails");
                    details.Add(new XElement("title", episode.Title));
                    details.Add(new XElement("season", episode.SeasonNumber));
                    details.Add(new XElement("episode", episode.EpisodeNumber));
                    details.Add(new XElement("aired", episode.AirDate));
                    details.Add(new XElement("plot", episode.Overview));

                    //If trakt ever gets airs before information for specials we should add set it
                    details.Add(new XElement("displayseason"));
                    details.Add(new XElement("displayepisode"));
                    
                    details.Add(new XElement("thumb", episode.Images.Single(i => i.CoverType == MediaCoverTypes.Screenshot).Url));
                    details.Add(new XElement("watched", "false"));
                    details.Add(new XElement("rating", episode.Ratings.Percentage));

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

            var metadata = new MetadataFile
            {
                SeriesId = series.Id,
                EpisodeFileId = episodeFile.Id,
                Consumer = GetType().Name,
                Type = MetadataType.SeasonImage,
                RelativePath = DiskProviderBase.GetRelativePath(series.Path, filename)
            };

            _eventAggregator.PublishEvent(new MetadataFileUpdated(metadata));
        }

        private void WriteEpisodeImages(Series series, EpisodeFile episodeFile)
        {
            var screenshot = episodeFile.Episodes.Value.First().Images.Single(i => i.CoverType == MediaCoverTypes.Screenshot);

            var filename = Path.ChangeExtension(episodeFile.Path, "").Trim('.') + "-thumb.jpg";

            DownloadImage(series, screenshot.Url, filename);

            var metadata = new MetadataFile
                           {
                               SeriesId = series.Id,
                               EpisodeFileId = episodeFile.Id,
                               Consumer = GetType().Name,
                               Type = MetadataType.SeasonImage,
                               RelativePath = DiskProviderBase.GetRelativePath(series.Path, filename)
                           };

            _eventAggregator.PublishEvent(new MetadataFileUpdated(metadata));
        }
    }
}
