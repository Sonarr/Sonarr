using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Model
{
    public class EpisodeParseResult
    {
        public string SeriesTitle { get; set; }

        public string CleanTitle
        {
            get
            {
                return Parser.NormalizeTitle(SeriesTitle);
            }
        }

        public string EpisodeTitle { get; set; }

        public int SeasonNumber { get; set; }

        public List<int> EpisodeNumbers { get; set; }

        public DateTime? AirDate { get; set; }

        public QualityModel Quality { get; set; }

        public LanguageType Language { get; set; }

        public string NzbUrl { get; set; }

        public string NzbInfoUrl { get; set; }

        public string OriginalString { get; set; }

        public Series Series { get; set; }

        public String Indexer { get; set; }

        public bool FullSeason { get; set; }

        public long Size { get; set; }

        public int Age { get; set; }

        public string ReleaseGroup { get; set; }

        public bool SceneSource { get; set; }

        public IList<Episode> Episodes { get; set; }

        public override string ToString()
        {
            string episodeString = "[Unknown Episode]";

            if (AirDate != null && EpisodeNumbers == null)
            {
                episodeString = string.Format("{0}", AirDate.Value.ToString("yyyy-MM-dd"));
            }
            else if (FullSeason)
            {
                episodeString = string.Format("Season {0:00}", SeasonNumber);
            }
            else if (EpisodeNumbers != null && EpisodeNumbers.Any())
            {
                episodeString = string.Format("S{0:00}E{1}", SeasonNumber, String.Join("-", EpisodeNumbers.Select(c => c.ToString("00"))));
            }

            return string.Format("{0} - {1} {2}", SeriesTitle, episodeString, Quality);
        }


        public string GetDownloadTitle()
        {
            var seriesTitle = MediaFileService.CleanFilename(Series.Title);

            //Handle Full Naming
            if (FullSeason)
            {
                var seasonResult = String.Format("{0} - Season {1} [{2}]", seriesTitle,
                                     SeasonNumber, Quality.Quality);

                if (Quality.Proper)
                    seasonResult += " [Proper]";

                return seasonResult;
            }

            if (Series.SeriesTypes == SeriesTypes.Daily)
            {
                var dailyResult = String.Format("{0} - {1:yyyy-MM-dd} - {2} [{3}]", seriesTitle,
                                     AirDate, Episodes.First().Title, Quality.Quality);

                if (Quality.Proper)
                    dailyResult += " [Proper]";

                return dailyResult;
            }

            //Show Name - 1x01-1x02 - Episode Name
            //Show Name - 1x01 - Episode Name
            var episodeString = new List<String>();
            var episodeNames = new List<String>();

            foreach (var episode in Episodes)
            {
                episodeString.Add(String.Format("{0}x{1:00}", episode.SeasonNumber, episode.EpisodeNumber));
                episodeNames.Add(Parser.CleanupEpisodeTitle(episode.Title));
            }

            var epNumberString = String.Join("-", episodeString);
            string episodeName;


            if (episodeNames.Distinct().Count() == 1)
                episodeName = episodeNames.First();

            else
                episodeName = String.Join(" + ", episodeNames.Distinct());

            var result = String.Format("{0} - {1} - {2} [{3}]", seriesTitle, epNumberString, episodeName, Quality.Quality);

            if (Quality.Proper)
            {
                result += " [Proper]";
            }

            return result;
        }
    }
}