using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Organizer
{

    public class NameSpecification : ModelBase
    {
        public static NameSpecification Default
        {
            get { return new NameSpecification(); }
        }

        public bool SortingUseSceneName { get; set; }

        public int SortingSeparatorStyle { get; set; }

        public int SortingNumberStyle { get; set; }

        public bool SortingIncludeSeriesName { get; set; }

        public int SortingMultiEpisodeStyle { get; set; }

        public bool SortingIncludeEpisodeTitle { get; set; }

        public bool SortingAppendQuality { get; set; }

        public bool SortingReplaceSpaces { get; set; }

        public string SortingSeasonFolderFormat { get; set; }
    }


    public interface IBuildFileNames
    {
        string GetNewFilename(IList<Episode> episodes, Series series, Quality quality, bool proper, EpisodeFile episodeFile);
        FileInfo CalculateFilePath(Series series, int seasonNumber, string fileName, string extension);
    }

    public class FileNameBuilder : IBuildFileNames
    {
        private readonly IBasicRepository<NameSpecification> _nameSpecificationRepository;
        private readonly Logger _logger;

        public FileNameBuilder(IBasicRepository<NameSpecification> nameSpecificationRepository, Logger logger)
        {
            _nameSpecificationRepository = nameSpecificationRepository;
            _logger = logger;
        }


        public NameSpecification GetSpecification()
        {
            var spec = _nameSpecificationRepository.SingleOrDefault();

            if (spec == null)
            {
                spec = NameSpecification.Default;
            }

            return spec;

        }

        public string GetNewFilename(IList<Episode> episodes, Series series, Quality quality, bool proper, EpisodeFile episodeFile)
        {
            var nameSpec = GetSpecification();


            if (nameSpec.SortingUseSceneName)
            {
                _logger.Trace("Attempting to use scene name");
                if (String.IsNullOrWhiteSpace(episodeFile.SceneName))
                {
                    var name = Path.GetFileNameWithoutExtension(episodeFile.Path);
                    _logger.Trace("Unable to use scene name, because it is null, sticking with current name: {0}", name);

                    return name;
                }

                return episodeFile.SceneName;
            }

            var sortedEpisodes = episodes.OrderBy(e => e.EpisodeNumber);

            var separatorStyle = EpisodeSortingHelper.GetSeparatorStyle(nameSpec.SortingSeparatorStyle);
            var numberStyle = EpisodeSortingHelper.GetNumberStyle(nameSpec.SortingNumberStyle);

            var episodeNames = new List<string>();

            episodeNames.Add(Parser.CleanupEpisodeTitle(sortedEpisodes.First().Title));

            string result = String.Empty;

            if (nameSpec.SortingIncludeSeriesName)
            {
                result += series.Title + separatorStyle.Pattern;
            }

            if (series.SeriesTypes == SeriesTypes.Standard)
            {
                result += numberStyle.Pattern.Replace("%0e",
                                                      String.Format("{0:00}", sortedEpisodes.First().EpisodeNumber));

                if (episodes.Count > 1)
                {
                    var multiEpisodeStyle =
                        EpisodeSortingHelper.GetMultiEpisodeStyle(nameSpec.SortingMultiEpisodeStyle);

                    foreach (var episode in sortedEpisodes.Skip(1))
                    {
                        if (multiEpisodeStyle.Name == "Duplicate")
                        {
                            result += separatorStyle.Pattern + numberStyle.Pattern;
                        }
                        else
                        {
                            result += multiEpisodeStyle.Pattern;
                        }

                        result = result.Replace("%0e", String.Format("{0:00}", episode.EpisodeNumber));
                        episodeNames.Add(Parser.CleanupEpisodeTitle(episode.Title));
                    }
                }

                result = result
                    .Replace("%s", String.Format("{0}", episodes.First().SeasonNumber))
                    .Replace("%0s", String.Format("{0:00}", episodes.First().SeasonNumber))
                    .Replace("%x", numberStyle.EpisodeSeparator)
                    .Replace("%p", separatorStyle.Pattern);
            }

            else
            {
                if (episodes.First().AirDate.HasValue)
                    result += episodes.First().AirDate.Value.ToString("yyyy-MM-dd");

                else
                    result += "Unknown";
            }

            if (nameSpec.SortingIncludeEpisodeTitle)
            {
                if (episodeNames.Distinct().Count() == 1)
                    result += separatorStyle.Pattern + episodeNames.First();

                else
                    result += separatorStyle.Pattern + String.Join(" + ", episodeNames.Distinct());
            }

            if (nameSpec.SortingAppendQuality)
            {
                result += String.Format(" [{0}]", quality);

                if (proper)
                    result += " [Proper]";
            }

            if (nameSpec.SortingReplaceSpaces)
                result = result.Replace(' ', '.');

            _logger.Trace("New File Name is: [{0}]", result.Trim());
            return CleanFilename(result.Trim());
        }

        public FileInfo CalculateFilePath(Series series, int seasonNumber, string fileName, string extension)
        {

            var nameSpec = GetSpecification();

            string path = series.Path;
            if (series.SeasonFolder)
            {
                var seasonFolder = nameSpec.SortingSeasonFolderFormat
                                                 .Replace("%0s", seasonNumber.ToString("00"))
                                                 .Replace("%s", seasonNumber.ToString());

                path = Path.Combine(path, seasonFolder);
            }

            path = Path.Combine(path, fileName + extension);

            return new FileInfo(path);
        }


        public static string CleanFilename(string name)
        {
            string result = name;
            string[] badCharacters = { "\\", "/", "<", ">", "?", "*", ":", "|", "\"" };
            string[] goodCharacters = { "+", "+", "{", "}", "!", "@", "-", "#", "`" };

            for (int i = 0; i < badCharacters.Length; i++)
                result = result.Replace(badCharacters[i], goodCharacters[i]);

            return result.Trim();
        }
    }
}