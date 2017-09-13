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
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Plex
{
    public class PlexMetadata : MetadataBase<PlexMetadataSettings>
    {
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public PlexMetadata(IMapCoversToLocal mediaCoverService,
                            IDiskProvider diskProvider,
                            Logger logger)
        {
            _mediaCoverService = mediaCoverService;
            _diskProvider = diskProvider;
            _logger = logger;
        }


        private static readonly Regex SeriesImagesRegex = new Regex(@"^(?<type>poster|banner|fanart)\.(?:png|jpg)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SeasonImagesRegex = new Regex(@"^(season (?<season>\d+))|(?<specials>specials)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string Name => "Plex";
        
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
            return null;
        }
 
        public override MetadataFileResult EpisodeMetadata(Series series, EpisodeFile episodeFile)
        {
            return null;
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
            if (!Settings.SeriesImages)
            {
                return new List<ImageFileResult>();
            }

            var seasonFolders = GetSeasonFolders(series);

            //Work out the path to this season - if we don't have a matching path then skip this season.
            string seasonFolder;
            if (!seasonFolders.TryGetValue(season.SeasonNumber, out seasonFolder))
            {
                _logger.Trace("Failed to find season folder for series {0}, season {1}.", series.Title, season.SeasonNumber);
                return new List<ImageFileResult>();
            }

            //Plex only supports one season image, so first of all try for poster otherwise just use whatever is first in the collection
            var image = season.Images.SingleOrDefault(c => c.CoverType == MediaCoverTypes.Poster) ?? season.Images.FirstOrDefault();
            if (image == null)
            {
                _logger.Trace("Failed to find suitable season image for series {0}, season {1}.", series.Title, season.SeasonNumber);
                return new List<ImageFileResult>();
            }

            var filename = string.Format("Season{0:00}.jpg", season.SeasonNumber, image.CoverType.ToString().ToLower());

            var path = Path.Combine(seasonFolder, filename );

            return new List<ImageFileResult> { new ImageFileResult(path, image.Url) };
                        
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
                _logger.Error(ex, "Unable to process episode image for file: {0}", Path.Combine(series.Path, episodeFile.RelativePath));

                return new List<ImageFileResult>();
            }        
        }

        private IEnumerable<ImageFileResult> ProcessSeriesImages(Series series)
        {
            foreach (var image in series.Images)
            {
                var source = _mediaCoverService.GetCoverPath(series.Id, image.CoverType);
                var destination = image.CoverType.ToString().ToLowerInvariant() + Path.GetExtension(source);

                yield return new ImageFileResult(destination, source);
            }
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
            return Path.ChangeExtension(episodeFilePath, "").Trim('.') + ".jpg";
        }

        private Dictionary<int, string> GetSeasonFolders(Series series)
        {
            var seasonFolderMap = new Dictionary<int, string>();

            foreach (var folder in _diskProvider.GetDirectories(series.Path))
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
                        if (int.TryParse(seasonNumber, out matchedSeason))
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

            return seasonFolderMap;
        }

    }
 }
