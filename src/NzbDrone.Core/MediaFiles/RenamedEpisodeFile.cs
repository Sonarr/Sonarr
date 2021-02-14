namespace NzbDrone.Core.MediaFiles
{
    public class RenamedEpisodeFile
    {
        public EpisodeFile EpisodeFile { get; set; }
        public string PreviousPath { get; set; }
        public string PreviousRelativePath { get; set; }
    }
}
