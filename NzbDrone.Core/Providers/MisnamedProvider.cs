using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class MisnamedProvider
    {
        private readonly MediaFileProvider _mediaFileProvider;
        private readonly IEpisodeService _episodeService;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public MisnamedProvider(MediaFileProvider mediaFileProvider, IEpisodeService episodeService)
        {
            _mediaFileProvider = mediaFileProvider;
            _episodeService = episodeService;
        }

        public virtual List<MisnamedEpisodeModel> MisnamedFiles(int pageNumber, int pageSize, out int totalItems)
        {       
            var misnamedFiles = new List<MisnamedEpisodeModel>();

            var episodesWithFiles = _episodeService.EpisodesWithFiles().GroupBy(e => e.EpisodeFileId).ToList();
            totalItems = episodesWithFiles.Count();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var misnamedFilesSelect = episodesWithFiles.AsParallel().Where(
                w =>
                w.First().EpisodeFile.Path !=
                _mediaFileProvider.GetNewFilename(w.Select(e => e).ToList(), w.First().Series,
                                                  w.First().EpisodeFile.Quality, w.First().EpisodeFile.Proper, w.First().EpisodeFile)).Skip(Math.Max(pageSize * (pageNumber - 1), 0)).Take(pageSize);

            //Process the episodes
            misnamedFilesSelect.AsParallel().ForAll(f =>
                                                      {
                                                          var episodes = f.Select(e => e).ToList();
                                                          var firstEpisode = episodes[0];
                                                          var properName = _mediaFileProvider.GetNewFilename(episodes,
                                                                                                             firstEpisode.Series,
                                                                                                             firstEpisode.EpisodeFile.Quality, firstEpisode.EpisodeFile.Proper, firstEpisode.EpisodeFile);

                                                          var currentName = Path.GetFileNameWithoutExtension(firstEpisode.EpisodeFile.Path);

                                                          if (properName != currentName)
                                                          {
                                                              misnamedFiles.Add(new MisnamedEpisodeModel
                                                              {
                                                                  CurrentName = currentName,
                                                                  EpisodeFileId = firstEpisode.EpisodeFileId,
                                                                  ProperName = properName,
                                                                  SeriesId = firstEpisode.SeriesId,
                                                                  SeriesTitle = firstEpisode.Series.Title
                                                              });
                                                          }
                                                      });

            stopwatch.Stop();
            return misnamedFiles.OrderBy(e => e.SeriesTitle).ToList();
        }
    }
}
