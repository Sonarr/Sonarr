using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Metadata.Files;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Metadata.Consumers.Xbmc
{
    public class XbmcMetadata : MetadataBase<XbmcMetadataSettings>
    {
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public XbmcMetadata(IMapCoversToLocal mediaCoverService,
                            IDiskProvider diskProvider,
                            Logger logger)
        {
            _mediaCoverService = mediaCoverService;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        private static readonly Regex SeriesImagesRegex = new Regex(@"^(?<type>poster|banner|fanart)\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SeasonImagesRegex = new Regex(@"^season(?<season>\d{2,}|-all|-specials)-(?<type>poster|banner|fanart)\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex EpisodeImageRegex = new Regex(@"-thumb\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override List<MetadataFile> AfterRename(Series series, List<MetadataFile> existingMetadataFiles, List<EpisodeFile> episodeFiles)
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
                        newFilename = GetEpisodeImageFilename(episodeFile.RelativePath);
                    }

                    else if (metadataFile.Type == MetadataType.EpisodeMetadata)
                    {
                        newFilename = GetEpisodeNfoFilename(episodeFile.RelativePath);
                    }

                    else
                    {
                        _logger.Debug("Unknown episode file metadata: {0}", metadataFile.RelativePath);
                        continue;
                    }

                    var existingFilename = Path.Combine(series.Path, metadataFile.RelativePath);
                    newFilename = Path.Combine(series.Path, newFilename);

                    if (!newFilename.PathEquals(existingFilename))
                    {
                        _diskProvider.MoveFile(existingFilename, newFilename);
                        metadataFile.RelativePath = series.Path.GetRelativePath(newFilename);

                        updatedMetadataFiles.Add(metadataFile);
                    }
                }
            }

            return updatedMetadataFiles;
        }

        public override MetadataFile FindMetadataFile(Series series, string path)
        {
            var filename = Path.GetFileName(path);

            if (filename == null) return null;

            var metadata = new MetadataFile
                           {
                               SeriesId = series.Id,
                               Consumer = GetType().Name,
                               RelativePath = series.Path.GetRelativePath(path)
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

                var seasonNumberMatch = seasonMatch.Groups["season"].Value;
                int seasonNumber;

                if (seasonNumberMatch.Contains("specials"))
                {
                    metadata.SeasonNumber = 0;
                }

                else if (Int32.TryParse(seasonNumberMatch, out seasonNumber))
                {
                    metadata.SeasonNumber = seasonNumber;
                }

                else
                {
                    return null;
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

        public override MetadataFileResult SeriesMetadata(Series series)
        {
            if (!Settings.SeriesMetadata)
            {
                return null;
            }

            _logger.Debug("Generating tvshow.nfo for: {0}", series.Title);
            var sb = new StringBuilder();
            var xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = false;

            var episodeGuideUrl = String.Format("http://www.thetvdb.com/api/1D62F2F90030C444/series/{0}/all/en.zip", series.TvdbId);

            using (var xw = XmlWriter.Create(sb, xws))
            {
                var tvShow = new XElement("tvshow");

                tvShow.Add(new XElement("title", series.Title));

                if (series.Ratings != null && series.Ratings.Votes > 0)
                {
                    tvShow.Add(new XElement("rating", series.Ratings.Value));
                }

                tvShow.Add(new XElement("plot", series.Overview));
                tvShow.Add(new XElement("episodeguide", new XElement("url", episodeGuideUrl)));
                tvShow.Add(new XElement("episodeguideurl", episodeGuideUrl));
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
                    var xmlActor = new XElement("actor",
                        new XElement("name", actor.Name),
                        new XElement("role", actor.Character));

                    if (actor.Images.Any())
                    {
                        xmlActor.Add(new XElement("thumb", actor.Images.First().Url));
                    }

                    tvShow.Add(xmlActor);
                }

                var doc = new XDocument(tvShow);
                doc.Save(xw);

                _logger.Debug("Saving tvshow.nfo for {0}", series.Title);

                return new MetadataFileResult(Path.Combine(series.Path, "tvshow.nfo"), doc.ToString());
            }
        }

        public override MetadataFileResult EpisodeMetadata(Series series, EpisodeFile episodeFile)
        {
            if (!Settings.EpisodeMetadata)
            {
                return null;
            }

            _logger.Debug("Generating Episode Metadata for: {0}", Path.Combine(series.Path, episodeFile.RelativePath));

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

                    if (episode.Ratings != null && episode.Ratings.Votes > 0)
                    {
                        details.Add(new XElement("rating", episode.Ratings.Value));
                    }

                    //Todo: get guest stars, writer and director
                    //details.Add(new XElement("credits", tvdbEpisode.Writer.FirstOrDefault()));
                    //details.Add(new XElement("director", tvdbEpisode.Directors.FirstOrDefault()));

                    doc.Add(details);
                    doc.Save(xw);

                    xmlResult += doc.ToString();
                    xmlResult += Environment.NewLine;
                }
            }

            return new MetadataFileResult(GetEpisodeNfoFilename(episodeFile.RelativePath), xmlResult.Trim(Environment.NewLine.ToCharArray()));
        }

        public override List<ImageFileResult> SeriesImages(Series series)
        {
            if (!Settings.SeriesImages)
            {
                return new List<ImageFileResult>();
            }

            return ProcessSeriesImages(series).ToList();
        }

        public override List<ImageFileResult> SeasonImages(Series series, Season season)
        {
            if (!Settings.SeasonImages)
            {
                return new List<ImageFileResult>();
            }

            return ProcessSeasonImages(series, season).ToList();
        }

        public override List<ImageFileResult> EpisodeImages(Series series, EpisodeFile episodeFile)
        {
            if (!Settings.EpisodeImages)
            {
                return new List<ImageFileResult>();
            }

            try
            {
                var screenshot = episodeFile.Episodes.Value.First().Images.SingleOrDefault(i => i.CoverType == MediaCoverTypes.Screenshot);

                if (screenshot == null)
                {
                    _logger.Debug("Episode screenshot not available");
                    return new List<ImageFileResult>();
                }

                return new List<ImageFileResult>
                   {
                       new ImageFileResult(GetEpisodeImageFilename(episodeFile.RelativePath), screenshot.Url)
                   };
            }
            catch (Exception ex)
            {
                _logger.Error("Unable to process episode image for file: " + Path.Combine(series.Path, episodeFile.RelativePath), ex);
                
                return new List<ImageFileResult>();
            }
        }

        private IEnumerable<ImageFileResult> ProcessSeriesImages(Series series)
        {
            foreach (var image in series.Images)
            {
                var source = _mediaCoverService.GetCoverPath(series.Id, image.CoverType);
                var destination = Path.Combine(series.Path, image.CoverType.ToString().ToLowerInvariant() + Path.GetExtension(source));

                yield return new ImageFileResult(destination, source);
            }
        }

        private IEnumerable<ImageFileResult> ProcessSeasonImages(Series series, Season season)
        {
            foreach (var image in season.Images)
            {
                var filename = String.Format("season{0:00}-{1}.jpg", season.SeasonNumber, image.CoverType.ToString().ToLower());

                if (season.SeasonNumber == 0)
                {
                    filename = String.Format("season-specials-{0}.jpg", image.CoverType.ToString().ToLower());
                }

                yield return new ImageFileResult(Path.Combine(series.Path, filename), image.Url);
            }
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
