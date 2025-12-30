using System.Collections.Generic;
using System.Threading.Tasks;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.LlmMatching
{
    /// <summary>
    /// Service interface for LLM-based series matching when traditional parsing fails.
    /// Acts as a fallback mechanism before requiring manual user intervention.
    /// </summary>
    public interface ILlmSeriesMatchingService
    {
        /// <summary>
        /// Checks if the LLM service is properly configured and available.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Attempts to match a parsed release title to a series using LLM intelligence.
        /// </summary>
        /// <param name="parsedEpisodeInfo">The parsed episode information from the release title.</param>
        /// <param name="availableSeries">List of series available in the user's library.</param>
        /// <returns>The matching result containing the series and confidence score, or null if no match found.</returns>
        Task<LlmMatchResult> TryMatchSeriesAsync(ParsedEpisodeInfo parsedEpisodeInfo, IEnumerable<Series> availableSeries);

        /// <summary>
        /// Attempts to match a raw release title to a series using LLM intelligence.
        /// </summary>
        /// <param name="releaseTitle">The raw release title string.</param>
        /// <param name="availableSeries">List of series available in the user's library.</param>
        /// <returns>The matching result containing the series and confidence score, or null if no match found.</returns>
        Task<LlmMatchResult> TryMatchSeriesAsync(string releaseTitle, IEnumerable<Series> availableSeries);
    }

    /// <summary>
    /// Result of an LLM-based series matching attempt.
    /// </summary>
    public class LlmMatchResult
    {
        /// <summary>
        /// The matched series, or null if no confident match was found.
        /// </summary>
        public Series Series { get; set; }

        /// <summary>
        /// Confidence score from 0.0 to 1.0 indicating how confident the LLM is in the match.
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// The reasoning provided by the LLM for the match decision.
        /// </summary>
        public string Reasoning { get; set; }

        /// <summary>
        /// Indicates whether the match should be considered successful based on confidence threshold.
        /// </summary>
        public bool IsSuccessfulMatch => Series != null && Confidence >= 0.7;

        /// <summary>
        /// Alternative matches with lower confidence scores for potential manual selection.
        /// </summary>
        public List<AlternativeMatch> Alternatives { get; set; } = [];
    }

    /// <summary>
    /// Represents an alternative match suggestion from the LLM.
    /// </summary>
    public class AlternativeMatch
    {
        public Series Series { get; set; }
        public double Confidence { get; set; }
        public string Reasoning { get; set; }
    }
}
