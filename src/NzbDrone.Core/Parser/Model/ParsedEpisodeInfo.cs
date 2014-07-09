using System;
using System.Linq;
using NzbDrone.Common;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Model
{
    public class ParsedEpisodeInfo
    {
        public String SeriesTitle { get; set; }
        public SeriesTitleInfo SeriesTitleInfo { get; set; }
        public QualityModel Quality { get; set; }
        public Int32 SeasonNumber { get; set; }
        public Int32[] EpisodeNumbers { get; set; }
        public Int32[] AbsoluteEpisodeNumbers { get; set; }
        public String AirDate { get; set; }
        public Language Language { get; set; }
        public Boolean FullSeason { get; set; }
        public Boolean Special { get; set; }
        public String ReleaseGroup { get; set; }
        public String ReleaseHash { get; set; }

        public ParsedEpisodeInfo()
        {
            EpisodeNumbers = new int[0];
            AbsoluteEpisodeNumbers = new int[0];
        }

        public bool IsDaily
        {
            get
            {
                return !String.IsNullOrWhiteSpace(AirDate);
            }
        }

        public bool IsAbsoluteNumbering
        {
            get
            {
                return AbsoluteEpisodeNumbers.Any();
            }
        }

        public bool IsPossibleSpecialEpisode
        {
            get
            {
                // if we don't have eny episode numbers we are likely a special episode and need to do a search by episode title
                return (AirDate.IsNullOrWhiteSpace() &&
                       SeriesTitle.IsNullOrWhiteSpace() &&
                       (EpisodeNumbers.Length == 0 || SeasonNumber == 0) ||
                       !SeriesTitle.IsNullOrWhiteSpace() && Special);
            }
        }

        public override string ToString()
        {
            string episodeString = "[Unknown Episode]";

            if (IsDaily && EpisodeNumbers == null)
            {
                episodeString = String.Format("{0}", AirDate);
            }
            else if (FullSeason)
            {
                episodeString = String.Format("Season {0:00}", SeasonNumber);
            }
            else if (EpisodeNumbers != null && EpisodeNumbers.Any())
            {
                episodeString = String.Format("S{0:00}E{1}", SeasonNumber, String.Join("-", EpisodeNumbers.Select(c => c.ToString("00"))));
            }
            else if (AbsoluteEpisodeNumbers != null && AbsoluteEpisodeNumbers.Any())
            {
                episodeString = String.Format("{0}", String.Join("-", AbsoluteEpisodeNumbers.Select(c => c.ToString("000"))));
            }

            return String.Format("{0} - {1} {2}", SeriesTitle, episodeString, Quality);
        }
    }
}