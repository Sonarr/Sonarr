using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.ImportLists.Exclusions
{
    public interface IImportListExclusionService
    {
        ImportListExclusion Add(ImportListExclusion importListExclusion);
        List<ImportListExclusion> All();
        void Delete(int id);
        ImportListExclusion Get(int id);
        ImportListExclusion FindByTvdbId(int tvdbId);
        ImportListExclusion Update(ImportListExclusion importListExclusion);
    }

    public class ImportListExclusionService : IImportListExclusionService, IHandleAsync<SeriesDeletedEvent>
    {
        private readonly IImportListExclusionRepository _repo;

        public ImportListExclusionService(IImportListExclusionRepository repo)
        {
            _repo = repo;
        }

        public ImportListExclusion Add(ImportListExclusion importListExclusion)
        {
            return _repo.Insert(importListExclusion);
        }

        public ImportListExclusion Update(ImportListExclusion importListExclusion)
        {
            return _repo.Update(importListExclusion);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }

        public ImportListExclusion Get(int id)
        {
            return _repo.Get(id);
        }

        public ImportListExclusion FindByTvdbId(int tvdbId)
        {
            return _repo.FindByTvdbId(tvdbId);
        }

        public List<ImportListExclusion> All()
        {
            return _repo.All().ToList();
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            if (!message.AddImportListExclusion)
            {
                return;
            }

            var exclusionsToAdd = new List<ImportListExclusion>();

            foreach (var series in message.Series.DistinctBy(s => s.TvdbId))
            {
                var existingExclusion = _repo.FindByTvdbId(series.TvdbId);

                if (existingExclusion != null)
                {
                    continue;
                }

                exclusionsToAdd.Add(new ImportListExclusion
                {
                    TvdbId = series.TvdbId,
                    Title = series.Title
                });
            }

            _repo.InsertMany(exclusionsToAdd);
        }
    }
}
