using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class RenameProvider
    {
        //TODO: Remove some of these dependencies. we shouldn't have a single class with dependency on the whole app!
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly DiskProvider _diskProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly SeriesProvider _seriesProvider;

        public RenameProvider(SeriesProvider seriesProvider,EpisodeProvider episodeProvider,
                                MediaFileProvider mediaFileProvider, DiskProvider diskProvider,
                                ConfigProvider configProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _mediaFileProvider = mediaFileProvider;
            _diskProvider = diskProvider;
            _configProvider = configProvider;
        }

        public virtual void RenameEpisodeFile(int episodeFileId, ProgressNotification notification)
        {
            var episodeFile = _mediaFileProvider.GetEpisodeFile(episodeFileId);

            try
            {
                notification.CurrentMessage = String.Format("Renaming '{0}'", episodeFile.Path);

                var series = _seriesProvider.GetSeries(episodeFile.SeriesId);
                var folder = new FileInfo(episodeFile.Path).DirectoryName;
                var newFileName = GetNewFilename(episodeFile, series.Title);
                var newFile = folder + Path.DirectorySeparatorChar + newFileName;
                _diskProvider.RenameFile(episodeFile.Path, newFile);

                notification.CurrentMessage = String.Format("Finished Renaming '{0}'", newFile);
            }

            catch (Exception e)
            {
                notification.CurrentMessage = String.Format("Failed to Rename '{0}'", episodeFile.Path);
                Logger.ErrorException("An error has occurred while renaming episode: " + episodeFile.Path, e);
                throw;
            }
        }

        public string GetNewFilename(EpisodeFile episodeFile, string seriesName)
        {
            var episodes = _episodeProvider.EpisodesByFileId(episodeFile.EpisodeFileId);
            //var series = _seriesProvider.GetSeries(episodeFile.SeriesId);

            var separatorStyle = EpisodeSortingHelper.GetSeparatorStyle(_configProvider.SeparatorStyle);
            var numberStyle = EpisodeSortingHelper.GetNumberStyle(_configProvider.NumberStyle);
            var useSeriesName = _configProvider.SeriesName;
            var useEpisodeName = _configProvider.EpisodeName;
            var replaceSpaces = _configProvider.ReplaceSpaces;
            var appendQuality = _configProvider.AppendQuality;

            var title = String.Empty;

            if (episodes.Count == 1)
            {
                if (useSeriesName)
                {
                    title += seriesName;
                    title += separatorStyle.Pattern;
                }

                title += numberStyle.Pattern.Replace("%s", String.Format("{0}", episodes[0].SeasonNumber))
                                .Replace("%0s", String.Format("{0:00}", episodes[0].SeasonNumber))
                                .Replace("%0e", String.Format("{0:00}", episodes[0].EpisodeNumber));
                
                if (useEpisodeName)
                {
                    title += separatorStyle.Pattern;
                    title += episodes[0].Title;
                }

                if (appendQuality)
                    title += String.Format(" [{0}]", episodeFile.Quality);

                if (replaceSpaces)
                    title = title.Replace(' ', '.');

                Logger.Debug("New File Name is: {0}", title);
                return title;
            }

            var multiEpisodeStyle = EpisodeSortingHelper.GetMultiEpisodeStyle(_configProvider.MultiEpisodeStyle);

            if (useSeriesName)
            {
                title += seriesName;
                title += separatorStyle.Pattern;
            }

            title += numberStyle.Pattern.Replace("%s", String.Format("{0}", episodes[0].SeasonNumber))
                                .Replace("%0s", String.Format("{0:00}", episodes[0].SeasonNumber))
                                .Replace("%0e", String.Format("{0:00}", episodes[0].EpisodeNumber));

            var numbers = String.Empty;
            var episodeNames = episodes[0].Title;

            for (int i = 1; i < episodes.Count; i++)
            {
                var episode = episodes[i];

                if (multiEpisodeStyle.Name == "Duplicate")
                {
                    numbers += separatorStyle.Pattern + numberStyle.Pattern.Replace("%s", String.Format("{0}", episode.SeasonNumber))
                                .Replace("%0s", String.Format("{0:00}", episode.SeasonNumber))
                                .Replace("%0e", String.Format("{0:00}", episode.EpisodeNumber));
                }
                else
                {
                    numbers += multiEpisodeStyle.Pattern.Replace("%s", String.Format("{0}", episode.SeasonNumber))
                                .Replace("%0s", String.Format("{0:00}", episode.SeasonNumber))
                                .Replace("%0e", String.Format("{0:00}", episode.EpisodeNumber))
                                .Replace("%x", numberStyle.EpisodeSeparator)
                                .Replace("%p", separatorStyle.Pattern);
                }

                episodeNames += String.Format(" + {0}", episode.Title);
            }

            title += numbers;

            if (useEpisodeName)
            {
                episodeNames = episodeNames.TrimEnd(' ', '+');

                title += separatorStyle.Pattern;
                title += episodeNames;
            }

            if (appendQuality)
                title += String.Format(" [{0}]", episodeFile.Quality);

            if (replaceSpaces)
                title = title.Replace(' ', '.');

            Logger.Debug("New File Name is: {0}", title);
            return title;
        }
    }
}