using Workarr.Datastore;
using Workarr.Messaging.Events;

namespace Workarr.Tags
{
    public interface ITagRepository : IBasicRepository<Tag>
    {
        Tag GetByLabel(string label);
        Tag FindByLabel(string label);
        List<Tag> GetTags(HashSet<int> tagIds);
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
    }
}
