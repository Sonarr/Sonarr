using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Tags
{
    public interface ITagService
    {
        Tag GetTag(Int32 tagId);
        List<Tag> All();
        Tag Add(Tag tag);
        Tag Update(Tag tag);
        void Delete(Int32 tagId);
    }

    public class TagService : ITagService
    {
        private readonly ITagRepository _repo;
        private readonly IEventAggregator _eventAggregator;

        public TagService(ITagRepository repo, IEventAggregator eventAggregator)
        {
            _repo = repo;
            _eventAggregator = eventAggregator;
        }

        public Tag GetTag(Int32 tagId)
        {
            return _repo.Get(tagId);
        }

        public List<Tag> All()
        {
            return _repo.All().OrderBy(t => t.Label).ToList();
        }

        public Tag Add(Tag tag)
        {
            //TODO: check for duplicate tag by label and return that tag instead?

            tag.Label = tag.Label.ToLowerInvariant();

            _repo.Insert(tag);
            _eventAggregator.PublishEvent(new TagsUpdatedEvent());

            return tag;
        }

        public Tag Update(Tag tag)
        {
            tag.Label = tag.Label.ToLowerInvariant();

            _repo.Update(tag);
            _eventAggregator.PublishEvent(new TagsUpdatedEvent());

            return tag;
        }

        public void Delete(Int32 tagId)
        {
            _repo.Delete(tagId);
            _eventAggregator.PublishEvent(new TagsUpdatedEvent());
        }
    }
}
