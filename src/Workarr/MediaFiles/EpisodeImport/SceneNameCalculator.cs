using Workarr.Extensions;
using Workarr.Parser;
using Workarr.Parser.Model;

namespace Workarr.MediaFiles.EpisodeImport
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

            var fileName = Path.GetFileNameWithoutExtension(PathExtensions.CleanFilePath(localEpisode.Path));

            if (SceneChecker.IsSceneTitle(fileName))
            {
                return fileName;
            }

            var folderTitle = localEpisode.FolderEpisodeInfo?.ReleaseTitle;

            if (!otherVideoFiles &&
                localEpisode.FolderEpisodeInfo?.FullSeason == false &&
                StringExtensions.IsNotNullOrWhiteSpace(folderTitle) &&
                SceneChecker.IsSceneTitle(folderTitle))
            {
                return folderTitle;
            }

            return null;
        }
    }
}
