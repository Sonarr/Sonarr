using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;


namespace NzbDrone.Core.Providers
{
    public class MisnamedProvider
    {
        private readonly IEpisodeService _episodeService;
        private readonly IBuildFileNames _buildFileNames;


        public MisnamedProvider(IEpisodeService episodeService, IBuildFileNames buildFileNames)
        {
            _episodeService = episodeService;
            _buildFileNames = buildFileNames;
        }

        public virtual List<MisnamedEpisodeModel> MisnamedFiles(int pageNumber, int pageSize, out int totalItems)
        {
            var misnamedFiles = new List<MisnamedEpisodeModel>();

            var episodesWithFiles = _episodeService.EpisodesWithFiles().GroupBy(e => e.EpisodeFileId).ToList();
            totalItems = episodesWithFiles.Count();

            var misnamedFilesSelect = episodesWithFiles.AsParallel().Where(
                w =>
                w.First().EpisodeFile.Value.Path !=
                _buildFileNames.BuildFilename(w.Select(e => e).ToList(), w.First().Series, w.First().EpisodeFile)).Skip(Math.Max(pageSize * (pageNumber - 1), 0)).Take(pageSize);

            //Process the episodes
            misnamedFilesSelect.AsParallel().ForAll(f =>
                {
                    var episodes = f.Select(e => e).ToList();
                    var firstEpisode = episodes[0];
                    var properName = _buildFileNames.BuildFilename(episodes, firstEpisode.Series,
                                                                   firstEpisode.EpisodeFile);

                    var currentName = Path.GetFileNameWithoutExtension(firstEpisode.EpisodeFile.Value.Path);

                    if (properName != currentName)
                    {
                        misnamedFiles.Add(new MisnamedEpisodeModel
                            {
                                CurrentName = currentName,
                                EpisodeFileId = firstEpisode.EpisodeFileId,
                                ProperName = properName,
                                SeriesTitle = firstEpisode.Series.Value.Title
                            });
                    }
                });

            return misnamedFiles.OrderBy(e => e.SeriesTitle).ToList();
        }
    }
}
