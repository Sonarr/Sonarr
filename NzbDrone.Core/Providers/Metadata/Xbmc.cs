using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using XemLib.Data;
using XemLib.Data.Banner;

namespace NzbDrone.Core.Providers.Metadata
{
    public class Xbmc : MetadataBase
    {
        public Xbmc(ConfigProvider configProvider, DiskProvider diskProvider, 
                    BannerProvider bannerProvider, EpisodeProvider episodeProvider)
            : base(configProvider, diskProvider, bannerProvider, episodeProvider)
        {
        }

        public override string Name
        {
            get { return "XBMC"; }
        }

        public override void CreateForSeries(Series series, TvdbSeries tvDbSeries)
        {
            //Create tvshow.nfo, fanart.jpg, folder.jpg and season##.tbn
            var episodeGuideUrl = GetEpisodeGuideUrl(series.SeriesId);

            _logger.Debug("Generating tvshow.nfo for: {0}", series.Title);
            var sb = new StringBuilder();
            var xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = false;

            using (var xw = XmlWriter.Create(sb, xws))
            {
                var tvShow = new XElement("tvshow");
             
                tvShow.Add(new XElement("title", tvDbSeries.SeriesName));
                tvShow.Add(new XElement("rating", tvDbSeries.Rating));
                tvShow.Add(new XElement("plot", tvDbSeries.Overview));
                tvShow.Add(new XElement("episodeguide", new XElement("url", episodeGuideUrl)));
                tvShow.Add(new XElement("episodeguideurl", episodeGuideUrl));
                tvShow.Add(new XElement("mpaa", tvDbSeries.ContentRating));
                tvShow.Add(new XElement("id", tvDbSeries.Id));
                tvShow.Add(new XElement("genre", tvDbSeries.Genres.FirstOrDefault()));
                tvShow.Add(new XElement("premiered", tvDbSeries.FirstAired.ToString("yyyy-MM-dd")));
                tvShow.Add(new XElement("studio", tvDbSeries.Network));     

                foreach(var actor in tvDbSeries.TvdbActors)
                {
                    tvShow.Add(new XElement("actor",
                                    new XElement("name", actor.Name),
                                    new XElement("role", actor.Role),
                                    new XElement("thumb", "http://www.thetvdb.com/banners/" + actor.Image)
                            ));
                }

                var doc = new XDocument(tvShow);
                doc.Save(xw);

                _logger.Debug("Saving tvshow.nfo for {0}", series.Title);
                _diskProvider.WriteAllText(Path.Combine(series.Path, "tvshow.nfo"), doc.ToString());
            }

            if (!_diskProvider.FileExists(Path.Combine(series.Path, "fanart.jpg")))
            {
                _logger.Debug("Downloading fanart for: {0}", series.Title);
                _bannerProvider.Download(tvDbSeries.Fanart, Path.Combine(series.Path, "fanart.jpg"));
            }

            if (!_diskProvider.FileExists(Path.Combine(series.Path, "folder.jpg")))
            {
                if(_configProvider.MetadataUseBanners)
                {
                    _logger.Debug("Downloading series banner for: {0}", series.Title);
                    _bannerProvider.Download(tvDbSeries.Banner, Path.Combine(series.Path, "folder.jpg"));

                    _logger.Debug("Downloading Season banners for {0}", series.Title);
                    DownloadSeasonThumbnails(series, tvDbSeries, TvdbSeasonBanner.Type.Banner);
                }

                else
                {
                    _logger.Debug("Downloading series thumbnail for: {0}", series.Title);
                    _bannerProvider.Download(tvDbSeries.Poster, Path.Combine(series.Path, "folder.jpg"));

                    _logger.Debug("Downloading Season posters for {0}", series.Title);
                    DownloadSeasonThumbnails(series, tvDbSeries, TvdbSeasonBanner.Type.Poster);
                }
            }
        }

