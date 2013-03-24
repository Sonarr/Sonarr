using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Organizer
{
    public interface IBuildFileNames
    {
        string BuildFilename(IList<Episode> episodes, Series series, EpisodeFile episodeFile);
        string BuildFilePath(Series series, int seasonNumber, string fileName, string extension);
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
            return _nameSpecificationRepository.SingleOrDefault() ?? NameSpecification.Default;
        }


        public string BuildFilename(IList<Episode> episodes, Series series, EpisodeFile episodeFile)
        {
            var nameSpec = GetSpecification();

            if (nameSpec.UseSceneName)
            {
                if (String.IsNullOrWhiteSpace(episodeFile.SceneName))
                {
                    return Path.GetFileNameWithoutExtension(episodeFile.Path);
                }

                return episodeFile.SceneName;
            }

            var sortedEpisodes = episodes.OrderBy(e => e.EpisodeNumber);

            var numberStyle = GetNumberStyle(nameSpec.NumberStyle);

            var episodeNames = new List<string>
                {
                    Parser.CleanupEpisodeTitle(sortedEpisodes.First().Title)
                };

            var result = String.Empty;

            if (nameSpec.IncludeSeriesName)
            {
                result += series.Title + nameSpec.Separator;
            }

            if (series.SeriesType == SeriesTypes.Standard)
            {
                result += numberStyle.Pattern.Replace("%0e",
                                                      String.Format("{0:00}", sortedEpisodes.First().EpisodeNumber));

                if (episodes.Count > 1)
                {
                    var multiEpisodeStyle =
                        GetMultiEpisodeStyle(nameSpec.MultiEpisodeStyle);

                    foreach (var episode in sortedEpisodes.Skip(1))
                    {
                        if (multiEpisodeStyle.Name == "Duplicate")
                        {
                            result += nameSpec.Separator + numberStyle.Pattern;
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
                    .Replace("%p", nameSpec.Separator);
            }

            else
            {
                if (episodes.First().AirDate.HasValue)
                    result += episodes.First().AirDate.Value.ToString("yyyy-MM-dd");

                else
                    result += "Unknown";
            }

            if (nameSpec.IncludeEpisodeTitle)
            {
                if (episodeNames.Distinct().Count() == 1)
                    result += nameSpec.Separator + episodeNames.First();

                else
                    result += nameSpec.Separator + String.Join(" + ", episodeNames.Distinct());
            }

            if (nameSpec.AppendQuality)
            {
                result += String.Format(" [{0}]", episodeFile.Quality);

                if (episodeFile.Proper)
                    result += " [Proper]";
            }

            if (nameSpec.ReplaceSpaces)
                result = result.Replace(' ', '.');

            _logger.Trace("New File Name is: [{0}]", result.Trim());
            return CleanFilename(result.Trim());
        }

        public string BuildFilePath(Series series, int seasonNumber, string fileName, string extension)
        {

            var nameSpec = GetSpecification();

            string path = series.Path;
            if (series.SeasonFolder)
            {
                var seasonFolder = nameSpec.SeasonFolderFormat
                                                 .Replace("%0s", seasonNumber.ToString("00"))
                                                 .Replace("%s", seasonNumber.ToString());

                path = Path.Combine(path, seasonFolder);
            }

            return Path.Combine(path, fileName + extension);
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


        private static readonly List<EpisodeSortingType> NumberStyles = new List<EpisodeSortingType>
                                                                            {
                                                                                new EpisodeSortingType
                                                                                    {
                                                                                        Id = 0,
                                                                                        Name = "1x05",
                                                                                        Pattern = "%sx%0e",
                                                                                        EpisodeSeparator = "x"

                                                                                    },
                                                                                new EpisodeSortingType
                                                                                    {
                                                                                        Id = 1,
                                                                                        Name = "01x05",
                                                                                        Pattern = "%0sx%0e",
                                                                                        EpisodeSeparator = "x"
                                                                                    },
                                                                                new EpisodeSortingType
                                                                                    {
                                                                                        Id = 2,
                                                                                        Name = "S01E05",
                                                                                        Pattern = "S%0sE%0e",
                                                                                        EpisodeSeparator = "E"
                                                                                    },
                                                                                new EpisodeSortingType
                                                                                    {
                                                                                        Id = 3,
                                                                                        Name = "s01e05",
                                                                                        Pattern = "s%0se%0e",
                                                                                        EpisodeSeparator = "e"
                                                                                    }
                                                                            };

        private static readonly List<EpisodeSortingType> MultiEpisodeStyles = new List<EpisodeSortingType>
                                                                                  {
                                                                                      new EpisodeSortingType
                                                                                          {
                                                                                              Id = 0,
                                                                                              Name = "Extend",
                                                                                              Pattern = "-%0e"
                                                                                          },
                                                                                      new EpisodeSortingType
                                                                                          {
                                                                                              Id = 1,
                                                                                              Name = "Duplicate",
                                                                                              Pattern = "%p%0s%x%0e"
                                                                                          },
                                                                                      new EpisodeSortingType
                                                                                          {
                                                                                              Id = 2,
                                                                                              Name = "Repeat",
                                                                                              Pattern = "%x%0e"
                                                                                          },
                                                                                        new EpisodeSortingType
                                                                                          {
                                                                                              Id = 3,
                                                                                              Name = "Scene",
                                                                                              Pattern = "-%x%0e"
                                                                                          }
                                                                                  };


        private static EpisodeSortingType GetNumberStyle(int id)
        {
            return NumberStyles.Single(s => s.Id == id);
        }

        private static EpisodeSortingType GetMultiEpisodeStyle(int id)
        {
            return MultiEpisodeStyles.Single(s => s.Id == id);
        }
    }
}