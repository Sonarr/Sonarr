using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.ImportLists.Exclusions
{
    public interface IImportListExclusionService
    {
        ImportListExclusion Add(ImportListExclusion importListExclusion);
        List<ImportListExclusion> All();
        PagingSpec<ImportListExclusion> Paged(PagingSpec<ImportListExclusion> pagingSpec);
        void Delete(int id);
        void Delete(List<int> ids);
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
            return _repo.InsertAsync(importListExclusion).GetAwaiter().GetResult();
        }

        public ImportListExclusion Update(ImportListExclusion importListExclusion)
        {
            return _repo.UpdateAsync(importListExclusion).GetAwaiter().GetResult();
        }

        public void Delete(int id)
        {
            _repo.DeleteAsync(id).GetAwaiter().GetResult();
        }

        public void Delete(List<int> ids)
        {
            _repo.DeleteManyAsync(ids).GetAwaiter().GetResult();
        }

        public ImportListExclusion Get(int id)
        {
            return _repo.GetAsync(id).GetAwaiter().GetResult();
        }

        public ImportListExclusion FindByTvdbId(int tvdbId)
        {
            return _repo.FindByTvdbIdAsync(tvdbId).GetAwaiter().GetResult();
        }

        public List<ImportListExclusion> All()
        {
            return _repo.AllAsync().GetAwaiter().GetResult().ToList();
        }

        public PagingSpec<ImportListExclusion> Paged(PagingSpec<ImportListExclusion> pagingSpec)
        {
            return _repo.GetPagedAsync(pagingSpec).GetAwaiter().GetResult();
        }

        public async Task HandleAsync(SeriesDeletedEvent message, CancellationToken cancellationToken)
        {
            if (!message.AddImportListExclusion)
            {
                return;
            }

            var exclusionsToAdd = new List<ImportListExclusion>();

            foreach (var series in message.Series.DistinctBy(s => s.TvdbId))
            {
                var existingExclusion = await _repo.FindByTvdbIdAsync(series.TvdbId, cancellationToken).ConfigureAwait(false);

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

            await _repo.InsertManyAsync(exclusionsToAdd, cancellationToken).ConfigureAwait(false);
        }
    }
}