        public override void CreateForEpisodeFile(EpisodeFile episodeFile, TvdbSeries tvDbSeries)
        {
            //Create filename.tbn and filename.nfo
            var episodes = _episodeProvider.GetEpisodesByFileId(episodeFile.EpisodeFileId);

            if (!episodes.Any())
            {
                _logger.Debug("No episodes where found for this episode file: {0}", episodeFile.EpisodeFileId);
                return;
            }

            var episodeFileThumbnail = tvDbSeries.Episodes.FirstOrDefault(
                                                       e =>
                                                       e.SeasonNumber == episodeFile.SeasonNumber &&
                                                       e.EpisodeNumber == episodes.First().EpisodeNumber);

            if (episodeFileThumbnail == null || String.IsNullOrWhiteSpace(episodeFileThumbnail.Banner))
            {
                _logger.Debug("No thumbnail is available for this episode");
                return;
            }

            if (!_diskProvider.FileExists(episodeFile.Path.Replace(Path.GetExtension(episodeFile.Path), ".tbn")))
            {
                _logger.Debug("Downloading episode thumbnail for: {0}", episodeFile.EpisodeFileId);
                _bannerProvider.Download(episodeFileThumbnail.Banner,
                                         episodeFile.Path.Replace(Path.GetExtension(episodeFile.Path), ".tbn"));
            }

            _logger.Debug("Generating filename.nfo for: {0}", episodeFile.EpisodeFileId);

            var xmlResult = String.Empty;
            foreach (var episode in episodes)
            {
                var sb = new StringBuilder();
                var xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;
                xws.Indent = false;

                using (var xw = XmlWriter.Create(sb, xws))
                {
                    var doc = new XDocument();
                    var tvdbEpisode = tvDbSeries.Episodes.FirstOrDefault(
                                                                e =>
                                                                e.Id == episode.TvDbEpisodeId);

                    if (tvdbEpisode == null)
                    {
                        _logger.Debug("Looking up by TvDbEpisodeId failed, trying to match via season/episode number combination");
                        tvdbEpisode = tvDbSeries.Episodes.FirstOrDefault(
                                                                            e =>
                                                                            e.SeasonNumber == episode.SeasonNumber &&
                                                                            e.EpisodeNumber == episode.EpisodeNumber);
                    }

                    if (tvdbEpisode == null)
                    {
                        _logger.Debug("Unable to find episode from TvDb - skipping");
                        return;
                    }

                    var details = new XElement("episodedetails");
                    details.Add(new XElement("title", tvdbEpisode.EpisodeName));
                    details.Add(new XElement("season", tvdbEpisode.SeasonNumber));
                    details.Add(new XElement("episode", tvdbEpisode.EpisodeNumber));
                    details.Add(new XElement("aired", tvdbEpisode.FirstAired.ToString("yyyy-MM-dd")));
                    details.Add(new XElement("plot", tvdbEpisode.Overview));
                    details.Add(new XElement("displayseason"));
                    details.Add(new XElement("displayepisode"));
                    details.Add(new XElement("thumb", "http://www.thetvdb.com/banners/" + tvdbEpisode.Banner));
                    details.Add(new XElement("watched", "false"));
                    details.Add(new XElement("credits", tvdbEpisode.Writers.FirstOrDefault()));
                    details.Add(new XElement("director", tvdbEpisode.Directors.FirstOrDefault()));
                    details.Add(new XElement("rating", tvdbEpisode.Rating));

                    foreach(var actor in tvdbEpisode.GuestStars)
                    {
                        if (!String.IsNullOrWhiteSpace(actor))
                            continue;

                        details.Add(new XElement("actor",
                                                new XElement("name", actor)
                                            ));
                    }

                    foreach(var actor in tvDbSeries.TvdbActors)
                    {
                        details.Add(new XElement("actor",
                                                new XElement("name", actor.Name),
                                                new XElement("role", actor.Role),
                                                new XElement("thumb", "http://www.thetvdb.com/banners/" + actor.Image)
                                            ));
                    }

                    doc.Add(details);
                    doc.Save(xw);

                    xmlResult += doc.ToString();
                    xmlResult += Environment.NewLine;
                }       
            }
            var filename = episodeFile.Path.Replace(Path.GetExtension(episodeFile.Path), ".nfo");
            _logger.Debug("Saving episodedetails to: {0}", filename);
            _diskProvider.WriteAllText(filename, xmlResult.Trim(EnvironmentProvider.NewLineChars));
        }

        public override void RemoveForSeries(Series series)
        {
            //Remove tvshow.nfo, fanart.jpg, folder.jpg and season##.tbn
            _logger.Debug("Deleting series metadata for: ", series.Title);

            _diskProvider.DeleteFile(Path.Combine(series.Path, "tvshow.nfo"));
            _diskProvider.DeleteFile(Path.Combine(series.Path, "fanart.jpg"));
            _diskProvider.DeleteFile(Path.Combine(series.Path, "fanart.jpg"));
            
            foreach (var file in _diskProvider.GetFiles(series.Path, SearchOption.TopDirectoryOnly))
            {
                if (Path.GetExtension(file) != ".tbn")
                    continue;

                if (!Path.GetFileName(file).StartsWith("season"))
                    continue;

                _logger.Debug("Deleting season thumbnail: {0}", file);
                _diskProvider.DeleteFile(file);
            }
        }

        public override void RemoveForEpisodeFile(EpisodeFile episodeFile)
        {
            //Remove filename.tbn and filename.nfo
            _logger.Debug("Deleting episode metadata for: {0}", episodeFile);

            _diskProvider.DeleteFile(episodeFile.Path.Replace(Path.GetExtension(episodeFile.Path), ".nfo"));
            _diskProvider.DeleteFile(episodeFile.Path.Replace(Path.GetExtension(episodeFile.Path), ".tbn"));
        }

        private void DownloadSeasonThumbnails(Series series, TvdbSeries tvDbSeries, TvdbSeasonBanner.Type bannerType)
        {
            var seasons = tvDbSeries.Banners.SeasonBanners.Where(s => s.BannerType == bannerType).Select(s => s.SeasonNumber);

            foreach (var season in seasons)
            {
                var banner = tvDbSeries.Banners.SeasonBanners.FirstOrDefault(b => b.BannerType == bannerType && b.SeasonNumber == season);
                _logger.Debug("Downloading banner for Season: {0} Series: {1}", season, series.Title);

                if (season == 0)
                {
                    if (!_diskProvider.FileExists(Path.Combine(series.Path, "season-specials.tbn")))
                    {
                        _bannerProvider.Download(banner.BannerPath,
                                                 Path.Combine(series.Path, "season-specials.tbn"));
                    }
                }

                else
                {
                    if (!_diskProvider.FileExists(Path.Combine(series.Path, String.Format("season{0:00}.tbn", season))))
                    {
                        _bannerProvider.Download(banner.BannerPath,
                                                 Path.Combine(series.Path, String.Format("season{0:00}.tbn", season)));
                    }
                }
            }
        }
    }
}
