using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using Newtonsoft.Json.Serialization;
using NLog;
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
        private readonly Logger _logger;

        public EpisodeFilePreferredWordCalculator(IPreferredWordService preferredWordService, Logger logger)
        {
            _preferredWordService = preferredWordService;
            _logger = logger;
        }

        public int Calculate(Series series, EpisodeFile episodeFile)
        {
            var scores = new List<int>();

            if (episodeFile.SceneName.IsNotNullOrWhiteSpace())
            {
                scores.Add(_preferredWordService.Calculate(series, episodeFile.SceneName, 0));
            }
            else
            {
                _logger.Trace("No stored scene name for {0}", episodeFile);
            }

            // The file may not have a screen name if the file/folder name contained spaces, but the original path is still store and valuable.
            if (episodeFile.OriginalFilePath.IsNotNullOrWhiteSpace())
            {
                var segments = episodeFile.OriginalFilePath.Split(Path.DirectorySeparatorChar).ToList();

                for (int i = 0; i < segments.Count; i++)
                {
                    var isLast = i == segments.Count - 1;
                    var segment = isLast ? Path.GetFileNameWithoutExtension(segments[i]) : segments[i];

                    scores.Add(_preferredWordService.Calculate(series, segment, 0));
                }
            }
            else
            {
                _logger.Trace("No stored scene name for {0}", episodeFile);
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
