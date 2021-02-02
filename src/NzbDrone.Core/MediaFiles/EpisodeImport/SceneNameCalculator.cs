using System.IO;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public static class SceneNameCalculator
    {
        public static string GetSceneName(LocalEpisode localEpisode)
        {
            var otherVideoFiles = localEpisode.OtherVideoFiles;
            var downloadClientInfo = localEpisode.DownloadClientEpisodeInfo;

            if (!otherVideoFiles && downloadClientInfo != null && !downloadClientInfo.FullSeason)
            {
                return Parser.Parser.RemoveFileExtension(downloadClientInfo.ReleaseTitle);
            }

            var fileName = Path.GetFileNameWithoutExtension(localEpisode.Path.CleanFilePath());

            if (SceneChecker.IsSceneTitle(fileName))
            {
                return fileName;
            }

            var folderTitle = localEpisode.FolderEpisodeInfo?.ReleaseTitle;

            if (!otherVideoFiles &&
                localEpisode.FolderEpisodeInfo?.FullSeason == false &&
                folderTitle.IsNotNullOrWhiteSpace() &&
                SceneChecker.IsSceneTitle(folderTitle))
            {
                return folderTitle;
            }

            return null;
        }
    }
}
