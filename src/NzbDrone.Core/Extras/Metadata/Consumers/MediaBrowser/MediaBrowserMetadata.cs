using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras.Metadata.Consumers.MediaBrowser
{
    public class MediaBrowserMetadata : MetadataBase<MediaBrowserMetadataSettings>
    {
        private readonly Logger _logger;

        public MediaBrowserMetadata(
                            Logger logger)
        {
            _logger = logger;
        }

        public override string Name => "Emby (Legacy)";

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

            if (filename.Equals("series.xml", StringComparison.InvariantCultureIgnoreCase))
            {
                metadata.Type = MetadataType.SeriesMetadata;
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

            _logger.Debug("Generating series.xml for: {0}", series.Title);
            var sb = new StringBuilder();
            var xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = false;

            using (var xw = XmlWriter.Create(sb, xws))
            {
                var tvShow = new XElement("Series");

                tvShow.Add(new XElement("id", series.TvdbId));
                tvShow.Add(new XElement("Status", series.Status));
                tvShow.Add(new XElement("Network", series.Network));
                tvShow.Add(new XElement("Airs_Time", series.AirTime));

                if (series.FirstAired.HasValue)
                {
                    tvShow.Add(new XElement("FirstAired", series.FirstAired.Value.ToString("yyyy-MM-dd")));
                }

                tvShow.Add(new XElement("ContentRating", series.Certification));
                tvShow.Add(new XElement("Added", series.Added.ToString("MM/dd/yyyy HH:mm:ss tt"))); 
                tvShow.Add(new XElement("LockData", "false"));
                tvShow.Add(new XElement("Overview", series.Overview));
                tvShow.Add(new XElement("LocalTitle", series.Title));

                if (series.FirstAired.HasValue)
                {
                    tvShow.Add(new XElement("PremiereDate", series.FirstAired.Value.ToString("yyyy-MM-dd")));
                }

                tvShow.Add(new XElement("Rating", series.Ratings.Value));
                tvShow.Add(new XElement("ProductionYear", series.Year));
                tvShow.Add(new XElement("RunningTime", series.Runtime));
                tvShow.Add(new XElement("IMDB", series.ImdbId));
                tvShow.Add(new XElement("TVRageId", series.TvRageId));
                tvShow.Add(new XElement("Genres", series.Genres.Select(genre => new XElement("Genre", genre))));

                var persons   = new XElement("Persons");

                foreach (var person in series.Actors)
                {
                    persons.Add(new XElement("Person",
                        new XElement("Name", person.Name),
                        new XElement("Type", "Actor"),
                        new XElement("Role", person.Character)
                        ));
                }

                tvShow.Add(persons);


                var doc = new XDocument(tvShow);
                doc.Save(xw);

                _logger.Debug("Saving series.xml for {0}", series.Title);

                return new MetadataFileResult("series.xml", doc.ToString());
            }
        }
 
        public override MetadataFileResult EpisodeMetadata(Series series, EpisodeFile episodeFile)
        {
            return null;
        }
            
        public override List<ImageFileResult> SeriesImages(Series series)
        {
            return new List<ImageFileResult>();
        }

        public override List<ImageFileResult> SeasonImages(Series series, Season season)
        {
            return new List<ImageFileResult>();
        }

        public override List<ImageFileResult> EpisodeImages(Series series, EpisodeFile episodeFile)
        {
            return new List<ImageFileResult>();
        }

        private IEnumerable<ImageFileResult> ProcessSeriesImages(Series series)
        {
            return new List<ImageFileResult>();
        }

        private IEnumerable<ImageFileResult> ProcessSeasonImages(Series series, Season season)
        {
            return new List<ImageFileResult>();
        }

        private string GetEpisodeNfoFilename(string episodeFilePath)
        {
            return null;
        }

        private string GetEpisodeImageFilename(string episodeFilePath)
        {
            return null;
        }
    }
}