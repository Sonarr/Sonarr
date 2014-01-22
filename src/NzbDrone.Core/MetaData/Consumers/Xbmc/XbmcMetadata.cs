using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Metadata.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Metadata.Consumers.Xbmc
{
    public class XbmcMetadata : MetadataConsumerBase<XbmcMetadataSettings>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly IDiskProvider _diskProvider;
        private readonly IHttpProvider _httpProvider;
        private readonly Logger _logger;

        public XbmcMetadata(IEventAggregator eventAggregator,
                            IMapCoversToLocal mediaCoverService,
                            IDiskProvider diskProvider,
                            IHttpProvider httpProvider,
                            Logger logger)
            : base(diskProvider, httpProvider, logger)
        {
            _eventAggregator = eventAggregator;
            _mediaCoverService = mediaCoverService;
            _diskProvider = diskProvider;
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public override void OnSeriesUpdated(Series series)
        {
            if (Settings.SeriesMetadata)
            {
                EnsureFolder(series.Path);
                WriteTvShowNfo(series);
            }

            if (Settings.SeriesImages)
            {
                EnsureFolder(series.Path);
                WriteSeriesImages(series);
            }

            if (Settings.SeasonImages)
            {
                EnsureFolder(series.Path);
                WriteSeasonImages(series);
            }
        }

        public override void OnEpisodeImport(Series series, EpisodeFile episodeFile, bool newDownload)
        {
            if (Settings.EpisodeMetadata)
            {
                WriteEpisodeNfo(episodeFile);
            }

            if (Settings.EpisodeImages)
            {
                WriteEpisodeImages(series, episodeFile);
            }
        }

        public override void AfterRename(Series series)
        {
            //TODO: Rename media files to match episode files

            throw new NotImplementedException();
        }

        private void WriteTvShowNfo(Series series)
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
                                    new XElement("thumb", actor.Images.First())
                            ));
                }

                var doc = new XDocument(tvShow);
                doc.Save(xw);

                _logger.Debug("Saving tvshow.nfo for {0}", series.Title);

                var path = Path.Combine(series.Path, "tvshow.nfo");

                _diskProvider.WriteAllText(path, doc.ToString());

                _eventAggregator.PublishEvent(new SeriesMetadataUpdated(series, GetType().Name, MetadataType.SeriesMetadata, DiskProvider.GetRelativePath(series.Path, path)));
            }
        }

        private void WriteSeriesImages(Series series)
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
                _eventAggregator.PublishEvent(new SeriesMetadataUpdated(series, GetType().Name, MetadataType.SeriesImage, DiskProvider.GetRelativePath(series.Path, destination)));
            }
        }

        private void WriteSeasonImages(Series series)
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
                    _eventAggregator.PublishEvent(new SeasonMetadataUpdated(series, season.SeasonNumber, GetType().Name, MetadataType.SeasonImage, DiskProvider.GetRelativePath(series.Path, path)));
                }
            }
        }

        private void WriteEpisodeNfo(EpisodeFile episodeFile)
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
                    details.Add(new XElement("displayseason", episode.SeasonNumber));
                    details.Add(new XElement("displayepisode", episode.EpisodeNumber));
                    details.Add(new XElement("thumb", episode.Images.Single(i => i.CoverType == MediaCoverTypes.Screenshot).Url));
                    details.Add(new XElement("watched", "false"));
//                    details.Add(new XElement("credits", tvdbEpisode.Writer.FirstOrDefault()));
//                    details.Add(new XElement("director", tvdbEpisode.Directors.FirstOrDefault()));
                    details.Add(new XElement("rating", episode.Ratings.Percentage));

                    //Todo: get guest stars, will need trakt to have them

                    doc.Add(details);
                    doc.Save(xw);

                    xmlResult += doc.ToString();
                    xmlResult += Environment.NewLine;
                }
            }
            
            _logger.Debug("Saving episodedetails to: {0}", filename);
            _diskProvider.WriteAllText(filename, xmlResult.Trim(Environment.NewLine.ToCharArray()));
        }

        private void WriteEpisodeImages(Series series, EpisodeFile episodeFile)
        {
            var screenshot = episodeFile.Episodes.Value.First().Images.Single(i => i.CoverType == MediaCoverTypes.Screenshot);
            var filename = Path.ChangeExtension(episodeFile.Path, "jpg");

            DownloadImage(series, screenshot.Url, filename);
            _eventAggregator.PublishEvent(new EpisodeMetadataUpdated(series, episodeFile, GetType().Name, MetadataType.SeasonImage, DiskProvider.GetRelativePath(series.Path, filename)));
        }
    }
}
