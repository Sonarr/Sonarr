using System.Linq;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalEpisode
    {
        public LocalEpisode()
        {
            Episodes = new List<Episode>();
        }

        public string Path { get; set; }
        public long Size { get; set; }
        public ParsedEpisodeInfo FileEpisodeInfo { get; set; }
        public ParsedEpisodeInfo DownloadClientEpisodeInfo { get; set; }
        public ParsedEpisodeInfo FolderEpisodeInfo { get; set; }
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
        public QualityModel Quality { get; set; }
        public Language Language { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public bool ExistingFile { get; set; }
        public bool SceneSource { get; set; }
        public string ReleaseGroup { get; set; }
        
        public int SeasonNumber 
        { 
            get
            {
                var seasons = Episodes.Select(c => c.SeasonNumber).Distinct().ToList();

                if (seasons.Empty())
                {
                    throw new InvalidSeasonException("Expected one season, but found none");
                }

                if (seasons.Count > 1)
                {
                    throw new InvalidSeasonException("Expected one season, but found {0} ({1})", seasons.Count, string.Join(", ", seasons));
                }

                return seasons.Single();
            } 
        }

        public bool IsSpecial => SeasonNumber == 0;

        public override string ToString()
        {
            return Path;
        }
    }
}
