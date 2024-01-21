namespace NzbDrone.Core.MediaFiles
{
    public class DeletedEpisodeFile
    {
        public string RecycleBinPath { get; set; }
        public EpisodeFile EpisodeFile { get; set; }

        public DeletedEpisodeFile(EpisodeFile episodeFile, string recycleBinPath)
        {
            EpisodeFile = episodeFile;
            RecycleBinPath = recycleBinPath;
        }
    }
}
