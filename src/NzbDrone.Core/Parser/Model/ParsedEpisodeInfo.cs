using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Model
{
    public class ParsedEpisodeInfo
    {
        public string ReleaseTitle { get; set; }
        public string SeriesTitle { get; set; }
        public SeriesTitleInfo SeriesTitleInfo { get; set; }
        public QualityModel Quality { get; set; }
        public int SeasonNumber { get; set; }
        public int[] EpisodeNumbers { get; set; }
        public int[] AbsoluteEpisodeNumbers { get; set; }
        public decimal[] SpecialAbsoluteEpisodeNumbers { get; set; }
        public string AirDate { get; set; }
        public Language Language { get; set; }
        public bool FullSeason { get; set; }
        public bool IsPartialSeason { get; set; }
        public bool IsMultiSeason { get; set; }
        public bool IsSeasonExtra { get; set; }
        public bool Special { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public int SeasonPart { get; set; }
        public string ReleaseTokens { get; set; }
        public int? DailyPart { get; set; }

        public ParsedEpisodeInfo()
        {
            EpisodeNumbers = new int[0];
            AbsoluteEpisodeNumbers = new int[0];
            SpecialAbsoluteEpisodeNumbers = new decimal[0];
        }

        public bool IsDaily
        {
            get
            {
                return !string.IsNullOrWhiteSpace(AirDate);
            }

            private set
            {
            }
        }

        public bool IsAbsoluteNumbering
        {
            get
            {
                return AbsoluteEpisodeNumbers.Any();
            }

            private set
            {
            }
        }

        public bool IsPossibleSpecialEpisode
        {
            get
            {
                return ((AirDate.IsNullOrWhiteSpace() &&
                       SeriesTitle.IsNullOrWhiteSpace() &&
                       (EpisodeNumbers.Length == 0 || SeasonNumber == 0)) || (!SeriesTitle.IsNullOrWhiteSpace() && Special)) ||
                       (EpisodeNumbers.Length == 1 && EpisodeNumbers[0] == 0);
            }

            private set
            {
            }
        }

        public bool IsPossibleSceneSeasonSpecial
        {
            get
            {
                return SeasonNumber != 0 && EpisodeNumbers.Length == 1 && EpisodeNumbers[0] == 0;
            }

            private set
            {
            }
        }

        public override string ToString()
        {
            string episodeString = "[Unknown Episode]";

            if (IsDaily && EpisodeNumbers.Empty())
            {
                episodeString = string.Format("{0}", AirDate);
            }
            else if (FullSeason)
            {
                episodeString = string.Format("Season {0:00}", SeasonNumber);
            }
            else if (EpisodeNumbers != null && EpisodeNumbers.Any())
            {
                episodeString = string.Format("S{0:00}E{1}", SeasonNumber, string.Join("-", EpisodeNumbers.Select(c => c.ToString("00"))));
            }
            else if (AbsoluteEpisodeNumbers != null && AbsoluteEpisodeNumbers.Any())
            {
                episodeString = string.Format("{0}", string.Join("-", AbsoluteEpisodeNumbers.Select(c => c.ToString("000"))));
            }
            else if (Special)
            {
                if (SeasonNumber != 0)
                {
                    episodeString = string.Format("[Unknown Season {0:00} Special]", SeasonNumber);
                }
                else
                {
                    episodeString = "[Unknown Special]";
                }
            }

            return string.Format("{0} - {1} {2}", SeriesTitle, episodeString, Quality);
        }
    }
}
