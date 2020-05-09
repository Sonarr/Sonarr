using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MediaFiles
{
    public interface IEpisodeFilePreferredWordCalculator
    {
        int Calculate(Series series, EpisodeFile episodeFile);
    }

    public class EpisodeFilePreferredWordCalculator : IEpisodeFilePreferredWordCalculator
    {
        private readonly IPreferredWordService _preferredWordService;

        public EpisodeFilePreferredWordCalculator(IPreferredWordService preferredWordService)
        {
            _preferredWordService = preferredWordService;
        }
        public int Calculate(Series series, EpisodeFile episodeFile)
        {
            var scores = new List<int>();

            if (episodeFile.SceneName.IsNotNullOrWhiteSpace())
            {
                scores.Add(_preferredWordService.Calculate(series, episodeFile.SceneName, 0));
            }

            // Calculate using RelativePath or Path, but not both
            if (episodeFile.RelativePath.IsNotNullOrWhiteSpace())
            {
                scores.Add(_preferredWordService.Calculate(series, episodeFile.RelativePath, 0));
            }
            else if (episodeFile.Path.IsNotNullOrWhiteSpace())
            {
                scores.Add(_preferredWordService.Calculate(series, episodeFile.Path, 0));
            }

            // Return the highest score, this will allow media info in file names to be used to improve preferred word scoring.
            // TODO: A full map of preferred words should be de-duped and used to create an aggregated score using the scene name and the file name.

            return scores.MaxOrDefault();
        }
    }
}
