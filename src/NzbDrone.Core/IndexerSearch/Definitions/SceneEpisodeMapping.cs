using System;
using System.Collections.Generic;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class SceneEpisodeMapping
    {
        public Episode Episode { get; set; }
        public SearchMode SearchMode { get; set; }
        public List<string> SceneTitles { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int? AbsoluteEpisodeNumber { get; set; }

        public override int GetHashCode()
        {
            return SearchMode.GetHashCode() ^ SeasonNumber.GetHashCode() ^ EpisodeNumber.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as SceneEpisodeMapping;

            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            return SeasonNumber == other.SeasonNumber && EpisodeNumber == other.EpisodeNumber && SearchMode == other.SearchMode;
        }
    }
}
