using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Tags
{
    public interface ITagRepository : IBasicRepository<Tag>
    {
        Tag GetByLabel(string label);
        Tag FindByLabel(string label);
        List<Tag> GetTags(HashSet<int> tagIds);

        Task<Tag> GetByLabelAsync(string label, CancellationToken cancellationToken = default);
        Task<Tag> FindByLabelAsync(string label, CancellationToken cancellationToken = default);
        Task<List<Tag>> GetTagsAsync(HashSet<int> tagIds, CancellationToken cancellationToken = default);
    }

    public class TagRepository : BasicRepository<Tag>, ITagRepository
    {
        public TagRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Tag GetByLabel(string label)
        {
            var model = Query(c => c.Label == label).SingleOrDefault();

            if (model == null)
            {
                throw new InvalidOperationException("Didn't find tag with label " + label);
            }

            return model;
        }

        public Tag FindByLabel(string label)
        {
            return Query(c => c.Label == label).SingleOrDefault();
        }

        public List<Tag> GetTags(HashSet<int> tagIds)
        {
            return Query(t => tagIds.Contains(t.Id));
        }

        public async Task<Tag> GetByLabelAsync(string label, CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(c => c.Label == label, cancellationToken).ConfigureAwait(false);
            var model = results.SingleOrDefault();

            if (model == null)
            {
                throw new InvalidOperationException("Didn't find tag with label " + label);
            }

            return model;
        }

        public async Task<Tag> FindByLabelAsync(string label, CancellationToken cancellationToken = default)
        {
            var results = await QueryAsync(c => c.Label == label, cancellationToken).ConfigureAwait(false);
            return results.SingleOrDefault();
        }

        public async Task<List<Tag>> GetTagsAsync(HashSet<int> tagIds, CancellationToken cancellationToken = default)
        {
            return await QueryAsync(t => tagIds.Contains(t.Id), cancellationToken).ConfigureAwait(false);
        }
    }
}
