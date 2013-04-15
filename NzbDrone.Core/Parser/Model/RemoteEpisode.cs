using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public class RemoteEpisode
    {
        public ReportInfo Report { get; set; }

        public bool FullSeason { get; set; }

        public Series Series { get; set; }

        public List<Episode> Episodes { get; set; }

        public QualityModel Quality { get; set; }

        public Language Language { get; set; }

        public int SeasonNumber
        {
            get { return Episodes.Select(e => e.SeasonNumber).Distinct().SingleOrDefault(); }
        }


        public DateTime? AirDate
        {
            get
            {
                return Episodes.Single().AirDate;
            }
        }

        public IEnumerable<int> EpisodeNumbers
        {
            get
            {
                return Episodes.Select(c => c.EpisodeNumber).Distinct();
            }
        }

        public string GetDownloadTitle()
        {
            var seriesTitle = FileNameBuilder.CleanFilename(Series.Title);

            //Handle Full Naming
            if (FullSeason)
            {
                var seasonResult = String.Format("{0} - Season {1} [{2}]", seriesTitle, SeasonNumber, Quality);

                if (Quality.Proper)
                    seasonResult += " [Proper]";

                return seasonResult;
            }

            if (Series.SeriesType == SeriesTypes.Daily)
            {
                var dailyResult = String.Format("{0} - {1:yyyy-MM-dd} - {2} [{3}]", seriesTitle,
                                     AirDate, Episodes.First().Title, Quality);

                if (Quality.Proper)
                    dailyResult += " [Proper]";

                return dailyResult;
            }

            //Show Name - 1x01-1x02 - Episode Name
            //Show Name - 1x01 - Episode Name
            var episodeString = new List<string>();
            var episodeNames = new List<string>();

            foreach (var episode in Episodes)
            {
                episodeString.Add(String.Format("{0}x{1:00}", episode.SeasonNumber, episode.EpisodeNumber));
                episodeNames.Add(Core.Parser.Parser.CleanupEpisodeTitle(episode.Title));
            }

            var epNumberString = String.Join("-", episodeString);
            string episodeName;


            if (episodeNames.Distinct().Count() == 1)
                episodeName = episodeNames.First();

            else
                episodeName = String.Join(" + ", episodeNames.Distinct());

            var result = String.Format("{0} - {1} - {2} [{3}]", seriesTitle, epNumberString, episodeName, Quality);

            if (Quality.Proper)
            {
                result += " [Proper]";
            }

            return result;
        }


        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}