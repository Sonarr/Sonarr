using Workarr.MediaFiles;
using Workarr.Messaging;
using Workarr.Parser.Model;
using Workarr.Tv;

namespace Workarr.Download
{
    public class UntrackedDownloadCompletedEvent : IEvent
    {
        public Series Series { get; private set; }
        public List<Episode> Episodes { get; private set; }
        public List<EpisodeFile> EpisodeFiles { get; private set; }
        public ParsedEpisodeInfo ParsedEpisodeInfo { get; private set; }
        public string SourcePath { get; private set; }

        public UntrackedDownloadCompletedEvent(Series series, List<Episode> episodes, List<EpisodeFile> episodeFiles, ParsedEpisodeInfo parsedEpisodeInfo, string sourcePath)
        {
            Series = series;
            Episodes = episodes;
            EpisodeFiles = episodeFiles;
            ParsedEpisodeInfo = parsedEpisodeInfo;
            SourcePath = sourcePath;
        }
    }
}
