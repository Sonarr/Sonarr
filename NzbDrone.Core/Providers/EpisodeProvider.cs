using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NzbDrone.Core.Repository;
using SubSonic.Repository;

namespace NzbDrone.Core.Providers
{
    public class EpisodeProvider : IEpisodeProvider
    {
        //TODO: Remove parsing of the series name, it should be done in series provider
        private static readonly Regex ParseRegex = new Regex(@"(?<showName>.*)
(?:
  s(?<seasonNumber>\d+)e(?<episodeNumber>\d+)-?e(?<episodeNumber2>\d+)
| s(?<seasonNumber>\d+)e(?<episodeNumber>\d+)
|  (?<seasonNumber>\d+)x(?<episodeNumber>\d+)
|  (?<airDate>\d{4}.\d{2}.\d{2})
)
(?:
  (?<episodeName>.*?)
  (?<release>
     (?:hdtv|pdtv|xvid|ws|720p|x264|bdrip|dvdrip|dsr|proper)
     .*)
| (?<episodeName>.*)
)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);


        private readonly IRepository _sonicRepo;
        private readonly ISeriesProvider _seriesProvider;

        public EpisodeProvider(IRepository sonicRepo, ISeriesProvider seriesProvider)
        {
            _sonicRepo = sonicRepo;
            _seriesProvider = seriesProvider;
        }

        public Episode GetEpisode(long id)
        {
            throw new NotImplementedException();
        }

        public Episode SaveEpisode(Episode episode)
        {
            throw new NotImplementedException();
        }

        public IList<Episode> GetEpisodesBySeason(long seasonId)
        {
            throw new NotImplementedException();
        }

        public IList<Episode> GetEpisodeBySeries(long seriesId)
        {
            throw new NotImplementedException();
        }

        public String GetSabTitle(Episode episode)
        {
            var series = _seriesProvider.GetSeries(episode.SeriesId);
            if (series == null) throw new ArgumentException("Unknown series. ID: " + episode.SeriesId);

            //TODO: This method should return a standard title for the sab episode.
            throw new NotImplementedException();

        }

        /// <summary>
        /// Comprehensive check on whether or not this episode is needed.
        /// </summary>
        /// <param name="episode">Episode that needs to be checked</param>
        /// <returns></returns>
        public bool IsNeeded(Episode episode)
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Parses a post title into list of episode objects
        /// </summary>
        /// <param name="title">Title of the report</param>
        /// <returns>List of episodes relating to the post</returns>
        public static List<Episode> Parse(string title)
        {
            var match = ParseRegex.Match(title);

            if (!match.Success)
                throw new ArgumentException(String.Format("Title doesn't match any know patterns. [{0}]", title));

            var result = new List<Episode>();

            result.Add(new Episode() { EpisodeNumber = Convert.ToInt32(match.Groups["episodeNumber"].Value) });

            if (match.Groups["episodeNumber2"].Success)
            {
                result.Add(new Episode() { EpisodeNumber = Convert.ToInt32(match.Groups["episodeNumber2"].Value) });
            }

            foreach (var ep in result)
            {
                //TODO: Get TVDB episode Title, Series name and the rest of the details
                ep.Season = Convert.ToInt32(match.Groups["seasonNumber"].Value);
                ep.Title = ReplaceSeparatorChars(match.Groups["episodeName"].Value);
                ep.Proper = title.Contains("PROPER");
                ep.Quality = Quality.Unknown;
            }

            return result;
        }

        private static string ReplaceSeparatorChars(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            return text.Replace('.', ' ').Replace('-', ' ').Replace('_', ' ').Trim();
        }

    }
}